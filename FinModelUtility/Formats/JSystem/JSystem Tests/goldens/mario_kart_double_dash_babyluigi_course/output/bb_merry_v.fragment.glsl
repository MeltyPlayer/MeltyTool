#version 400


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture0;
uniform vec3 color_GxMaterialColor0;
uniform float scalar_GxMaterialAlpha0;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb*color_GxMaterialColor0, 0, 1);

  float alphaComponent = texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).a*scalar_GxMaterialAlpha0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(fragColor.a >= 0.5019608)) {
    discard;
  }
}
