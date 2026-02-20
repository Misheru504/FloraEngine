using System;
using Silk.NET.OpenGL;

namespace FloraEngine.Rendering.Textures;

/// <summary>
/// A 3d texture is an "image" that can be used in compute shaders
/// </summary>
internal unsafe class Texture3D : Texture, IDisposable
{
    public Texture3D(GL graphics, void* data, uint width, uint height, uint depth, InternalFormat format, PixelFormat pixel, PixelType type, TextureUnit unit)
    {
        _graphics = graphics;
        handle = _graphics.GenTexture();
        this.unit = unit;
        target = TextureTarget.Texture3D;
        Bind();

        _graphics.TexImage3D(
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
        _graphics.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        _graphics.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        _graphics.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        _graphics.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        _graphics.TexParameter(target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        Unbind();
    }
}
