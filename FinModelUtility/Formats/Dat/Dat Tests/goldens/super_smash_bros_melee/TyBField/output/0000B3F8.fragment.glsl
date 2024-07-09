#version 400

in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*vec3(0.7019608020782471)*vertexColor0.rgb;

  float alphaComponent = vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
