#version 310 es
precision highp float;


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture0;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.9686275)*texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb;

  float alphaComponent = texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.01)) {
    discard;
  }
}