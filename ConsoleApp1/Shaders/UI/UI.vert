#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in int texIndex;
layout (location = 3) in vec2 aSize;

out vec2 TexCoord;
flat out int TexIndex;
out vec2 TexSize;

uniform mat4 projection;

void main()
{
    gl_Position = vec4(aPos, 1.0) * projection;
    TexCoord = aTexCoord;
    TexIndex = texIndex;
    TexSize = aSize;
}