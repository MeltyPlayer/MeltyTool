#version 450

layout (std140, binding = 1) uniform GlobalMatrices {
  mat4 projectionViewMatrix;
};

layout (std140, binding = 2) uniform CurrentMatrices {
  mat4 modelMatrix;
  mat4 boneMatrices[19];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in int in_BoneIds;
layout(location = 3) in float in_BoneWeights;
layout(location = 4) in vec2 in_Uv0;
layout(location = 5) in vec2 in_Uv1;
layout(location = 6) in vec4 in_Color0;

out vec3 vertexPosition;
out vec3 vertexNormal;
out vec2 uv0;
out vec2 uv1;
out vec4 vertexColor0;

void main() {
  mat4 mvpMatrix = projectionViewMatrix * modelMatrix;
  mat4 mergedBoneMatrix = boneMatrices[in_BoneIds] * in_BoneWeights;


  mat4 vertexModelMatrix = modelMatrix * mergedBoneMatrix;
  mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(vertexModelMatrix * vec4(in_Normal, 0)).xyz;
  uv0 = in_Uv0;
  uv1 = in_Uv1;
  vertexColor0 = in_Color0;
}
