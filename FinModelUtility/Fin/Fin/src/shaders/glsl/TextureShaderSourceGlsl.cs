using System.Linq;
using System.Text;

using fin.model;
using fin.util.enumerables;
using fin.util.image;

namespace fin.shaders.glsl;

public class TextureShaderSourceGlsl : IShaderSourceGlsl {
  public TextureShaderSourceGlsl(IReadOnlyModel model,
                                 IReadOnlyTextureMaterial material,
                                 IShaderRequirements shaderRequirements) {
    this.VertexShaderSource = GlslUtil.GetVertexSrc(model, shaderRequirements);

    var animations = model.AnimationManager.Animations;

    var diffuseTexture = material.Textures.FirstOrDefault();
    var uvIndex = diffuseTexture?.UvIndex ?? 0;
    var hasColors = shaderRequirements.UsedColors.AnyTrue();
    var hasNormals = shaderRequirements.HasNormals;

    var fragmentSrc = new StringBuilder();
    fragmentSrc.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    fragmentSrc.AppendLine(GlslConstants.FLOAT_PRECISION);

    if (hasNormals) {
      fragmentSrc.AppendLine(
          $"""

           {GlslUtil.GetLightHeader(true)}
           """);
    }

    fragmentSrc.AppendTextureStructIfNeeded(material.Textures, animations);
    fragmentSrc.AppendLine();

    if (material.DiffuseColor != null) {
      fragmentSrc.AppendLine("uniform vec4 diffuseColor;");
    }

    fragmentSrc.AppendLine(
        $"uniform {GlslUtil.GetTypeOfTexture(diffuseTexture, animations)} diffuseTexture;");

    if (hasNormals) {
      fragmentSrc.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    fragmentSrc.AppendLine(
        """

        out vec4 fragColor;

        """);

    if (hasColors) {
      fragmentSrc.AppendLine($"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (hasNormals) {
      fragmentSrc.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
    }

    fragmentSrc.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{uvIndex};");

    if (hasNormals) {
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
          """);

    fragmentSrc.AppendLine(
        $"  fragColor = {GlslUtil.ReadColorFromTexture("diffuseTexture", $"uv{uvIndex}", diffuseTexture, animations)}" +
        (hasColors ? $" * vertexColor0" : "") +
        (material.DiffuseColor != null ? " * diffuseColor" : "") +
        ";");

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