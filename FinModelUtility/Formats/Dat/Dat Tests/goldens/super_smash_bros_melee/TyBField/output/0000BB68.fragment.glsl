#version 400


out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*vec3(1,0.5490196347236633,0);

  float alphaComponent = 1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
