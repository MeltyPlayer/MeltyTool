#version 450
precision mediump float;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vertexColor0.rgb;

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, 1);
}