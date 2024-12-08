#version 330 core

in vec2 TexCoord;
flat in int TexIndex;

out vec4 FragColor;

uniform sampler2DArray textureArray;

void main()
{
    vec4 texColor = texture(textureArray, vec3(TexCoord.xy, TexIndex));
    
    if (texColor.a < 0.01)
        discard;
    
    FragColor = texColor;
}