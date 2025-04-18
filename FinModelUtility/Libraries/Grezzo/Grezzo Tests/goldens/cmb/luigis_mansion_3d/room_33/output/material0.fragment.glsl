#version 310 es
precision highp float;

uniform vec3 color_3dsColor0;
out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_3dsColor0, 0.0, 1.0);

  float alphaComponent = 0.0;

  fragColor = vec4(colorComponent, alphaComponent);
}