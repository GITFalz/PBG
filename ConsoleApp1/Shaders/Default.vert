#version 330 core
layout (location = 0) in vec3 aPos; // vertex coordinates
layout (location = 1) in vec2 aTexCoord; // vertex color
layout (location = 2) in int texIndex; // texture index

out vec2 TexCoord; // color
out int TexIndex; // texture index

// uniform variables
uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = vec4(aPos, 1.0) * model * view * projection; // coordinates
    TexCoord = aTexCoord; // color
    TexIndex = texIndex; // texture index
}