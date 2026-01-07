using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using FloreEngine.Rendering.Shaders;
using FloreEngine.Rendering.Textures;
using FloreEngine.World;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System.Numerics;
using System.Runtime.InteropServices;
using FragVertShader = FloreEngine.Rendering.Shaders.FragVertShader;

namespace FloreEngine.Temp;

internal unsafe class RayRenderer : IDisposable
{
    private static readonly Lazy<RayRenderer> _instance = new Lazy<RayRenderer>(() => new RayRenderer());
    public static RayRenderer Instance => _instance.Value;
    private static IWindow GLWindow => Program.EngineWindow;
    private static GL Graphics => Program.Graphics;
    private static int Width => Program.WindowResolution.X;
    private static int Height => Program.WindowResolution.Y;

    // Shaders and textures
    private readonly ComputeShader computeShader;
    private Texture2D outputTexture;
    private readonly Texture2D block;
    private readonly Texture3D voxelTexture;
    private readonly BufferObject<int> ubo;

    // Display quad
    private readonly VertexArrayObject vao;
    private readonly BufferObject<float> vbo;
    private readonly FragVertShader displayShader;

    private const int VOXEL_SIZE = WorldManager.CHUNK_RESOLUTION;

    private Vector3 sunPos;

    // Storing shader code for simplicity, there are methods in Shader class to read them from files
    private const string VERTEX_SHADER = @"
        #version 450
        layout(location = 0) in vec2 aPos;
        layout(location = 1) in vec2 aTexCoord;
        out vec2 texCoord;
        void main() {
            gl_Position = vec4(aPos, 0.0, 1.0);
            texCoord = aTexCoord;
        }
    ";

    private const string FRAGMENT_SHADER = @"
        #version 450
        in vec2 texCoord;
        out vec4 fragColor;
        uniform sampler2D screenTexture;
        void main() {
            fragColor = texture(screenTexture, texCoord);
        }
    ";

    private const string RAYTRACER_SHADER = "Assets/voxel_raytracer.comp";
    private const string RAYTRACER_TEXTURED = "Assets/textured.comp";

    private RayRenderer()
    {
        Logger.Render("Loading renderer...");

        computeShader = new ComputeShader(ComputeShader.LoadShaderFromFile(RAYTRACER_SHADER));
        voxelTexture = CreateVoxelTexture();
        block = Texture2D.FromFile("Assets/block.png", TextureUnit.Texture3);
        block.SetDefaultParameters();
        outputTexture = CreateOutputTexture();
        ubo = CreateCameraUBO();
        (vao, vbo, displayShader) = CreateDisplayQuad();

        sunPos = Vector3.Zero;

        Logger.Render("Renderer loaded correctly!");
    }

    internal void Draw()
    {
        // Update camera uniform buffer
        UpdateCameraUBO();

        double t = 0.3f * GLWindow.Time;
        sunPos.X = (float) Math.Sin(t + 2);
        sunPos.Y = (float)Math.Sin(t - 2);
        sunPos.Z = (float)Math.Sin(t);

        // Dispatch compute shader
        computeShader.UseProgram();
        computeShader.SetUniform("voxelGridSize", VOXEL_SIZE);
        computeShader.SetUniform("sunPosition", sunPos);

        // Bind output texture as image
        Graphics.BindImageTexture(0, outputTexture.GetHandle(), 0, false, 0, BufferAccessARB.WriteOnly, InternalFormat.Rgba8);

        // Bind voxel texture
        voxelTexture.Bind();

        block.Bind();

        // Dispatch compute shader (8x8 work groups)
        Graphics.DispatchCompute((uint)((Width + 7) / 8), (uint)((Height + 7) / 8), 1);

        // Memory barrier to ensure compute shader writes are visible
        Graphics.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);

        displayShader.UseProgram();
        outputTexture.Bind();

