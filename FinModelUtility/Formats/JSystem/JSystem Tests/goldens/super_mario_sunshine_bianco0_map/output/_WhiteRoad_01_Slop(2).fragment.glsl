#version 400


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture0;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb*vertexColor0.rgb, 0, 1);

  float alphaComponent = texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).a*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}
