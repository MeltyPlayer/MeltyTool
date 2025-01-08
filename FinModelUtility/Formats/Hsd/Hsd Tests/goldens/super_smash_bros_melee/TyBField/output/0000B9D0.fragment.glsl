#version 310 es
precision highp float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2.0)*vec3(1.0,0.701960802078,0.0);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}