        vao.Bind();
        Graphics.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
    }

    internal static Texture3D CreateVoxelTexture()
    {
        byte[] voxelData = Chunk.FillVoxels(Vector3.Zero);

        Texture3D voxelTexture;

        fixed (byte* ptr = voxelData)
        {
            voxelTexture = new Texture3D(
                ptr,
                VOXEL_SIZE,
                VOXEL_SIZE,
                VOXEL_SIZE,
                InternalFormat.Rgba8,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                TextureUnit.Texture1
            );
        }

        voxelTexture.SetDefaultParameters();

        return voxelTexture;
    }

    internal static byte[] CreateVoxelScene()
    {
        byte[] voxelData = new byte[VOXEL_SIZE * VOXEL_SIZE * VOXEL_SIZE * 4]; // RGBA
        Random random = new Random(42);

        for (int z = 0; z < VOXEL_SIZE; z++)
        {
            for (int y = 0; y < VOXEL_SIZE; y++)
            {
                for (int x = 0; x < VOXEL_SIZE; x++)
                {
                    int index = (z * VOXEL_SIZE * VOXEL_SIZE + y * VOXEL_SIZE + x) * 4;

                    bool isSolid = false;
                    byte r = 0, g = 0, b = 0, a = 0;

                    // Ground plane
                    if (y < 10)
                    {
                        isSolid = true;
                        r = 100; g = 180; b = 100; // Green ground
                    }
                    // Create some walls and corners to showcase AO
                    else if (y < 30)
                    {
                        // Corner walls
                        if ((x < 5 || x > VOXEL_SIZE - 6) && (z < 5 || z > VOXEL_SIZE - 6))
                        {
                            isSolid = true;
                            r = 150; g = 150; b = 150; // Gray walls
                        }
                        // L-shaped wall
                        else if ((x > 15 && x < 20) || (z > 15 && z < 20 && x > 15 && x < 30))
                        {
                            isSolid = true;
                            r = 180; g = 150; b = 120; // Tan
                        }
                        // Random columns
                        else if (((x - 40) * (x - 40) + (z - 40) * (z - 40) < 9) ||
                                 ((x - 24) * (x - 24) + (z - 50) * (z - 50) < 9))
                        {
                            isSolid = true;
                            r = 120; g = 100; b = 140; // Purple
                        }
                    }
                    // Floating platform with hole
                    else if (y > 35 && y < 38)
                    {
                        float dx = x - VOXEL_SIZE / 2f;
                        float dz = z - VOXEL_SIZE / 2f;
                        float dist = MathF.Sqrt(dx * dx + dz * dz);

                        if (dist < 20f && dist > 8f)
                        {
                            isSolid = true;
                            r = 200; g = 180; b = 160;
                        }
                    }
                    // Small structures on top
                    else if (y > 10 && y < 25 && random.NextDouble() < 0.015)
                    {
                        // Check if we're near existing structures for clustering
                        bool nearStructure = false;
                        if (x > 20 && x < 45 && z > 20 && z < 45)
                        {
                            nearStructure = true;
                        }

                        if (nearStructure)
                        {
                            isSolid = true;
                            r = (byte)(100 + random.Next(100));
                            g = (byte)(100 + random.Next(100));
                            b = (byte)(100 + random.Next(100));
                        }
                    }
                    // Central sphere structure
                    else
                    {
                        float dx = x - VOXEL_SIZE / 2f;
                        float dy = y - VOXEL_SIZE / 2f;
                        float dz = z - VOXEL_SIZE / 2f;
                        float dist = MathF.Sqrt(dx * dx + dy * dy + dz * dz);

                        if (dist < 15f && dist > 12f)
                        {
                            isSolid = true;
                            r = 255; g = 100; b = 100; // Red sphere
                        }
                    }

                    if (isSolid)
                    {
                        voxelData[index + 0] = r;
                        voxelData[index + 1] = g;
                        voxelData[index + 2] = b;
                        voxelData[index + 3] = 255; // Alpha = solid
                    }
                    else
                    {
                        voxelData[index + 0] = 0;
                        voxelData[index + 1] = 0;
                        voxelData[index + 2] = 0;
                        voxelData[index + 3] = 0; // Alpha = empty
                    }
                }
            }
        }

        return voxelData;
    }

    internal static Texture2D CreateOutputTexture()
    {
        Texture2D outputTexture = new Texture2D(
            null,
            (uint) Width,
            (uint) Height,
            InternalFormat.Rgba8,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            TextureUnit.Texture0
        );

        outputTexture.Bind();
        Graphics.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        Graphics.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        outputTexture.Unbind();

        return outputTexture;
    }

    internal static BufferObject<int> CreateCameraUBO()
    {
        int bufferSize = 16 * 4 * 4 + 4 * 4 + 4 * 4;

        BufferObject<int> ubo = new BufferObject<int>((nuint)bufferSize, BufferTargetARB.UniformBuffer, BufferUsageARB.DynamicDraw);
        ubo.BindBase(2);
        ubo.Unbind();

        return ubo;
    }

    internal void UpdateCameraUBO()
    {
        Matrix4x4.Invert(Camera.Instance.ViewMatrix * Camera.Instance.ProjectionMatrix, out Matrix4x4 invViewProjection);

        CameraData cameraData = new CameraData
        {
            ViewMatrix = Camera.Instance.ViewMatrix,
            ProjectionMatrix = Camera.Instance.ProjectionMatrix,
            InvViewProjection = invViewProjection,
            CameraPos = Camera.Instance.Position,
            Time = (float)GLWindow.Time,
            Resolution = new Vector2(Width, Height),
            Padding = Vector2.Zero
        };

        ubo.Bind();
        ubo.SubData(0, (nuint)Marshal.SizeOf<CameraData>(), &cameraData);
        ubo.Unbind();
    }

    internal static (VertexArrayObject, BufferObject<float>, FragVertShader) CreateDisplayQuad()
    {
        float[] quadVertices = [
            -1f, -1f,  0f, 0f,
             1f, -1f,  1f, 0f,
            -1f,  1f,  0f, 1f,
             1f,  1f,  1f, 1f
        ];

        VertexArrayObject vao = new VertexArrayObject();
        BufferObject<float> vbo = new BufferObject<float>(quadVertices, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);

        VertexArrayObject.VertexAttributePointer<float>(0, 2, VertexAttribPointerType.Float, 4, 0);
        VertexArrayObject.VertexAttributePointer<float>(1, 2, VertexAttribPointerType.Float, 4, 2);
        VertexArrayObject.Unbind();

        FragVertShader displayShader = new FragVertShader(VERTEX_SHADER, FRAGMENT_SHADER);

        return (vao, vbo, displayShader);
    }

    internal void OnResize()
    {
        // Recreate output texture with new size
        outputTexture.Dispose();
        outputTexture = CreateOutputTexture();
    }

    public void Dispose()
    {
        computeShader?.Dispose();
        outputTexture?.Dispose();
        voxelTexture?.Dispose();
        ubo?.Dispose();

        vao?.Dispose();
        vbo?.Dispose();
        displayShader?.Dispose();
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct CameraData
    {
        public Matrix4x4 ViewMatrix;
        public Matrix4x4 ProjectionMatrix;
        public Matrix4x4 InvViewProjection;
        public Vector3 CameraPos;
        public float Time;
        public Vector2 Resolution;
        public Vector2 Padding;
    }
}