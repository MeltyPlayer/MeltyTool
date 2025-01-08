#version 310 es
precision highp float;

uniform vec3 color_GxMaterialColor78;
uniform float scalar_GxMaterialAlpha78;
out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxMaterialColor78, 0.0, 1.0);

  float alphaComponent = scalar_GxMaterialAlpha78;

  fragColor = vec4(colorComponent, alphaComponent);
}