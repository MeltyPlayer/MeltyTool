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
    fragmentShaderSrc.AppendLine();

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
      fragmentShaderSrc.AppendLine(
          $"""
           {GlslUtil.GetLightHeader(true)}

           """);
    }

    fragmentShaderSrc.AppendTextureStructIfNeeded(
        material.Textures,
        animations);

    var needsNewline = false;
    if (hasDiffuseTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(diffuseTexture, animations)} diffuseTexture;");
      needsNewline = true;
    }

    if (hasNormalTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(normalTexture, animations)} normalTexture;");
      needsNewline = true;
    }

    if (hasSpecularTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(specularTexture, animations)} specularTexture;");
      needsNewline = true;
    }

    if (hasAmbientOcclusionTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(ambientOcclusionTexture, animations)} ambientOcclusionTexture;");
      needsNewline = true;
    }

    if (hasEmissiveTexture) {
      fragmentShaderSrc.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(emissiveTexture, animations)} emissiveTexture;");
      needsNewline = true;
    }

    if (hasNormals) {
      fragmentShaderSrc.AppendLine(
          $"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
      needsNewline = true;
    }

    if (needsNewline) {
      fragmentShaderSrc.AppendLine();
    }

    fragmentShaderSrc.AppendLine(
        $"""
         out vec4 fragColor;

         in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;
         """);

    if (hasNormals) {
      fragmentShaderSrc.AppendLine(
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
      fragmentShaderSrc.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(hasAmbientOcclusionTexture)}
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

    fragmentShaderSrc.AppendLine(
        $"  fragColor = {(hasDiffuseTexture ? "diffuseColor * " : "")}{GlslConstants.IN_VERTEX_COLOR_NAME}0;");

    if (hasNormals) {
      fragmentShaderSrc.AppendLine();
      if (hasAmbientOcclusionTexture) {
        fragmentShaderSrc.AppendLine(
            $"  vec4 ambientOcclusionColor = {GlslUtil.ReadColorFromTexture("ambientOcclusionTexture", $"{GlslConstants.IN_UV_NAME}{ambientOcclusionTexture?.UvIndex ?? 0}", ambientOcclusionTexture, animations)};");
      }

      if (!hasNormalTexture) {
        fragmentShaderSrc.AppendLine(
            """
              // Have to renormalize because the vertex normals can become distorted when interpolated.
              vec3 fragNormal = normalize(vertexNormal);
            """);
      } else {
        fragmentShaderSrc.AppendLine(
            $"""
               // Have to renormalize because the vertex normals can become distorted when interpolated.
               vec3 fragNormal = normalize(vertexNormal);
               vec3 textureNormal = {GlslUtil.ReadColorFromTexture("normalTexture", $"{GlslConstants.IN_UV_NAME}{normalTexture?.UvIndex ?? 0}", normalTexture, animations)}.xyz * 2 - 1;
               fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);
             """);
      }

      // TODO: Is this right?
      fragmentShaderSrc.AppendLine(
          $"  fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, {(hasSpecularTexture ? $"{GlslUtil.ReadColorFromTexture("specularTexture", "uv0", specularTexture, animations)}" : "vec4(1)")}{(hasAmbientOcclusionTexture ? ", ambientOcclusionColor.r" : "")}).rgb, {GlslConstants.UNIFORM_USE_LIGHTING_NAME});");
    }

    if (hasEmissiveTexture) {
      fragmentShaderSrc.AppendLine();
      fragmentShaderSrc.AppendLine(
          $"  vec4 emissiveColor = {GlslUtil.ReadColorFromTexture("emissiveTexture", $"{GlslConstants.IN_UV_NAME}{emissiveTexture?.UvIndex ?? 0}", emissiveTexture, animations)};");
      fragmentShaderSrc.AppendLine(
          """
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1);
          """);
    }

    if (material.TransparencyType == TransparencyType.MASK) {
      fragmentShaderSrc.AppendLine();
      fragmentShaderSrc.AppendLine(
          $$"""
              if (fragColor.a < {{GlslConstants.MIN_ALPHA_BEFORE_DISCARD_TEXT}}) {
                discard;
              }
            """);
    }

    fragmentShaderSrc.Append("}");


    this.FragmentShaderSource = fragmentShaderSrc.ToString();
  }

  public string VertexShaderSource { get; }

  public string FragmentShaderSource { get; set; }
}