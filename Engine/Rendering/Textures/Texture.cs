using Silk.NET.OpenGL;
namespace FloreEngine.Rendering.Textures;

internal abstract class Texture : IDisposable
{
    protected private static GL Graphics => Program.Graphics;
    protected private TextureUnit unit;
    protected private TextureTarget target;
    protected private uint handle;

    /// <summary>
    /// Unbind this texture's target to 0 (none)
    /// </summary>
    public void Unbind() => Graphics.BindTexture(target, 0);

    /// <summary>
    /// Binds this texture's target to the handle
    /// </summary>
    public void Bind()
    {
        Graphics.ActiveTexture(unit);
        Graphics.BindTexture(target, handle);
    }

    /// <summary>
    /// Delete the texture in the GPU
    /// </summary>
    public void Dispose() => Graphics.DeleteTexture(handle);

    /// <summary>
    /// Sets the default parameters for this texture
    /// </summary>
    public abstract void SetDefaultParameters();
}
