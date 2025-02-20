#version 310 es
precision highp float;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform vec3 color_GxColorRegister18;
uniform float scalar_GxMaterialAlpha9;

in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = color_GxColorRegister18*texture(texture0, uv0).rgb;

  float alphaComponent = texture(texture1, uv1).a*scalar_GxMaterialAlpha9;

  fragColor = vec4(colorComponent, alphaComponent);
}