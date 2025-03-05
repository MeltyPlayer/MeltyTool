using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using fin.data.indexable;
using fin.math;
using fin.model;
using fin.util.enums;
using fin.image.util;

namespace fin.shaders.glsl;

public enum FinShaderType {
  FIXED_FUNCTION,
  TEXTURE,
  COLOR,
  SHADER,
  STANDARD,
  HIDDEN,
  NULL
}

[Flags]
public enum TextureTransformType {
  NONE = 0,
  TWO_D = 1 << 0,
  THREE_D = 1 << 1,
}

public static class TextureTransformTypeExtensions {
  public static TextureTransformType Merge(this TextureTransformType lhs,
                                           TextureTransformType rhs)
    => lhs | rhs;
}

public static class GlslUtil {
  public static FinShaderType
      GetShaderType(this IReadOnlyMaterial? material) {
    if (DebugFlags.ENABLE_FIXED_FUNCTION_SHADER &&
        !DebugFlags.ENABLE_WEIGHT_COLORS &&
        material is IFixedFunctionMaterial) {
      return FinShaderType.FIXED_FUNCTION;
    }

    switch (material) {
      case IStandardMaterial: return FinShaderType.STANDARD;
      case IColorMaterial:    return FinShaderType.COLOR;
      case IShaderMaterial:   return FinShaderType.SHADER;
      case IHiddenMaterial:   return FinShaderType.HIDDEN;
    }

    if (material != null && material is not INullMaterial) {
      return FinShaderType.TEXTURE;
    }

    return FinShaderType.NULL;
  }

