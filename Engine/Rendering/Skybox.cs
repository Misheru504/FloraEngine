using FloraEngine.Core;
using FloraEngine.Rendering.Shaders;
using Silk.NET.OpenGL;
using System.Numerics;

namespace FloraEngine.Rendering;

public class Skybox
{
    private readonly GL _graphics;

    private VertexArrayObject _vao = null!;
    private BufferObject<float> _vbo = null!;
    private readonly SkyboxConfig _skyboxConfig;
    private readonly FragVertShader _shader = null!;

    private const string VERTEX_SHADER = @"
#version 330 core
layout (location = 0) in vec3 vPos;

uniform mat4 projection;
uniform mat4 view;

out vec3 fPos;
out vec3 fViewDir;

void main()
{
    fPos = vPos;
    fViewDir = normalize(vPos);

    vec4 pos = projection * view * vec4(vPos, 1.0);
    gl_Position = pos.xyww;
}
";

    private const string FRAGMENT_SHADER = @"
#version 330 core

const int DEFAULT = 0;
const int POSITION = 1;
const int SKY_MASK = 2;
const int SUN_MASK = 3;

in vec3 fPos;
in vec3 fViewDir;

out vec4 fColor;

uniform int fSkyboxMode;

uniform float sunSize;
uniform vec3 sunDir;
uniform vec3 dayColor;
uniform vec3 nightColor;

bool isCelestialBody(vec3 bodyDir, float bodySize)
{
    float angle = step(bodySize, acos(dot(normalize(fViewDir), normalize(bodyDir))));
    return angle < bodySize;
}
void main() {
    vec3 viewDir = normalize(fViewDir);
    fColor = vec4(vec3(0), 1.0);

    float skyMask = smoothstep(-0.1, 0.1, sunDir.y);
    float sunMask = smoothstep(0, 0.05, viewDir.y); // hides sun below horizon
    
    fColor = mix(vec4(nightColor, 1), vec4(dayColor, 1), skyMask);

    if (isCelestialBody(sunDir, sunSize)){
        fColor = mix(fColor, vec4(vec3(1, 1, 0), 1), sunMask);
    }

    if (isCelestialBody(-sunDir, sunSize)){
        fColor = mix(fColor, vec4(1), sunMask);
    }

    switch(fSkyboxMode){
        default:
        case DEFAULT:
            break;
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
    }
}
";

    public Skybox(GL graphics, SkyboxConfig skyboxConfig)
    {
        _graphics = graphics;
        _skyboxConfig = skyboxConfig;
        InitializeBuffers();
        _shader = new FragVertShader(_graphics, VERTEX_SHADER, FRAGMENT_SHADER);
    }

    private void InitializeBuffers()
    {
        float[] skyboxVertices = {
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            -1.0f,  1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f
        };

        _vao = new VertexArrayObject(_graphics);
        _vbo = new BufferObject<float>(_graphics, skyboxVertices, BufferTargetARB.ArrayBuffer, BufferUsageARB.DynamicDraw);

        VertexArrayObject.VertexAttributePointer<float>(0, 3, VertexAttribPointerType.Float, 3, 0);
        _graphics.BindVertexArray(0);
    }

    private float time = 12;
    private float sunAngle = 0;

    public Vector3 SunDirection { get; private set; } = Vector3.Zero;

    public void Render(double deltaTime, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        const float SUN_SIZE = 0.05f;
        Vector3 dayColor = new Vector3(100f, 149f, 237f) / 255f;
        Vector3 nightColor = new Vector3(17f, 24f, 38f) / 255f;

        sunAngle = (time / 24.0f) * MathF.PI * 2.0f - MathF.PI / 2.0f;
        SunDirection = new Vector3(
            0.0f,
            MathF.Sin(sunAngle),
            MathF.Cos(sunAngle)
        );
        time += (float)deltaTime;
        time = time % 24;

        _graphics.DepthFunc(DepthFunction.Lequal);
        _vao.Bind();
        _shader.UseProgram();
        _shader.SetUniform("view", viewMatrix);
        _shader.SetUniform("projection", projectionMatrix);

        _shader.SetUniform("fSkyboxMode", (int) _skyboxConfig.SkyboxMode);

        _shader.SetUniform("sunSize", SUN_SIZE);
        _shader.SetUniform("sunDir", SunDirection);
        _shader.SetUniform("dayColor", dayColor);
        _shader.SetUniform("nightColor", nightColor);

        _graphics.DrawArrays(PrimitiveType.Triangles, 0, 36);
        _graphics.BindVertexArray(0);
        _graphics.DepthFunc(DepthFunction.Less);
    }
}
