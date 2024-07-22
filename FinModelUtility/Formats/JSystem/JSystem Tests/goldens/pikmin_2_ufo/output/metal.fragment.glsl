#version 430

uniform vec3 color_GxColorRegister7;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxColorRegister7*vec3(2), 0, 1);

  float alphaComponent = 0;

  fragColor = vec4(colorComponent, alphaComponent);
}
