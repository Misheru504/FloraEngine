using FloreEngine.Rendering;
using FloreEngine.Utils;
using System.Numerics;

namespace FloreEngine.World;

internal class Chunk : IDisposable
{
    public static int Size => WorldManager.CHUNK_RESOLUTION;
    public static FastNoise Noise => WorldManager.Instance.Noise;

    public Vector3 Position;
    public int Level;
    public ushort[]? Voxels;
    public Mesh? Mesh;
    public ChunkRenderer? Renderer;

    public int Scale => 1 << Level;
    public int WorldSize => Size * Scale;

    public Chunk(Vector3 position, int level)
    {
        Position = position;
        Level = level;
    }

    public void FillVoxels()
    {
        Voxels = new ushort[Size * Size * Size];
        float[] noiseMap = new float[WorldSize * WorldSize]; 
        Noise.GenUniformGrid2D(noiseMap, (int)Position.X, (int)Position.Z, WorldSize, WorldSize, FastNoise.FREQUENCY, Noise.Seed);

        for (int x = 0; x < Size; x++)
        {
            int worldX = x * Scale;
            for (int z = 0; z < Size; z++)
            {
                int worldZ = z * Scale;
                float height = noiseMap[worldZ * WorldSize + worldX];

                for (int y = 0; y < Size; y++)
                {
                    float worldY = (y*Scale) + Position.Y + 64;

                    if (worldY <= height)
                    {
                        Voxels[Index(x, y, z)] = 1;
                    }
                }
            }
        }

        Mesh = new Mesh();
        Mesh.CreateMesh(ref Voxels, Size);
        if (Level != 0) Voxels = null;
    }

    public static byte[] FillVoxels(Vector3 position)
    {
        byte[] voxels = new byte[Size * Size * Size * 4];
        float[] noiseMap = new float[Size * Size];
        Noise.GenUniformGrid2D(noiseMap, (int)position.X, (int)position.Z, Size, Size, FastNoise.FREQUENCY, Noise.Seed);

        for (int z = 0; z < Size; z++)
        {
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float height = noiseMap[z * Size + x];
                    float worldY = y + position.Y + 64;
                    int index = OtherIndex(x, y, z) * 4;

                    if (worldY <= height)
                    {
                        voxels[index + 0] = 36;
                        voxels[index + 1] = 181;
                        voxels[index + 2] = 102;
                        voxels[index + 3] = 255;
                    }
                    else
                    {
                        voxels[index + 0] = 0;
                        voxels[index + 1] = 0;
                        voxels[index + 2] = 0;
                        voxels[index + 3] = 0;
                    }
                }
            }
        }

        return voxels;
    }

    public static int Index(int x, int y, int z) => x + z * Size + y * Size * Size;
    public static int OtherIndex(int x, int y, int z) => z * Size * Size + y * Size + x;

    public void CreateRendering()
    {
        if (Mesh == null) return;
        Renderer?.Dispose();
        Renderer = new ChunkRenderer();
        Renderer.CreateBuffers(Mesh);
        Mesh.Dispose();
        Mesh = null;
    }

    public void Dispose()
    {
        Renderer?.Dispose();
        Mesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}