#version 310 es
precision mediump float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.0);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}