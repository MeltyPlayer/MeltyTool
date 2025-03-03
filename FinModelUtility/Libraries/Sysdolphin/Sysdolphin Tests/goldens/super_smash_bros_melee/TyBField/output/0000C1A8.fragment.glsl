#version 310 es
precision highp float;

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[7];  
};

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

struct Texture {
  sampler2D sampler;
  mat4 transform3d;
};

vec2 transformUv3d(mat4 transform3d, vec2 inUv) {
  vec4 rawTransformedUv = (transform3d * vec4(inUv, 0, 1));

  // We need to manually divide by w for perspective correction!
  return rawTransformedUv.xy / rawTransformedUv.w;
}

uniform Texture texture0;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);

  vec2 sphericalReflectionUv = acos(normalize(projectionMatrix * viewMatrix * vec4(fragNormal, 0)).xy) / 3.14159;

  vec3 colorComponent = (ambientLightColor.rgb + vec3(1.0))*vec3(2.0)*vertexColor0.rgb*vec3(0.5) + texture(texture0.sampler, transformUv3d(texture0.transform3d, sphericalReflectionUv)).rgb*vec3(0.5);

  float alphaComponent = 0.34999999404*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}