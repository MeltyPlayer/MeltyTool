#version 310 es
precision mediump float;

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[7];  
};


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
uniform Texture texture1;

in vec3 vertexPosition;
in vec3 vertexNormal;
in vec2 uv0;

out vec4 fragColor;

void main() {
  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);

  vec2 sphericalReflectionUv = acos(normalize(projectionMatrix * viewMatrix * vec4(fragNormal, 0)).xy) / 3.14159;

  vec3 colorComponent = vec3(2.0)*texture(texture0.sampler, transformUv3d(texture0.transform3d, uv0)).rgb*texture(texture1.sampler, transformUv3d(texture1.transform3d, sphericalReflectionUv)).rgb;

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}