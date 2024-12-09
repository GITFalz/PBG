#version 330 core

in vec2 TexCoord;
in vec2 Size;
in vec3 ViewPos;

out vec4 FragColor;

uniform sampler2D texture0;
uniform sampler2D diffuse0;
uniform sampler2D specular0;
uniform vec3 camPos;

const int tileCount = 2;
const float tileSize = 1.0 / tileCount;

const float near = 0.1f;
const float far = 1000.0f;

vec4 direcLight()
{
    // ambient lighting
    float ambient = 0.20f;

    // diffuse lighting
    vec3 normal = normalize(vec3(0.0f, 1.0f, 0.0f));
    vec3 lightDirection = normalize(vec3(1.0f, 1.0f, 0.0f));
    float diffuse = max(dot(normal, lightDirection), 0.0f);

    // specular lighting
    float specularLight = 0.50f;
    vec3 viewDirection = normalize(camPos - ViewPos);
    vec3 reflectionDirection = reflect(-lightDirection, normal);
    float specAmount = pow(max(dot(viewDirection, reflectionDirection), 0.0f), 16);
    float specular = specAmount * specularLight;

    return (texture(diffuse0, TexCoord) * (diffuse + ambient) + texture(specular0, TexCoord).r * specular) * vec4(1.0f, 0.0f, 0.0f, 1.0f);
}


float linearizeDepth(float depth) {
    return (2.0 * near * far) / (far + near - (depth * 2.0 - 1.0) * (far - near));
}

float logisticDepth(float depth, float steepness, float offset) {
    float zVal = linearizeDepth(depth);
    return (1 / (1 + exp(-steepness * (zVal - offset))));
}

void main()
{
    /*
    float depth = logisticDepth(gl_FragCoord.z, 0.01, 100);
    //FragColor = vec4(depth, depth, depth, 1.0);
    
    if (depth > 0.9)
        discard;
        */
    
    float modX = mod(TexCoord.x, 1.0);
    float modY = mod(TexCoord.y, 1.0);
    
    vec2 texCoord = vec2(modX * tileSize + Size.x, 1 - modY * tileSize - Size.y);
    
    vec4 texColor = texture(texture0, texCoord);
    
    if (texColor.a < 0.01)
        discard;
    
    FragColor = texColor;
}