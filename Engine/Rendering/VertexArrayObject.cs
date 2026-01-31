using Silk.NET.OpenGL;

namespace FloraEngine.Rendering;

/// <summary>
/// An object that provides methods for uploading vertex data (position, normals, etc) to the gpu
/// </summary>
public unsafe class VertexArrayObject : IDisposable
{
    private static GL Graphics => Program.Graphics;
    private readonly uint handle;

    public VertexArrayObject()
    {
        handle = Graphics.GenVertexArray();
        Bind();
    }

    public static void VertexAttributePointer<VertexType>(uint index, int size, VertexAttribPointerType type, uint vertexSize, int offSet)
        where VertexType : unmanaged
    {
        Graphics.VertexAttribPointer(index, size, type, false, vertexSize * (uint) sizeof(VertexType), (void*) (offSet * sizeof(VertexType)));
        Graphics.EnableVertexAttribArray(index);
    }

    public void Bind() => Graphics.BindVertexArray(handle);
    public static void Unbind() => Graphics.BindVertexArray(0);

    public void Dispose() => Graphics.DeleteVertexArray(handle);
}
