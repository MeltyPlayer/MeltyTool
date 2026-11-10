#version 450
precision mediump float;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vertexColor0.rgb;

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, 1);

  if (!(alphaComponent > 0.01)) {
    discard;
  }
}