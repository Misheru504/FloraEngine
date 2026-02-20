using System;
using Silk.NET.OpenGL;

namespace FloraEngine.Rendering.Textures;

internal abstract class Texture : IDisposable
{
    protected private static GL _graphics = null!;
    protected private TextureUnit unit;
    protected private TextureTarget target;
    protected private uint handle;

    /// <summary>
    /// Unbind this texture's target to 0 (none)
    /// </summary>
    public void Unbind() => _graphics.BindTexture(target, 0);

    /// <summary>
    /// Binds this texture's target to the handle
    /// </summary>
    public void Bind()
    {
        _graphics.ActiveTexture(unit);
        _graphics.BindTexture(target, handle);
    }

    /// <summary>
    /// Delete the texture in the GPU
    /// </summary>
    public void Dispose() => _graphics.DeleteTexture(handle);

    /// <summary>
    /// Sets the default parameters for this texture
    /// </summary>
    public abstract void SetDefaultParameters();
}
