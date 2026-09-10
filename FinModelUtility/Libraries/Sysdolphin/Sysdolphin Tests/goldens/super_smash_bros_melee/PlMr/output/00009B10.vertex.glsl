#version 450

layout (std140, binding = 1) uniform GlobalMatrices {
  mat4 projectionViewMatrix;
};

layout (std140, binding = 2) uniform CurrentMatrices {
  mat4 modelMatrix;
  mat4 boneMatrices[62];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in ivec3 in_BoneIds;
layout(location = 3) in vec3 in_BoneWeights;
layout(location = 4) in vec2 in_Uv0;

out vec3 vertexPosition;
out vec3 vertexNormal;
out vec2 uv0;

void main() {
  mat4 mvpMatrix = projectionViewMatrix * modelMatrix;
  mat4 mergedBoneMatrix = boneMatrices[in_BoneIds.x] * in_BoneWeights.x +
                          boneMatrices[in_BoneIds.y] * in_BoneWeights.y +
                          boneMatrices[in_BoneIds.z] * in_BoneWeights.z;


  mat4 vertexModelMatrix = modelMatrix * mergedBoneMatrix;
  mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(vertexModelMatrix * vec4(in_Normal, 0)).xyz;
  uv0 = in_Uv0;
}
