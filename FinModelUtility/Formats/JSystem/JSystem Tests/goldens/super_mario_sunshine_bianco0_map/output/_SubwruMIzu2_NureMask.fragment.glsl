#version 310 es
precision mediump float;

uniform vec3 color_GxMaterialColor1;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(vec3(vertexColor0.a)*color_GxMaterialColor1, 0.0, 1.0);

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}