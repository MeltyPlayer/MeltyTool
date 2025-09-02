#version 310 es
precision highp float;

struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};


uniform Texture diffuseTexture;

out vec4 fragColor;

in vec4 vertexColor0;
in vec2 uv0;

void main() {
  fragColor = texture(diffuseTexture.sampler, diffuseTexture.transform2d * vec3((uv0).x, (uv0).y, 1)) * vertexColor0;

  if (fragColor.a < .01) {
    discard;
  }
}