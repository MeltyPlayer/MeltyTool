using System.Numerics;

using fin.model;
using fin.model.accessor;
using fin.model.skeleton;
using fin.shaders.glsl;

using OpenTK.Graphics.OpenGL4;

namespace fin.ui.rendering.gl;

public sealed partial class GlVertexBufferManager {
  private sealed class VertexArrayObject : IDisposable {
    private readonly IReadOnlyModel model_;
    private readonly IModelRequirements modelRequirements_;
    private readonly BufferUsageHint bufferType_;

    private const int POSITION_SIZE_ = 3;
    private const int NORMAL_SIZE_ = 3;
    private const int TANGENT_SIZE_ = 4;
    private const int BONE_ID_SIZE = 4;
    private const int BONE_WEIGHT_SIZE = 4;
    private const int UV_SIZE_ = 2;
    private const int COLOR_SIZE_ = 4;

    private readonly IReadOnlyList<IReadOnlyVertex> vertices_;
    private readonly ConsistentVertexAccessor vertexAccessor_;

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
      this.model_ = model;
      this.modelRequirements_ = ModelRequirements.FromModel(model);
      this.bufferType_ = bufferType;

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
      boneTransformManager.CalculateStaticMatricesForRendering(this.model_);

      var usedBoneIndexMap = this.model_.Skin.BonesUsedByVertices
                                 .Select((bone, index) => (index, bone))
                                 .ToDictionary(
                                     pair => (IReadOnlyBone) pair.bone,
                                     pair => pair.index);

      if (!this.modelRequirements_.HasNormals) {
        for (var i = 0; i < this.vertices_.Count; ++i) {
          var vertex = this.vertices_[i];

          boneTransformManager.ProjectVertexPosition(
              vertex,
              out var position);

          this.positionData_[i] = position;
        }
      } else if (!this.modelRequirements_.HasTangents) {
        for (var i = 0; i < this.vertices_.Count; ++i) {
          this.vertexAccessor_.Target(this.vertices_[i]);
          var vertex = this.vertexAccessor_;

          boneTransformManager.ProjectVertexPositionNormal(
              vertex,
              out var position,
              out var normal);

          this.positionData_[i] = position;
          this.normalData_![i] = normal;
        }
      } else {
        for (var i = 0; i < this.vertices_.Count; ++i) {
          this.vertexAccessor_.Target(this.vertices_[i]);
          var vertex = this.vertexAccessor_;

          boneTransformManager.ProjectVertexPositionNormalTangent(
              vertex,
              out var position,
              out var normal,
              out var tangent);

          this.positionData_[i] = position;
          this.normalData_![i] = normal;
          this.tangentData_![i] = tangent;
        }
      }

      var numBones = (int) this.modelRequirements_.NumBones;
      for (var i = 0; i < this.vertices_.Count; ++i) {
        this.vertexAccessor_.Target(this.vertices_[i]);
        var vertex = this.vertexAccessor_;

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

      GlUtil.BindVao(this.vaoId_);

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
                    this.bufferType_);
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
                      this.bufferType_);
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
                      this.bufferType_);
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
                      this.bufferType_);
        GL.EnableVertexAttribArray(vertexAttribBoneIds);
        GL.VertexAttribIPointer(
            vertexAttribBoneIds,
            numBones,
            VertexAttribIntegerType.Int,
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
                      this.bufferType_);
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
                        this.bufferType_);
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
                        this.bufferType_);
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
      GlUtil.ResetVao();
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    }
  }
}