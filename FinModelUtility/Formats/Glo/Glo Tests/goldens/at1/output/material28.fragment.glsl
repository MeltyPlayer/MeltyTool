#version 400

uniform sampler2D diffuseTexture;
uniform float shininess;
uniform float useLighting;

out vec4 fragColor;

in vec4 vertexColor0;
in vec2 uv0;

void main() {
  vec4 diffuseColor = texture(diffuseTexture, uv0);

  fragColor = diffuseColor * vertexColor0;
}