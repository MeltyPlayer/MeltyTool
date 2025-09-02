#version 310 es
precision highp float;

uniform sampler2D texture0;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.9686275)*texture(texture0, uv0).rgb;

  float alphaComponent = texture(texture0, uv0).a;

  fragColor = vec4(colorComponent, 1);

  if (!(alphaComponent > 0.95)) {
    discard;
  }
}