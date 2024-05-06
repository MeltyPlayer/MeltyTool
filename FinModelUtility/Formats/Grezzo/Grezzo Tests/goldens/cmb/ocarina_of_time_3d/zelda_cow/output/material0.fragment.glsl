#version 400

uniform sampler2D texture0;
uniform sampler2D texture1;

in vec2 normalUv;
in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = (vertexColor0.rgb*texture(texture0, uv0).rgb*vec3(2) + texture(texture1, asin(normalUv) / 3.14159 + 0.5).rgb)*vec3(0.49803921580314636);

  float alphaComponent = vertexColor0.a*texture(texture0, uv0).a;

  fragColor = vec4(colorComponent, alphaComponent);
}
