using System.Numerics;

using fin.math;
using fin.math.floats;
using fin.model;
using fin.shaders.glsl;
using fin.shaders.glsl.source;
using fin.ui.rendering.gl.texture;


namespace fin.ui.rendering.gl.material;

public abstract class BGlMaterialShader<TMaterial> : IGlMaterialShader
    where TMaterial : IReadOnlyMaterial {
  private LinkedList<CachedTextureUniformData> cachedTextureUniformDatas_ =
      [];

  private readonly IReadOnlyModel model_;
  private readonly IReadOnlyTextureTransformManager? textureTransformManager_;
  private readonly IReadOnlyTextureFlipbookSwapManager
      textureFlipbookSwapManager_;
  private readonly GlShaderProgram impl_;

  private readonly IShaderUniform<Vector3> cameraPositionUniform_;

  private readonly IShaderUniform<bool> hasSpecularUniform_;
  private readonly IShaderUniform<float> shininessUniform_;

  protected BGlMaterialShader(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      TMaterial material,
      IReadOnlyTextureTransformManager? textureTransformManager,
      IReadOnlyTextureFlipbookSwapManager textureFlipbookSwapManager) {
    this.model_ = model;
    this.Material = material;
    this.textureTransformManager_ = textureTransformManager;
    this.textureFlipbookSwapManager_ = textureFlipbookSwapManager;

    var shaderSource
        = this.GenerateShaderSource(model, modelRequirements, material);
    this.impl_ = GlShaderProgram.FromShaders(
        shaderSource.VertexShaderSource,
        shaderSource.FragmentShaderSource);

    this.hasSpecularUniform_ = this.impl_.GetUniformBool(
        GlslConstants.UNIFORM_HAS_SPECULAR_NAME);
    this.shininessUniform_ = this.impl_.GetUniformFloat(
        GlslConstants.UNIFORM_SHININESS_NAME);

    this.cameraPositionUniform_ = this.impl_.GetUniformVec3(
        GlslConstants.UNIFORM_CAMERA_POSITION_NAME);

    this.Setup(material, this.impl_);
  }

  ~BGlMaterialShader() => this.Dispose();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.impl_?.Dispose();
    this.DisposeInternal();
  }

  protected abstract void DisposeInternal();

  protected virtual IShaderSourceGlsl GenerateShaderSource(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      TMaterial material) => material.ToShaderSource(model, modelRequirements);

  protected abstract void Setup(TMaterial material,
                                GlShaderProgram shaderProgram);

  protected abstract void PassUniformsAndBindTextures(
      GlShaderProgram shaderProgram);

  public IReadOnlyMaterial? Material { get; }
  public IReadOnlyShaderProgram ShaderProgram => this.impl_;

  public void Use() {
    this.cameraPositionUniform_.SetAndMaybeMarkDirty(Camera.Instance.Position);

    var shininess = this.Material?.Shininess ?? 0;
    this.hasSpecularUniform_.SetAndMaybeMarkDirty(!shininess.IsRoughly0());
    this.shininessUniform_.SetAndMaybeMarkDirty(shininess);

    foreach (var cachedTextureUniformData in
             this.cachedTextureUniformDatas_) {
      cachedTextureUniformData.BindTextureAndPassInUniforms();
    }

    this.PassUniformsAndBindTextures(this.impl_);

    this.impl_.Use();
  }

  protected void SetUpTexture(
      string textureName,
      int textureIndex,
      IReadOnlyTexture? finTexture,
      IGlTexture fallbackGlTexture)
    => this.cachedTextureUniformDatas_.AddLast(
        new CachedTextureUniformData(textureName,
                                     textureIndex,
                                     finTexture,
                                     fallbackGlTexture,
                                     this.model_.AnimationManager.Animations,
                                     this.textureTransformManager_,
                                     this.textureFlipbookSwapManager_,
                                     this.impl_));
}