#version 310 es
precision mediump float;

uniform sampler2D texture0;
uniform float scalar_3dsAlpha1;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(texture(texture0, uv0).rgb, 0.0, 1.0);

  float alphaComponent = vertexColor0.a*scalar_3dsAlpha1;

  fragColor = vec4(colorComponent, alphaComponent);
}