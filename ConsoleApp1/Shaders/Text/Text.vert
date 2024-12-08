#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec2 aTexCoord;
layout(location = 2) in ivec2 aLength;

uniform mat4 projection;

out vec2 TexCoord;
flat out ivec2 Length;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * projection;
    TexCoord = aTexCoord;
    Length = aLength;
}