using System.Numerics;

using fin.data;
using fin.math;
using fin.model;
using fin.model.accessor;
using fin.shaders.glsl;
using fin.ui.rendering.gl.model;
using fin.util.enumerables;
using fin.util.linq;

using OpenTK.Graphics.OpenGL;

using PrimitiveType = fin.model.PrimitiveType;

namespace fin.ui.rendering.gl;

public interface IGlBufferManager : IDisposable {
  IGlBufferRenderer CreateRenderer(
      PrimitiveType primitiveType,
      IReadOnlyList<IReadOnlyVertex> triangleVertices,
      bool isFlipped = false);

  IGlBufferRenderer CreateRenderer(MergedPrimitive mergedPrimitive);
}

public interface IDynamicGlBufferManager : IGlBufferManager {
  void UpdateBuffer();
}

public interface IGlBufferRenderer : IDisposable, IRenderable;

public class GlBufferManager : IDynamicGlBufferManager {
  private readonly IReadOnlyModel model_;
  private readonly IModelRequirements modelRequirements_;
  private readonly BufferUsageHint bufferType_;
  private readonly VertexArrayObject vao_;

  public static IGlBufferManager CreateStatic(
      IReadOnlyModel model,
      IModelRequirements modelRequirements)
    => new GlBufferManager(model,
                           modelRequirements,
                           BufferUsageHint.StaticDraw);

  public static IDynamicGlBufferManager CreateDynamic(
      IReadOnlyModel model,
      IModelRequirements modelRequirements)
    => new GlBufferManager(model,
                           modelRequirements,
                           BufferUsageHint.DynamicDraw);

  private GlBufferManager(IReadOnlyModel model,
                          IModelRequirements modelRequirements,
                          BufferUsageHint bufferType) {
    this.model_ = model;
    this.modelRequirements_ = modelRequirements;
    this.bufferType_ = bufferType;
    this.vao_ = vaoCache_.GetAndIncrement(
        (this.model_, this.modelRequirements_, bufferType));
  }

