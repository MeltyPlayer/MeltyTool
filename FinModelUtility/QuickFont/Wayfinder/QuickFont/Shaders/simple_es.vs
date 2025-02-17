#version 100
precision mediump float;

uniform mat4 proj_matrix;
uniform mat4 modelview_matrix;

attribute vec3 in_position;
attribute vec2 in_tc;
attribute vec4 in_colour;

varying vec2 tc;
varying vec4 colour;

void main(void)
{
	tc = in_tc;
	colour = in_colour;
	gl_Position = proj_matrix * modelview_matrix * vec4(in_position, 1.);
}
