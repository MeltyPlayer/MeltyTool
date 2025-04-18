#version 310 es
precision highp float;


struct Texture {
  sampler2D sampler;
  mat3x2 transform2d;
};

uniform Texture texture1;
uniform sampler2D texture2;
uniform vec3 color_3dsColor5;
uniform vec3 color_3dsColor4;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp((vec3(1.0) + vec3(-1.0)*vertexColor0.rgb)*color_3dsColor4 + clamp(clamp((texture(texture2, uv0).rgb*(vec3(1.0) + vec3(-1.0)*vertexColor0.rgb) + clamp(texture(texture1.sampler, texture1.transform2d * vec3((uv0).x, (uv0).y, 1)).rgb, 0.0, 1.0)*vertexColor0.rgb), 0.0, 1.0)*color_3dsColor5, 0.0, 1.0), 0.0, 1.0);

  float alphaComponent = (texture(texture1.sampler, texture1.transform2d * vec3((uv0).x, (uv0).y, 1)).a + texture(texture2, uv0).a)*vertexColor0.a*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}