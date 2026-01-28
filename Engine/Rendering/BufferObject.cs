using Silk.NET.OpenGL;

namespace FloreEngine.Rendering;

/// <summary>
/// A buffer stores an array of memory allocated by the GPU on its side
/// </summary>
/// <typeparam name="DataType"></typeparam>
internal unsafe class BufferObject<DataType> : IDisposable
    where DataType : unmanaged
{
    private static GL Graphics => Program.Graphics;
    private readonly BufferTargetARB target;
    private readonly uint handle;

    public BufferObject(Span<DataType> data, BufferTargetARB target, BufferUsageARB usage)
    {
        this.target = target;
        handle = Graphics.GenBuffer();
        Bind();

        fixed (DataType* ptr = data)
        {
            Graphics.BufferData(target, (nuint)(data.Length * sizeof(DataType)), ptr, usage);
        }
    }

    /// <summary>
    /// Binds this buffer in the GPU
    /// </summary>
    public void Bind() => Graphics.BindBuffer(target, handle);

    /// <summary>
    /// Unbinds this buffer
    /// </summary>
    public void Unbind() => Graphics.BindBuffer(target, 0);

    /// <summary>
    /// Delete this buffer
    /// </summary>
    public void Dispose() => Graphics.DeleteBuffer(handle);
}
