using System.Collections.Generic;
using System.Linq;

using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class TextureShaderSourceGlsl : IShaderSourceGlsl {
  private readonly IReadOnlyMaterial material_;
  private readonly IReadOnlyList<IReadOnlyModelAnimation> animations_;
  private readonly bool isSelectable_;

  private readonly bool hasLighting_;
  private readonly bool hasColors_;
  private readonly bool hasDiffuseColor_;

  private readonly IReadOnlyTexture? diffuseTexture_;
  private readonly int uvIndex_;

  public TextureShaderSourceGlsl(
      IReadOnlyModel model,
      IModelRequirements modelRequirements,
      IReadOnlyTextureMaterial material,
      IShaderRequirements shaderRequirements,
      bool isSelectable) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements);

    this.animations_ = model.AnimationManager.Animations;
    this.isSelectable_ = isSelectable;

    this.material_ = material;

    this.diffuseTexture_ = material.Textures.FirstOrDefault();
    this.uvIndex_ = this.diffuseTexture_?.UvIndex ?? 0;
    this.hasColors_ = shaderRequirements.UsedColors.AnyTrue();
    this.hasDiffuseColor_ = material.DiffuseColor != null;
    var hasNormals = shaderRequirements.HasNormals;
    this.hasLighting_ = !material.IgnoreLights && hasNormals;

    var sb = new IndentedStringBuilder();
    sb.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    sb.AppendLine(GlslConstants.FLOAT_PRECISION);

    if (this.hasLighting_) {
      sb.AppendLine(
          $"""

           {GlslUtil.LIGHT_HEADER}
           """);
    }

    sb.AppendTextureHeadersIfNeeded(material.Textures, this.animations_);
    sb.AppendLine();

    if (material.DiffuseColor != null) {
      sb.AppendLine("uniform vec4 diffuseColor;");
    }

    sb.AppendLine(
        $"uniform {GlslUtil.GetTypeOfTexture(this.diffuseTexture_, this.animations_)} diffuseTexture;");

    if (this.hasLighting_) {
      sb.AppendLine($"uniform bool {GlslConstants.UNIFORM_HAS_SPECULAR_NAME};");
      sb.AppendLine($"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    GlslUtil.MaybeAppendPrimitiveMetadataHeader(sb, isSelectable);

    sb.AppendLine(
        """

        out vec4 fragColor;

        """);

    if (this.hasColors_) {
      sb.AppendLine($"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (this.hasLighting_) {
      sb.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
    }

    sb.AppendLine($"in vec2 {GlslConstants.IN_UV_NAME}{this.uvIndex_};");

    if (this.hasLighting_) {
      sb.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    sb.AppendLine();
    sb.AppendBlock(
        "void main()",
        () => this.AppendFragmentMain(sb));

    this.FragmentShaderSource = sb.ToString();
  }

  public string VertexShaderSource { get; }
  public string FragmentShaderSource { get; }

  public void AppendFragmentMain(IndentedStringBuilder sb) {
    sb.AppendLine(
        $"{GlslConstants.UNIFORM_FRAG_COLOR_NAME} = {GlslUtil.ReadColorFromTexture("diffuseTexture", $"uv{this.uvIndex_}", this.diffuseTexture_, this.animations_)}" +
        (this.hasColors_ ? " * vertexColor0" : "") +
        (this.hasDiffuseColor_ ? " * diffuseColor" : "") +
        ";");

    if (this.hasLighting_) {
      sb.AppendLine(
          $"""

           // Have to renormalize because the vertex normals can become distorted when interpolated.
           vec3 fragNormal = normalize(vertexNormal);
           {GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = mix({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, {GlslConstants.UNIFORM_FRAG_COLOR_NAME}, vec4(1)).rgb,  {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
           """);
    }

    GlslUtil.MaybeAppendSelectedColorMutation(
        sb,
        this.isSelectable_);
    GlslUtil.AppendAlphaDiscard(sb, this.material_);
  }
}