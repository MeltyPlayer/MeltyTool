#version 400

uniform sampler2D texture0;
uniform float scalar_3dsAlpha1;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = texture(texture0, uv0).rgb;

  float alphaComponent = vertexColor0.a*scalar_3dsAlpha1;

  fragColor = vec4(colorComponent, alphaComponent);
}
