#version 330 core

in vec2 UV;
in vec3 Normal;

out vec3 Color;

void main()
{
	Color = normalize(Normal) * 0.5f + 0.5f;
}