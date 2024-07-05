#version 400

uniform sampler2D texture0;
uniform vec3 color_GxMaterialColor1;
uniform float scalar_GxMaterialAlpha1;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(texture(texture0, uv0).rgb*color_GxMaterialColor1, 0, 1);

  float alphaComponent = texture(texture0, uv0).a*scalar_GxMaterialAlpha1;

  fragColor = vec4(colorComponent, alphaComponent);
}
