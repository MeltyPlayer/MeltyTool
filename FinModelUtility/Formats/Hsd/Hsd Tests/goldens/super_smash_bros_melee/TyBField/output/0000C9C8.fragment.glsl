#version 310 es
precision mediump float;


struct Texture {
  sampler2D sampler;
  mat4 transform3d;
};

vec2 transformUv3d(mat4 transform3d, vec2 inUv) {
  vec4 rawTransformedUv = (transform3d * vec4(inUv, 0, 1));

  // We need to manually divide by w for perspective correction!
  return rawTransformedUv.xy / rawTransformedUv.w;
}

uniform Texture texture0;

in vec2 uv0;

out vec4 fragColor;

void main() {
  vec3 colorComponent = vec3(2.0)*texture(texture0.sampler, transformUv3d(texture0.transform3d, uv0)).rgb;

  float alphaComponent = 1.0;

  fragColor = vec4(colorComponent, alphaComponent);

  if (!(alphaComponent > 0.0)) {
    discard;
  }
}