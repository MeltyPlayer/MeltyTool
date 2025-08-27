using System.Text;

using fin.model;
using fin.util.enumerables;

namespace fin.shaders.glsl.source;

public class StandardShaderSourceGlsl : IShaderSourceGlsl {
  public StandardShaderSourceGlsl(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyStandardMaterial material,
      IShaderRequirements shaderRequirements) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

    var animations = model.AnimationManager.Animations;

    var fragmentShaderSrc = new StringBuilder();
    fragmentShaderSrc.AppendLine(
        $"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    fragmentShaderSrc.AppendLine(GlslConstants.FLOAT_PRECISION);
    fragmentShaderSrc.AppendLine();

    var hasColor = shaderRequirements.UsedColors.AnyTrue();

    var diffuseTexture = material.DiffuseTexture;
    var hasDiffuseTexture = diffuseTexture != null;

    var normalTexture = material.NormalTexture;
    var hasNormalTexture = normalTexture != null;
    var hasNormals = shaderRequirements.HasNormals;
    var hasTangents = shaderRequirements.TangentType != TangentType.NOT_PRESENT;
    var hasBinormals = hasNormals && hasTangents;

    var ambientOcclusionTexture = material.AmbientOcclusionTexture;
    var hasAmbientOcclusionTexture = ambientOcclusionTexture != null;

    var emissiveTexture = material.EmissiveTexture;
    var hasEmissiveTexture = emissiveTexture != null;

    var specularTexture = material.SpecularTexture;
    var hasSpecularTexture = specularTexture != null;

    if (hasNormals) {
      fragmentShaderSrc.AppendLine(
          $"""
           {GlslUtil.LIGHT_HEADER}

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
        """
        out vec4 fragColor;

        """);

    var needsLineAboveMain = false;

    if (hasColor) {
      needsLineAboveMain = true;
      fragmentShaderSrc.AppendLine(
          $"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (hasNormals) {
      needsLineAboveMain = true;
      fragmentShaderSrc.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
    }

    if (hasTangents) {
      needsLineAboveMain = true;
      fragmentShaderSrc.AppendLine(
          """
          in vec3 tangent;
          """);
    }

    if (hasBinormals) {
      needsLineAboveMain = true;
      fragmentShaderSrc.AppendLine(
          """
          in vec3 binormal;
          """);
    }

    var usedUvs = shaderRequirements.UsedUvs;
    for (var i = 0; i < usedUvs.Length; ++i) {
      if (usedUvs[i]) {
        needsLineAboveMain = true;
        fragmentShaderSrc.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{i};");
      }
    }

    if (hasNormals) {
      if (needsLineAboveMain) {
        fragmentShaderSrc.AppendLine();
      }

      fragmentShaderSrc.AppendLine(
          $"""
           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(hasAmbientOcclusionTexture)}
           """);
    }

    if (needsLineAboveMain) {
      fragmentShaderSrc.AppendLine();
    }

    fragmentShaderSrc.AppendLine("void main() {");

    var getDiffuseTextureColor = GlslUtil.ReadColorFromTexture(
        "diffuseTexture",
        $"{GlslConstants.IN_UV_NAME}{diffuseTexture?.UvIndex ?? 0}",
        diffuseTexture,
        animations);
    fragmentShaderSrc.AppendLine(
        $"  fragColor = {(hasDiffuseTexture, hasColor) switch {
            (false, false) => "vec4(1)",
            (false, true) => $"{GlslConstants.IN_VERTEX_COLOR_NAME}0",
            (true, false) => getDiffuseTextureColor,
            (true, true) => $"{getDiffuseTextureColor} * {GlslConstants.IN_VERTEX_COLOR_NAME}0"
        }};");

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
               vec3 textureNormal = {GlslUtil.ReadColorFromTexture("normalTexture", $"{GlslConstants.IN_UV_NAME}{normalTexture?.UvIndex ?? 0}", normalTexture, animations)}.xyz * 2.0 - 1.0;
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
            fragColor.rgb = min(fragColor.rgb, 1.0);
          """);
    }
    
    GlslUtil.AppendAlphaDiscard(fragmentShaderSrc, material);
    fragmentShaderSrc.Append("}");

    this.FragmentShaderSource = fragmentShaderSrc.ToString();
  }

  public string VertexShaderSource { get; }

  public string FragmentShaderSource { get; set; }
}