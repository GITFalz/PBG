#version 330 core

flat in int TexIndex;
in vec2 TexCoord;

const float borderThickness = 2.0;

uniform float colorAlpha;

void main() {
    vec2 uvPixelSize = fwidth(TexCoord);

    vec2 borderWidth = uvPixelSize * borderThickness;

    if (TexIndex == 0)
    gl_FragColor = vec4(1.0, 1.0, 1.0, colorAlpha);
    else if (TexIndex == 1)
    gl_FragColor = vec4(0.0, 0.0, 1.0, colorAlpha);
    else if (TexIndex == 2)
    gl_FragColor = vec4(1.0, 0.0, 0.0, colorAlpha);
}
