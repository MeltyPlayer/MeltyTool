using System.Numerics;

using fin.image;
using fin.math;
using fin.math.floats;
using fin.math.matrix.two;
using fin.math.rotations;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.util.linq;

namespace fin.ui.rendering.viewer;

public sealed class BackgroundRenderer : IRenderable, IDisposable {
  private IModelRenderer? impl_;
  private TextureSwapManager? textureSwapManager_;

  private float prevCameraYawRadians_;
  private Vector2 prevCameraPosition_;
  private float scrollX_;

  private IShaderUniform<float> aspectRatioUniform_;
  private IShaderUniform<float> scrollXUniform_;

  private bool textureDirty_ = false;

  ~BackgroundRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.impl_?.Dispose();
    this.textureSwapManager_?.Dispose();
  }

  public IReadOnlyImage? BackgroundImage {
    get;
    set {
      this.textureDirty_ = true;
      field = value;
    }
  }

  public float BackgroundImageScale {
    get;
    set {
      this.textureDirty_ = true;
      field = value;
    }
  } = 1;

  public float AspectRatio { get; set; } = 1;

  public bool IsValid => this.BackgroundImage != null;

  public void Render() {
    if (!this.IsValid) {
      return;
    }

    this.UpdateModelIfDirty_();

    if (this.BackgroundImage != null) {
      var scrollXDelta = 0f;

      var camera = Camera.Instance;
      var cameraYawRadians = camera.YawDegrees / 180 * MathF.PI;
      {
        var deltaCameraYawRadians
            = RadiansUtil.CalculateRadiansTowards(
                this.prevCameraYawRadians_,
                cameraYawRadians);

        var rotationSpeed = 5;
        scrollXDelta += -deltaCameraYawRadians /
                        MathF.Tau *
                        rotationSpeed /
                        this.BackgroundImageScale /
                        this.AspectRatio;

        this.prevCameraYawRadians_ = cameraYawRadians;
      }

      {
        var cameraPosition = camera.Position.Xy();
        var deltaCameraPosition = cameraPosition - this.prevCameraPosition_;

        var right
            = SystemVector2Util.FromRadians(cameraYawRadians - MathF.PI / 2);
        var projectionAgainstRight
            = deltaCameraPosition.ProjectionScalar(right);

        var panSpeed = 100;
        scrollXDelta += panSpeed *
                        projectionAgainstRight /
                        this.BackgroundImage.Width *
                        this.BackgroundImageScale /
                        this.AspectRatio;

        this.prevCameraPosition_ = cameraPosition;
      }

      this.aspectRatioUniform_.SetAndMarkDirty(this.AspectRatio);

      this.scrollX_ = (this.scrollX_ + scrollXDelta) % (this.BackgroundImageScale / this.AspectRatio);
      this.scrollXUniform_.SetAndMarkDirty(this.scrollX_);
    }

    this.impl_?.Render();
  }

  private void UpdateModelIfDirty_() {
    if (!this.textureDirty_) {
      return;
    }

    this.textureDirty_ = false;

    this.impl_?.Dispose();
    this.textureSwapManager_?.Dispose();
    if (this.BackgroundImage == null) {
      return;
    }

    var model = ModelImpl.CreateForViewer();

    var mesh = model.Skin.AddMesh();

    var material = model.MaterialManager.AddShaderMaterial(
        $$"""
          #version {{GlslConstants.VERTEX_SHADER_VERSION}}

          {{GlslUtil.GetMatricesHeaders(model)}}

          uniform float aspectRatio;
          uniform float scrollX;

          layout(location = 0) in vec3 in_Position;
          layout(location = 1) in vec2 in_Uv0;

          out vec2 uv;
            
          void main() {
            uv = (in_Uv0 + vec2(scrollX, 0.0)) * vec2(aspectRatio, 1);
            gl_Position = {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}} * vec4(in_Position, 1.0);
          }
          """,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          uniform sampler2D diffuseTexture;

          in vec2 uv;

          out vec4 fragColor;

          void main() {
            fragColor = texture(diffuseTexture, uv);
          }
          """);
    material.DepthMode = DepthMode.READ_ONLY;

    var texture = model.MaterialManager.CreateTexture(this.BackgroundImage);
    texture.WrapModeU = WrapMode.REPEAT;
    texture.WrapModeV = this.BackgroundImageScale.IsRoughly1()
        ? WrapMode.CLAMP
        : WrapMode.REPEAT;

    material.AddTexture("diffuseTexture", texture);

    var v0 = model.Skin.AddVertex(-1, -1, 0);
    var v1 = model.Skin.AddVertex(1, -1, 0);
    var v2 = model.Skin.AddVertex(1, 1, 0);
    var v3 = model.Skin.AddVertex(-1, 1, 0);

    var uvValue = 1 / this.BackgroundImageScale;
    v0.SetUv(0, 0);
    v1.SetUv(uvValue, 0);
    v2.SetUv(uvValue, uvValue);
    v3.SetUv(0, uvValue);

    mesh.AddQuads(v0, v1, v2, v3).SetMaterial(material);

    var textureSwapManager = this.textureSwapManager_ =
        new TextureSwapManager(model.MaterialManager.Textures);
    textureSwapManager.UpdateCurrentFlipbookSwaps(null);
    var modelRenderer = new ModelRenderer(model,
                                          null,
                                          null,
                                          textureSwapManager);
    modelRenderer.GenerateModelIfNull();

    var shaders = modelRenderer
                  .GetMaterialShaders(material)
                  .ToArray();
    var shader = shaders.WhereIs<IGlMaterialShader, GlShaderMaterialShader>()
                        .Single();
    var shaderProgram = shader.ShaderProgram;
    
    this.aspectRatioUniform_ = shaderProgram.GetUniformFloat("aspectRatio");
    this.scrollXUniform_ = shaderProgram.GetUniformFloat("scrollX");

    this.impl_ = modelRenderer;
  }
}