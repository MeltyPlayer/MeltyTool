#version 430

uniform sampler2D diffuseTexture;

out vec4 fragColor;

in vec4 vertexColor0;
in vec2 uv0;

void main() {
  fragColor = texture(diffuseTexture, uv0) * vertexColor0;
}