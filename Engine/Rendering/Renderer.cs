using System;
using FloraEngine.Core;
using FloraEngine.Core.Logging;
using FloraEngine.Rendering.Shaders;
using FloraEngine.Rendering.Textures;
using FloraEngine.World;
using Silk.NET.OpenGL;
using System.Numerics;

namespace FloraEngine.Rendering;

public unsafe class Renderer : IDisposable
{

    // Vertex stride: 3 (position) + 3 (normal) + 2 (uv) + 2 (aos) = 10 floats
    public const int VERTEX_STRIDE = 10;

    private readonly GL _graphics;
    private readonly RenderConfig _renderConfig;
    private readonly Camera _camera;
    private readonly WorldManager _worldManager;
    private readonly ShaderWatcher _shaderWatcher;

    private readonly TextureArray _atlas;
    private readonly Skybox _skybox;

    public Renderer(GL graphics, RenderConfig renderConfig, SkyboxConfig skyboxConfig, Camera camera, WorldManager worldManager)
    {
        Logger.Render("Loading renderer...");
        _graphics = graphics;
        _renderConfig = renderConfig;
        _camera = camera;
        _worldManager = worldManager;
        _shaderWatcher = new ShaderWatcher(_graphics);

        _shaderWatcher.RegisterFragVertShader("terrain");

        _atlas = new TextureArray(_graphics, "atlas.png", TextureUnit.Texture0, 16);
        _atlas.SetDefaultParameters();

        Mesh.RenderConfig = _renderConfig;
        Mesh.Graphics = _graphics;

        _skybox = new Skybox(_graphics, skyboxConfig, _shaderWatcher);

        Logger.Render("Successfully loaded!");
    }

    internal void Draw(double deltaTime)
    {
        _shaderWatcher.Update();
        _graphics.PolygonMode(GLEnum.FrontAndBack, _renderConfig.IsWireframe ? GLEnum.Line : GLEnum.Fill);
        _atlas.Bind();
        
        FragVertShader shader = _shaderWatcher.GetShader("terrain");
        shader.UseProgram();
        shader.SetUniform("uView", _camera.RelativeViewMatrix);
        shader.SetUniform("uProjection", _camera.ProjectionMatrix);
        shader.SetUniform("fRenderMode", (int) _renderConfig.RenderMode);
        shader.SetUniform("fTexture", 0);
        shader.SetUniform("fAmbientLight", _renderConfig.IsFullbright ? 1f : 0.3f);
        shader.SetUniform("fSunDir", _skybox.SunDirection);

        _renderConfig.VertexCount = 0;
        foreach(Chunk chunk in _worldManager.RenderedChunks.Values)
            DrawChunk(chunk, shader);

        _skybox.Render(deltaTime, _camera.RelativeViewMatrix, _camera.ProjectionMatrix);
    }

    private void DrawChunk(Chunk chunk, FragVertShader shader)
    {
        if (chunk.Mesh == null || chunk.Mesh.vao == null) return;
        if (!IsInFrustum(chunk, _camera.Frustum)) return;

        _renderConfig.VertexCount += chunk.Mesh.VertexCount;
        chunk.Mesh.vao.Bind();
        shader.SetUniform("uModel", Matrix4x4.CreateScale(chunk.Scale) * Matrix4x4.CreateTranslation(_camera.RelativePosition(chunk.Position)));
        _graphics.DrawElements(PrimitiveType.Triangles, chunk.Mesh.IndexCount, DrawElementsType.UnsignedInt, (void*) 0);
    }

    public void Dispose()
    {
        _shaderWatcher.Dispose();
        _skybox.Dispose();
    }

    private static bool IsInFrustum(Chunk c, Frustum frustum)
    {
        foreach (var plane in frustum.Planes)
        {
            Vector3 chunkCenter = c.Position + (new Vector3(c.WorldSize) / 2);
            float distance = Plane.DotCoordinate(plane, chunkCenter);
            if (distance < -c.WorldSize)
                return false;
        }
        return true;
    }
}
