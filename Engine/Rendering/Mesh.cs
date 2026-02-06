using FloraEngine.Utils;
using FloraEngine.World;
using Silk.NET.OpenGL;

namespace FloraEngine.Rendering;

public class Mesh : IDisposable
{
    public static bool IsUsingGreedyMeshing = false;

    public VertexArrayObject? vao;
    private BufferObject<float>? vbo;
    private BufferObject<uint>? ebo;
    private MeshData? meshData;

    public uint IndexCount;
    public int VertexCount;

    /// <summary>
    /// Contains the list of vertices and indices for a mesh
    /// </summary>
    internal struct MeshData
    {
        internal List<float> Vertices;
        internal List<uint> Indices;
    }

    /// <summary>
    /// Creates a new mesh for a chunk
    /// </summary>
    /// <param name="currentChunk">Chunk to mesh</param>
    public Mesh(Chunk currentChunk)
    {
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();

        if(IsUsingGreedyMeshing) GreedyMesher.CreateGreedyMesh(currentChunk, vertices, indices);
        else CulledMesher.CreateCulledMesh(currentChunk, vertices, indices);

        VertexCount = vertices.Count / Renderer.VertexStride;
        IndexCount = (uint)indices.Count;

        meshData = new MeshData()
        {
            Vertices = vertices,
            Indices = indices
        };
    }

    public void CreateBuffers()
    {
        if (meshData == null) return;

        vao = new VertexArrayObject();

        vbo = new BufferObject<float>(meshData?.Vertices.ToArray(), BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        ebo = new BufferObject<uint>(meshData?.Indices.ToArray(), BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw);

        VertexArrayObject.VertexAttributePointer<float>(0, 3, VertexAttribPointerType.Float, 10, 0); // Position
        VertexArrayObject.VertexAttributePointer<float>(1, 3, VertexAttribPointerType.Float, 10, 3); // Normals
        VertexArrayObject.VertexAttributePointer<float>(2, 2, VertexAttribPointerType.Float, 10, 6); // UVs
        VertexArrayObject.VertexAttributePointer<float>(3, 1, VertexAttribPointerType.Float, 10, 8); // AOs
        VertexArrayObject.VertexAttributePointer<float>(4, 1, VertexAttribPointerType.Float, 10, 9); // Texture layer

        VertexArrayObject.Unbind();
        vbo.Unbind();
        ebo.Unbind();

        meshData?.Vertices.Clear();
        meshData?.Indices.Clear();
        meshData = null;
    }

    public void Dispose()
    {
        vao?.Dispose();
        vbo?.Dispose();
        ebo?.Dispose();

        GC.SuppressFinalize(this);
    }
}
