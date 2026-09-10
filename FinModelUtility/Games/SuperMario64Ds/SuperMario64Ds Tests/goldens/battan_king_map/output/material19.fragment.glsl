#version 450
precision mediump float;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.8078431,0.7764706,0.0)*vertexColor0.rgb;

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.01)) {
    discard;
  }
}