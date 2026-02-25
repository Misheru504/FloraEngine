using System;
using System.IO;
using Silk.NET.OpenGL;
using StbImageSharp;

namespace FloraEngine.Rendering.Textures;

/// <summary>
/// A texture is an image applied to vertices
/// </summary>
internal unsafe class Texture2D : Texture, IDisposable
{
    public static Texture2D FromFile(GL graphics, string path, TextureUnit unit)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Texture was not found at {path}");

        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, Program.ASSETS_FOLDER, path)), ColorComponents.RedGreenBlueAlpha);

        fixed(void* ptr = result.Data)
        {
            return new Texture2D(graphics, ptr, (uint) result.Width, (uint) result.Height, InternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, unit);
        }
    }

    public Texture2D(GL graphics, void* data, uint width, uint height, InternalFormat format, PixelFormat pixel, PixelType type, TextureUnit unit)
    {
        _graphics = graphics;
        handle = _graphics.GenTexture();
        this.unit = unit;
        target = TextureTarget.Texture2D;
        Bind();

        _graphics.TexImage2D(
            TextureTarget.Texture2D, 
            0, 
            format, 
            width,
            height, 
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
        _graphics.GenerateMipmap(target);
        _graphics.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
        _graphics.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        _graphics.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        _graphics.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        _graphics.TexParameter(target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
        Unbind();
    }
    
    public uint GetHandle() => handle;
}
