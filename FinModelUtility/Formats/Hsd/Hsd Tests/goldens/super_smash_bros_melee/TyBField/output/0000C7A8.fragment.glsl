#version 430

uniform sampler2D texture0;
uniform sampler2D texture1;
in vec2 sphericalReflectionUv;

in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*texture(texture0, uv0).rgb*texture(texture1, sphericalReflectionUv).rgb;

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
