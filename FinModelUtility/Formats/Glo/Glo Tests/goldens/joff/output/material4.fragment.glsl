#version 310 es
precision mediump float;

out vec4 fragColor;

in vec4 vertexColor0;

void main() {
  fragColor = vertexColor0;
}