#version 400

uniform sampler2D texture0;
uniform sampler2D texture1;

in vec2 normalUv;
in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*texture(texture0, uv0).rgb*texture(texture1, asin(normalUv) / 3.14159 + 0.5).rgb;

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(fragColor.a > 0)) {
    discard;
  }
}
