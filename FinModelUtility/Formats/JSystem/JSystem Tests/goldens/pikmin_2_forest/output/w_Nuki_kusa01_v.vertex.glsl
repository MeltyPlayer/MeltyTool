#version 430

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[2];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in vec4 in_Tangent;
layout(location = 3) in vec2 in_Uv0;
layout(location = 4) in vec2 in_Uv1;
layout(location = 7) in vec4 in_Color0;


out vec3 vertexPosition;
out vec3 vertexNormal;
out vec3 tangent;
out vec3 binormal;
out vec2 uv0;
out vec2 uv1;
out vec4 vertexColor0;

void main() {
  mat4 mvMatrix = viewMatrix * modelMatrix;
  mat4 mvpMatrix = projectionMatrix * mvMatrix;

  gl_Position = mvpMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(modelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(modelMatrix * vec4(in_Normal, 0)).xyz;
  tangent = normalize(modelMatrix * vec4(in_Tangent)).xyz;
  binormal = cross(vertexNormal, tangent);
  uv0 = in_Uv0;
  uv1 = in_Uv1;
  vertexColor0 = in_Color0;
}
