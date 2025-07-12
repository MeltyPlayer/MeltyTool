#version 310 es

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[20];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in ivec2 in_BoneIds;
layout(location = 3) in vec2 in_BoneWeights;
layout(location = 4) in vec2 in_Uv0;
layout(location = 5) in vec2 in_Uv1;

out vec3 vertexPosition;
out vec3 vertexNormal;
out vec2 uv0;
out vec2 uv1;

void main() {
  mat4 mvMatrix = viewMatrix * modelMatrix;
  mat4 mvpMatrix = projectionMatrix * mvMatrix;
  mat4 mergedBoneMatrix = boneMatrices[in_BoneIds.x] * in_BoneWeights.x +
                          boneMatrices[in_BoneIds.y] * in_BoneWeights.y;


  mat4 vertexModelMatrix = modelMatrix * mergedBoneMatrix;
  mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(vertexModelMatrix * vec4(in_Normal, 0)).xyz;
  uv0 = in_Uv0;
  uv1 = in_Uv1;
}
