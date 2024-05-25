using System.Linq;
using System.Text;

using fin.model;
using fin.model.extensions;
using fin.util.image;

namespace fin.shaders.glsl {
  public class StandardShaderSourceGlsl : IShaderSourceGlsl {
    public StandardShaderSourceGlsl(
        IReadOnlyModel model,
        IReadOnlyStandardMaterial material,
        bool useBoneMatrices) {
      this.VertexShaderSource = GlslUtil.GetVertexSrc(model, useBoneMatrices);

      var hasNormalTexture = material.NormalTexture != null;
      var hasNormals = hasNormalTexture ||
                       model.Skin.HasNormalsForMaterial(material);

      var fragmentShaderSrc = new StringBuilder();
      fragmentShaderSrc.Append("#version 400");

      var diffuseTexture = material.DiffuseTexture;
      var normalTexture = material.NormalTexture;
      var ambientOcclusionTexture = material.AmbientOcclusionTexture;
      var emissiveTexture = material.EmissiveTexture;

      var specularTexture = material.SpecularTexture;
      var hasSpecularTexture = specularTexture != null;

      if (hasNormals) {
        fragmentShaderSrc.Append(
            $"""


             {GlslUtil.GetLightHeader(true)}
             """);
      }

      if (material.Textures.Any(GlslUtil.RequiresFancyTextureData)) {
        fragmentShaderSrc.Append(
            $"""


             {GlslUtil.GetTextureStruct()}
             """);
      }

      fragmentShaderSrc.Append(
          $"""


           uniform {GlslUtil.GetTypeOfTexture(diffuseTexture)} diffuseTexture;
           """);

      if (hasNormalTexture) {
        fragmentShaderSrc.Append(
            $"""

             uniform {GlslUtil.GetTypeOfTexture(normalTexture)} normalTexture;
             """);
      }

      if (hasSpecularTexture) {
        fragmentShaderSrc.Append(
            $"""

             uniform {GlslUtil.GetTypeOfTexture(specularTexture)} specularTexture;
             """);
      }

      fragmentShaderSrc.Append(
          $"""

           uniform {GlslUtil.GetTypeOfTexture(ambientOcclusionTexture)} ambientOcclusionTexture;
           uniform {GlslUtil.GetTypeOfTexture(emissiveTexture)} emissiveTexture;
           uniform float {GlslConstants.UNIFORM_SHININESS_NAME};
           uniform float {GlslConstants.UNIFORM_USE_LIGHTING_NAME};

           out vec4 fragColor;

           in vec4 vertexColor0;
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

      fragmentShaderSrc.Append(
          """

          in vec2 uv0;
          """);

      if (hasNormals) {
        fragmentShaderSrc.Append(
            $"""


             {GlslUtil.GetGetIndividualLightColorsFunction()}

             {GlslUtil.GetGetMergedLightColorsFunction()}

             {GlslUtil.GetApplyMergedLightColorsFunction(true)}
             """);
      }

      fragmentShaderSrc.Append(
          $$"""


            void main() {
              vec4 diffuseColor = {{GlslUtil.ReadColorFromTexture("diffuseTexture", "uv0", diffuseTexture)}};
              vec4 ambientOcclusionColor = {{GlslUtil.ReadColorFromTexture("ambientOcclusionTexture", "uv0", ambientOcclusionTexture)}};
              vec4 emissiveColor = {{GlslUtil.ReadColorFromTexture("emissiveTexture", "uv0", emissiveTexture)}};
            
              fragColor = diffuseColor * vertexColor0;
            """);

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
                 vec3 textureNormal = {GlslUtil.ReadColorFromTexture("normalTexture", "uv0", normalTexture)}.xyz * 2 - 1;
                 fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);
               """);
        }

        // TODO: Is this right?
        fragmentShaderSrc.Append(
            $"""
             
             
               fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, {GlslConstants.UNIFORM_SHININESS_NAME}, fragColor, {(hasSpecularTexture ? $"{GlslUtil.ReadColorFromTexture("specularTexture", "uv0", specularTexture)}" : "vec4(1)")}, ambientOcclusionColor.r).rgb, {GlslConstants.UNIFORM_USE_LIGHTING_NAME});
             """);
      }

      // TODO: Is this right?
      fragmentShaderSrc.Append(
          """
          
            fragColor.rgb += emissiveColor.rgb;
            fragColor.rgb = min(fragColor.rgb, 1);
          """);

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
}