#version 460 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 vColor;

void main()
{
    gl_Position = vec4(aPosition, 1.0) * model * view * projection;
    gl_PointSize = 10.0;
    vColor = aColor;
}