  // TODO: Only include uvs/colors as needed
  public static string GetVertexSrc(IReadOnlyModel model,
                                    IModelRequirements modelRequirements,
                                    IShaderRequirements shaderRequirements) {
    var usedUvs = shaderRequirements.UsedUvs;
    var usedColors = shaderRequirements.UsedColors;

    var location = 0;

    var vertexSrc = new StringBuilder();

    var hasNormals = modelRequirements.HasNormals;

    var hasTangents = modelRequirements.HasTangents;
    var hasBinormals = hasNormals && hasTangents;

    var numBones = modelRequirements.NumBones;

    vertexSrc.AppendLine($"""
                          #version {GlslConstants.VERTEX_SHADER_VERSION}

                          {GetMatricesHeader(model)}

                          uniform vec3 {GlslConstants.UNIFORM_CAMERA_POSITION_NAME};

                          layout(location = {location++}) in vec3 in_Position;
                          """);

    if (hasNormals) {
      vertexSrc.AppendLine(
          $"layout(location = {location++}) in vec3 in_Normal;");
    }

    if (hasTangents) {
      vertexSrc.AppendLine(
          $"layout(location = {location++}) in vec4 in_Tangent;");
    }

    if (numBones > 0) {
      var boneIdType = numBones switch {
          1 => "int",
          2 => "ivec2",
          3 => "ivec3",
          4 => "ivec4",
          _ => throw new ArgumentOutOfRangeException()
      };
      vertexSrc.AppendLine(
          $"layout(location = {location++}) in {boneIdType} in_BoneIds;");

      var boneWeightsType = numBones switch {
          1 => "float",
          2 => "vec2",
          3 => "vec3",
          4 => "vec4",
          _ => throw new ArgumentOutOfRangeException()
      };
      vertexSrc.AppendLine(
          $"layout(location = {location++}) in {boneWeightsType} in_BoneWeights;");
    }

    for (var i = 0; i < modelRequirements.NumUvs; ++i) {
      if (shaderRequirements.UsedUvs[i]) {
        vertexSrc.AppendLine(
            $"layout(location = {location}) in vec2 in_Uv{i};");
      }

      location++;
    }

    for (var i = 0; i < modelRequirements.NumColors; ++i) {
      if (shaderRequirements.UsedColors[i]) {
        vertexSrc.AppendLine(
            $"layout(location = {location++}) in vec4 in_Color{i};");
      }

      location++;
    }

    vertexSrc.AppendLine("""

                         out vec3 vertexPosition;
                         """);

    if (hasNormals) {
      vertexSrc.AppendLine("out vec3 vertexNormal;");
    }

    if (hasTangents) {
      vertexSrc.AppendLine("out vec3 tangent;");
    }

    if (hasBinormals) {
      vertexSrc.AppendLine("out vec3 binormal;");
    }

    for (var i = 0; i < usedUvs.Length; ++i) {
      if (usedUvs[i]) {
        vertexSrc.AppendLine($"out vec2 {GlslConstants.IN_UV_NAME}{i};");
      }
    }

    for (var i = 0; i < usedColors.Length; ++i) {
      if (usedColors[i]) {
        vertexSrc.AppendLine(
            $"out vec4 {GlslConstants.IN_VERTEX_COLOR_NAME}{i};");
      }
    }

    vertexSrc.Append($$"""

                       void main() {
                         mat4 mvMatrix = {{GlslConstants.UNIFORM_VIEW_MATRIX_NAME}} * {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}};
                         mat4 mvpMatrix = {{GlslConstants.UNIFORM_PROJECTION_MATRIX_NAME}} * mvMatrix;

                       """);

    if (modelRequirements.NumBones > 0) {
      switch (numBones) {
        case 1: {
          vertexSrc.AppendLine(
              $"""
                 mat4 mergedBoneMatrix = {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds] * in_BoneWeights;
               """);
          break;
        }
        case 2: {
          vertexSrc.AppendLine(
              $"""
                 mat4 mergedBoneMatrix = {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.x] * in_BoneWeights.x +
                                         {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.y] * in_BoneWeights.y;
               """);
          break;
        }
        case 3: {
          vertexSrc.AppendLine(
              $"""
                 mat4 mergedBoneMatrix = {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.x] * in_BoneWeights.x +
                                         {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.y] * in_BoneWeights.y +
                                         {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.z] * in_BoneWeights.z;
               """);
          break;
        }
        case 4: {
          vertexSrc.AppendLine(
              $"""
                 mat4 mergedBoneMatrix = {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.x] * in_BoneWeights.x +
                                         {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.y] * in_BoneWeights.y +
                                         {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.z] * in_BoneWeights.z +
                                         {GlslConstants.UNIFORM_BONE_MATRICES_NAME}[in_BoneIds.w] * in_BoneWeights.w;
               """);
          break;
        }
        default: throw new NotImplementedException();
      }

      vertexSrc.AppendLine(
          $"""
           
           
             mat4 vertexModelMatrix = {GlslConstants.UNIFORM_MODEL_MATRIX_NAME} * mergedBoneMatrix;
             mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;
           
             gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);
           
             vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
           """);

      if (hasNormals) {
        vertexSrc.AppendLine(
            "  vertexNormal = normalize(vertexModelMatrix * vec4(in_Normal, 0)).xyz;");
      }

      if (hasTangents) {
        vertexSrc.AppendLine(
            "  tangent = normalize(vertexModelMatrix * vec4(in_Tangent)).xyz;");
      }

      if (hasBinormals) {
        vertexSrc.AppendLine("  binormal = cross(vertexNormal, tangent);");
      }
    } else {
      vertexSrc.AppendLine(
          $"""
           
             gl_Position = mvpMatrix * vec4(in_Position, 1);
           
             vertexPosition = vec3({GlslConstants.UNIFORM_MODEL_MATRIX_NAME} * vec4(in_Position, 1));
           """);

      if (hasNormals) {
        vertexSrc.AppendLine(
            $"  vertexNormal = normalize({GlslConstants.UNIFORM_MODEL_MATRIX_NAME} * vec4(in_Normal, 0)).xyz;");
      }

      if (hasTangents) {
        vertexSrc.AppendLine(
            $"  tangent = normalize({GlslConstants.UNIFORM_MODEL_MATRIX_NAME} * vec4(in_Tangent)).xyz;");
      }

      if (hasBinormals) {
        vertexSrc.AppendLine("  binormal = cross(vertexNormal, tangent);");
      }
    }

    for (var i = 0; i < usedUvs.Length; ++i) {
      if (usedUvs[i]) {
        vertexSrc.AppendLine($"  {GlslConstants.IN_UV_NAME}{i} = in_Uv{i};");
      }
    }

    for (var i = 0; i < usedColors.Length; ++i) {
      if (usedColors[i]) {
        vertexSrc.AppendLine(
            $"  {GlslConstants.IN_VERTEX_COLOR_NAME}{i} = in_Color{i};");
      }
    }

    vertexSrc.AppendLine("}");

    return vertexSrc.ToString();
  }

