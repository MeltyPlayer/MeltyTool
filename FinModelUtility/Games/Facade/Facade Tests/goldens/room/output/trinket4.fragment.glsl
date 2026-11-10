#version 450
precision mediump float;

uniform sampler2D diffuseTexture;

out vec4 fragColor;

in vec2 uv0;

void main() {
  fragColor = texture(diffuseTexture, uv0);

  if (fragColor.a < .95) {
    discard;
  }
}