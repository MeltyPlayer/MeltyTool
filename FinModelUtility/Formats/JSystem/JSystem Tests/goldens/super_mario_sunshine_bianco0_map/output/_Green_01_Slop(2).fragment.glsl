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

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(texture(texture0.sampler, clamp(texture0.transform2d * vec3((uv0).x, (uv0).y, 1), texture0.clampMin, texture0.clampMax)).rgb*vertexColor0.rgb, 0, 1);

  float alphaComponent = texture(texture0.sampler, clamp(texture0.transform2d * vec3((uv0).x, (uv0).y, 1), texture0.clampMin, texture0.clampMax)).a*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}
