#version 430

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
  int useLighting;
};

uniform vec3 cameraPosition;
uniform float shininess;
uniform sampler2D texture0;
uniform sampler2D texture1;
uniform sampler2D texture2;
uniform sampler2D texture3;
uniform vec3 color_GxMaterialColor0;
uniform vec3 color_GxAmbientColor0;
uniform vec3 color_GxAmbientColor1;
uniform vec3 color_GxColorRegister7;
uniform vec3 color_GxColorRegister8;
uniform float scalar_GxMaterialAlpha0;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;


void getSurfaceToLightNormalAndAttenuation(Light light, vec3 position, vec3 normal, out vec3 surfaceToLightNormal, out float attenuation) {
  vec3 surfaceToLight = light.position - position;
  
  surfaceToLightNormal = (light.sourceType == 3)
    ? -light.normal : normalize(surfaceToLight);

  if (light.attenuationFunction == 0) {
    attenuation = 1;
    return;
  }
  

  // Attenuation is calculated as a fraction, (cosine attenuation) / (distance attenuation).

  // Numerator (Cosine attenuation)
  vec3 cosAttn = light.cosineAttenuation;
  
  vec3 attnDotLhs = (light.attenuationFunction == 1)
    ? normal : surfaceToLightNormal;
  float attn = dot(attnDotLhs, light.normal);
  vec3 attnPowers = vec3(1, attn, attn*attn);

  float attenuationNumerator = max(0, dot(cosAttn, attnPowers));

  // Denominator (Distance attenuation)
  float attenuationDenominator = 1;
  if (light.sourceType != 3) {
    vec3 distAttn = light.distanceAttenuation;
    
    if (light.attenuationFunction == 1) {
      float attn = max(0, -dot(normal, light.normal));
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
  float attenuation = 0;
  getSurfaceToLightNormalAndAttenuation(light, position, normal, surfaceToLightNormal, attenuation);

  float diffuseLightAmount = 1;
  if (light.diffuseFunction == 1 || light.diffuseFunction == 2) {
    diffuseLightAmount = max(0, dot(normal, surfaceToLightNormal));
  }
  diffuseColor = light.color * diffuseLightAmount * attenuation;
  
  if (dot(normal, surfaceToLightNormal) >= 0) {
    vec3 surfaceToCameraNormal = normalize(cameraPosition - position);
    float specularLightAmount = pow(max(0, dot(reflect(-surfaceToLightNormal, normal), surfaceToCameraNormal)), shininess);
    specularColor = light.color * specularLightAmount * attenuation;
  }
}

void main() {
  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);

  vec4 individualLightDiffuseColors[8];
  vec4 individualLightSpecularColors[8];
  
  for (int i = 0; i < 8; ++i) {
    vec4 diffuseLightColor = vec4(0);
    vec4 specularLightColor = vec4(0);
    
    getIndividualLightColors(lights[i], vertexPosition, fragNormal, shininess, diffuseLightColor, specularLightColor);
    
    individualLightDiffuseColors[i] = diffuseLightColor;
    individualLightSpecularColors[i] = specularLightColor;
  }
  
  vec3 colorComponent = clamp(color_GxMaterialColor0*clamp((individualLightDiffuseColors[2].rgb + clamp(color_GxAmbientColor1, 0, 1)), 0, 1)*(vec3(1) + vec3(-1)*(vec3(texture(texture1, uv1).a).g*65280 + vec3(texture(texture1, uv1).a).r*255 > 24575.625 ? vec3(1) : vec3(0))) + clamp((color_GxColorRegister7*(vec3(1) + vec3(-1)*texture(texture0, uv0).rgb) + color_GxColorRegister8*texture(texture0, uv0).rgb), 0, 1)*(vec3(texture(texture1, uv1).a).g*65280 + vec3(texture(texture1, uv1).a).r*255 > 24575.625 ? vec3(1) : vec3(0)) + texture(texture2, uv1).rgb*(vec3(1) + vec3(-1)*(vec3(texture(texture1, uv1).a).g*65280 + vec3(texture(texture1, uv1).a).r*255 > 24575.625 ? vec3(1) : vec3(0))) + clamp((vec3(0.625) + texture(texture3, uv0).rgb*(vec3(1) + vec3(-1)*vec3(0.625)) + color_GxMaterialColor0*clamp((individualLightDiffuseColors[0].rgb + color_GxAmbientColor0), 0, 1)*vec3(0.625) + vec3(-0.5)), 0, 1)*(vec3(1) + vec3(-1)*vec3(0.5)) + color_GxMaterialColor0*clamp((individualLightDiffuseColors[2].rgb + clamp(color_GxAmbientColor1, 0, 1)), 0, 1)*vec3(0.5) + vec3(-0.5), 0, 1);

  float alphaComponent = scalar_GxMaterialAlpha0;

  fragColor = vec4(colorComponent, alphaComponent);
}
