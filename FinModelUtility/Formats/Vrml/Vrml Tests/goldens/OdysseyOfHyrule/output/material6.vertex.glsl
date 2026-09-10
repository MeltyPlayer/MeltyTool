#version 450

layout (std140, binding = 1) uniform GlobalMatrices {
  mat4 projectionViewMatrix;
};

layout (std140, binding = 2) uniform CurrentMatrices {
  mat4 modelMatrix;
  mat4 boneMatrices[1];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;

out vec3 vertexPosition;
out vec3 vertexNormal;

void main() {
  mat4 mvpMatrix = projectionViewMatrix * modelMatrix;

  gl_Position = mvpMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(modelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(modelMatrix * vec4(in_Normal, 0)).xyz;
}
