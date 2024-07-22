#version 430


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture0;
uniform Texture texture1;

in vec2 normalUv;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(clamp((texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb + vec3(-0.5))*vec3(0.5), 0, 1) + (texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb + vec3(-0.5))*vec3(0.5)*(vec3(1) + vec3(-1)*texture(texture1.sampler, texture1.transform2d * vec3((acos(normalUv) / 3.14159).x, (acos(normalUv) / 3.14159).y, 1)).rgb) + texture(texture1.sampler, texture1.transform2d * vec3((acos(normalUv) / 3.14159).x, (acos(normalUv) / 3.14159).y, 1)).rgb*texture(texture1.sampler, texture1.transform2d * vec3((acos(normalUv) / 3.14159).x, (acos(normalUv) / 3.14159).y, 1)).rgb, 0, 1);

  float alphaComponent = 0;

  fragColor = vec4(colorComponent, 1);
}
