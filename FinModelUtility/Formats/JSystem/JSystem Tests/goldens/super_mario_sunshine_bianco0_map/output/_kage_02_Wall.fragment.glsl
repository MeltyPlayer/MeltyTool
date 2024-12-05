#version 310 es
precision mediump float;

uniform sampler2D texture0;
uniform vec3 color_GxColorRegister2;
uniform float scalar_GxAlphaRegister2;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(color_GxColorRegister2, 0.0, 1.0);

  float alphaComponent = scalar_GxAlphaRegister2*texture(texture0, uv0).a;

  fragColor = vec4(colorComponent, alphaComponent);
}