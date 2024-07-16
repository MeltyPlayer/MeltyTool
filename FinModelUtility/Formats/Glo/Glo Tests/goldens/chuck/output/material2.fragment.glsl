#version 400

uniform sampler2D diffuseTexture;
uniform float shininess;
uniform int useLighting;

out vec4 fragColor;

in vec4 vertexColor0;
in vec2 uv0;

void main() {
  fragColor = texture(diffuseTexture, uv0) * vertexColor0;

  if (fragColor.a < .95) {
    discard;
  }
}