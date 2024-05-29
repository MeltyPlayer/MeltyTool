#version 400

uniform sampler2D texture0;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*texture(texture0, uv0).rgb;

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(fragColor.a > 0)) {
    discard;
  }
}
