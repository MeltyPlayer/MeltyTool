#version 400


out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(0.062745101749897);

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
