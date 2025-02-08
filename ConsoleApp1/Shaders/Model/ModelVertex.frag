#version 460 core
out vec4 FragColor;

in vec3 vColor;

void main()
{
    vec2 circCoord = 2.0 * gl_PointCoord - 1.0;
    float dist = length(circCoord);
    
    if(dist > 1.0)
        discard;
    
    float alpha = 1.0 - smoothstep(0.8, 1.0, dist);
    
    FragColor = vec4(vColor, alpha);
}