#version 310 es
precision highp float;

uniform sampler2D texture0;
uniform sampler2D texture1;

in vec4 vertexColor0;
in vec2 uv0;
in vec2 uv1;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp((texture(texture0, uv0).rgb*vec3(vertexColor0.a) + texture(texture1, uv1).rgb*(vec3(1.0) + vec3(-1.0)*vec3(vertexColor0.a)))*vertexColor0.rgb, 0.0, 1.0);

  float alphaComponent = (texture(texture0, uv0).a*0.4980392 + texture(texture1, uv1).a*0.5019608)*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);
}