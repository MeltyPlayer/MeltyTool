#version 310 es
precision highp float;

uniform float scalar_3dsAlpha5;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(vec3(1.0 + -1.0*vertexColor0.a) + vec3(scalar_3dsAlpha5), 0.0, 1.0);

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}