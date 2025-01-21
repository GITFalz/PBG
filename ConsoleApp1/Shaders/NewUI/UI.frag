#version 330 core

in vec2 TexCoord;
flat in int TexIndex;
in vec2 TexSize;

out vec4 FragColor;

uniform sampler2DArray textureArray;

const float cellSize = 10;
const float mapSize = 0.15;

vec2 uvCorner(vec2 uv, float a, float b)
{
    uv.x = uv.x / a * mapSize;
    uv.y = uv.y / b * mapSize;
    return uv;
}

float mapRange(float a, float b, float value, float map) {
    if (a == b) return map;
    value = clamp(value, a, b);
    float normalized = (value - a) / (b - a);
    return mix(map, 1.0 - map, normalized);
}

void main()
{
    vec2 uv = TexCoord;
    
    float x1 = cellSize / TexSize.x;
    float x2 = 1.0 - x1;
    
    float y1 = cellSize / TexSize.y;
    float y2 = 1.0 - y1;
    
    bool x1b = uv.x < x1;
    bool x2b = uv.x > x2;
    
    bool y1b = uv.y < y1;
    bool y2b = uv.y > y2;
    
    if (x1b || x2b || y1b || y2b) {
        // Bottom left
        if (x1b && y1b) 
        {
            uv = uvCorner(uv, x1, y1);
        }
        // Top left
        else if (x1b && y2b) 
        {
            uv.y -= y2;
            uv = uvCorner(uv, x1, y1);
            uv.y = mapSize - uv.y;
        }
        // Bottom right
        else if (x2b && y1b) 
        {
            uv.x -= x2;
            uv = uvCorner(uv, x1, y1);
            uv.x = mapSize - uv.x;
        }
        // Top right
        else if (x2b && y2b) 
        {
            uv.x -= x2;
            uv.y -= y2;
            uv = uvCorner(uv, x1, y1);
            uv.x = mapSize - uv.x;
            uv.y = mapSize - uv.y;
        }
        else if (x1b) 
        {
            uv.y = mapRange(y1, y2, uv.y, mapSize);
            uv.x = uv.x / x1 * mapSize;
        }
        else if (x2b) 
        {
            uv.x -= x2;
            uv.y = mapRange(y1, y2, uv.y, mapSize);
            uv.x = mapSize - uv.x / x1 * mapSize;
        }
        else if (y1b) 
        {
            uv.x = mapRange(x1, x2, uv.x, mapSize);
            uv.y = uv.y / y1 * mapSize;
        }
        else if (y2b)
        {
            uv.y -= y2;
            uv.x = mapRange(x1, x2, uv.x, mapSize);
            uv.y = mapSize - uv.y / y1 * mapSize;
        }
        else
        {
            uv.x = mapRange(x1, x2, uv.x, mapSize);
            uv.y = mapRange(y1, y2, uv.y, mapSize);
        }
    }
    else
    {
        uv.x = mapRange(x1, x2, uv.x, mapSize);
        uv.y = mapRange(y1, y2, uv.y, mapSize);
    }

    vec4 texColor = texture(textureArray, vec3(uv.xy, TexIndex));
    FragColor = vec4(texColor.rgb, 1.0);

    //FragColor = vec4(uv.x, uv.y, 0.0, 1.0);
    //FragColor = texture(textureArray, vec3(uv.xy, TexIndex));
    //FragColor = vec4(0, 0, 0, 1.0);
}