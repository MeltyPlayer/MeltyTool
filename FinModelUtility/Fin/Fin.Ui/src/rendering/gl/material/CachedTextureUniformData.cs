using System.Numerics;

using fin.math;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.texture;


namespace fin.ui.rendering.gl.material;

public class CachedTextureUniformData {
  private readonly bool needsStruct_;

  public int TextureIndex { get; }
  public IReadOnlyTexture? FinTexture { get; }
  public readonly IReadOnlyTextureTransformManager? textureTransformManager_;
  public IGlTexture GlTexture { get; }

  public IShaderUniform<int> SamplerUniform { get; }
  public IShaderUniform<Vector2> ClampMinUniform { get; }
  public IShaderUniform<Vector2> ClampMaxUniform { get; }
  public IShaderUniform<Matrix3x2> Transform2dUniform { get; }
  public IShaderUniform<Matrix4x4> Transform3dUniform { get; }

  public CachedTextureUniformData(
      string textureName,
      int textureIndex,
      IReadOnlyTexture? finTexture,
      IReadOnlyList<IReadOnlyModelAnimation> animations,
      IReadOnlyTextureTransformManager? textureTransformManager,
      IGlTexture glTexture,
      GlShaderProgram shaderProgram) {
    this.TextureIndex = textureIndex;
    this.FinTexture = finTexture;
    this.textureTransformManager_ = textureTransformManager;
    this.GlTexture = glTexture;

    this.needsStruct_ = finTexture.NeedsTextureShaderStruct(animations);
    if (!this.needsStruct_) {
      this.SamplerUniform = shaderProgram.GetUniformInt($"{textureName}");
    } else {
      this.SamplerUniform =
          shaderProgram.GetUniformInt($"{textureName}.sampler");
      this.ClampMinUniform =
          shaderProgram.GetUniformVec2($"{textureName}.clampMin");
      this.ClampMaxUniform =
          shaderProgram.GetUniformVec2($"{textureName}.clampMax");
      this.Transform2dUniform =
          shaderProgram.GetUniformMat3x2($"{textureName}.transform2d");
      this.Transform3dUniform =
          shaderProgram.GetUniformMat4($"{textureName}.transform3d");
    }
  }

  public void BindTextureAndPassInUniforms() {
    this.GlTexture.Bind(this.TextureIndex);
    this.SamplerUniform.SetAndMaybeMarkDirty(this.TextureIndex);

    if (this.needsStruct_) {
      Vector2 clampMin = new(-10000);
      Vector2 clampMax = new(10000);

      if (this.FinTexture?.WrapModeU == WrapMode.MIRROR_CLAMP) {
        clampMin.X = -1;
        clampMax.X = 2;
      }

      if (this.FinTexture?.WrapModeV == WrapMode.MIRROR_CLAMP) {
        clampMin.Y = -1;
        clampMax.Y = 2;
      }

      var clampS = this.FinTexture?.ClampS;
      var clampT = this.FinTexture?.ClampT;

      if (clampS != null) {
        clampMin.X = clampS.Value.X;
        clampMax.X = clampS.Value.Y;
      }

      if (clampT != null) {
        clampMin.Y = clampT.Value.X;
        clampMax.Y = clampT.Value.Y;
      }

      this.ClampMinUniform.SetAndMaybeMarkDirty(clampMin);
      this.ClampMaxUniform.SetAndMaybeMarkDirty(clampMax);

      var setMatrix = false;
      if (this.FinTexture != null && this.textureTransformManager_ != null) {
        var matrixValues
            = this.textureTransformManager_.GetMatrix(this.FinTexture);
        if (matrixValues != null) {
          setMatrix = true;

          var (is2d, twoDMatrix, threeDMatrix) = matrixValues.Value;
          if (is2d) {
            this.Transform2dUniform.SetAndMarkDirty(twoDMatrix);
          } else {
            this.Transform3dUniform.SetAndMarkDirty(threeDMatrix);
          }
        }
      } 
      
      if (!setMatrix) {
        this.Transform2dUniform.SetAndMaybeMarkDirty(Matrix3x2.Identity);
      }
    }
  }
}