#version 310 es
precision highp float;

uniform sampler2D texture0;
uniform sampler2D texture1;
uniform vec3 color_3dsColor2;
uniform vec3 color_3dsColor3;
uniform float scalar_3dsAlpha1;

in vec4 vertexColor0;
in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = clamp(clamp((color_3dsColor3 + vec3(texture(texture0, uv0).rgb.g)), 0.0, 1.0)*clamp(clamp((color_3dsColor2*vec3(1.0 + -1.0*vertexColor0.rgb.r*vertexColor0.rgb.r*2.0) + clamp(vertexColor0.rgb, 0.0, 1.0)), 0.0, 1.0)*texture(texture1, uv0).rgb, 0.0, 1.0), 0.0, 1.0);

  float alphaComponent = vertexColor0.a*texture(texture1, uv0).a*scalar_3dsAlpha1;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.4980392)) {
    discard;
  }
}