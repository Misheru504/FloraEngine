using FloreEngine.Rendering;
using FloreEngine.Utils;
using System.Numerics;

namespace FloreEngine.World;

internal class Chunk : IDisposable
{
    public const int Size = 16;
    public static FastNoise Noise => WorldManager.Instance.Noise;

    public Vector3 Position;
    public int Level;
    public bool IsProto; // A proto chunk filled with the base terrain, without features and mesh

    public ushort[]? Voxels;
    public Mesh? Mesh;

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
    }

    public static int Index(int x, int y, int z) => x + z * Size + y * Size * Size;

    public void CreateMesh()
    {
        if (Voxels == null) return;

        Mesh = new Mesh();
        Mesh.CreateMesh(this);
    }

    public void CreateRendering()
    {
        if (Mesh == null || Mesh.vao != null) return;
        if (Level != 0) Voxels = null;

        Mesh.CreateBuffers();
    }

    public void Dispose()
    {
        Mesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}