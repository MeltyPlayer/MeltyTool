#version 400


out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.501960813999176);

  float alphaComponent = 0.75;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
