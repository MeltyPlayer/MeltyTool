#version 310 es
precision highp float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(1.0);

  float alphaComponent = 0.95;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}