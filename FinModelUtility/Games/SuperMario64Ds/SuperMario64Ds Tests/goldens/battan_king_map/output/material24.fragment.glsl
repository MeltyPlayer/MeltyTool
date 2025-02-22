#version 310 es
precision highp float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(1.0);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, 1);
}