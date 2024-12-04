#version 430

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0);

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}