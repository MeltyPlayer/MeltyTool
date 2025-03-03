#version 310 es
precision highp float;

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

layout (std140, binding = 2) uniform Lights {
  Light lights[8];
  vec4 ambientLightColor;
  float useLighting;
};

uniform vec3 cameraPosition;

uniform sampler2D diffuseTexture;
uniform sampler2D normalTexture;
uniform sampler2D specularTexture;
uniform float shininess;

out vec4 fragColor;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec3 tangent;
in vec3 binormal;
in vec2 uv0;

void getSurfaceToLightNormalAndAttenuation(Light light, vec3 position, vec3 normal, out vec3 surfaceToLightNormal, out float attenuation) {
  vec3 surfaceToLight = light.position - position;
  
  surfaceToLightNormal = (light.sourceType == 3)
    ? -light.normal : normalize(surfaceToLight);

  if (light.attenuationFunction == 0) {
    attenuation = 1.0;
    return;
  }

  // Attenuation is calculated as a fraction, (cosine attenuation) / (distance attenuation).

  // Numerator (Cosine attenuation)
  vec3 cosAttn = light.cosineAttenuation;
  
  vec3 attnDotLhs = (light.attenuationFunction == 1)
    ? normal : surfaceToLightNormal;
  float attn = dot(attnDotLhs, light.normal);
  vec3 attnPowers = vec3(1, attn, attn*attn);

  float attenuationNumerator = max(0.0, dot(cosAttn, attnPowers));

  // Denominator (Distance attenuation)
  float attenuationDenominator = 1.0;
  if (light.sourceType != 3) {
    vec3 distAttn = light.distanceAttenuation;
    
    if (light.attenuationFunction == 1) {
      float attn = max(0.0, -dot(normal, light.normal));
      if (light.diffuseFunction != 0) {
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
  if (light.diffuseFunction == 1 || light.diffuseFunction == 2) {
    diffuseLightAmount = max(0.0, dot(normal, surfaceToLightNormal));
  }
  diffuseColor = light.color * diffuseLightAmount * attenuation;
  
  if (light.attenuationFunction != 0 && dot(normal, surfaceToLightNormal) >= 0.0) {
    vec3 surfaceToCameraNormal = normalize(cameraPosition - position);
    float specularLightAmount = pow(max(0.0, dot(reflect(-surfaceToLightNormal, normal), surfaceToCameraNormal)), shininess);
    specularColor = light.color * specularLightAmount * attenuation;
  }
}

void getMergedLightColors(vec3 position, vec3 normal, float shininess, out vec4 diffuseColor, out vec4 specularColor) {
  for (int i = 0; i < 8; ++i) {
    vec4 currentDiffuseColor = vec4(0);
    vec4 currentSpecularColor = vec4(0);
  
    getIndividualLightColors(lights[i], position, normal, shininess, currentDiffuseColor, currentSpecularColor);

    diffuseColor += currentDiffuseColor;
    specularColor += currentSpecularColor;
  }
}

vec4 applyMergedLightingColors(vec3 position, vec3 normal, float shininess, vec4 diffuseSurfaceColor, vec4 specularSurfaceColor) {
  vec4 mergedDiffuseLightColor = vec4(0);
  vec4 mergedSpecularLightColor = vec4(0);
  getMergedLightColors(position, normal, shininess, mergedDiffuseLightColor, mergedSpecularLightColor);

  // We double it because all the other kids do. (Other fixed-function games.)
  vec4 diffuseComponent = 2.0 * diffuseSurfaceColor * (ambientLightColor + mergedDiffuseLightColor);
  vec4 specularComponent = specularSurfaceColor * mergedSpecularLightColor;
  
  return clamp(diffuseComponent + specularComponent, 0.0, 1.0);
}

void main() {
  fragColor = texture(diffuseTexture, uv0);

  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);
  vec3 textureNormal = texture(normalTexture, uv0).xyz * 2.0 - 1.0;
  fragNormal = normalize(mat3(tangent, binormal, fragNormal) * textureNormal);
  fragColor.rgb = mix(fragColor.rgb, applyMergedLightingColors(vertexPosition, fragNormal, shininess, fragColor, texture(specularTexture, uv0)).rgb, useLighting);

  if (fragColor.a < .95) {
    discard;
  }
}