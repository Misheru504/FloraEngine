using Silk.NET.OpenGL;
using StbImageSharp;

namespace FloreEngine.Rendering.Textures;

/// <summary>
/// A 3d texture is an "image" that can be used in compute shaders
/// </summary>
internal unsafe class Texture3D : Texture, IDisposable
{
    public Texture3D(void* data, uint width, uint height, uint depth, InternalFormat format, PixelFormat pixel, PixelType type, TextureUnit unit)
    {
        handle = Graphics.GenTexture();
        this.unit = unit;
        target = TextureTarget.Texture3D;
        Bind();

        Graphics.TexImage3D(
            target,
            0,
            format,
            width,
            height,
            depth,
            0,
            pixel,
            type,
            data
        );


        Unbind();
    }

    override public void SetDefaultParameters()
    {
        Bind();
        Graphics.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        Graphics.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        Graphics.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        Graphics.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        Graphics.TexParameter(target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        Unbind();
    }
}
