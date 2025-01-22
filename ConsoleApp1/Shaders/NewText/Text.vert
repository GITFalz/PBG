#version 460 core

layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in ivec2 aLength;
layout(location = 3) in int transformIndex;

layout(std430, binding = 0) buffer TransformBuffer {
    mat4 transformMatrices[];
};

uniform mat4 model;
uniform mat4 projection;

out vec2 TexCoord;
flat out ivec2 Length;

void main()
{
    vec4 transformedPosition = transformMatrices[transformIndex] * vec4(aPosition, 1.0);

    gl_Position = transformedPosition * model * projection;
    TexCoord = aTexCoord;
    Length = aLength;
}