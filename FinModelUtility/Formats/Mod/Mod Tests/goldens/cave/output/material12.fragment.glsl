#version 310 es
precision mediump float;

uniform sampler2D texture0;
uniform vec3 color_GxColorRegister21;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = color_GxColorRegister21*texture(texture0, uv0).rgb;

  float alphaComponent = 0.0;

  fragColor = vec4(colorComponent, alphaComponent);
}