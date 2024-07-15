#version 400

uniform sampler2D texture0;
uniform vec3 color_GxColorRegister36;

in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = color_GxColorRegister36*texture(texture0, uv0).rgb;

  float alphaComponent = 0;

  fragColor = vec4(colorComponent, alphaComponent);
}
