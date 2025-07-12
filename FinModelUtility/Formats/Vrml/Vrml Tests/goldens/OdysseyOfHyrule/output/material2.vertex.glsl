#version 310 es

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[1];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;

out vec3 vertexPosition;
out vec3 vertexNormal;

void main() {
  mat4 mvMatrix = viewMatrix * modelMatrix;
  mat4 mvpMatrix = projectionMatrix * mvMatrix;

  gl_Position = mvpMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(modelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(modelMatrix * vec4(in_Normal, 0)).xyz;
}
