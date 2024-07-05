#version 400

uniform vec3 color_GxMaterialColor24;
uniform float scalar_GxMaterialAlpha24;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxMaterialColor24, 0, 1);

  float alphaComponent = scalar_GxMaterialAlpha24;

  fragColor = vec4(colorComponent, alphaComponent);
}
