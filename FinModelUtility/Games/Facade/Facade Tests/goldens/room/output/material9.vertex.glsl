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
layout(location = 1) in int in_BoneIds;
layout(location = 2) in float in_BoneWeights;

out vec3 vertexPosition;

void main() {
  mat4 mvpMatrix = projectionViewMatrix * modelMatrix;
  mat4 mergedBoneMatrix = boneMatrices[in_BoneIds] * in_BoneWeights;


  mat4 vertexModelMatrix = modelMatrix * mergedBoneMatrix;
  mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
}
