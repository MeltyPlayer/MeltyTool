#version 400


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture0;
uniform Texture texture1;

in vec4 vertexColor0;
in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp((texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb*vec3(vertexColor0.a) + texture(texture1.sampler, texture1.transform2d * vec3((uv1).x, (uv1).y, 1)).rgb*(vec3(1) + vec3(-1)*vec3(vertexColor0.a)))*vertexColor0.rgb, 0, 1);

  float alphaComponent = (texture(texture0.sampler, texture0.transform2d * vec3((uv0).x, (uv0).y, 1)).a*0.4980392156862745 + texture(texture1.sampler, texture1.transform2d * vec3((uv1).x, (uv1).y, 1)).a*(1 + -1*0.4980392156862745))*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}
