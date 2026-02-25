#version 330 core

const int DEFAULT = 0;
const int DEPTH = 1;
const int NORMAL = 2;
const int UV = 3;
const int AO = 4;
const int LAYER = 5;

in vec2 fUV;
in vec3 fNormal;
in float fAO;
in float fTextureLayer;

uniform sampler2DArray fTexture;
uniform int fRenderMode;
uniform float fAmbientLight;
uniform vec3 fSunDir;

out vec4 fragColor;

vec3 hashColor(float n) {
    // Pseudo-random hash that gives consistent colors per layer
    vec3 p = vec3(n * 0.1031, n * 0.1030, n * 0.0973);
    p = fract(p * vec3(127.1, 311.7, 74.7));
    p += dot(p, p.yzx + 33.33);
    vec3 color = fract((p.xxy + p.yzz) * p.zyx);
    return mix(color, vec3(1.0), 0.5);
}

void main()
{
    vec3 normal = normalize(fNormal);
    vec3 light = normalize(fSunDir);
    float diff = max(dot(normal, light), 0.0);

    float directional = (1.0 - fAmbientLight) * diff;

    // Apply AO to both ambient and slightly to directional lighting
    float aoFactor = fAO;
    float aoAmbient = fAmbientLight * (0.5 + 0.5 * aoFactor);  // AO affects ambient more
    float aoDirectional = directional * (0.7 + 0.3 * aoFactor);  // AO affects directional less

    float lighting = aoAmbient + aoDirectional;

    vec4 texColor = texture(fTexture, vec3(fUV, fTextureLayer));
    
    switch(fRenderMode){
        default:
        case DEFAULT:
            fragColor = vec4(texColor.xyz * lighting, texColor.w);
            break;
        case DEPTH:
            float near = 0.1;
            float far = 1000.0;
            float ndc = gl_FragCoord.z * 2.0 - 1.0; 
            float linearDepth = (2.0 * near * far) / (far + near - ndc * (far - near));
            fragColor = vec4(vec3(linearDepth / far), texColor.w);
            break;
        case NORMAL:
            fragColor = vec4(normalize(fNormal) * 0.5 + 0.5, 1.0);
            break;
        case UV:
            fragColor = vec4(fUV.x, fUV.y, 0, texColor.w);
            break;
        case AO:
            fragColor = vec4(vec3(fAO), 1.0);
            break;
        case LAYER:
            fragColor = vec4(hashColor(fTextureLayer), 1.0);
            break;
    }
}