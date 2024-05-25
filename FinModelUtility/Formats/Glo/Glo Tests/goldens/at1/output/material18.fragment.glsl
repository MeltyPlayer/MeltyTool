#version 400

uniform sampler2D diffuseTexture;
uniform sampler2D ambientOcclusionTexture;
uniform sampler2D emissiveTexture;
uniform float shininess;
uniform float useLighting;

out vec4 fragColor;

in vec4 vertexColor0;
in vec2 uv0;

void main() {
  vec4 diffuseColor = texture(diffuseTexture, uv0);
  vec4 ambientOcclusionColor = texture(ambientOcclusionTexture, uv0);
  vec4 emissiveColor = texture(emissiveTexture, uv0);

  fragColor = diffuseColor * vertexColor0;
  fragColor.rgb += emissiveColor.rgb;
  fragColor.rgb = min(fragColor.rgb, 1);
}
