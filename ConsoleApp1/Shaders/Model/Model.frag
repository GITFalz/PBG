#version 330 core

flat in int TexIndex;
in vec2 TexCoord;

const float borderThickness = 2.0;

void main() {
    vec2 uvPixelSize = fwidth(TexCoord);

    vec2 borderWidth = uvPixelSize * borderThickness;

    if (TexCoord.x < borderWidth.x || TexCoord.x > 1.0 - borderWidth.x ||
    TexCoord.y < borderWidth.y || TexCoord.y > 1.0 - borderWidth.y) {

        gl_FragColor = vec4(0.0, 0.0, 0.0, 1.0);
    } else {

        if (TexIndex == 0)
        gl_FragColor = vec4(0.0, 1.0, 0.0, 1.0);
        else if (TexIndex == 1)
        gl_FragColor = vec4(0.0, 0.0, 1.0, 1.0);
        else if (TexIndex == 2)
        gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
    }
}
