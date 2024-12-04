#version 430

layout (std140, binding = 1) uniform Matrices {
  mat4 modelMatrix;
  mat4 viewMatrix;
  mat4 projectionMatrix;
  
  mat4 boneMatrices[71];  
};

uniform vec3 cameraPosition;

layout(location = 0) in vec3 in_Position;
layout(location = 1) in vec3 in_Normal;
layout(location = 2) in vec4 in_Tangent;
layout(location = 3) in vec2 in_Uv0;


out vec3 vertexPosition;
out vec3 vertexNormal;
out vec3 tangent;
out vec3 binormal;
out vec2 sphericalReflectionUv;
out vec2 uv0;

void main() {
  mat4 mvMatrix = viewMatrix * modelMatrix;
  mat4 mvpMatrix = projectionMatrix * mvMatrix;

  gl_Position = mvpMatrix * vec4(in_Position, 1);

  vertexPosition = vec3(modelMatrix * vec4(in_Position, 1));
  vertexNormal = normalize(modelMatrix * vec4(in_Normal, 0)).xyz;
  tangent = normalize(modelMatrix * vec4(in_Tangent)).xyz;
  binormal = cross(vertexNormal, tangent);
  // Hello

  vec3 e = normalize( vec3( mvMatrix * vec4(in_Position, 1)) );
  vec3 n = normalize( vec3( mvMatrix * vec4(in_Normal, 0)) );
  
  vec3 r = reflect( e, n );
  float m = 2. * sqrt(
    pow( r.x, 2. ) +
    pow( r.y, 2. ) +
    pow( r.z + 1., 2. )
  );

  sphericalReflectionUv = r.xy / m + .5;
  uv0 = in_Uv0;
}
