using System;
using FloraEngine.Core;
using FloraEngine.Rendering.Shaders;
using Silk.NET.OpenGL;
using System.Numerics;

namespace FloraEngine.Rendering;

public class Skybox : IDisposable
{
    private readonly GL _graphics;

    private VertexArrayObject _vao = null!;
    private BufferObject<float> _vbo = null!;
    private readonly SkyboxConfig _skyboxConfig;
    private readonly ShaderWatcher _shaderWatcher;
    private float _time = 12;
    private float _sunAngle = 0;

    public Vector3 SunDirection { get; private set; } = Vector3.Zero;
    
    public Skybox(GL graphics, SkyboxConfig skyboxConfig, ShaderWatcher shaderWatcher)
    {
        _graphics = graphics;
        _skyboxConfig = skyboxConfig;
        _shaderWatcher = shaderWatcher;
        InitializeBuffers();
        shaderWatcher.RegisterFragVertShader("skybox");
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

    public void Render(double deltaTime, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
    {
        const float SUN_SIZE = 0.05f;
        Vector3 sunColor = new Vector3(255f, 255f, 179f) / 255f;
        Vector3 moonColor = new Vector3(204f) / 255f;

        Vector3 dayColor = new Vector3(119f, 181f, 254f) / 255f;
        Vector3 nightColor = new Vector3(17f, 24f, 38f) / 255f;
        Vector3 horizonColor = new Vector3(230f, 76f, 0) / 255f;

        _sunAngle = (_time / 24.0f) * MathF.PI * 2.0f - MathF.PI / 2.0f;
        SunDirection = new Vector3(
            0.0f,
            MathF.Sin(_sunAngle),
            MathF.Cos(_sunAngle)
        );
        _time += (float)deltaTime;
        _time = _time % 24;

        _graphics.DepthFunc(DepthFunction.Lequal);
        _vao.Bind();
        
        FragVertShader shader = _shaderWatcher.GetShader("skybox");
        shader.UseProgram();
        shader.SetUniform("view", viewMatrix);
        shader.SetUniform("projection", projectionMatrix);

        shader.SetUniform("fSkyboxMode", (int) _skyboxConfig.SkyboxMode);

        shader.SetUniform("sunSize", SUN_SIZE);
        shader.SetUniform("sunDir", SunDirection);
        shader.SetUniform("sunColor", sunColor);
        shader.SetUniform("moonColor", moonColor);
        shader.SetUniform("dayColor", dayColor);
        shader.SetUniform("nightColor", nightColor);
        shader.SetUniform("horizonColor", horizonColor);

        _graphics.DrawArrays(PrimitiveType.Triangles, 0, 36);
        _graphics.BindVertexArray(0);
        _graphics.DepthFunc(DepthFunction.Less);
    }

    public void Dispose()
    {
        _vbo.Dispose();
        _vao.Dispose();
    }
}
