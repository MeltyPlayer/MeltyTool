#version 400

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform vec3 color_GxColor0;

in vec4 vertexColor0;
in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = color_GxColor0*texture(texture0, uv0).rgb;

  float alphaComponent = texture(texture1, uv1).a*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}
