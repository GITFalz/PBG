#version 460 core

in vec2 TexCoord;
flat in int TexIndex;
in vec2 TexSize;
in vec2 Slice;

out vec4 FragColor;

uniform sampler2DArray textureArray;

// Remaps UVs for the corners
vec2 uvCorner(vec2 uv, vec2 offset, vec2 scale, bool flipX, bool flipY, float mapSize) {
    uv = (uv - offset) / scale * mapSize;
    if (flipX) uv.x = mapSize - uv.x;
    if (flipY) uv.y = mapSize - uv.y;
    return uv;
}

// Remaps UVs for the edges and center stretching
float mapRange(float a, float b, float value, float map) {
    if (a == b) return map;
    value = clamp(value, a, b);
    return mix(map, 1.0 - map, (value - a) / (b - a));
}

void main() {
    if (TexIndex >= 0)
    {
        const float cellSize = Slice.x;
        const float mapSize = Slice.y;

        if (cellSize <= 0 && mapSize <= 0)
        {
            FragColor = texture(textureArray, vec3(TexCoord, TexIndex));
        }
        else
        {
            vec2 uv = TexCoord;
            vec2 uvMod = uv;

            // Compute border positions in UV space
            vec2 border = vec2(cellSize) / TexSize;
            vec2 innerStart = border;
            vec2 innerEnd = vec2(1.0) - border;

            // Generate masks for each section
            bool left   = uv.x < innerStart.x;
            bool right  = uv.x > innerEnd.x;
            bool bottom = uv.y < innerStart.y;
            bool top    = uv.y > innerEnd.y;

            bool isCorner = (left || right) && (top || bottom);
            bool isEdge   = (left || right || top || bottom) && !isCorner;

            if (isCorner) {
                bool flipX = right;
                bool flipY = top;
                vec2 offset = vec2(left ? 0.0 : innerEnd.x, bottom ? 0.0 : innerEnd.y);
                vec2 scale = border;
                uvMod = uvCorner(uv, offset, scale, flipX, flipY, mapSize);
            } 
            else if (isEdge) {
                if (left || right) {
                    uvMod.x = mapSize * (uv.x - (left ? 0.0 : innerEnd.x)) / border.x;
                    uvMod.y = mapRange(innerStart.y, innerEnd.y, uv.y, mapSize);
                    if (right) uvMod.x = mapSize - uvMod.x;
                } 
                else if (top || bottom) {
                    uvMod.x = mapRange(innerStart.x, innerEnd.x, uv.x, mapSize);
                    uvMod.y = mapSize * (uv.y - (bottom ? 0.0 : innerEnd.y)) / border.y;
                    if (top) uvMod.y = mapSize - uvMod.y;
                }
            } 
            else { 
                // Center region (stretches normally minus border)
                uvMod.x = mix(mapSize, 1.0 - mapSize, uv.x);
                uvMod.y = mix(mapSize, 1.0 - mapSize, uv.y);
            }

            // Sample texture from texture array
            FragColor = texture(textureArray, vec3(uvMod, TexIndex));
        }
    }
    else
    {
        FragColor = vec4(0.0);
    }
}