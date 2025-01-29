#version 460 core

in vec2 TexCoord;
in flat int TextureIndex;

out vec4 FragColor;

void main()
{
    FragColor = vec4(0.2, 0.2, 0.3, 1);
}