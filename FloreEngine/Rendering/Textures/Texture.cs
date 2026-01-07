using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloreEngine.Rendering.Textures;

internal abstract class Texture : IDisposable
{
    protected static GL Graphics => Program.Graphics;
    protected TextureUnit unit;
    protected TextureTarget target;
    protected uint handle;

    internal uint GetHandle() => handle;

    public void Unbind() => Graphics.BindTexture(target, 0);

    public void Bind()
    {
        Graphics.ActiveTexture(unit);
        Graphics.BindTexture(target, handle);
    }

    public void Dispose() => Graphics.DeleteTexture(handle);

    public abstract void SetDefaultParameters();
}
