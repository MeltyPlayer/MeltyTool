#version 450
precision mediump float;

uniform vec4 diffuseColor;

out vec4 fragColor;

void main() {
  fragColor = diffuseColor;
}