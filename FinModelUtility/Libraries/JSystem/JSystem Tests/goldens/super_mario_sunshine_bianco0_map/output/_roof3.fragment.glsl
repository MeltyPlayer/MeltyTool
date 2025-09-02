#version 310 es
precision highp float;

uniform sampler2D texture0;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(texture(texture0, uv0).rgb*vertexColor0.rgb, 0.0, 1.0);

  float alphaComponent = texture(texture0, uv0).a*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}