  public static string GetMatricesHeader(IReadOnlyModel model)
    => $$"""
         layout (std140, binding = {{GlslConstants.UBO_MATRICES_BINDING_INDEX}}) uniform {{GlslConstants.UBO_MATRICES_NAME}} {
           mat4 {{GlslConstants.UNIFORM_MODEL_MATRIX_NAME}};
           mat4 {{GlslConstants.UNIFORM_VIEW_MATRIX_NAME}};
           mat4 {{GlslConstants.UNIFORM_PROJECTION_MATRIX_NAME}};
           
           mat4 {{GlslConstants.UNIFORM_BONE_MATRICES_NAME}}[{{1 + model.Skin.BonesUsedByVertices.Count}}];  
         };
         """;

  public static string GetLightHeader(bool withAmbientLight)
    => $$"""
         struct Light {
           // 0x00 (vec3 needs to be 16-byte aligned)
           vec3 position;
           bool enabled;
         
           // 0x10 (vec3 needs to be 16-byte aligned)
           vec3 normal;
           int sourceType;
         
           // 0x20 (vec4 needs to be 16-byte aligned)
           vec4 color;
           
           // 0x30 (vec3 needs to be 16-byte aligned)
           vec3 cosineAttenuation;
           int diffuseFunction;
         
           // 0x40 (vec3 needs to be 16-byte aligned)
           vec3 distanceAttenuation;
           int attenuationFunction;
         };

         layout (std140, binding = {{GlslConstants.UBO_LIGHTS_BINDING_INDEX}}) uniform {{GlslConstants.UBO_LIGHTS_NAME}} {
           Light lights[{{MaterialConstants.MAX_LIGHTS}}];
           vec4 ambientLightColor;
           float {{GlslConstants.UNIFORM_USE_LIGHTING_NAME}};
         };

         uniform vec3 {{GlslConstants.UNIFORM_CAMERA_POSITION_NAME}};
         """;

  public static string GetGetIndividualLightColorsFunction() {
    // Shamelessly stolen from:
    // https://github.com/LordNed/JStudio/blob/93c5c4479ffb1babefe829cfc9794694a1cb93e6/JStudio/J3D/ShaderGen/VertexShaderGen.cs#L336C9-L336C9
    return
        $$"""
          void getSurfaceToLightNormalAndAttenuation(Light light, vec3 position, vec3 normal, out vec3 surfaceToLightNormal, out float attenuation) {
            vec3 surfaceToLight = light.position - position;
            
            surfaceToLightNormal = (light.sourceType == {{(int) LightSourceType.LINE}})
              ? -light.normal : normalize(surfaceToLight);
          
            if (light.attenuationFunction == {{(int) AttenuationFunction.NONE}}) {
              attenuation = 1.0;
              return;
            }
          
            // Attenuation is calculated as a fraction, (cosine attenuation) / (distance attenuation).
          
            // Numerator (Cosine attenuation)
            vec3 cosAttn = light.cosineAttenuation;
            
            vec3 attnDotLhs = (light.attenuationFunction == {{(int) AttenuationFunction.SPECULAR}})
              ? normal : surfaceToLightNormal;
            float attn = dot(attnDotLhs, light.normal);
            vec3 attnPowers = vec3(1, attn, attn*attn);
          
            float attenuationNumerator = max(0.0, dot(cosAttn, attnPowers));
          
            // Denominator (Distance attenuation)
            float attenuationDenominator = 1.0;
            if (light.sourceType != {{(int) LightSourceType.LINE}}) {
              vec3 distAttn = light.distanceAttenuation;
              
              if (light.attenuationFunction == {{(int) AttenuationFunction.SPECULAR}}) {
                float attn = max(0.0, -dot(normal, light.normal));
                if (light.diffuseFunction != {{(int) DiffuseFunction.NONE}}) {
                  distAttn = normalize(distAttn);
                }
                
                attenuationDenominator = dot(distAttn, attnPowers);
              } else {
                float dist2 = dot(surfaceToLight, surfaceToLight);
                float dist = sqrt(dist2);
                attenuationDenominator = dot(distAttn, vec3(1, dist, dist2));
              }
            }
          
            attenuation = attenuationNumerator / attenuationDenominator;
          }

          void getIndividualLightColors(Light light, vec3 position, vec3 normal, float shininess, out vec4 diffuseColor, out vec4 specularColor) {
            if (!light.enabled) {
               diffuseColor = specularColor = vec4(0);
               return;
            }
          
            vec3 surfaceToLightNormal = vec3(0);
            float attenuation = 0.0;
            getSurfaceToLightNormalAndAttenuation(light, position, normal, surfaceToLightNormal, attenuation);
          
            float diffuseLightAmount = 1.0;
            if (light.diffuseFunction == {{(int) DiffuseFunction.SIGNED}} || light.diffuseFunction == {{(int) DiffuseFunction.CLAMP}}) {
              diffuseLightAmount = max(0.0, dot(normal, surfaceToLightNormal));
            }
            diffuseColor = light.color * diffuseLightAmount * attenuation;
            
            if (light.attenuationFunction != {{(int) AttenuationFunction.NONE}} && dot(normal, surfaceToLightNormal) >= 0.0) {
              vec3 surfaceToCameraNormal = normalize(cameraPosition - position);
              float specularLightAmount = pow(max(0.0, dot(reflect(-surfaceToLightNormal, normal), surfaceToCameraNormal)), {{GlslConstants.UNIFORM_SHININESS_NAME}});
              specularColor = light.color * specularLightAmount * attenuation;
            }
          }
          """;
  }

