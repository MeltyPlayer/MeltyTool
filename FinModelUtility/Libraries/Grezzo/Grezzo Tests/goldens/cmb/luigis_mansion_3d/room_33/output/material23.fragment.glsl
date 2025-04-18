#version 310 es
precision highp float;

uniform vec3 color_3dsColor0;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(clamp(color_3dsColor0, 0.0, 1.0)*vertexColor0.rgb, 0.0, 1.0);

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}