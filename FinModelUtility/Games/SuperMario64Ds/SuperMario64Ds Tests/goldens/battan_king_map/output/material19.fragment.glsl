#version 310 es
precision highp float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.780392169952,0.749019622803,0.0);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, 1);
}