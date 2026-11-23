
using System.Drawing;
using System.Numerics;

using fin.color;
using fin.math.matrix.four;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.util.linq;

namespace fin.ui.rendering.viewer;

public interface ISkyboxRenderer : IOrthoRenderable {
  float NearPlane { get; set; }
  float FarPlane { get; set; }
}

public sealed class SkyboxRenderer : ISkyboxRenderer {
  private IModelRenderer? impl_;

  private IShaderUniform<Matrix4x4> inverseProjectionViewMatrixUniform_;
  private IShaderUniform<float> nearPlaneUniform_;
  private IShaderUniform<float> farPlaneUniform_;

  public float ViewportWidth { get; set; }
  public float ViewportHeight { get; set; }

  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

  ~SkyboxRenderer() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_?.Dispose();


  public void Render() {
    this.impl_ ??= this.GenerateModelIfNull_();

    this.inverseProjectionViewMatrixUniform_
        .SetAndMaybeMarkDirty(
            (GlTransform.ViewMatrix * GlTransform.ProjectionMatrix)
            .AssertInvert());

    this.nearPlaneUniform_.SetAndMaybeMarkDirty(this.NearPlane);
    this.farPlaneUniform_.SetAndMaybeMarkDirty(this.FarPlane);

    GlTransform.MatrixMode(TransformMatrixMode.MODEL);
    GlTransform.LoadIdentity();

    var width = this.ViewportWidth;
    var height = this.ViewportHeight;

    var hWidth = width / 2f;
    var hHeight = height / 2f;

    GlTransform.Ortho2d(0, (int) width, (int) height, 0);
    GlTransform.Translate(hWidth, hHeight, 0);
    GlTransform.Scale(hWidth, hHeight, 1);

    this.impl_.Render();
  }

  private IModelRenderer GenerateModelIfNull_() {
    var model = ModelImpl.CreateForViewer();

    var mesh = model.Skin.AddMesh();

    var material = model.MaterialManager.AddShaderMaterial(
        $$"""
          #version {{GlslConstants.VERTEX_SHADER_VERSION}}

          {{GlslUtil.GetMatricesHeaders(model)}}

          uniform mat4 inverseProjectionViewMatrix;
          uniform float nearPlane;
          uniform float farPlane;
          
          layout(location = 0) in vec3 in_Position;

          out vec3 rayWorld;
            
          void main() {
            gl_Position = {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}} * vec4(in_Position, 1.0);

            // ray from camera to fragment in world space
            vec2 screenPosition = in_Position.xy * vec2(1.0, -1.0);
            rayWorld = (inverseProjectionViewMatrix * vec4(screenPosition * (farPlane - nearPlane), farPlane + nearPlane, farPlane - nearPlane)).xyz;
            rayWorld = -normalize(rayWorld);
          }
          """,
        $$"""
          {{GlslConstants.FRAGMENT_HEADER_WITHOUT_TEXTURES}}

          {{GlslUtil.GetMatricesHeaders(model)}}

          uniform vec3 {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}};
          
          in vec3 rayWorld;

          out vec4 fragColor;

          void main() {
            vec4 groundColor = {{FinColor.FromHexString("#423431").ToGlslVec4()}};
            vec4 skyColor1 = {{FinColor.FromSystemColor(Color.AliceBlue).ToGlslVec4()}};
            vec4 skyColor2 = {{FinColor.FromSystemColor(Color.DarkBlue).ToGlslVec4()}};
          
            if (rayWorld.z > 0.0) {
              // calculate fragment position in world space
              float t = -({{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}}.z) / rayWorld.z;
              vec2 vertexPosition = {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}}.xy + t * rayWorld.xy;
              
              // calculate planar distance from camera to fragment (used for fading)
              float distPlanar = distance(vec3(vertexPosition, 0.0), {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}});
            
              // distance fade factor
              float fadeStart = {{ViewerConstants.FOG_START:0.0###########}};
              float fadeEnd = {{ViewerConstants.FOG_END:0.0###########}};
              float fadeFactor = clamp((distPlanar - fadeStart) / (fadeEnd - fadeStart), 0.0, 1.0);
          
              fragColor = mix(groundColor, skyColor1, fadeFactor);
            } else {
              fragColor = mix(skyColor1, skyColor2, clamp(-rayWorld.z, 0.0, 1.0));
            }
          }
          """);
    material.DepthMode = DepthMode.READ_ONLY;

    var v0 = model.Skin.AddVertex(-1, -1, 0);
    var v1 = model.Skin.AddVertex(1, -1, 0);
    var v2 = model.Skin.AddVertex(1, 1, 0);
    var v3 = model.Skin.AddVertex(-1, 1, 0);

    mesh.AddQuads(v0, v1, v2, v3).SetMaterial(material);

    var modelRenderer = new ModelRenderer(model);
    modelRenderer.GenerateModelIfNull();

    var shaders = modelRenderer
                  .GetMaterialShaders(material)
                  .ToArray();
    var shader = shaders.WhereIs<IGlMaterialShader, GlShaderMaterialShader>()
                        .Single();
    var shaderProgram = shader.ShaderProgram;
    this.inverseProjectionViewMatrixUniform_
        = shaderProgram.GetUniformMat4("inverseProjectionViewMatrix");
    this.nearPlaneUniform_ = shaderProgram.GetUniformFloat("nearPlane");
    this.farPlaneUniform_ = shaderProgram.GetUniformFloat("farPlane");

    return modelRenderer;
  }
}