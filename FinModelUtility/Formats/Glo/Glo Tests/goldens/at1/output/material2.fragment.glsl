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


uniform Texture diffuseTexture;
uniform float shininess;
uniform int useLighting;

out vec4 fragColor;

in vec4 vertexColor0;
in vec2 uv0;

void main() {
  vec4 diffuseColor = texture(diffuseTexture.sampler, clamp(diffuseTexture.transform2d * vec3((uv0).x, (uv0).y, 1), diffuseTexture.clampMin, diffuseTexture.clampMax));

  fragColor = diffuseColor * vertexColor0;
}