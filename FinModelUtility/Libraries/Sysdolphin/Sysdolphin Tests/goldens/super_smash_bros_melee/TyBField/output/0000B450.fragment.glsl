#version 310 es
precision highp float;

struct Light {
  // 0x00 (vec3 needs to be 16-byte aligned)
  vec3 position;
  bool enabled;

  // 0x10 (vec3 needs to be 16-byte aligned)
  vec3 normal;
  int sourceType;

  // 0x20 (vec4 needs to be 16-byte aligned)
  vec4 color;
  
  // 0x30 (vec3 needs to be 16-byte aligned)
  vec3 cosineAttenuation;
  int diffuseFunction;

  // 0x40 (vec3 needs to be 16-byte aligned)
  vec3 distanceAttenuation;
  int attenuationFunction;
};

layout (std140, binding = 2) uniform Lights {
  Light lights[8];
  vec4 ambientLightColor;
  float useLighting;
};

uniform vec3 cameraPosition;
uniform float shininess;
in vec4 vertexColor0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = (vec3(0.501960813999)*ambientLightColor.rgb + vec3(1.0))*vertexColor0.rgb;

  float alphaComponent = 0.5*vertexColor0.a;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}