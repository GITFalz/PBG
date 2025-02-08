#version 460 core

out vec4 FragColor;

uniform vec3 edgeColor;

void main()
{
    FragColor = vec4(edgeColor, 1.0);
}