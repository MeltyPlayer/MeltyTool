using System.Text;

using fin.model;
using fin.model.extensions;
using fin.util.image;

namespace fin.shaders.glsl;

public class StandardShaderSourceGlsl : IShaderSourceGlsl {
  public StandardShaderSourceGlsl(
      IReadOnlyModel model,
      IReadOnlyStandardMaterial material,
      bool useBoneMatrices,
      IShaderRequirements shaderRequirements) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, useBoneMatrices, shaderRequirements);

    var animations = model.AnimationManager.Animations;

    var fragmentShaderSrc = new StringBuilder();
    fragmentShaderSrc.AppendLine($"#version {GlslConstants.SHADER_VERSION}");

    var diffuseTexture = material.DiffuseTexture;
    var hasDiffuseTexture = diffuseTexture != null;

    var normalTexture = material.NormalTexture;
    var hasNormalTexture = normalTexture != null;
    var hasNormals = hasNormalTexture ||
                     model.Skin.HasNormalsForMaterial(material);

    var ambientOcclusionTexture = material.AmbientOcclusionTexture;
    var hasAmbientOcclusionTexture = ambientOcclusionTexture != null;

    var emissiveTexture = material.EmissiveTexture;
    var hasEmissiveTexture = emissiveTexture != null;

    var specularTexture = material.SpecularTexture;
    var hasSpecularTexture = specularTexture != null;

    if (hasNormals) {
      fragmentShaderSrc.Append(
          $"""


           {GlslUtil.GetLightHeader(true)}
           """);
    }

    fragmentShaderSrc.AppendTextureStructIfNeeded(
        material.Textures,
        animations);

    if (hasDiffuseTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(diffuseTexture, animations)} diffuseTexture;");
    }

    if (hasNormalTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(normalTexture, animations)} normalTexture;");
    }

    if (hasSpecularTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(specularTexture, animations)} specularTexture;");
    }

    if (hasAmbientOcclusionTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(ambientOcclusionTexture, animations)} ambientOcclusionTexture;");
    }

    if (hasEmissiveTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(emissiveTexture, animations)} emissiveTexture;");
    }

    fragmentShaderSrc.Append(
        $"""

         uniform float {GlslConstants.UNIFORM_SHININESS_NAME};

         out vec4 fragColor;

         in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;

         """);

    if (hasNormals) {
      fragmentShaderSrc.Append(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          in vec3 tangent;
          in vec3 binormal;

          """);
    }

    var usedUvs = shaderRequirements.UsedUvs;
    for (var i = 0; i < usedUvs.Length; ++i) {
      if (usedUvs[i]) {
        fragmentShaderSrc.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{i};");
      }
    }

    if (hasNormals) {
      fragmentShaderSrc.Append(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(true)}

           """);
    }

    fragmentShaderSrc.AppendLine(
        """

        void main() {
        """
    );

    if (hasDiffuseTexture) {
      fragmentShaderSrc.AppendLine(
          $"  vec4 diffuseColor = {GlslUtil.ReadColorFromTexture("diffuseTexture", $"{GlslConstants.IN_UV_NAME}{diffuseTexture?.UvIndex ?? 0}", diffuseTexture, animations)};");
    }

    if (hasAmbientOcclusionTexture) {
      fragmentShaderSrc.AppendLine(
          $"  vec4 ambientOcclusionColor = {GlslUtil.ReadColorFromTexture("ambientOcclusionTexture", $"{GlslConstants.IN_UV_NAME}{ambientOcclusionTexture?.UvIndex ?? 0}", ambientOcclusionTexture, animations)};");
    }

    if (hasEmissiveTexture) {
      fragmentShaderSrc.AppendLine(
          $"  vec4 emissiveColor = {GlslUtil.ReadColorFromTexture("emissiveTexture", $"{GlslConstants.IN_UV_NAME}{emissiveTexture?.UvIndex ?? 0}", emissiveTexture, animations)};");
    }

    fragmentShaderSrc.AppendLine(
        $"  fragColor = {(hasDiffuseTexture ? "diffuseColor * " : "")} {GlslConstants.IN_VERTEX_COLOR_NAME}0;");

    if (hasNormals) {
      if (!hasNormalTexture) {
        fragmentShaderSrc.Append(
            """
            
              // Have to renormalize because the vertex normals can become distorted when interpolated.
              vec3 fragNormal = normalize(vertexNormal);
            """);
      } else {
        fragmentShaderSrc.Append(
            $"""
             
               // Have to renormalize because the vertex normals can become distorted when interpolated.
               vec3 fragNormal = normalize(vertexNormal);
               vec3 textureNormal = {GlslUtil.ReadColorFromTexture("normalTexture", $"{GlslConstants.IN_UV_NAME}{normalTexture?.UvIndex ?? 0}", normalTexture, animations)}.xyz * 2 - 1;
               fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);
             """);
      }

      // TODO: Is this right?
      fragmentShaderSrc.Append(
          $"""
           
           
             fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, {(hasSpecularTexture ? $"{GlslUtil.ReadColorFromTexture("specularTexture", "uv0", specularTexture, animations)}" : "vec4(1)")}, ambientOcclusionColor.r).rgb, {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
           """);
    }

    if (hasEmissiveTexture) {
      fragmentShaderSrc.Append(
          """
          
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1);
          """);
    }

    if (material.TransparencyType == TransparencyType.MASK) {
      fragmentShaderSrc.Append(
          $$"""
            
            
              if (fragColor.a < {{GlslConstants.MIN_ALPHA_BEFORE_DISCARD_TEXT}}) {
                discard;
              }
            """);
    }

    fragmentShaderSrc.Append(
        """

        }

        """);


    this.FragmentShaderSource = fragmentShaderSrc.ToString();
  }

  public string VertexShaderSource { get; }

  public string FragmentShaderSource { get; set; }
}