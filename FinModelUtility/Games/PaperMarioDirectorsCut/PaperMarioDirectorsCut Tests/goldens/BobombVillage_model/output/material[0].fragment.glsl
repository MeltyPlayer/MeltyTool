#version 400

out vec4 fragColor;

in vec4 vertexColor0;

void main() {
  fragColor = vertexColor0;
}