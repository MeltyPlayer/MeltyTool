#version 400

uniform sampler2D texture0;

in vec2 normalUv;
in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2)*vertexColor0.rgb*vec3(0.5) + texture(texture0, asin(normalUv) / 3.14159 + 0.5).rgb*vec3(0.5);

  float alphaComponent = 0.3499999940395355*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0)) {
    discard;
  }
}
