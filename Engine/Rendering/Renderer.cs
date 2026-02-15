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
    public const int VertexStride = 10;

    private readonly GL _graphics = null!;
    private readonly FragVertShader shader;
    private readonly RenderConfig _renderConfig;
    private readonly Camera _camera;
    private readonly WorldManager _worldManager;

    // Storing shader code for simplicity, there are methods in Shader class to read them from files
    private const string VERTEX_SHADER = @"
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
    ";

    private const string FRAGMENT_SHADER = @"
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
        out vec4 fragColor;

        vec3 lightPos = vec3(0.3, 1.0, 0.7);

        vec3 hashColor(float n) {
            // Pseudo-random hash that gives consistent colors per layer
            vec3 p = vec3(n * 0.1031, n * 0.1030, n * 0.0973);
            p = fract(p * vec3(127.1, 311.7, 74.7));
            p += dot(p, p.yzx + 33.33);
            return fract((p.xxy + p.yzz) * p.zyx);
        }

        void main()
        {
            vec3 normal = normalize(fNormal);
            vec3 light = normalize(lightPos);
            float diff = max(dot(normal, light), 0.0);

            float ambient = 0.3;
            float directional = (1.0 - ambient) * diff;
        
            // Apply AO to both ambient and slightly to directional lighting
            float aoFactor = fAO;
            float aoAmbient = ambient * (0.5 + 0.5 * aoFactor);  // AO affects ambient more
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
    ";

    private readonly TextureArray atlas;

    public Renderer(GL graphics, RenderConfig renderConfig, Camera camera, WorldManager worldManager)
    {
        Logger.Render("Loading renderer...");
        _graphics = graphics;
        _renderConfig = renderConfig;
        _camera = camera;
        _worldManager = worldManager;

        shader = new FragVertShader(_graphics, VERTEX_SHADER, FRAGMENT_SHADER);

        atlas = new TextureArray(_graphics, "Assets/atlas.png", TextureUnit.Texture1, 16);
        atlas.SetDefaultParameters();

        Mesh.RenderConfig = _renderConfig;
        Mesh.Graphics = _graphics;

        Logger.Render("Successfully loaded!");
    }

    internal void Draw()
    {
        shader.UseProgram();
        atlas.Bind();

        shader.SetUniform("uView", _camera.RelativeViewMatrix);
        shader.SetUniform("uProjection", _camera.ProjectionMatrix);
        shader.SetUniform("fRenderMode", (int) _renderConfig.RenderMode);
        shader.SetUniform("fTexture", 1);

        _renderConfig.VertexCount = 0;
        foreach(Chunk chunk in _worldManager.RenderedChunks.Values)
            DrawChunk(chunk);
    }

    private void DrawChunk(Chunk chunk)
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
        shader?.Dispose();
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
