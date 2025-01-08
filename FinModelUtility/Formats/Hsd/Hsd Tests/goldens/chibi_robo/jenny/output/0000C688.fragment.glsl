#version 310 es
precision highp float;

uniform sampler2D texture0;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2.0)*vec3(0.701960802078)*vertexColor0.rgb*texture(texture0, uv0).rgb;

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}