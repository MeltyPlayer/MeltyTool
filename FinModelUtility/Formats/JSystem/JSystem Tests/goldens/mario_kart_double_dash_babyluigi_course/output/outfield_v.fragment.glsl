#version 430

uniform sampler2D texture0;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = texture(texture0, uv0).rgb*vertexColor0.rgb;

  float alphaComponent = texture(texture0, uv0).a*0.4980392156862745;

  fragColor = vec4(colorComponent, alphaComponent);
}