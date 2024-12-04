#version 310 es
precision mediump float;

uniform sampler2D texture0;
uniform sampler2D texture1;

in vec4 vertexColor0;
in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vertexColor0.rgb*(texture(texture0, uv0).rgb*vec3(1.0 + -1.0*vertexColor0.a) + texture(texture1, uv1).rgb*vec3(vertexColor0.a));

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);
}