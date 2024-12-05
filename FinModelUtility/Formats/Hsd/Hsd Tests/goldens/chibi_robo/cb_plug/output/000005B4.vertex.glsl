#version 430

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[5];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in int in_BoneIds;
layout(location = 3) in float in_BoneWeights;


out vec3 vertexPosition;
out vec3 vertexNormal;
out vec2 sphericalReflectionUv;

void main() {
  mat4 mvMatrix = viewMatrix * modelMatrix;
  mat4 mvpMatrix = projectionMatrix * mvMatrix;
  mat4 mergedBoneMatrix = boneMatrices[in_BoneIds] * in_BoneWeights;


  mat4 vertexModelMatrix = modelMatrix * mergedBoneMatrix;
  mat4 projectionVertexModelMatrix = mvpMatrix * mergedBoneMatrix;

  gl_Position = projectionVertexModelMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(vertexModelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(vertexModelMatrix * vec4(in_Normal, 0)).xyz;

  vec3 u = normalize( vec3( mvMatrix * mergedBoneMatrix * vec4(in_Position, 1)) );

  mat3 normalMatrix = transpose(inverse(mat3(mvMatrix * mergedBoneMatrix)));
  vec3 n = normalize( normalMatrix * in_Normal );
  
  vec3 r = reflect( u, n );
  float m = 2. * sqrt(
    pow( r.x, 2. ) +
    pow( r.y, 2. ) +
    pow( r.z + 1., 2. )
  );

  sphericalReflectionUv = r.xy / m + .5;
}
