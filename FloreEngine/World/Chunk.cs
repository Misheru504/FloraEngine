using FloreEngine.Rendering;
using FloreEngine.Utils;
using System.Drawing;
using System.Numerics;

namespace FloreEngine.World;

internal class Chunk : IDisposable
{
    public const int SIZE = 16;
    public const int VOLUME = SIZE * SIZE * SIZE;

    private static FastNoise Noise => WorldManager.Instance.Noise;

    public Vector3 Position { get; }
    public int LodLevel { get; }
    public bool HasFeatures { get; set; }
    public int Scale { get; }
    public int WorldSize { get; }
    public Mesh? Mesh { get; private set; }

    private readonly ushort[] voxels;

    public Chunk(Vector3 position, int level, bool hasFeatures)
    {
        Position = position;
        LodLevel = level;
        HasFeatures = hasFeatures;
        Scale = 1 << LodLevel;
        WorldSize = Scale * SIZE;

        voxels = new ushort[VOLUME];

        CreateBaseTerrain();
        if (HasFeatures)
        {
            CreateFeatures();
            UpdateMesh();
        }
    }

    private void CreateBaseTerrain()
    {
        float[] noiseMap = new float[WorldSize * WorldSize];
        Noise.GenUniformGrid2D(noiseMap, (int)Position.X, (int)Position.Z, WorldSize, WorldSize, FastNoise.FREQUENCY, Noise.Seed);

        for (int x = 0; x < SIZE; x++)
        {
            int worldX = x * Scale;
            for (int z = 0; z < SIZE; z++)
            {
                int worldZ = z * Scale;
                float height = noiseMap[worldZ * WorldSize + worldX];

                for (int y = 0; y < SIZE; y++)
                {
                    float worldY = (y * Scale) + Position.Y + 64;

                    if (worldY <= height)
                    {
                        SetVoxelAt(x, y, z, 1);
                    }
                }
            }
        }
    }
    public void CreateFeatures()
    {
        HasFeatures = true;

        // TODO: TERRAIN FEATURES
    }
    public void UpdateMesh()
    {
        Mesh?.Dispose();
        Mesh = new Mesh(this);
    }

    public static int GetIndex(int x, int y, int z) => x + z * SIZE + y * SIZE * SIZE;
    public ushort GetVoxelAt(int x, int y, int z) => voxels[GetIndex(x, y, z)];
    public void SetVoxelAt(int x, int y, int z, ushort voxel) => voxels[GetIndex(x, y, z)] = voxel;

    public void Dispose()
    {
        Mesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}