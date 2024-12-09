#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in int texIndex;

out vec2 TexCoord;
out vec2 Size;
out vec3 ViewPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

const int tileCount = 2;
const float tileSize = 1.0 / tileCount;

void main()
{
    gl_Position = vec4(aPos, 1.0) * model * view * projection;
    
    TexCoord = aTexCoord;
    Size = vec2(texIndex % tileCount, texIndex / tileCount) * tileSize;
    ViewPos = (view * model * vec4(aPos, 1.0)).xyz;
}