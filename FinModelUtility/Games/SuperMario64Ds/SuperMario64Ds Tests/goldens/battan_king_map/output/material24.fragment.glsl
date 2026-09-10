#version 450
precision mediump float;

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(1.0,0.9686275,0.0)*vertexColor0.rgb;

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.01)) {
    discard;
  }
}