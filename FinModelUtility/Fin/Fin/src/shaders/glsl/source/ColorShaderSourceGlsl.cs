using fin.model;
using fin.util.enumerables;
using fin.util.strings;

namespace fin.shaders.glsl.source;

public sealed class ColorShaderSourceGlsl : IShaderSourceGlsl {
  private readonly IReadOnlyMaterial material_;
  private readonly bool hasColors_;
  private readonly bool hasLighting_;
  private readonly bool isSelectable_;

  public ColorShaderSourceGlsl(IReadOnlyModel model,
                               IModelRequirements modelRequirements,
                               IReadOnlyMaterial material,
                               IShaderRequirements shaderRequirements,
                               bool isSelectable) {
    this.VertexShaderSource
        = GlslUtil.GetVertexSrc(model, modelRequirements, shaderRequirements, isSelectable);

    this.material_ = material;
    this.hasColors_ = shaderRequirements.UsedColors.AnyTrue();
    var hasNormals = shaderRequirements.HasNormals;
    this.hasLighting_ = !material.IgnoreLights && hasNormals;
    this.isSelectable_ = isSelectable;

    var sb = new IndentedStringBuilder();
    sb.AppendLine($"#version {GlslConstants.FRAGMENT_SHADER_VERSION}");
    sb.AppendLine(GlslConstants.FLOAT_PRECISION);
    sb.AppendLine();

    if (this.hasLighting_) {
      sb.AppendLine(
          $"""
           {GlslUtil.LIGHT_HEADER}

           """);
    }

    sb.AppendLine("uniform vec4 diffuseColor;");

    if (this.hasLighting_) {
      sb.AppendLine($"uniform bool {GlslConstants.UNIFORM_HAS_SPECULAR_NAME};");
      sb.AppendLine($"uniform float {GlslConstants.UNIFORM_SHININESS_NAME};");
    }

    sb.AppendLine(
        """

        out vec4 fragColor;

        """);

    var hadAnyIns = false;
    if (this.hasColors_) {
      hadAnyIns = true;
      sb.AppendLine($"in vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}0;");
    }

    if (this.hasLighting_) {
      hadAnyIns = true;
      sb.AppendLine(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
      sb.AppendLine(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    if (hadAnyIns) {
      sb.AppendLine();
    }

    sb.AppendBlock("void main()", () => this.AppendFragmentMain(sb));

    this.FragmentShaderSource = sb.ToString();
  }

  public string VertexShaderSource { get; }
  public string FragmentShaderSource { get; }

  public void AppendFragmentMain(IndentedStringBuilder sb) {
    sb.AppendLine(
        $"fragColor = diffuseColor{this.hasColors_ switch {
            false => "",
            true  => $" * {GlslConstants.IN_VERTEX_COLOR_NAME}0",
        }};");

    if (this.hasLighting_) {
      sb.AppendLine(
          $"""

           // Have to renormalize because the vertex normals can become distorted when interpolated.
           vec3 fragNormal = normalize(vertexNormal);
           {GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb = mix({GlslConstants.UNIFORM_FRAG_COLOR_NAME}.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, {GlslConstants.UNIFORM_FRAG_COLOR_NAME}, vec4(1)).rgb,  {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
           """);
    }

    GlslUtil.MaybeAppendSelectedColorMutation(sb, this.isSelectable_);
    GlslUtil.AppendAlphaDiscard(sb, this.material_);
  }
}