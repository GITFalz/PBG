#version 460 core
layout (location = 0) in vec3 aPos;
layout (location = 1) in vec2 aTexCoord;
layout (location = 2) in int texIndex;
layout (location = 3) in vec3 aNormal;
layout (location = 4) in ivec4 inBoneIndices;

layout(std430, binding = 0) buffer BoneBuffer {
    mat4 boneMatrices[];
};

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

flat out int TexIndex;
out vec2 TexCoord;
out vec3 Normal;

void main()
{
    vec4 transformedPosition = vec4(0.0);

    for (int i = 0; i < 4; ++i) {
        int boneIndex = inBoneIndices[i];
        if (boneIndex >= 0)
        {
            transformedPosition += (boneMatrices[boneIndex] * vec4(aPos, 1.0));
        }
    }

    gl_Position = transformedPosition * model * view * projection;
    TexIndex = texIndex;
    TexCoord = aTexCoord;
    Normal = aNormal;
}