#version 460 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in int texIndex;
layout (location = 3) in vec2 aSize;
layout (location = 4) in int transformIndex;

layout(std430, binding = 0) buffer TransformBuffer {
    mat4 transformMatrices[];
};

out vec2 TexCoord;
flat out int TexIndex;
out vec2 TexSize;

uniform mat4 model;
uniform mat4 projection;

void main()
{
    vec4 transformedPosition = transformMatrices[transformIndex] * vec4(aPos, 1.0);

    gl_Position = transformedPosition * model * projection;
    TexCoord = aTexCoord;
    TexIndex = texIndex;
    TexSize = aSize;
}