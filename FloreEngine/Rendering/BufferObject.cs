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

    public BufferObject(nuint size, nuint data, BufferTargetARB target, BufferUsageARB usage)
    {
        this.target = target;

        handle = Graphics.GenBuffer();
        Bind();
        Graphics.BufferData(target, size, (void*) data, usage);
    }

    public BufferObject(nuint size, BufferTargetARB target, BufferUsageARB usage)
    {

        this.target = target;

        handle = Graphics.GenBuffer();
        Bind();
        Graphics.BufferData(target, size, null, usage);
    }

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

    public void BindBase(uint index) => Graphics.BindBufferBase(target, index, handle);
    public void Bind() => Graphics.BindBuffer(target, handle);
    public void Unbind() => Graphics.BindBuffer(target, 0);
    public void SubData(nint offset, nuint size, void* data) => Graphics.BufferSubData(target, offset, size, data);

    public void Dispose() => Graphics.DeleteBuffer(handle);
}
