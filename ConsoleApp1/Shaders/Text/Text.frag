#version 330 core

in vec2 TexCoord;
flat in ivec2 Length;

out vec4 FragColor;

uniform sampler2D texture0;
uniform isamplerBuffer charBuffer;

const float charSize = 1.0 / 20.0;

void main()
{
    float sizeX = Length.x;
    float sizeY = 1;

    int index = int(TexCoord.x * Length.x) + Length.y;
    int char = texelFetch(charBuffer, index).r;

    if (TexCoord.x > (1 / sizeX) * Length.x || TexCoord.y > (1 / sizeY) || char == -1)
        discard;

    vec2 uv = mod(vec2(TexCoord.x * sizeX, TexCoord.y * sizeY), 1.0);
    
    float xOffset = mod(char, 20);
    float yOffset = int(char / 20);

    
    vec2 atlasUV = vec2(
    ((uv.x / 20)) + charSize * xOffset,
    -(uv.y / 20) - charSize * yOffset
    );

    vec4 texColor = texture(texture0, atlasUV);

    FragColor = texColor;
}