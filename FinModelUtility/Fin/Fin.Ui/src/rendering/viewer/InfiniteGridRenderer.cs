using fin.model;
using fin.model.impl;
using fin.model.util;
using fin.shaders.glsl;
using fin.ui.rendering.gl;
using fin.ui.rendering.gl.material;
using fin.ui.rendering.gl.model;
using fin.util.linq;

namespace fin.ui.rendering.viewer;

/// <summary>
///   Shamelessly stolen from:
///    - https://godotshaders.com/shader/infinite-ground-grid/
///    - https://madebyevan.com/shaders/grid/
/// </summary>
public class InfiniteGridRenderer : IRenderable {
  private IModelRenderer? impl_;

  private IShaderUniform<float> nearPlaneUniform_;
  private IShaderUniform<float> farPlaneUniform_;

  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

  public void Render() {
    this.impl_ ??= this.GenerateModel_();

    this.nearPlaneUniform_.SetAndMaybeMarkDirty(this.NearPlane);
    this.farPlaneUniform_.SetAndMaybeMarkDirty(this.FarPlane);

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
            screenPosition = in_Position.xy * vec2(1.0, -1.0);
            gl_Position = {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}} * vec4(in_Position, 1.0);
          }
          """,
        $$"""
          #version {{GlslConstants.FRAGMENT_SHADER_VERSION}}
          {{GlslConstants.FLOAT_PRECISION}}

          {{GlslUtil.GetMatricesHeader(model)}}

          uniform vec3 {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}};

          uniform float nearPlane;
          uniform float farPlane;
          
          in vec2 screenPosition;

          out vec4 fragColor;

          // calculate line mask, using a bit of fwidth() magic to make line width not affected by perspective
          vec2 grid(vec2 pos, float unit, float thickness) {
            // Compute anti-aliased world-space grid lines
            vec2 value = abs(fract(pos / unit - 0.5) * unit - unit * .5) / (fwidth(pos) * .5 * thickness);
            value = 1.0 - value;
            
            value = clamp(value, 0.0, 1.0);
          
            // Apply gamma correction
            value = vec2(pow(value.x, 1.0 / 2.2), pow(value.y, 1.0 / 2.2));
          
            return value;         
          }

          void main() {
            // ray from camera to fragment in world space
            mat4 invProjectionViewMatrix = inverse({{GlslConstants.UNIFORM_PROJECTION_MATRIX_NAME}} * {{GlslConstants.UNIFORM_VIEW_MATRIX_NAME}});
            vec3 rayWorld = (invProjectionViewMatrix * vec4(screenPosition * (farPlane - nearPlane), farPlane + nearPlane, farPlane - nearPlane)).xyz;
            rayWorld = -normalize(rayWorld);
            
            // calculate fragment position in world space
            float t = -({{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}}.z) / rayWorld.z;
            vec2 vertexPosition = {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}}.xy + t * rayWorld.xy;
            
            vec4 pp = {{GlslConstants.UNIFORM_PROJECTION_MATRIX_NAME}} * {{GlslConstants.UNIFORM_VIEW_MATRIX_NAME}} * vec4(vertexPosition, 0.0, 1.0);
            float ndcDepth = pp.z / pp.w;
            
            float near = gl_DepthRange.near;
            float far = gl_DepthRange.far;
            gl_FragDepth = (((far - near) * ndcDepth) + near + far) / 2.0;
            
            // calculate planar distance from camera to fragment (used for fading)
            float distPlanar = distance(vec3(vertexPosition, 0.0), {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}});
            float fadeStart = {{ViewerConstants.GRID_FADE_START:0.0###########}};
            float fadeEnd = {{ViewerConstants.GRID_FADE_END:0.0###########}};
            float fadeFactor = 1.0 - clamp((distPlanar - fadeStart) / (fadeEnd - fadeStart), 0.0, 1.0);
            
            fadeFactor = pow(fadeFactor, {{ViewerConstants.GRID_FADE_FALLOFF_EXPONENT:0.0###########}});
            float sizeFactor = fadeFactor;
            
            float unitSize = 1.0;
            float majorLineThickness = 2.0 * sizeFactor;
            float minorLineThickness = 1.0 * sizeFactor;
            int subdivisions = 8;
            float minorLineAlpha = .3;
            vec4 gridColor = vec4(1.0, 1.0, 1.0, 1.0);
          
            // grid
            vec2 line = grid(vertexPosition, unitSize, majorLineThickness);
            line += grid(vertexPosition, unitSize / float(subdivisions), minorLineThickness) * minorLineAlpha;
            
            vec2 lineIndex = abs(vertexPosition / (unitSize / float(subdivisions)));
            vec4 lineColor = gridColor;
            
            // X line
            if (line.x >= line.y && lineIndex.x < 1.0) {
              lineColor = vec4(1.0, 0.0, 0.0, 1.0);
            } 
            // Y line
            else if (line.y >= line.x && lineIndex.y < 1.0) {
              lineColor = vec4(0.0, 0.0, 1.0, 1.0);
            }
            
            float lineStrength = max(line.x, line.y);
            lineStrength = clamp(lineStrength, 0.0, 1.0);
          
            // final alpha
            float alphaGrid = lineStrength * lineColor.a;
            float alpha = clamp(alphaGrid, 0.0, 1.0) * fadeFactor;
            // eliminate grid above the horizon
            alpha *= step(t, 0.0);
          
            // final color (premultiplied alpha blend)
            vec3 color = (1.0 - alphaGrid) + (lineColor.rgb * alphaGrid);
            
            fragColor = vec4(color, alpha);
            
            if (alpha == 0.0) {
              discard;
            }
          }
          """);
    material.DepthCompareType = DepthCompareType.LEqual;

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
    this.nearPlaneUniform_ = shaderProgram.GetUniformFloat("nearPlane");
    this.farPlaneUniform_ = shaderProgram.GetUniformFloat("farPlane");

    return modelRenderer;
  }
}