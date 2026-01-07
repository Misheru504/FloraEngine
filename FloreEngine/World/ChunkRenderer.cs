using FloreEngine.Rendering;
using Silk.NET.OpenGL;

namespace FloreEngine.World;

internal class ChunkRenderer : IDisposable
{
    public VertexArrayObject? vao;
    private BufferObject<float>? vbo;
    private BufferObject<uint>? ebo;
    public uint IndexCount;
    public int VertexCount;

    public void CreateBuffers(Mesh mesh)
    {
        if (mesh.Vertices == null || mesh.Indices == null) return;

        vao = new VertexArrayObject();
        IndexCount = (uint)mesh.Indices.Length;
        VertexCount = mesh.Vertices.Length;

        vbo = new BufferObject<float>(mesh.Vertices, BufferTargetARB.ArrayBuffer, BufferUsageARB.StaticDraw);
        ebo = new BufferObject<uint>(mesh.Indices, BufferTargetARB.ElementArrayBuffer, BufferUsageARB.StaticDraw);

        VertexArrayObject.VertexAttributePointer<float>(0, 3, VertexAttribPointerType.Float, 5, 0);
        VertexArrayObject.VertexAttributePointer<float>(1, 2, VertexAttribPointerType.Float, 5, 3);

        VertexArrayObject.Unbind();
        vbo.Unbind();
        ebo.Unbind();
    }

    public void Dispose()
    {
        vao?.Dispose();
        vbo?.Dispose();
        ebo?.Dispose();
        GC.SuppressFinalize(this);
    }
}
