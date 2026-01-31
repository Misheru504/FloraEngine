using FloraEngine.Rendering;
using FloraEngine.Utils;
using System.Numerics;

namespace FloraEngine.World;

public class Chunk : IDisposable
{
    public const int SIZE = 16;
    public const int VOLUME = SIZE * SIZE * SIZE;

    private static FastNoise Noise => WorldManager.Instance.Noise;

    public Vector3 Position { get; }
    public byte LodLevel { get; }
    public bool HasFeatures { get; set; }
    public int Scale { get; }
    public int WorldSize { get; }
    public Mesh? Mesh { get; private set; }

    private readonly VoxelData[] voxels;

    public Chunk(Vector3 position, byte level, bool createFeatures)
    {
        Position = position;
        LodLevel = level;
        HasFeatures = false; // Always start false
        Scale = 1 << LodLevel;
        WorldSize = Scale * SIZE;

        voxels = new VoxelData[VOLUME];

        if (createFeatures)
        {
            CreateBaseTerrain();
            CreateFeatures();
            UpdateMesh();
            UpdateBuffers();
        }
    }

    public Chunk(ChunkData data)
    {
        Position = new Vector3(data.x, data.y, data.z);
        LodLevel = data.lodLevel;
        HasFeatures = data.hasFeatures;
        Scale = 1 << LodLevel;
        WorldSize = Scale * SIZE;

        voxels = data.voxels;
    }

    public void CreateBaseTerrain()
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
                        SetVoxelAt(x, y, z, Voxel.PURPLE.GetDefaultData());
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

    public void UpdateBuffers()
    {
        Mesh?.CreateBuffers();
    }

    public static int GetIndex(int x, int y, int z) => x + z * SIZE + y * SIZE * SIZE;
    public VoxelData GetVoxelAt(int x, int y, int z) => voxels[GetIndex(x, y, z)];
    public void SetVoxelAt(int x, int y, int z, VoxelData voxel) => voxels[GetIndex(x, y, z)] = voxel;

    internal VoxelData[] GetVoxels() => voxels;

    public void Dispose()
    {
        Mesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}