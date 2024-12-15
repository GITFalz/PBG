#version 330 core

flat in int TexIndex;

void main() {
    
    if (TexIndex == 0)
        gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
    else if (TexIndex == 1)
        gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
    else if (TexIndex == 2)
        gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}
