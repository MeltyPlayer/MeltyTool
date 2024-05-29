#version 400


struct Light {
  bool enabled;

  int sourceType;
  vec3 position;
  vec3 normal;

  vec4 color;
  
  int diffuseFunction;
  int attenuationFunction;
  vec3 cosineAttenuation;
  vec3 distanceAttenuation;
};

uniform Light lights[8];

uniform vec3 cameraPosition;
uniform float shininess;

struct Texture {
  sampler2D sampler;
  vec2 clampMin;
  vec2 clampMax;
  mat3x2 transform2d;
  mat4 transform3d;
};

vec2 transformUv3d(mat4 transform3d, vec2 inUv) {
  vec4 rawTransformedUv = (transform3d * vec4(inUv, 0, 1));

  // We need to manually divide by w for perspective correction!
  return rawTransformedUv.xy / rawTransformedUv.w;
}

uniform Texture texture0;
uniform Texture texture1;
uniform Texture texture2;
uniform sampler2D texture3;
uniform vec3 color_GxMaterialColor0;
uniform vec3 color_GxAmbientColor0;
uniform vec3 color_GxMaterialColor1;
uniform vec3 color_GxAmbientColor1;
uniform vec3 color_GxColor5;
uniform vec3 color_GxColor6;
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
  
  vec3 colorComponent = clamp(color_GxMaterialColor1*clamp((individualLightDiffuseColors[2].rgb + clamp(color_GxAmbientColor1, 0, 1)), 0, 1)*(vec3(1) + vec3(-1)*(vec3(texture(texture1.sampler, clamp(texture1.transform2d * vec3((uv1).x, (uv1).y, 1), texture1.clampMin, texture1.clampMax)).a).g*65280 + vec3(texture(texture1.sampler, clamp(texture1.transform2d * vec3((uv1).x, (uv1).y, 1), texture1.clampMin, texture1.clampMax)).a).r*255 > 0.375*65280 + 0.375*255 ? vec3(1) : vec3(0))) + clamp((color_GxMaterialColor1*clamp((individualLightDiffuseColors[2].rgb + clamp(color_GxAmbientColor1, 0, 1)), 0, 1) + color_GxColor5*(vec3(1) + vec3(-1)*texture(texture0.sampler, clamp(texture0.transform2d * vec3((uv0).x, (uv0).y, 1), texture0.clampMin, texture0.clampMax)).rgb) + color_GxColor6*texture(texture0.sampler, clamp(texture0.transform2d * vec3((uv0).x, (uv0).y, 1), texture0.clampMin, texture0.clampMax)).rgb), 0, 1)*(vec3(texture(texture1.sampler, clamp(texture1.transform2d * vec3((uv1).x, (uv1).y, 1), texture1.clampMin, texture1.clampMax)).a).g*65280 + vec3(texture(texture1.sampler, clamp(texture1.transform2d * vec3((uv1).x, (uv1).y, 1), texture1.clampMin, texture1.clampMax)).a).r*255 > 0.375*65280 + 0.375*255 ? vec3(1) : vec3(0)) + texture(texture2.sampler, clamp(texture2.transform2d * vec3((uv1).x, (uv1).y, 1), texture2.clampMin, texture2.clampMax)).rgb*(vec3(1) + vec3(-1)*(vec3(texture(texture1.sampler, clamp(texture1.transform2d * vec3((uv1).x, (uv1).y, 1), texture1.clampMin, texture1.clampMax)).a).g*65280 + vec3(texture(texture1.sampler, clamp(texture1.transform2d * vec3((uv1).x, (uv1).y, 1), texture1.clampMin, texture1.clampMax)).a).r*255 > 0.375*65280 + 0.375*255 ? vec3(1) : vec3(0))) + clamp((vec3(0.625) + texture(texture3, uv0).rgb*(vec3(1) + vec3(-1)*vec3(0.625)) + color_GxMaterialColor0*clamp((individualLightDiffuseColors[0].rgb + color_GxAmbientColor0), 0, 1)*vec3(0.625) + vec3(-0.5)), 0, 1)*(vec3(1) + vec3(-1)*vec3(0.2980392156862745,0.4235294117647059,0.3803921568627451)) + vec3(-0.5), 0, 1);

  float alphaComponent = scalar_GxMaterialAlpha0;

  fragColor = vec4(colorComponent, alphaComponent);
}
