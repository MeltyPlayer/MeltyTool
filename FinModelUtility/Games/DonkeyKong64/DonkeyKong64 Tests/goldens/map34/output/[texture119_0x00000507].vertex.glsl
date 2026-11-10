#version 450

layout (std140, binding = 1) uniform GlobalMatrices {
  mat4 projectionViewMatrix;
};
layout (std430, binding = 2) readonly buffer CurrentMatrices {
  mat4 modelMatrix;
  mat4 boneMatrices[];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec2 in_Uv0;
layout(location = 3) in vec4 in_Color0;

out vec3 vertexPosition;
out vec2 uv0;
out vec4 vertexColor0;

void main() {
  mat4 mvpMatrix = projectionViewMatrix * modelMatrix;

  gl_Position = mvpMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(modelMatrix * vec4(in_Position, 1));
  uv0 = in_Uv0;
  vertexColor0 = in_Color0;
}
