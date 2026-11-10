using System.Runtime.InteropServices;

using CommunityToolkit.HighPerformance;

using fin.math;
using fin.model;
using fin.ui.rendering.gl.buffers;
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

public sealed partial class GlVertexBufferManager {
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

    GlUtil.BindVao(this.VaoId);

    var eboIndices = new List<int>();
    var eboInstanceCount = 0;
    var mergedPrimitivesMetadata
        = new List<(MergedPrimitive, int vertexOffset, int vertexCount, bool isArray)>(mergedPrimitives.Count);
    foreach (var mergedPrimitive in mergedPrimitives) {
      bool isArray;
      int vertexOffset, vertexCount;

      // Nice, can reuse an existing primitive's range - but we need to make
      // sure we update isArray/vertexOffset/vertexCount appropriately
      if (mergedPrimitive.Base != null) {
        IEnumerable<IReadOnlyPrimitive> primitives = mergedPrimitive.Vertices.Select(t => t.Item1);
        (isArray, vertexOffset, _) = rangeByPrimitive[primitives.First()];
        (_, int lastOffset, int lastCount) = rangeByPrimitive[primitives.Last()];
        vertexCount = lastOffset + lastCount - vertexOffset;
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

      mergedPrimitivesMetadata.Add((mergedPrimitive, vertexOffset, vertexCount, isArray));
    }

    IGlBuffer? ebo = null;
    var eboDisposable = new Disposable(null);
    if (eboIndices.Count > 0) {
      ebo = GlBuffer.CreateStaticBuffer(
          BufferTarget.ElementArrayBuffer,
          CollectionsMarshal.AsSpan(eboIndices).AsBytes());

      var count = eboInstanceCount;
      eboDisposable.Add(() => {
        if (--count == 0) {
          ebo.Dispose();
        }
      });
    }

    for (var i = 0; i < mergedPrimitivesMetadata.Count; ++i) {
      var (mergedPrimitive, vertexOffset, vertexCount, isArray) =
          mergedPrimitivesMetadata[i];

      var primitives = mergedPrimitive.Vertices.Select(v => v.Item1)
                                      .Collapsed()
                                      .ToArray();

      // Primitives that reuse an existing range need to inherit primitiveType
      // and isFlipped from that one
      var primitiveType = mergedPrimitive.Base?.PrimitiveType ?? mergedPrimitive.PrimitiveType;
      var isFlipped = mergedPrimitive.Base?.IsFlipped ?? mergedPrimitive.IsFlipped;

      if (isArray) {
        renderers[i] = new GlBufferRenderer(
            this.VaoId,
            primitives,
            primitiveType,
            isFlipped,
            null,
            null,
            vertexOffset,
            vertexCount);
      } else {
        renderers[i] = new GlBufferRenderer(
            this.VaoId,
            primitives,
            primitiveType,
            isFlipped,
            ebo,
            eboDisposable,
            vertexOffset,
            vertexCount
        );
      }
    }

    return renderers;
  }

  public sealed class GlBufferRenderer(
      int vaoId,
      IReadOnlyList<IReadOnlyPrimitive> primitives,
      FinPrimitiveType primitiveType,
      bool isFlipped,
      IReadOnlyGlBuffer? ebo,
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

    private PrimitiveMetadataSsbo? selectionSsbo_;

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
      this.selectionSsbo_ ??= new PrimitiveMetadataSsbo(primitives);
      this.selectionSsbo_.Bind();

      GlUtil.SetFlipFaces(isFlipped);
      GlUtil.BindVao(vaoId);

      if (ebo != null) {
        GlUtil.BindEbo(ebo.Id);

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