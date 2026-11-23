using System;
using System.Drawing;
using System.Linq;
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
using fin.ui.rendering.viewer;
using fin.util.linq;
using fin.util.time;

namespace marioartisttool.view;

public sealed class PolygonStudioSkyboxRenderer : ISkyboxRenderer {
  private IModelRenderer? impl_;

  private IShaderUniform<Matrix4x4> inverseProjectionViewMatrixUniform_;
  private IShaderUniform<float> nearPlaneUniform_;
  private IShaderUniform<float> farPlaneUniform_;
  private IShaderUniform<float> iTimeUniform_;

  public float ViewportWidth { get; set; }
  public float ViewportHeight { get; set; }

  public float NearPlane { get; set; }
  public float FarPlane { get; set; }

  ~PolygonStudioSkyboxRenderer() => this.ReleaseUnmanagedResources_();

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
    this.iTimeUniform_.SetAndMarkDirty(
        (float) FrameTime.ElapsedTimeSinceApplicationOpened.TotalSeconds);

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

          layout(location = 0) in vec3 in_Position;

          out vec2 screenPosition;
            
          void main() {
            screenPosition = in_Position.xy * vec2(1.0, -1.0);
            gl_Position = {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}} * vec4(in_Position, 1.0);
          }
          """,
        $$"""
          #version {{GlslConstants.FRAGMENT_HEADER_WITHOUT_TEXTURES}}

          {{GlslUtil.GetMatricesHeaders(model)}}

          uniform vec3 {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}};
          
          uniform mat4 inverseProjectionViewMatrix;
          uniform float nearPlane;
          uniform float farPlane;
          uniform float iTime;
          
          in vec2 screenPosition;

          out vec4 fragColor;
          
          // 3D Gradient noise from:
          // - https://www.shadertoy.com/view/Xsl3Dl
          // - https://www.shadertoy.com/view/NtsBzB
          vec3 hash( vec3 p ) {
          	p = vec3( dot(p,vec3(127.1,311.7, 74.7)),
          			  dot(p,vec3(269.5,183.3,246.1)),
          			  dot(p,vec3(113.5,271.9,124.6)));
          
          	return -1.0 + 2.0*fract(sin(p)*43758.5453123);
          }
          
          float noise( in vec3 p ) {
              vec3 i = floor( p );
              vec3 f = fract( p );
          	
          	vec3 u = f*f*(3.0-2.0*f);
          
            return mix( mix( mix( dot( hash( i + vec3(0.0,0.0,0.0) ), f - vec3(0.0,0.0,0.0) ), 
                                  dot( hash( i + vec3(1.0,0.0,0.0) ), f - vec3(1.0,0.0,0.0) ), u.x),
                             mix( dot( hash( i + vec3(0.0,1.0,0.0) ), f - vec3(0.0,1.0,0.0) ), 
                                  dot( hash( i + vec3(1.0,1.0,0.0) ), f - vec3(1.0,1.0,0.0) ), u.x), u.y),
                        mix( mix( dot( hash( i + vec3(0.0,0.0,1.0) ), f - vec3(0.0,0.0,1.0) ), 
                                  dot( hash( i + vec3(1.0,0.0,1.0) ), f - vec3(1.0,0.0,1.0) ), u.x),
                             mix( dot( hash( i + vec3(0.0,1.0,1.0) ), f - vec3(0.0,1.0,1.0) ), 
                                  dot( hash( i + vec3(1.0,1.0,1.0) ), f - vec3(1.0,1.0,1.0) ), u.x), u.y), u.z );
          }
          
          // All components are in the range [0…1], including hue.
          vec3 hsv2rgb(in vec3 c) {
            vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
            vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
            return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
          }
          
          const mat3 m = mat3( 0.00,  0.80,  0.60,
                              -0.80,  0.36, -0.48,
                              -0.60, -0.48,  0.64 );
          
          float simpleNoise(vec3 q) {
            float f  = 0.5000*noise( q ); q = m*q*2.01;
            f += 0.2500*noise( q ); q = m*q*2.02;
            f += 0.1250*noise( q ); q = m*q*2.03;
            f += 0.0625*noise( q ); q = m*q*2.01;
        
            float r = 0.3;
        
            f = smoothstep( -r, r, f );
            
            return f;
          }
          
          vec4 calculateStarRgba(vec3 stars_direction) {
            // Stars computation:
          	float stars_threshold = 8.0f; // modifies the number of stars that are visible
          	float stars_exposure = 200.0f; // modifies the overall strength of the stars
          	
            float starAlpha = pow(clamp(noise(stars_direction * 200.0f), 0.0f, 1.0f), stars_threshold);
            starAlpha *= stars_exposure;
          	starAlpha *= mix(0.4, 1.4, noise(stars_direction * 100.0f + vec3(iTime))); // time based flickering
                  
            vec3 starHsv = vec3(
                simpleNoise(stars_direction * 50.0f), 
                simpleNoise(stars_direction * 60.0f),
                1.0);
            vec3 starRgb = hsv2rgb(starHsv);
          	
            // Output to screen
            return vec4(starRgb, starAlpha);
          }

          void main() {
            // ray from camera to fragment in world space
            vec3 rayWorld = (inverseProjectionViewMatrix * vec4(screenPosition * (farPlane - nearPlane), farPlane + nearPlane, farPlane - nearPlane)).xyz;
            rayWorld = -normalize(rayWorld);
            
            vec3 skyColor1 = {{FinColor.FromSystemColor(Color.FromArgb(0, 1, 12)).ToGlslVec3()}};
            vec3 skyColor2 = {{FinColor.FromSystemColor(Color.FromArgb(1, 1, 56)).ToGlslVec3()}};
          
            vec3 bgColor = mix(skyColor1, skyColor2, clamp(0.5 + rayWorld.z / 2.0, 0.0, 1.0));
            vec4 starColor = calculateStarRgba(rayWorld);
            
            fragColor = vec4(mix(bgColor, starColor.rgb, starColor.a), 1.0);
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
    this.iTimeUniform_ = shaderProgram.GetUniformFloat("iTime");

    return modelRenderer;
  }
}