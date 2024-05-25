using System.Linq;
using System.Text;

using fin.model;
using fin.model.extensions;
using fin.util.image;

namespace fin.shaders.glsl {
  public class TextureShaderSourceGlsl : IShaderSourceGlsl {
    public TextureShaderSourceGlsl(IReadOnlyModel model,
                                   IReadOnlyMaterial material,
                                   bool useBoneMatrices) {
      this.VertexShaderSource = GlslUtil.GetVertexSrc(model, useBoneMatrices);

      var diffuseTexture = material.Textures.FirstOrDefault();
      var hasNormals = model.Skin.HasNormalsForMaterial(material);

      var fragmentSrc = new StringBuilder();
      fragmentSrc.Append("#version 400");

      if (hasNormals) {
        fragmentSrc.Append(
            $"""


             {GlslUtil.GetLightHeader(true)}
             """);
      }

      if (GlslUtil.RequiresFancyTextureData(diffuseTexture)) {
        fragmentSrc.Append(
            $"""


             {GlslUtil.GetTextureStruct()}
             """);
      }

      fragmentSrc.Append(
          $"""


           uniform {GlslUtil.GetTypeOfTexture(diffuseTexture)} diffuseTexture;
           uniform float {GlslConstants.UNIFORM_SHININESS_NAME};
           uniform float {GlslConstants.UNIFORM_USE_LIGHTING_NAME};

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

      fragmentSrc.Append(
          """

          in vec2 uv0;
          """);

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
          $$"""


            void main() {
              vec4 diffuseColor = {{GlslUtil.ReadColorFromTexture("diffuseTexture", "uv0", diffuseTexture)}};
            
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
              """);
      }

      fragmentSrc.Append(
          """

          }
          """);

      this.FragmentShaderSource = fragmentSrc.ToString();
    }

    public string VertexShaderSource { get; }
    public string FragmentShaderSource { get; }
  }
}