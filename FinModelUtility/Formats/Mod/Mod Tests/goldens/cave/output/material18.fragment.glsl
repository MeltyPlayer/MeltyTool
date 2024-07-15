#version 400

uniform vec3 color_GxMaterialColor18;
uniform float scalar_GxMaterialAlpha18;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxMaterialColor18, 0, 1);

  float alphaComponent = scalar_GxMaterialAlpha18;

  fragColor = vec4(colorComponent, alphaComponent);
}