  public static string GetGetMergedLightColorsFunction() {
    return
        $$"""
          void getMergedLightColors(vec3 position, vec3 normal, float shininess, out vec4 diffuseColor, out vec4 specularColor) {
            for (int i = 0; i < {{MaterialConstants.MAX_LIGHTS}}; ++i) {
              vec4 currentDiffuseColor = vec4(0);
              vec4 currentSpecularColor = vec4(0);
            
              getIndividualLightColors(lights[i], position, normal, shininess, currentDiffuseColor, currentSpecularColor);
          
              diffuseColor += currentDiffuseColor;
              specularColor += currentSpecularColor;
            }
          }
          """;
  }

  public static string GetApplyMergedLightColorsFunction(
      bool withAmbientOcclusion) {
    return
        $$"""
          vec4 applyMergedLightingColors(vec3 position, vec3 normal, float shininess, vec4 diffuseSurfaceColor, vec4 specularSurfaceColor{{(withAmbientOcclusion ? ", float ambientOcclusionAmount" : "")}}) {
            vec4 mergedDiffuseLightColor = vec4(0);
            vec4 mergedSpecularLightColor = vec4(0);
            getMergedLightColors(position, normal, shininess, mergedDiffuseLightColor, mergedSpecularLightColor);
          
            // We double it because all the other kids do. (Other fixed-function games.)
            vec4 diffuseComponent = 2.0 * diffuseSurfaceColor * ({{(withAmbientOcclusion ? "ambientOcclusionAmount * " : "")}}ambientLightColor + mergedDiffuseLightColor);
            vec4 specularComponent = specularSurfaceColor * mergedSpecularLightColor;
            
            return clamp(diffuseComponent + specularComponent, 0.0, 1.0);
          }
          """;
  }


  public static void AppendTextureStructIfNeeded(
      this StringBuilder sb,
      IEnumerable<IReadOnlyTexture?> textures,
      IReadOnlyList<IReadOnlyModelAnimation> animations
  ) {
    var usesClamping = false;
    var textureTransformType = TextureTransformType.NONE;
    foreach (var texture in textures) {
      usesClamping = usesClamping || texture.UsesShaderClamping();
      textureTransformType = textureTransformType.Merge(
          texture.GetTextureTransformType_(animations));
    }

    if (!usesClamping && textureTransformType == TextureTransformType.NONE) {
      return;
    }

    sb.Append(
        """

        struct Texture {
          sampler2D sampler;

        """);


    if (usesClamping) {
      sb.Append(
          """
            vec2 clampMin;
            vec2 clampMax;

          """);
    }

    if (textureTransformType.CheckFlag(TextureTransformType.TWO_D)) {
      sb.Append(
          """
            mat3x2 transform2d;

          """);
    }

    if (textureTransformType.CheckFlag(TextureTransformType.THREE_D)) {
      sb.Append(
          """
            mat4 transform3d;

          """);
    }

    sb.Append(
        """
        };


        """);

    if (textureTransformType.CheckFlag(TextureTransformType.THREE_D)) {
      sb.Append(
          """
          vec2 transformUv3d(mat4 transform3d, vec2 inUv) {
            vec4 rawTransformedUv = (transform3d * vec4(inUv, 0, 1));
          
            // We need to manually divide by w for perspective correction!
            return rawTransformedUv.xy / rawTransformedUv.w;
          }


          """);
    }
  }

  public static string GetTypeOfTexture(
      IReadOnlyTexture? finTexture,
      IReadOnlyList<IReadOnlyModelAnimation> animations)
    => finTexture.NeedsTextureShaderStruct(animations)
        ? "Texture"
        : "sampler2D";

