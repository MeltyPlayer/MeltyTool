#version 310 es
precision highp float;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.96862745285,0.937254905701,0.0);

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, 1);
}