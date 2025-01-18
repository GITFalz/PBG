#version 460 core

flat in int TexIndex;
in vec2 TexCoord;
in vec3 Normal;

const vec3 lightDirection = normalize(vec3(1.7, 2.0, 1.3));

uniform float colorAlpha;

void main() {
    vec3 color = vec3(1.0, 1.0, 1.0);
    vec3 normal = normalize(Normal);
    float light = dot(normal, lightDirection);
    light = clamp(light, 0.4, 1.0);
    gl_FragColor = vec4(color * light, colorAlpha);
}
