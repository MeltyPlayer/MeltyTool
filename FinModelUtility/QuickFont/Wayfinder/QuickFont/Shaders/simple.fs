
uniform sampler2D tex_object;

in vec2 tc;
in vec4 colour;

out vec4 fragColour;

void main(void)
{
	fragColour = texture(tex_object, tc) * vec4(colour);
}