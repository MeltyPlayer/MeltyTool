#version 310 es

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[6];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in int in_BoneIds;
layout(location = 2) in float in_BoneWeights;
layout(location = 3) in vec2 in_Uv0;
layout(location = 4) in vec2 in_Uv1;
layout(location = 5) in vec4 in_Color0;

out vec3 vertexPosition;
out vec2 uv0;
out vec2 uv1;
out vec4 vertexColor0;

void main() {
  mat4 mvMatrix = viewMatrix * modelMatrix;
  mat4 mvpMatrix = projectionMatrix * mvMatrix;
  mat4 mergedBoneMatrix = boneMatrices[in_BoneIds] * in_BoneWeights;


  mat4 vertexModelMatrix = modelMatrix * mergedBoneMatrix;
  mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
  uv0 = in_Uv0;
  uv1 = in_Uv1;
  vertexColor0 = in_Color0;
}
