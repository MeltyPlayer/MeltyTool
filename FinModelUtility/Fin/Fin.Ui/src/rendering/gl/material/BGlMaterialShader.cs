using System.Numerics;

using fin.math;
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
  private readonly GlShaderProgram impl_;

  private readonly IShaderUniform<Vector3> cameraPositionUniform_;
  private readonly IShaderUniform<float> shininessUniform_;

  protected BGlMaterialShader(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      TMaterial material,
      IReadOnlyTextureTransformManager? textureTransformManager) {
    this.model_ = model;
    this.Material = material;
    this.textureTransformManager_ = textureTransformManager;

    var shaderSource
        = this.GenerateShaderSource(model, modelRequirements, material);
    this.impl_ = GlShaderProgram.FromShaders(
        shaderSource.VertexShaderSource,
        shaderSource.FragmentShaderSource);

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
    this.impl_.Dispose();

    if (this.DisposeTextures) {
      foreach (var cachedTextureUniformData in
               this.cachedTextureUniformDatas_) {
        GlMaterialConstants.DisposeIfNotCommon(
            cachedTextureUniformData.GlTexture);
      }
    }

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

  public string VertexShaderSource => this.impl_.VertexShaderSource;
  public string FragmentShaderSource => this.impl_.FragmentShaderSource;

  public IReadOnlyMaterial? Material { get; }

  public bool UseLighting { get; set; }
  public bool DisposeTextures { get; set; } = true;

  public void Use() {
    this.cameraPositionUniform_.SetAndMaybeMarkDirty(Camera.Instance.Position);

    this.shininessUniform_.SetAndMaybeMarkDirty(
        this.Material?.Shininess ?? 0);

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
      IGlTexture glTexture)
    => this.cachedTextureUniformDatas_.AddLast(
        new CachedTextureUniformData(textureName,
                                     textureIndex,
                                     finTexture,
                                     this.model_.AnimationManager.Animations,
                                     this.textureTransformManager_,
                                     glTexture,
                                     this.impl_));
}