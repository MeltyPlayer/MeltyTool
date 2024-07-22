using System.Numerics;

using fin.math;
using fin.model;
using fin.shaders.glsl;


namespace fin.ui.rendering.gl.material;

public abstract class BGlMaterialShader<TMaterial> : IGlMaterialShader
    where TMaterial : IReadOnlyMaterial {
  private LinkedList<CachedTextureUniformData> cachedTextureUniformDatas_ =
      [];

  private LinkedList<CachedLightUniformData> cachedLightUniformDatas_ = [];

  private readonly IReadOnlyModel model_;
  private readonly IReadOnlyLighting? lighting_;
  private readonly IReadOnlyTextureTransformManager? textureTransformManager_;
  private readonly GlShaderProgram impl_;

  private readonly IShaderUniform<Vector3> cameraPositionUniform_;
  private readonly IShaderUniform<float> shininessUniform_;

  private readonly IShaderUniform<bool> useLightingUniform_;
  private readonly IShaderUniform<Vector4> ambientLightColorUniform_;

  protected BGlMaterialShader(
      IReadOnlyModel model,
      TMaterial material,
      IReadOnlyTextureTransformManager? textureTransformManager,
      IReadOnlyLighting? lighting) {
    this.model_ = model;
    this.Material = material;
    this.textureTransformManager_ = textureTransformManager;
    this.lighting_ = lighting;

    var shaderSource = this.GenerateShaderSource(model, material);
    this.impl_ = GlShaderProgram.FromShaders(
        shaderSource.VertexShaderSource,
        shaderSource.FragmentShaderSource);

    this.shininessUniform_ = this.impl_.GetUniformFloat(
        GlslConstants.UNIFORM_SHININESS_NAME);

    this.useLightingUniform_ = this.impl_.GetUniformBool(
        GlslConstants.UNIFORM_USE_LIGHTING_NAME);

    this.cameraPositionUniform_ = this.impl_.GetUniformVec3(
        GlslConstants.UNIFORM_CAMERA_POSITION_NAME);

    this.ambientLightColorUniform_ = this.impl_.GetUniformVec4(
        "ambientLightColor");

    if (lighting != null) {
      var lights = lighting.Lights;
      for (var i = 0; i < lights.Count; ++i) {
        var light = lights[i];
        this.cachedLightUniformDatas_.AddLast(
            new CachedLightUniformData(i, light, this.impl_));
      }
    }

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
      TMaterial material) => material.ToShaderSource(model, true);

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
    var cameraPosition = Camera.Instance;
    var scCamX = cameraPosition.X;
    var scCamY = cameraPosition.Y;
    var scCamZ = cameraPosition.Z;
    this.cameraPositionUniform_.SetAndMaybeMarkDirty(
        new Vector3(scCamX, scCamY, scCamZ));

    this.shininessUniform_.SetAndMaybeMarkDirty(
        this.Material?.Shininess ?? 0);

    this.PassInLightUniforms_(this.lighting_);

    foreach (var cachedTextureUniformData in
             this.cachedTextureUniformDatas_) {
      cachedTextureUniformData.BindTextureAndPassInUniforms();
    }

    this.PassUniformsAndBindTextures(this.impl_);

    this.impl_.Use();
  }

  private void PassInLightUniforms_(IReadOnlyLighting? lighting) {
    var useLighting = this.UseLighting && this.lighting_ != null;
    this.useLightingUniform_.SetAndMaybeMarkDirty(useLighting);

    if (!useLighting) {
      return;
    }

    var ambientLightStrength = lighting.AmbientLightStrength;
    var ambientLightColor = lighting.AmbientLightColor;
    this.ambientLightColorUniform_.SetAndMaybeMarkDirty(new Vector4(
          ambientLightColor.Rf * ambientLightStrength,
          ambientLightColor.Gf * ambientLightStrength,
          ambientLightColor.Bf * ambientLightStrength,
          ambientLightColor.Af * ambientLightStrength));

    foreach (var cachedLightUniformData in this.cachedLightUniformDatas_) {
      cachedLightUniformData.PassInUniforms();
    }
  }

  protected void SetUpTexture(
      string textureName,
      int textureIndex,
      IReadOnlyTexture? finTexture,
      GlTexture glTexture)
    => this.cachedTextureUniformDatas_.AddLast(
        new CachedTextureUniformData(textureName,
                                     textureIndex,
                                     finTexture,
                                     this.model_.AnimationManager.Animations,
                                     this.textureTransformManager_,
                                     glTexture,
                                     this.impl_));
}