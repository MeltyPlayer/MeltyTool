#version 400


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture0;
uniform vec3 color_GxColor2;
uniform float scalar_GxAlpha2;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxColor2, 0, 1);

  float alphaComponent = scalar_GxAlpha2*texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).a;

  fragColor = vec4(colorComponent, alphaComponent);
}
