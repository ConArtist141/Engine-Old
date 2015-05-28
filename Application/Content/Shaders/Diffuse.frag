#version 330 core

in vec2 UV;
in vec3 Normal;

out vec3 Color;

uniform sampler2D Diffuse;

void main()
{
	Color = texture(Diffuse, UV).rgb;
}