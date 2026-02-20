using System;
using Silk.NET.OpenGL;

namespace FloraEngine.Rendering;

/// <summary>
/// An object that provides methods for uploading vertex data (position, normals, etc) to the gpu
/// </summary>
public unsafe class VertexArrayObject : IDisposable
{
    private static GL _graphics = null!;
    private readonly uint handle;

    public VertexArrayObject(GL graphics)
    {
        _graphics = graphics;
        handle = _graphics.GenVertexArray();
        Bind();
    }

    public static void VertexAttributePointer<VertexType>(uint index, int size, VertexAttribPointerType type, uint vertexSize, int offSet)
        where VertexType : unmanaged
    {
        _graphics.VertexAttribPointer(index, size, type, false, vertexSize * (uint) sizeof(VertexType), (void*) (offSet * sizeof(VertexType)));
        _graphics.EnableVertexAttribArray(index);
    }

    public void Bind() => _graphics.BindVertexArray(handle);
    public static void Unbind() => _graphics.BindVertexArray(0);

    public void Dispose() => _graphics.DeleteVertexArray(handle);
}
