
uniform mat4 proj_matrix;
uniform mat4 modelview_matrix;

in vec3 in_position;
in vec2 in_tc;
in vec4 in_colour;

out vec2 tc;
out vec4 colour;

void main(void)
{
	tc = in_tc;
	colour = in_colour;
	gl_Position = proj_matrix * modelview_matrix * vec4(in_position, 1.); 
}
