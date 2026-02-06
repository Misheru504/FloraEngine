using FloraEngine.Diagnostics;
using Silk.NET.OpenGL;
using StbImageSharp;
using Buffer = System.Buffer;

namespace FloraEngine.Rendering.Textures;

internal unsafe class TextureArray : Texture, IDisposable
{
    public int TileSize { get; }
    public int LayerCount { get; }
    public int TilesPerRow { get; }

    public TextureArray(string path, TextureUnit unit, int tileSize = 16)
    {
        TileSize = tileSize;

        ImageResult image;
        using (var stream = File.OpenRead(path))
        {
            image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        }

        int imageWidth = image.Width;
        int imageHeight = image.Height;
        byte[] pixels = image.Data;

        TilesPerRow = imageWidth / tileSize;
        int tilesPerCol = imageHeight / tileSize;
        LayerCount = TilesPerRow * tilesPerCol;

        // Create the texture array
        handle = Graphics.GenTexture();
        this.unit = unit;
        target = TextureTarget.Texture2DArray;
        Bind();

        // Allocate storage for all layers
        Graphics.TexImage3D(
            target: target,
            level: 0,
            internalformat: InternalFormat.Rgba8,
            width: (uint)tileSize,
            height: (uint)tileSize,
            depth: (uint)LayerCount,
            border: 0,
            format: PixelFormat.Rgba,
            type: PixelType.UnsignedByte,
            pixels: ReadOnlySpan<byte>.Empty
        );

        // Extract and upload each tile as a layer
        byte[] tileData = new byte[tileSize * tileSize * 4];

        for (int tileY = 0; tileY < tilesPerCol; tileY++)
        {
            for (int tileX = 0; tileX < TilesPerRow; tileX++)
            {
                int layer = tileY * TilesPerRow + tileX;

                // Extract the tile pixels from the atlas
                ExtractTile(pixels, imageWidth, tileX * tileSize, tileY * tileSize, tileSize, tileData);

                // Upload to the specific layer
                fixed (byte* ptr = tileData)
                {
                    Graphics.TexSubImage3D(
                        target: TextureTarget.Texture2DArray,
                        level: 0,
                        xoffset: 0,
                        yoffset: 0,
                        zoffset: layer,
                        width: (uint)tileSize,
                        height: (uint)tileSize,
                        depth: 1,
                        format: PixelFormat.Rgba,
                        type: PixelType.UnsignedByte,
                        pixels: ptr
                    );
                }
            }
        }

        var error = Graphics.GetError();
        if (error != GLEnum.NoError)
        {
            Logger.Print($"OpenGL Error: {error}", Logger.LogLevel.ERROR);
        }

        Unbind();
    }

    private static void ExtractTile(byte[] sourcePixels, int sourceWidth, int startX, int startY, int tileSize, byte[] destBuffer)
    {

        const int bytesPerPixel = 4; // RGBA
        int sourceStride = sourceWidth * bytesPerPixel;
        int tileStride = tileSize * bytesPerPixel;

        for (int y = 0; y < tileSize; y++)
        {
            int sourceOffset = ((startY + y) * sourceWidth + startX) * bytesPerPixel;
            int destOffset = y * tileStride;

            // Copy one row of the tile
            Buffer.BlockCopy(sourcePixels, sourceOffset, destBuffer, destOffset, tileStride);
        }
    }

    public override void SetDefaultParameters()
    {
        Bind();
        Graphics.GenerateMipmap(TextureTarget.Texture2DArray);
        Graphics.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
        Graphics.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        Graphics.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        Graphics.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        Graphics.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapR, (int)TextureWrapMode.Repeat);
        Unbind();
    }
}
