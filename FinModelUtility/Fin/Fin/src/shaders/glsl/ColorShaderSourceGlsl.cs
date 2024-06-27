using System.Text;

using fin.model;
using fin.model.extensions;
using fin.util.image;

namespace fin.shaders.glsl;

public class ColorShaderSourceGlsl : IShaderSourceGlsl {
  public ColorShaderSourceGlsl(IReadOnlyModel model,
                               IReadOnlyMaterial material,
                               bool useBoneMatrices) {
    this.VertexShaderSource = GlslUtil.GetVertexSrc(model, useBoneMatrices);

    var hasNormals = model.Skin.HasNormalsForMaterial(material);

    var fragmentSrc = new StringBuilder();
    fragmentSrc.Append("#version 400");

    if (hasNormals) {
      fragmentSrc.Append(
          $"""

           {GlslUtil.GetLightHeader(true)}
           """);
    }

    fragmentSrc.Append(
        $"""

         uniform vec4 diffuseColor;
         uniform float {GlslConstants.UNIFORM_SHININESS_NAME};
         uniform int {GlslConstants.UNIFORM_USE_LIGHTING_NAME};

         out vec4 fragColor;

         in vec4 vertexColor0;
         """);

    if (hasNormals) {
      fragmentSrc.Append(
          """
          in vec3 vertexPosition;
          in vec3 vertexNormal;
          """);
    }

    if (hasNormals) {
      fragmentSrc.Append(
          $"""

           {GlslUtil.GetGetIndividualLightColorsFunction()}

           {GlslUtil.GetGetMergedLightColorsFunction()}

           {GlslUtil.GetApplyMergedLightColorsFunction(false)}
           """
      );
    }

    fragmentSrc.Append(
        """

        void main() {
          fragColor = diffuseColor * vertexColor0;
        """);

    if (hasNormals) {
      fragmentSrc.Append(
          $"""
           
             // Have to renormalize because the vertex normals can become distorted when interpolated.
             vec3 fragNormal = normalize(vertexNormal);
             fragColor.rgb =
                 mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, vec4(1)).rgb,  {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
           """);
    }

    if (material.TransparencyType == TransparencyType.MASK) {
      fragmentSrc.Append(
          $$"""
            
            
              if (fragColor.a < {{GlslConstants.MIN_ALPHA_BEFORE_DISCARD_TEXT}}) {
                discard;
              }
            }
            """);
    }

    this.FragmentShaderSource = fragmentSrc.ToString();
  }

  public string VertexShaderSource { get; }
  public string FragmentShaderSource { get; }
}