#version 330 core

in vec2 TexCoord;
flat in int TexIndex;

out vec4 FragColor;

uniform sampler2DArray textureArray;

void main()
{
    FragColor = texture(textureArray, vec3(TexCoord.xy, TexIndex));
    //FragColor = vec4(1.0, 1.0, 1.0, 1.0);
}