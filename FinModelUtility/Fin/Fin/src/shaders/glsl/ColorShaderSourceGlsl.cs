using System.Text;

using fin.model;
using fin.model.extensions;
using fin.util.image;

namespace fin.shaders.glsl;

public class ColorShaderSourceGlsl : IShaderSourceGlsl {
  public ColorShaderSourceGlsl(IReadOnlyModel model,
                               IReadOnlyMaterial material,
                               bool useBoneMatrices,
                               IShaderRequirements shaderRequirements) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, useBoneMatrices, shaderRequirements);

    var hasNormals = model.Skin.HasNormalsForMaterial(material);

    var fragmentSrc = new StringBuilder();
    fragmentSrc.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    fragmentSrc.AppendLine(GlslConstants.FLOAT_PRECISION);
    fragmentSrc.AppendLine();

    if (hasNormals) {
      fragmentSrc.AppendLine(
          $"""
           {GlslUtil.GetLightHeader(true)}

           """);
    }

    fragmentSrc.AppendLine("uniform vec4 diffuseColor;");

    if (hasNormals) {
      fragmentSrc.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    fragmentSrc.AppendLine(
        $"""

         out vec4 fragColor;

         in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;
         """);

    if (hasNormals) {
      fragmentSrc.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
      fragmentSrc.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    fragmentSrc.AppendLine(
        $$"""

          void main() {
            fragColor = diffuseColor * {{GlslConstants.IN_VERTEX_COLOR_NAME}}0;
          """);

    if (hasNormals) {
      fragmentSrc.AppendLine(
          $"""
           
             // Have to renormalize because the vertex normals can become distorted when interpolated.
             vec3 fragNormal = normalize(vertexNormal);
             fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, vec4(1)).rgb,  {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
           """);
    }

    if (material.TransparencyType == TransparencyType.MASK) {
      fragmentSrc.AppendLine(
          $$"""
            
              if (fragColor.a < {{GlslConstants.MIN_ALPHA_BEFORE_DISCARD_TEXT:0.0###########}}) {
                discard;
              }
            """);
    }

    fragmentSrc.Append("}");

    this.FragmentShaderSource = fragmentSrc.ToString();
  }

  public string VertexShaderSource { get; }
  public string FragmentShaderSource { get; }
}