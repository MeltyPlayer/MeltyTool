using System.Drawing;

using fin.color;
using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl.model;

namespace fin.ui.rendering.viewer;

public class SkyboxRenderer : IRenderable {
  private IModelRenderer? impl_;

  public void Render() {
    this.impl_ ??= this.GenerateModel_();
    this.impl_.Render();
  }

  private IModelRenderer GenerateModel_() {
    var model = ModelImpl.CreateForViewer();

    var mesh = model.Skin.AddMesh();

    var material = model.MaterialManager.AddShaderMaterial(
        $$"""
          #version {{GlslConstants.VERTEX_SHADER_VERSION}}

          {{GlslUtil.GetMatricesHeader(model)}}

          layout(location = 0) in vec3 in_Position;

          out vec2 screenPosition;
            
          void main() {
            screenPosition = in_Position.xy * vec2(1, -1);
            gl_Position = {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}} * vec4(in_Position, 1.0);
          }
          """,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.GetMatricesHeader(model)}}

          uniform vec3 {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}};

          in vec2 screenPosition;

          out vec4 fragColor;

          void main() {
            float near = {{UiConstants.NEAR_PLANE:0.0###########}};
            float far = {{UiConstants.FAR_PLANE:0.0###########}};
          
            // ray from camera to fragment in world space
            mat4 invProjectionViewMatrix = inverse({{GlslConstants.UNIFORM_PROJECTION_MATRIX_NAME}} * {{GlslConstants.UNIFORM_VIEW_MATRIX_NAME}});
            vec3 rayWorld = (invProjectionViewMatrix * vec4(screenPosition * (far - near), far + near, far - near)).xyz;
            rayWorld = -normalize(rayWorld);
            
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

    return new ModelRendererV2(model);
  }
}