#version 430

uniform sampler2D texture0;
in vec2 sphericalReflectionUv;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = texture(texture0, asin(sphericalReflectionUv) / 3.14159 + 0.5).rgb;

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
