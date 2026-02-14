using Silk.NET.OpenGL;

namespace FloraEngine.Rendering;

/// <summary>
/// A buffer stores an array of memory allocated by the GPU on its side
/// </summary>
/// <typeparam name="DataType"></typeparam>
internal unsafe class BufferObject<DataType> : IDisposable
    where DataType : unmanaged
{
    private static GL _graphics = null!;
    private readonly BufferTargetARB target;
    private readonly uint handle;

    public BufferObject(GL graphics, Span<DataType> data, BufferTargetARB target, BufferUsageARB usage)
    {
        _graphics = graphics;
        this.target = target;
        handle = _graphics.GenBuffer();
        Bind();

        fixed (DataType* ptr = data)
        {
            _graphics.BufferData(target, (nuint)(data.Length * sizeof(DataType)), ptr, usage);
        }
    }

    /// <summary>
    /// Binds this buffer in the GPU
    /// </summary>
    public void Bind() => _graphics.BindBuffer(target, handle);

    /// <summary>
    /// Unbinds this buffer
    /// </summary>
    public void Unbind() => _graphics.BindBuffer(target, 0);

    /// <summary>
    /// Delete this buffer
    /// </summary>
    public void Dispose() => _graphics.DeleteBuffer(handle);
}
