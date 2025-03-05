#version 310 es
precision highp float;

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[115];  
};

uniform sampler2D texture0;

in vec3 vertexPosition;
in vec3 vertexNormal;

out vec4 fragColor;

void main() {
  // Have to renormalize because the vertex normals can become distorted when interpolated.
  vec3 fragNormal = normalize(vertexNormal);

  vec2 sphericalReflectionUv = acos(normalize(projectionMatrix * viewMatrix * vec4(fragNormal, 0)).xy) / 3.14159;

  vec3 colorComponent = texture(texture0, sphericalReflectionUv).rgb;

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}