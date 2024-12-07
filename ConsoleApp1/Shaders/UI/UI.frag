#version 330 core

in vec2 TexCoord;

out vec4 FragColor;

uniform sampler2D texture0;

void main()
{
    vec4 texColor = texture(texture0, TexCoord);

    if (texColor.a < 0.01)
    discard;

    FragColor = texColor;
}