using FloreEngine.Diagnostics;
using FloreEngine.Rendering.Shaders;
using FloreEngine.Rendering.Textures;
using FloreEngine.World;
using Silk.NET.OpenGL;
using System.Numerics;

namespace FloreEngine.Rendering;

internal unsafe class MainRenderer : IDisposable
{
    private static readonly Lazy<MainRenderer> _instance = new Lazy<MainRenderer>(() => new MainRenderer());
    public static MainRenderer Instance => _instance.Value;
    private static GL Graphics => Program.Graphics;

    // Vertex stride: 3 (position) + 3 (normal) + 2 (uv) = 8 floats
    public const int VertexStride = 8;
    private readonly FragVertShader shader;
    private readonly Texture2D texture;
    internal RenderMode RenderingMode;

    public long VertexCount;



    // Storing shader code for simplicity, there are methods in Shader class to read them from files
    private const string VERTEX_SHADER = @"
        #version 330 core
        layout (location = 0) in vec3 vPos;
        layout (location = 1) in vec3 vNormal;
        layout (location = 2) in vec2 vUV;

        uniform mat4 uModel; 
        uniform mat4 uView;
        uniform mat4 uProjection;

        out vec2 fUV;
        out vec3 fNormal;

        void main()
        {
            //Multiplying our uniform with the vertex position, the multiplication order here does matter.
            gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
            fUV = vUV;
            fNormal = vNormal;
        }
    ";

    private const string FRAGMENT_SHADER = @"
        #version 330 core

        const int DEFAULT = 0;
        const int DEPTH = 1;
        const int NORMAL = 2;
        const int UV = 3;

        in vec2 fUV;
        in vec3 fNormal;

        uniform sampler2D fTexture;
        uniform int fRenderMode;
        out vec4 fragColor;

        vec3 lightPos = vec3(0.3, 1.0, 0.7);

        void main()
        {
            vec3 normal = normalize(fNormal);
            vec3 light = normalize(lightPos);
            float diff = max(dot(normal, light), 0.0);

            float ambient = 0.3;
            float lighting = ambient + (1.0 - ambient) * diff;

            vec4 texColor = texture(fTexture, fUV);
            
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
            }
        }
    ";

    public enum RenderMode
    {
        Default = 0,
        Depth = 1,
        Normals = 2,
        UV = 3,
    }

    private MainRenderer()
    {
        Logger.Render("Loading renderer...");

        shader = new FragVertShader(VERTEX_SHADER, FRAGMENT_SHADER);
        texture = Texture2D.FromFile("Assets/block.png", TextureUnit.Texture0);
        texture.SetDefaultParameters();

        RenderingMode = RenderMode.Default;

        Logger.Render("Successfully loaded!");
    }

    internal void Draw()
    {
        shader.UseProgram();
        texture.Bind();

        shader.SetUniform("uView", Camera.Instance.RelativeViewMatrix);
        // shader.SetUniform("uProjection", Program.IsFarProjection ? Camera.Instance.FarProjectionMatrix : Camera.Instance.ProjectionMatrix);

        shader.SetUniform("uProjection", Camera.Instance.ProjectionMatrix);
        shader.SetUniform("fRenderMode", (int) RenderingMode);
        shader.SetUniform("fTexture", 0);

        VertexCount = 0;
        foreach(Chunk chunk in WorldManager.Instance.ChunkMap.Values)
            DrawChunk(chunk);
    }

    private void DrawChunk(Chunk chunk)
    {
        if (chunk.Renderer == null || chunk.Renderer.vao == null) return;
        VertexCount += chunk.Renderer.VertexCount;
        chunk.Renderer.vao.Bind();
        shader.SetUniform("uModel", Matrix4x4.CreateScale(chunk.Scale) * Matrix4x4.CreateTranslation(Camera.Instance.RelativePosition(chunk.Position)));
        Graphics.DrawElements(PrimitiveType.Triangles, chunk.Renderer.IndexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    public void Dispose()
    {
        shader?.Dispose();
    }
}
