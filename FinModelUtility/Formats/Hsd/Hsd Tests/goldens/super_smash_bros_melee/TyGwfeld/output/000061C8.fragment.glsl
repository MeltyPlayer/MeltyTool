#version 310 es
precision mediump float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.0);

  float alphaComponent = 0.20000000298;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}