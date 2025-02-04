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
uniform float shininess;
in vec3 vertexPosition;
in vec3 vertexNormal;

out vec4 fragColor;

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

void main() {
  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);

  vec4 mergedLightDiffuseColor = vec4(0);
  vec4 mergedLightSpecularColor = vec4(0);
  getMergedLightColors(vertexPosition, fragNormal, shininess, mergedLightDiffuseColor, mergedLightSpecularColor);

  vec3 colorComponent = (ambientLightColor.rgb + mergedLightDiffuseColor.rgb)*vec3(2.0)*vec3(0.298038989305);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, 1);
}