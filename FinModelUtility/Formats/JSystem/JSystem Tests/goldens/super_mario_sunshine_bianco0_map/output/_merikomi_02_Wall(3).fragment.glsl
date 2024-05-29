#version 400


struct Texture {
  sampler2D sampler;
  vec2 clampMin;
  vec2 clampMax;
  mat3x2 transform2d;
  mat4 transform3d;
};

vec2 transformUv3d(mat4 transform3d, vec2 inUv) {
  vec4 rawTransformedUv = (transform3d * vec4(inUv, 0, 1));

  // We need to manually divide by w for perspective correction!
  return rawTransformedUv.xy / rawTransformedUv.w;
}

uniform Texture texture0;
uniform vec3 color_GxColor2;
uniform float scalar_GxAlpha2;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxColor2, 0, 1);

  float alphaComponent = scalar_GxAlpha2*texture(texture0.sampler, clamp(texture0.transform2d * vec3((uv0).x, (uv0).y, 1), texture0.clampMin, texture0.clampMax)).a;

  fragColor = vec4(colorComponent, alphaComponent);
}
