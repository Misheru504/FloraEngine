using Silk.NET.OpenGL;
using StbImageSharp;

namespace FloreEngine.Rendering.Textures;

/// <summary>
/// A texture is an image applied to vertices
/// </summary>
internal unsafe class Texture2D : Texture, IDisposable
{
    public static Texture2D FromFile(string path, TextureUnit unit)
    {
        if (!File.Exists(path)) throw new FileNotFoundException($"Texture was not found at {path}");

        ImageResult result = ImageResult.FromMemory(File.ReadAllBytes(path), ColorComponents.RedGreenBlueAlpha);

        fixed(void* ptr = result.Data)
        {
            return new Texture2D(ptr, (uint) result.Width, (uint) result.Height, InternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, unit);
        }
    }

    public Texture2D(void* data, uint width, uint height, InternalFormat format, PixelFormat pixel, PixelType type, TextureUnit unit)
    {
        handle = Graphics.GenTexture();
        this.unit = unit;
        target = TextureTarget.Texture2D;
        Bind();

        Graphics.TexImage2D(
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
        Graphics.GenerateMipmap(target);
        Graphics.TexParameter(target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
        Graphics.TexParameter(target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        Graphics.TexParameter(target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        Graphics.TexParameter(target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        Graphics.TexParameter(target, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
        Unbind();
    }
}
