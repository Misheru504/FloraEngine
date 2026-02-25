#version 330 core
layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNormal;
layout (location = 2) in vec2 vUV;
layout (location = 3) in float vAO;
layout (location = 4) in float vTextureLayer;

uniform mat4 uModel; 
uniform mat4 uView;
uniform mat4 uProjection;

out vec2 fUV;
out vec3 fNormal;
out float fAO;
out float fTextureLayer;

void main()
{
    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
    fUV = vUV;
    fNormal = vNormal;
    fAO = vAO;
    fTextureLayer = vTextureLayer;
}