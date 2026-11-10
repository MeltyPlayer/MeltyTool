#version 450
precision mediump float;

uniform sampler2D texture0;
uniform sampler2D texture1;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = texture(texture0, uv0).rgb*texture(texture1, uv0).rgb*vertexColor0.rgb;

  float alphaComponent = texture(texture0, uv0).a*texture(texture1, uv0).a*vertexColor0.a;

  fragColor = vec4(colorComponent, 1);

  if (!(alphaComponent > 0.95)) {
    discard;
  }
}