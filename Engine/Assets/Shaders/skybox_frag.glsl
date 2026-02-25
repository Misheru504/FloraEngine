#version 330 core

const int DEFAULT = 0;
const int POSITION = 1;
const int SKY_MASK = 2;
const int SUN_MASK = 3;
const int HORIZON_MASK = 4;

in vec3 fPos;
in vec3 fViewDir;

out vec4 fColor;

uniform int fSkyboxMode;

uniform float sunSize;
uniform vec3 sunDir;
uniform vec3 sunColor;
uniform vec3 moonColor;
uniform vec3 dayColor;
uniform vec3 nightColor;
uniform vec3 horizonColor;

float getCelestialBodyMask(vec3 bodyDir, float bodySize, float edgeSoftness) {
    vec3 viewDir = normalize(fViewDir);
    vec3 bodyDirNorm = normalize(bodyDir);
    
    float cosAngle = dot(viewDir, bodyDirNorm);
    float angle = acos(clamp(cosAngle, -1.0, 1.0));

    float mask = smoothstep(bodySize + edgeSoftness, bodySize - edgeSoftness, angle);
    
    return mask;
}

float computeHorizonActivation(vec3 dir)
{
    return (-smoothstep(0, 0.2, dir.y) + smoothstep(-0.2, 0, dir.y));
}

void main() {
    vec3 viewDir = normalize(fViewDir);
    fColor = vec4(vec3(0), 1.0);

    float skyMask = smoothstep(-0.1, 0.1, sunDir.y);
    float sunMask = smoothstep(0, 0.05, viewDir.y); // hides sun below horizon
    float horizonActivation = computeHorizonActivation(sunDir);
    float horizonMask = pow(smoothstep(0.3, 0.0, viewDir.y),2);
    
    fColor = mix(vec4(nightColor, 1), vec4(dayColor, 1), skyMask);
    fColor = mix(fColor, vec4(horizonColor, 1), horizonMask * horizonActivation);
    
    float sunCelestialMask = getCelestialBodyMask(sunDir, sunSize, sunSize * 0.15);
    fColor = mix(fColor, vec4(sunColor, 1), sunCelestialMask * sunMask);

    float moonCelestialMask = getCelestialBodyMask(-sunDir, sunSize, sunSize * 0.15);
    fColor = mix(fColor, vec4(moonColor, 1), moonCelestialMask * sunMask);

    if(fSkyboxMode == DEFAULT) return;

    switch(fSkyboxMode){
        case POSITION:
            fColor = vec4(vec3(viewDir), 1.0);
            break;
        case SKY_MASK:
            skyMask = smoothstep(-0.1, 0.1, viewDir.y);
            fColor = vec4(vec3(skyMask), 1.0);
            break;
        case SUN_MASK:
            fColor = vec4(vec3(sunMask), 1.0);
            break;
        case HORIZON_MASK:
            horizonActivation = computeHorizonActivation(viewDir);
            fColor = vec4(vec3(horizonMask), 1.0);
            break;
    }
    
    if(viewDir.y < 0.001 && viewDir.y > -0.001){
        fColor = vec4(vec3(1,0,0),1); 
    }
    if(viewDir.z < 0.001 && viewDir.z > -0.001){
        fColor = vec4(vec3(0,1,0),1); 
    }
    if(viewDir.x < 0.001 && viewDir.x > -0.001){
        fColor = vec4(vec3(0,0,1),1); 
    }
}
