#version 400

uniform sampler2D texture0;
uniform vec3 color_GxColor2;
uniform float scalar_GxAlpha2;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxColor2, 0, 1);

  float alphaComponent = scalar_GxAlpha2*texture(texture0, uv0).a;

  fragColor = vec4(colorComponent, alphaComponent);
}