  public static string ReadColorFromTexture(
      string textureName,
      string uvName,
      IReadOnlyTexture? finTexture,
      IReadOnlyList<IReadOnlyModelAnimation> animations) {
    var needsClamp = finTexture.UsesShaderClamping();
    var textureTransformType = finTexture.GetTextureTransformType_(animations);

    if (!needsClamp && textureTransformType == TextureTransformType.NONE) {
      return $"texture({textureName}, {uvName})";
    }

    var transformedUv = textureTransformType switch {
        TextureTransformType.TWO_D =>
            $"{textureName}.transform2d * vec3(({uvName}).x, ({uvName}).y, 1)",
        TextureTransformType.THREE_D =>
            $"transformUv3d({textureName}.transform3d, {uvName})",
        _ => uvName
    };

    if (needsClamp) {
      return
          $"texture({textureName}.sampler, " +
          "clamp(" +
          $"{transformedUv}, " +
          $"{textureName}.clampMin, " +
          $"{textureName}.clampMax" +
          ")" + // clamp
          ")";  // texture
    } else {
      return $"texture({textureName}.sampler, {transformedUv})";
    }
  }

  public static bool NeedsTextureShaderStruct(
      this IReadOnlyTexture? finTexture,
      IReadOnlyList<IReadOnlyModelAnimation>? animations)
    => finTexture.UsesShaderClamping() ||
       finTexture.GetTextureTransformType_(animations) !=
       TextureTransformType.NONE;

  public static bool UsesShaderClamping(this IReadOnlyTexture? finTexture) {
    if (finTexture == null) {
      return false;
    }

    if (finTexture.WrapModeU == WrapMode.MIRROR_CLAMP ||
        finTexture.WrapModeV == WrapMode.MIRROR_CLAMP) {
      return true;
    }

    return (finTexture.ClampS != null &&
            !finTexture.ClampS.Value.IsRoughly01()) ||
           (finTexture.ClampT != null &&
            !finTexture.ClampT.Value.IsRoughly01());
  }

  public static TextureTransformType GetTextureTransformType_(
      this IReadOnlyTexture? finTexture,
      IReadOnlyList<IReadOnlyModelAnimation>? animations) {
    if (finTexture == null) {
      return TextureTransformType.NONE;
    }

    var isTransform3d = finTexture.IsTransform3d;
    var isScrollingTexture = finTexture is IScrollingTexture;

    var staticType = GetTextureTransformType_(isTransform3d,
                                              isScrollingTexture,
                                              finTexture.Translation,
                                              finTexture.RotationRadians,
                                              finTexture.Scale);
    if (staticType != TextureTransformType.NONE) {
      return staticType;
    }

    if (animations != null) {
      foreach (var animation in animations) {
        if (animation.TextureTracks.TryGetValue(finTexture,
                                                out var textureTracks)) {
          return finTexture.IsTransform3d
              ? TextureTransformType.THREE_D
              : TextureTransformType.TWO_D;
        }
      }
    }

    return TextureTransformType.NONE;
  }

  private static TextureTransformType GetTextureTransformType_(
      bool isTransform3d,
      bool isScrollingTexture,
      Vector3? translationOrNull,
      Vector3? rotationOrNull,
      Vector3? scaleOrNull) {
    var hasTransform
        = (translationOrNull is { } translation && !translation.IsRoughly0() ||
           (rotationOrNull is { } radians && !radians.IsRoughly0()) ||
           isScrollingTexture);
    if (!hasTransform && scaleOrNull is { } scale) {
      hasTransform = isTransform3d
          ? !scale.IsRoughly1()
          : !scale.Xy().IsRoughly1();
    }

    return hasTransform
        ? (isTransform3d
            ? TextureTransformType.THREE_D
            : TextureTransformType.TWO_D)
        : TextureTransformType.NONE;
  }

  public static void AppendAlphaDiscard(StringBuilder src,
                                        IReadOnlyMaterial material) {
    switch (material.TransparencyType) {
      case TransparencyType.MASK: {
        src.AppendLine(
            $$"""
              
                if (fragColor.a < {{GlslConstants.MIN_ALPHA_BEFORE_DISCARD_MASK_TEXT}}) {
                  discard;
                }
              """);
        break;
      }
      case TransparencyType.TRANSPARENT: {
        src.AppendLine(
            $$"""
              
                if (fragColor.a < {{GlslConstants.MIN_ALPHA_BEFORE_DISCARD_TRANSPARENT_TEXT}}) {
                  discard;
                }
              """);
        break;
      }
    }
  }
}