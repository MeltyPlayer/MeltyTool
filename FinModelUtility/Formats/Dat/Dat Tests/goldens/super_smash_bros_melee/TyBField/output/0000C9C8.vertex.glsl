#version 430

layout (std140, binding = 1) uniform Matrix {
  mat4 modelMatrix;
  mat4 modelViewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[7];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in vec4 in_Tangent;
layout(location = 3) in vec2 in_Uvs[4];
layout(location = 7) in vec4 in_Colors[2];

out vec3 vertexPosition;
out vec3 vertexNormal;
out vec3 tangent;
out vec3 binormal;
out vec2 normalUv;
out vec2 uv0;
out vec2 uv1;
out vec2 uv2;
out vec2 uv3;
out vec4 vertexColor0;
out vec4 vertexColor1;
void main() {
  mat4 mvpMatrix = projectionMatrix * modelViewMatrix;
  gl_Position = mvpMatrix * vec4(in_Position, 1);
  vertexNormal = normalize(modelMatrix * vec4(in_Normal, 0)).xyz;
  tangent = normalize(modelMatrix * vec4(in_Tangent)).xyz;
  binormal = cross(vertexNormal, tangent); 
  normalUv = normalize(mvpMatrix * vec4(in_Normal, 0)).xy;
  uv0 = in_Uvs[0];
  uv1 = in_Uvs[1];
  uv2 = in_Uvs[2];
  uv3 = in_Uvs[3];
  vertexColor0 = in_Colors[0];
  vertexColor1 = in_Colors[1];
}