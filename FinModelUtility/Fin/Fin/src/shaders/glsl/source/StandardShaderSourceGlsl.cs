using System.Collections.Generic;
using System.Drawing;

using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class StandardShaderSourceGlsl : IShaderSourceGlsl {
  private readonly IReadOnlyMaterial material_;
  private readonly IReadOnlyList<IReadOnlyModelAnimation> animations_;
  
  private readonly bool hasLighting_;
  private readonly bool hasColor_;
  private readonly bool isSelectable_;

  private readonly IReadOnlyTexture? diffuseTexture_;
  private readonly Color? diffuseColor_;
  private readonly IReadOnlyTexture? normalTexture_;
  private readonly IReadOnlyTexture? ambientOcclusionTexture_;
  private readonly IReadOnlyTexture? emissiveTexture_;
  private readonly IReadOnlyTexture? specularTexture_;

  public StandardShaderSourceGlsl(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyStandardMaterial material,
      IShaderRequirements shaderRequirements,
      bool isSelectable) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

    this.material_ = material;

    this.animations_ = model.AnimationManager.Animations;

    var sb = new IndentedStringBuilder();
    sb.AppendLine(
        $"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    sb.AppendLine(GlslConstants.FLOAT_PRECISION);
    sb.AppendLine();

    this.hasColor_ = shaderRequirements.UsedColors.AnyTrue();
    this.isSelectable_ = isSelectable;

    this.diffuseTexture_ = material.DiffuseTexture;
    this.diffuseColor_ = material.DiffuseColor;

    this.normalTexture_ = material.NormalTexture;
    var hasNormals = shaderRequirements.HasNormals;
    this.hasLighting_ = !material.IgnoreLights && hasNormals;
    var hasTangents = shaderRequirements.TangentType != TangentType.NOT_PRESENT;
    var hasBinormals = hasNormals && hasTangents;

    this.ambientOcclusionTexture_ = material.AmbientOcclusionTexture;
    this.emissiveTexture_ = material.EmissiveTexture;
    this.specularTexture_ = material.SpecularTexture;

    if (this.hasLighting_) {
      sb.AppendLine(
          $"""
           {GlslUtil.LIGHT_HEADER}

           """);
    }

    sb.AppendTextureHeadersIfNeeded(
        material.Textures,
        this.animations_);

    var needsNewline = false;
    if (this.diffuseTexture_ != null) {
      sb.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(this.diffuseTexture_, this.animations_)} diffuseTexture;");
      needsNewline = true;
    }
    if (this.diffuseColor_ != null) {
      sb.AppendLine("uniform vec4 diffuseColor;");
      needsNewline = true;
    }

    if (this.hasLighting_) {
      if (this.normalTexture_ != null) {
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(this.normalTexture_, this.animations_)} normalTexture;");
        needsNewline = true;
      }

      if (this.specularTexture_ != null) {
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(this.specularTexture_, this.animations_)} specularTexture;");
        needsNewline = true;
      }

      if (this.ambientOcclusionTexture_ != null) {
        sb.AppendLine(
            $"uniform {GlslUtil.GetTypeOfTexture(this.ambientOcclusionTexture_, this.animations_)} ambientOcclusionTexture;");
        needsNewline = true;
      }
    }

    if (this.emissiveTexture_ != null) {
      sb.AppendLine(
          $"uniform {GlslUtil.GetTypeOfTexture(this.emissiveTexture_, this.animations_)} emissiveTexture;");
      needsNewline = true;
    }

    if (this.hasLighting_) {
      sb.AppendLine($"uniform bool {GlslConstants.UNIFORM_HAS_SPECULAR_NAME};");
      sb.AppendLine($"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
      needsNewline = true;
    }

    GlslUtil.MaybeAppendSelectedHeader(sb, isSelectable);

    if (needsNewline) {
      sb.AppendLine();
    }

    sb.AppendLine(
        """
        out vec4 fragColor;

        """);

    var needsLineAboveMain = false;

    if (this.hasColor_) {
      needsLineAboveMain = true;
      sb.AppendLine(
          $"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (this.hasLighting_) {
      needsLineAboveMain = true;
      sb.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);

      if (hasTangents) {
        needsLineAboveMain = true;
        sb.AppendLine(
            """
            in vec3 tangent;
            """);
      }

      if (hasBinormals) {
        needsLineAboveMain = true;
        sb.AppendLine(
            """
            in vec3 binormal;
            """);
      }
    }

    var usedUvs = shaderRequirements.UsedUvs;
    for (var i = 0; i < usedUvs.Length; ++i) {
      if (usedUvs[i]) {
        needsLineAboveMain = true;
        sb.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{i};");
      }
    }

    if (this.hasLighting_) {
      if (needsLineAboveMain) {
        sb.AppendLine();
      }

      sb.AppendLine(
          $"""
           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(this.ambientOcclusionTexture_ != null)}
           """);
    }

    if (needsLineAboveMain) {
      sb.AppendLine();
    }

    sb.AppendBlock(
        "void main()",
        () => this.AppendFragmentMain(sb));

    this.FragmentShaderSource = sb.ToString();
  }

  public string VertexShaderSource { get; }

  public string FragmentShaderSource { get; set; }

  public void AppendFragmentMain(IndentedStringBuilder sb) {
    var diffuseColors = new List<string>();
    if (this.diffuseTexture_ != null) {
      var getDiffuseTextureColor = GlslUtil.ReadColorFromTexture(
          "diffuseTexture",
          $"{GlslConstants.IN_UV_NAME}{this.diffuseTexture_?.UvIndex ?? 0}",
          this.diffuseTexture_,
          this.animations_);
      diffuseColors.Add(getDiffuseTextureColor);
    }
    if (this.diffuseColor_ != null) {
      diffuseColors.Add("diffuseColor");
    }
    if (this.hasColor_) {
      diffuseColors.Add($"{GlslConstants.IN_VERTEX_COLOR_NAME}0");
    }

    if (diffuseColors.Count > 0) {
      sb.AppendLine($"fragColor = {diffuseColors.Join(" * ")};");
    } else {
      sb.AppendLine("fragColor = vec4(1);");
    }

    if (this.hasLighting_) {
      sb.AppendLine();
      if (this.ambientOcclusionTexture_ != null) {
        sb.AppendLine(
            $"vec4 ambientOcclusionColor = {GlslUtil.ReadColorFromTexture("ambientOcclusionTexture", $"{GlslConstants.IN_UV_NAME}{this.ambientOcclusionTexture_?.UvIndex ?? 0}", this.ambientOcclusionTexture_, this.animations_)};");
      }

      if (this.normalTexture_ == null) {
        sb.AppendLine(
            """
            // Have to renormalize because the vertex normals can become distorted when interpolated.
            vec3 fragNormal = normalize(vertexNormal);
            """);
      } else {
        sb.AppendLine(
            $"""
             // Have to renormalize because the vertex normals can become distorted when interpolated.
             vec3 fragNormal = normalize(vertexNormal);
             vec3 textureNormal = {GlslUtil.ReadColorFromTexture("normalTexture", $"{GlslConstants.IN_UV_NAME}{this.normalTexture_?.UvIndex ?? 0}", this.normalTexture_, this.animations_)}.xyz * 2.0 - 1.0;
             fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);
             """);
      }

      // TODO: Is this right?
      sb.AppendLine(
          $"{GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = mix({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, {GlslConstants.UNIFORM_FRAG_COLOR_NAME}, {(this.specularTexture_ != null ? $"{GlslUtil.ReadColorFromTexture("specularTexture", "uv0", this.specularTexture_, this.animations_)}" : "vec4(1)")}{(this.ambientOcclusionTexture_ != null ? ", ambientOcclusionColor.r" : "")}).rgb, {GlslConstants.UNIFORM_USE_LIGHTING_NAME});");
    }

    if (this.emissiveTexture_ != null) {
      sb.AppendLine();
      sb.AppendLine(
          $"vec4 emissiveColor = {GlslUtil.ReadColorFromTexture("emissiveTexture", $"{GlslConstants.IN_UV_NAME}{this.emissiveTexture_?.UvIndex ?? 0}", this.emissiveTexture_, this.animations_)};");
      sb.AppendLine(
          $"""
           {GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb += emissiveColor.rgb;
           {GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = min({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb, 1.0);
           """);
    }

    GlslUtil.MaybeAppendSelectedColorMutation(sb, this.isSelectable_);
    GlslUtil.AppendAlphaDiscard(sb, this.material_);
  }
}