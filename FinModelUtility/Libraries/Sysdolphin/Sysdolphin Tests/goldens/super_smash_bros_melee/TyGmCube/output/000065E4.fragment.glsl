#version 450
precision mediump float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.0313726);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, 1);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}