  ~GlBufferManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    vaoCache_.DecrementAndMaybeDispose((this.model_,
                                        this.modelRequirements_,
                                        this.bufferType_));
  }

  private class VertexArrayObject : IDisposable {
    private readonly IReadOnlyModel model;
    private readonly IModelRequirements modelRequirements;
    private readonly BufferUsageHint bufferType;

    private const int POSITION_SIZE_ = 3;
    private const int NORMAL_SIZE_ = 3;
    private const int TANGENT_SIZE_ = 4;
    private const int BONE_ID_SIZE = 4;
    private const int BONE_WEIGHT_SIZE = 4;
    private const int UV_SIZE_ = 2;
    private const int COLOR_SIZE_ = 4;

    private readonly IReadOnlyList<IReadOnlyVertex> vertices_;
    private readonly IVertexAccessor vertexAccessor_;

    private int vaoId_;

    private int[] vboIds_;

    private readonly Vector3[] positionData_;
    private readonly Vector3[]? normalData_;
    private readonly Vector4[]? tangentData_;
    private readonly int[]? boneIdsData_;
    private readonly float[]? boneWeightsData_;

    private readonly float[][]? uvData_;
    private readonly float[][]? colorData_;

    public VertexArrayObject(IReadOnlyModel model,
                             IModelRequirements modelRequirements,
                             BufferUsageHint bufferType) {
      this.model = model;
      this.modelRequirements = ModelRequirements.FromModel(model);
      this.bufferType = bufferType;

      this.vertices_ = model.Skin.Vertices;
      this.vertexAccessor_ =
          ConsistentVertexAccessor.GetAccessorForModel(model);

      var vboCount = 1;
      this.positionData_ = new Vector3[this.vertices_.Count];

      if (modelRequirements.HasNormals) {
        vboCount++;
        this.normalData_ = new Vector3[this.vertices_.Count];
      }

      if (modelRequirements.HasTangents) {
        vboCount++;
        this.tangentData_ = new Vector4[this.vertices_.Count];
      }

      var numBones = modelRequirements.NumBones;
      if (numBones > 0) {
        vboCount += 2;
        this.boneIdsData_ = new int[numBones * this.vertices_.Count];
        this.boneWeightsData_ = new float[numBones * this.vertices_.Count];
      }

      var numUvs = modelRequirements.NumUvs;
      if (numUvs > 0) {
        vboCount += (int) numUvs;
        this.uvData_ = new float[numUvs][];
        for (var i = 0; i < this.uvData_.Length; ++i) {
          this.uvData_[i] = new float[UV_SIZE_ * this.vertices_.Count];
        }
      }

      var numColors = modelRequirements.NumColors;
      if (numColors > 0) {
        vboCount += (int) numColors;
        this.colorData_ = new float[numColors][];
        for (var i = 0; i < this.colorData_.Length; ++i) {
          var colorData = this.colorData_[i] =
              new float[COLOR_SIZE_ * this.vertices_.Count];
          for (var c = 0; c < colorData.Length; ++c) {
            colorData[c] = 1;
          }
        }
      }

      GL.GenVertexArrays(1, out this.vaoId_);
      this.vboIds_ = new int[vboCount];
      GL.GenBuffers(this.vboIds_.Length, this.vboIds_);

      this.UpdateBuffer();
    }

    ~VertexArrayObject() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      GL.DeleteVertexArrays(1, ref this.vaoId_);
      GL.DeleteBuffers(this.vboIds_.Length, this.vboIds_);
    }

    public int VaoId => this.vaoId_;

    public void UpdateBuffer() {
      var boneTransformManager = new BoneTransformManager();
      boneTransformManager.CalculateStaticMatricesForRendering(model);

      var usedBoneIndexMap = model.Skin.BonesUsedByVertices
                                  .Select((bone, index) => (index, bone))
                                  .ToDictionary(
                                      pair => (IReadOnlyBone) pair.bone,
                                      pair => pair.index);

      var numBones = (int) modelRequirements.NumBones;
      for (var i = 0; i < this.vertices_.Count; ++i) {
        this.vertexAccessor_.Target(this.vertices_[i]);
        var vertex = this.vertexAccessor_;

        boneTransformManager.ProjectVertexPositionNormalTangent(
            vertex,
            out var position,
            out var normal,
            out var tangent);
        this.positionData_[i] = position;
        if (this.normalData_ != null) {
          this.normalData_[i] = normal;
        }

        if (this.tangentData_ != null) {
          this.tangentData_[i] = tangent;
        }

        if (numBones > 0) {
          if ((vertex.BoneWeights?.Weights.Count ?? 0) == 0) {
            this.boneIdsData_[numBones * i] = 0;
            this.boneWeightsData_[numBones * i] = 1;

            for (var b = 1; b < numBones; ++b) {
              this.boneIdsData_[numBones * i + b] = 0;
              this.boneWeightsData_[numBones * i + b] = 0;
            }
          } else {
            var boneWeights = vertex.BoneWeights!.Weights;
            for (var b = 0; b < numBones; ++b) {
              int boneIndex;
              float weight;

              if (b < boneWeights.Count) {
                var boneWeight = boneWeights[b];
                boneIndex = 1 + usedBoneIndexMap[boneWeight.Bone];
                weight = boneWeight.Weight;
              } else {
                boneIndex = 0;
                weight = 0;
              }

              this.boneIdsData_[numBones * i + b] = boneIndex;
              this.boneWeightsData_[numBones * i + b] = weight;
            }
          }
        }

        if (this.uvData_ != null) {
          var uvCount = Math.Min(this.uvData_.Length, vertex.UvCount);
          for (var u = 0; u < uvCount; ++u) {
            var uv = vertex.GetUv(u);
            if (uv != null) {
              var uvOffset = UV_SIZE_ * i;
              var uvData = this.uvData_[u];
              uvData[uvOffset + 0] = uv?.X ?? 0;
              uvData[uvOffset + 1] = uv?.Y ?? 0;
            }
          }
        }

        if (this.colorData_ != null) {
          var colorCount = Math.Min(this.colorData_.Length, vertex.ColorCount);
          for (var c = 0; c < colorCount; ++c) {
            var color = vertex.GetColor(c);
            if (color != null) {
              var colorOffset = COLOR_SIZE_ * i;
              var colorData = this.colorData_[c];
              colorData[colorOffset + 0] = color?.Rf ?? 1;
              colorData[colorOffset + 1] = color?.Gf ?? 1;
              colorData[colorOffset + 2] = color?.Bf ?? 1;
              colorData[colorOffset + 3] = color?.Af ?? 1;
            }
          }
        }
      }

      GL.BindVertexArray(this.vaoId_);

      var vertexAttribIndex = 0;

      // Position
      var vertexAttribPosition = vertexAttribIndex++;
      GL.BindBuffer(BufferTarget.ArrayBuffer,
                    this.vboIds_[vertexAttribPosition]);
      GL.BufferData(BufferTarget.ArrayBuffer,
                    new IntPtr(sizeof(float) *
                               POSITION_SIZE_ *
                               this.positionData_.Length),
                    this.positionData_,
                    bufferType);
      GL.EnableVertexAttribArray(vertexAttribPosition);
      GL.VertexAttribPointer(
          vertexAttribPosition,
          POSITION_SIZE_,
          VertexAttribPointerType.Float,
          false,
          0,
          0);

      // Normal
      if (this.normalData_ != null) {
        var vertexAttribNormal = vertexAttribIndex++;
        GL.BindBuffer(BufferTarget.ArrayBuffer,
                      this.vboIds_[vertexAttribNormal]);
        GL.BufferData(BufferTarget.ArrayBuffer,
                      new IntPtr(sizeof(float) *
                                 NORMAL_SIZE_ *
                                 this.normalData_.Length),
                      this.normalData_,
                      bufferType);
        GL.EnableVertexAttribArray(vertexAttribNormal);
        GL.VertexAttribPointer(
            vertexAttribNormal,
            NORMAL_SIZE_,
            VertexAttribPointerType.Float,
            false,
            0,
            0);
      }

      // Tangent
      if (this.tangentData_ != null) {
        var vertexAttribTangent = vertexAttribIndex++;
        GL.BindBuffer(BufferTarget.ArrayBuffer,
                      this.vboIds_[vertexAttribTangent]);
        GL.BufferData(BufferTarget.ArrayBuffer,
                      new IntPtr(sizeof(float) *
                                 TANGENT_SIZE_ *
                                 this.tangentData_.Length),
                      this.tangentData_,
                      bufferType);
        GL.EnableVertexAttribArray(vertexAttribTangent);
        GL.VertexAttribPointer(
            vertexAttribTangent,
            TANGENT_SIZE_,
            VertexAttribPointerType.Float,
            false,
            0,
            0);
      }

      if (numBones > 0) {
        // Bone ids
        var vertexAttribBoneIds = vertexAttribIndex++;
        GL.BindBuffer(BufferTarget.ArrayBuffer,
                      this.vboIds_[vertexAttribBoneIds]);
        GL.BufferData(BufferTarget.ArrayBuffer,
                      new IntPtr(sizeof(int) *
                                 this.boneIdsData_.Length),
                      this.boneIdsData_,
                      bufferType);
        GL.EnableVertexAttribArray(vertexAttribBoneIds);
        GL.VertexAttribIPointer(
            vertexAttribBoneIds,
            numBones,
            VertexAttribIPointerType.Int,
            0,
            0);

        // Bone weights
        var vertexAttribBoneWeights = vertexAttribIndex++;
        GL.BindBuffer(BufferTarget.ArrayBuffer,
                      this.vboIds_[vertexAttribBoneWeights]);
        GL.BufferData(BufferTarget.ArrayBuffer,
                      new IntPtr(sizeof(float) *
                                 this.boneWeightsData_.Length),
                      this.boneWeightsData_,
                      bufferType);
        GL.EnableVertexAttribArray(vertexAttribBoneWeights);
        GL.VertexAttribPointer(
            vertexAttribBoneWeights,
            numBones,
            VertexAttribPointerType.Float,
            false,
            0,
            0);
      }

      // Uv
      if (this.uvData_ != null) {
        for (var i = 0; i < this.uvData_.Length; ++i) {
          var vertexAttribUv = vertexAttribIndex++;
          GL.BindBuffer(BufferTarget.ArrayBuffer, this.vboIds_[vertexAttribUv]);
          GL.BufferData(BufferTarget.ArrayBuffer,
                        new IntPtr(sizeof(float) * this.uvData_[i].Length),
                        this.uvData_[i],
                        bufferType);
          GL.EnableVertexAttribArray(vertexAttribUv);
          GL.VertexAttribPointer(
              vertexAttribUv,
              UV_SIZE_,
              VertexAttribPointerType.Float,
              false,
              0,
              0);
        }
      }


      // Color
      if (this.colorData_ != null) {
        for (var i = 0; i < this.colorData_.Length; ++i) {
          var vertexAttribColor = vertexAttribIndex++;
          GL.BindBuffer(BufferTarget.ArrayBuffer,
                        this.vboIds_[vertexAttribColor]);
          GL.BufferData(BufferTarget.ArrayBuffer,
                        new IntPtr(sizeof(float) * this.colorData_[i].Length),
                        this.colorData_[i],
                        bufferType);
          GL.EnableVertexAttribArray(vertexAttribColor);
          GL.VertexAttribPointer(
              vertexAttribColor,
              COLOR_SIZE_,
              VertexAttribPointerType.Float,
              false,
              0,
              0);
        }
      }

      // Make sure the buffers are not changed by outside code
      GL.BindVertexArray(0);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
  }

  private static ReferenceCountCacheDictionary<
          (IReadOnlyModel, IModelRequirements, BufferUsageHint),
          VertexArrayObject>
      vaoCache_ = new(modelAndBufferType => new VertexArrayObject(
                          modelAndBufferType.Item1,
                          modelAndBufferType.Item2,
                          modelAndBufferType.Item3),
                      (_, vao) => vao.Dispose());

  public IGlBufferRenderer CreateRenderer(
      PrimitiveType primitiveType,
      IReadOnlyList<IReadOnlyVertex> triangleVertices,
      bool isFlipped = false)
    => new GlBufferRenderer(this.vao_.VaoId,
                            primitiveType,
                            isFlipped,
                            triangleVertices);

  public IGlBufferRenderer CreateRenderer(MergedPrimitive mergedPrimitive)
    => new GlBufferRenderer(this.vao_.VaoId, mergedPrimitive);

  public void UpdateBuffer() => this.vao_.UpdateBuffer();

  public class GlBufferRenderer : IGlBufferRenderer {
    private readonly int vaoId_;
    private int eboId_;
    private BeginMode beginMode_;
    private readonly bool isFlipped_;

    private readonly int[] indices_;

    private const DrawElementsType INDEX_TYPE = DrawElementsType.UnsignedInt;

    public GlBufferRenderer(
        int vaoId,
        PrimitiveType primitiveType,
        bool isFlipped,
        IEnumerable<IReadOnlyVertex> vertices) : this(
        vaoId,
        new MergedPrimitive {
            PrimitiveType = primitiveType,
            Vertices = vertices.Yield(),
            IsFlipped = isFlipped
        }) { }

    public GlBufferRenderer(
        int vaoId,
        MergedPrimitive mergedPrimitive) {
      this.vaoId_ = vaoId;
      this.beginMode_ = mergedPrimitive.PrimitiveType switch {
          PrimitiveType.POINTS         => BeginMode.Points,
          PrimitiveType.LINES          => BeginMode.Lines,
          PrimitiveType.LINE_STRIP     => BeginMode.LineStrip,
          PrimitiveType.TRIANGLES      => BeginMode.Triangles,
          PrimitiveType.TRIANGLE_FAN   => BeginMode.TriangleFan,
          PrimitiveType.TRIANGLE_STRIP => BeginMode.TriangleStrip,
          PrimitiveType.QUADS          => BeginMode.Quads,
      };
      this.isFlipped_ = mergedPrimitive.IsFlipped;

      GL.BindVertexArray(this.vaoId_);
      GL.GenBuffers(1, out this.eboId_);

      IReadOnlyList<int> restartIndex = [
          (int) (INDEX_TYPE switch {
              DrawElementsType.UnsignedByte  => byte.MaxValue,
              DrawElementsType.UnsignedShort => ushort.MaxValue,
              DrawElementsType.UnsignedInt   => uint.MaxValue,
          })
      ];
      this.indices_ =
          mergedPrimitive
              .Vertices
              .Select(vertices
                          => vertices.Select(vertex => vertex.Index))
              .Intersperse(restartIndex)
              .SelectMany(indices => indices)
              .ToArray();

      GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.eboId_);
      GL.BufferData(BufferTarget.ElementArrayBuffer,
                    new IntPtr(sizeof(int) * this.indices_.Length),
                    this.indices_,
                    BufferUsageHint.StaticDraw);

      GL.BindVertexArray(0);
    }

    ~GlBufferRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() {
      GL.DeleteBuffers(1, ref this.eboId_);
    }

    public void Render() {
      GlUtil.SetFlipFaces(this.isFlipped_);
      GlUtil.BindVao(this.vaoId_);

      GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.eboId_);

      GL.DrawElements(
          this.beginMode_,
          this.indices_.Length,
          INDEX_TYPE,
          0);
    }
  }
}