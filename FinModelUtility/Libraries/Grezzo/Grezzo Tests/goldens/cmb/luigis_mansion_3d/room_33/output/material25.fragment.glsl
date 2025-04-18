#version 310 es
precision highp float;

uniform sampler2D texture0;
uniform vec3 color_3dsColor0;
uniform vec3 color_3dsColor3;
uniform float scalar_3dsAlpha0;
uniform float scalar_3dsAlpha1;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(clamp((color_3dsColor3 + vec3(texture(texture0, uv0).rgb.g)), 0.0, 1.0)*clamp(vertexColor0.rgb*clamp(color_3dsColor0, 0.0, 1.0)*vec3(2.0), 0.0, 1.0), 0.0, 1.0);

  float alphaComponent = scalar_3dsAlpha0*scalar_3dsAlpha1;

  fragColor = vec4(colorComponent, alphaComponent);
}