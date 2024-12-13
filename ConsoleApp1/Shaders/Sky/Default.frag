#version 330 core
in vec2 TexCoord;
out vec4 FragColor;

const int tileCount = 2;
const float tileSize = 1.0 / tileCount;

void main()
{
    float t = TexCoord.y * 0.5 + 0.5;
    vec3 topColor = vec3(0.529, 0.808, 0.922);
    vec3 bottomColor = vec3(0.8, 0.8, 0.8);
    vec4 texColor = vec4(mix(bottomColor, topColor, t), 1.0);

    FragColor = texColor;
}