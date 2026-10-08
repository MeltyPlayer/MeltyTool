using fin.math;
using fin.model;
using fin.ui.rendering.gl.model;
using fin.util.enumerables;

using Nito.Disposables;

using OpenTK.Graphics.OpenGL4;

using FinPrimitiveType = fin.model.PrimitiveType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace fin.ui.rendering.gl;

public partial interface IGlBufferManager {
  IGlBufferRenderer[] CreateRenderers(
      IReadOnlyList<MergedPrimitive> mergedPrimitives);
}

public interface IGlBufferRenderer : IRenderable;

public sealed partial class GlBufferManager {
  private const DrawElementsType INDEX_TYPE = DrawElementsType.UnsignedInt;

  public IGlBufferRenderer[] CreateRenderers(
      IReadOnlyList<MergedPrimitive> mergedPrimitives) {
    var restartIndex =
        (int) (INDEX_TYPE switch {
            DrawElementsType.UnsignedByte => byte.MaxValue,
            DrawElementsType.UnsignedShort => ushort.MaxValue,
            DrawElementsType.UnsignedInt => uint.MaxValue,
            _ => throw new ArgumentOutOfRangeException()
        });

    var rangeByPrimitive
        = new Dictionary<IReadOnlyPrimitive, (bool isArray, int vertexOffset,
            int vertexCount)>();

    IGlBufferRenderer[] renderers
        = new IGlBufferRenderer[mergedPrimitives.Count];
    var eboDisposable = new Disposable(null);

    GlUtil.BindVao(this.VaoId);
    GL.GenBuffers(1, out int eboId);

    var eboIndices = new List<int>();
    var eboInstanceCount = 0;
    for (var i = 0; i < mergedPrimitives.Count; ++i) {
      var mergedPrimitive = mergedPrimitives[i];

      bool isArray;
      int vertexOffset, vertexCount;

      // Nice, can reuse an existing primitive's range
      if (mergedPrimitive.Base != null) {
        var primitive = mergedPrimitive.Vertices.First().Item1;
        (isArray, vertexOffset, vertexCount) = rangeByPrimitive[primitive];
      }
      // Otherwise, find the new range
      else {
        var mpIndices = new List<int>();
        var primitiveRanges
            = new List<(IReadOnlyPrimitive primitive, int offset, int count)>();

        foreach (var (primitive, vertices) in mergedPrimitive.Vertices) {
          if (mpIndices.Count > 0 && mergedPrimitive.RequiresSeparators) {
            mpIndices.Add(restartIndex);
          }

          var offset = mpIndices.Count;

          foreach (var vertex in vertices) {
            mpIndices.Add(vertex.Index);
          }

          var count = mpIndices.Count - offset;

          primitiveRanges.Add((primitive, offset, count));
        }

        isArray = mpIndices.IsSequentiallyIncreasing(
            out vertexOffset,
            out vertexCount);
        if (!isArray) {
          vertexOffset = eboIndices.Count;
          vertexCount = mpIndices.Count;
          eboIndices.AddRange(mpIndices);
        }

        foreach (var (primitive, offset, count) in primitiveRanges) {
          rangeByPrimitive[primitive]
              = (isArray, vertexOffset + offset, count);
        }
      }

      var primitives = mergedPrimitive.Vertices.Select(v => v.Item1)
                                      .Collapsed()
                                      .ToArray();

      if (isArray) {
        renderers[i] = new GlBufferRenderer(
            this.VaoId,
            primitives,
            mergedPrimitive.PrimitiveType,
            mergedPrimitive.IsFlipped,
            null,
            null,
            vertexOffset,
            vertexCount);
      } else {
        renderers[i] = new GlBufferRenderer(
            this.VaoId,
            primitives,
            mergedPrimitive.PrimitiveType,
            mergedPrimitive.IsFlipped,
            eboId,
            eboDisposable,
            vertexOffset,
            vertexCount
        );
      }
    }

    if (eboIndices.Count == 0) {
      GL.DeleteBuffers(1, ref eboId);
    } else {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboId);
      GL.BufferData(BufferTarget.ElementArrayBuffer,
                    new IntPtr(sizeof(int) * eboIndices.Count),
                    eboIndices.ToArray(),
                    BufferUsageHint.StaticDraw);

      var count = eboInstanceCount;
      eboDisposable.Add(() => {
        if (--count == 0) {
          GL.DeleteBuffers(1, ref eboId);
        }
      });
    }

    return renderers;
  }

  public sealed class GlBufferRenderer(
      int vaoId,
      IReadOnlyList<IReadOnlyPrimitive> primitives,
      FinPrimitiveType primitiveType,
      bool isFlipped,
      int? eboId,
      IDisposable? eboDisposable,
      int vertexOffset,
      int vertexCount) : IGlBufferRenderer {
    private readonly PrimitiveType beginMode_
        = primitiveType switch {
            FinPrimitiveType.POINTS => PrimitiveType.Points,
            FinPrimitiveType.LINES => PrimitiveType.Lines,
            FinPrimitiveType.LINE_STRIP => PrimitiveType.LineStrip,
            FinPrimitiveType.TRIANGLES => PrimitiveType.Triangles,
            FinPrimitiveType.TRIANGLE_FAN => PrimitiveType.TriangleFan,
            FinPrimitiveType.TRIANGLE_STRIP => PrimitiveType.TriangleStrip,
            _ => throw new ArgumentOutOfRangeException()
        };

    private SelectionSsbo? selectionSsbo_;

    ~GlBufferRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      this.selectionSsbo_?.Dispose();
      eboDisposable?.Dispose();
    }

    public void Render() {
      this.selectionSsbo_ ??= new SelectionSsbo(primitives);
      this.selectionSsbo_.Bind();

      GlUtil.SetFlipFaces(isFlipped);
      GlUtil.BindVao(vaoId);

      if (eboId != null) {
        GlUtil.BindEbo(eboId.Value);

        GlUtil.ValidateCurrentProgram();
        GL.DrawElements(
            this.beginMode_,
            vertexCount,
            INDEX_TYPE,
            new IntPtr(sizeof(int) * vertexOffset));
      } else {
        GL.DrawArrays(
            this.beginMode_,
            vertexOffset,
            vertexCount);
      }
    }
  }
}