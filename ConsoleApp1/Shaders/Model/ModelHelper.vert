#version 330 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in int texIndex;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

flat out int TexIndex;
out vec2 TexCoord;

void main()
{
    vec4 transformedPos = vec4(aPos, 1.0) * model * view * projection;
    gl_Position = transformedPos;
    TexIndex = texIndex;
    TexCoord = aTexCoord;
}