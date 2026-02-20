using System;
using FloraEngine.Core;
using FloraEngine.Core.Noise;
using FloraEngine.Rendering;
using System.Numerics;

namespace FloraEngine.World;

public class Chunk : IDisposable
{
    public const int VOLUME = WorldConstants.CHUNK_SIZE * WorldConstants.CHUNK_SIZE * WorldConstants.CHUNK_SIZE;

    public Vector3 Position { get; }
    public byte LodLevel { get; }
    public int Scale { get; }
    public int WorldSize { get; }
    public Mesh? Mesh { get; private set; }

    private readonly VoxelData[] voxels;

    public Chunk(Vector3 position, byte level)
    {
        Position = position;
        LodLevel = level;
        Scale = 1 << LodLevel;
        WorldSize = Scale * WorldConstants.CHUNK_SIZE;

        voxels = new VoxelData[VOLUME];
    }

    public Chunk(ChunkData data)
    {
        Position = new Vector3(data.x, data.y, data.z);
        LodLevel = data.lodLevel;
        Scale = 1 << LodLevel;
        WorldSize = Scale * WorldConstants.CHUNK_SIZE;

        voxels = data.voxels;
    }

    public void CreateTerrain(FastNoise noise)
    {
        float[] noiseMap = new float[WorldSize * WorldSize];
        noise.GenUniformGrid2D(noiseMap, (int)Position.X, (int)Position.Z, WorldSize, WorldSize, FastNoise.FREQUENCY, noise.Seed);

        for (int x = 0; x < WorldConstants.CHUNK_SIZE; x++)
        {
            int worldX = x * Scale;
            for (int z = 0; z < WorldConstants.CHUNK_SIZE; z++)
            {
                int worldZ = z * Scale;
                float height = noiseMap[worldZ * WorldSize + worldX];

                for (int y = 0; y < WorldConstants.CHUNK_SIZE; y++)
                {
                    float worldY = (y * Scale) + Position.Y + 64;

                    if (worldY <= height - 4)
                    {
                        SetVoxelAt(x, y, z, Voxel.STONE.GetDefaultData());
                    }
                    else if (worldY <= height - 1)
                    {
                        SetVoxelAt(x, y, z, Voxel.DIRT.GetDefaultData());
                    }
                    else if (worldY <= height)
                    {
                        SetVoxelAt(x, y, z, Voxel.GRASS.GetDefaultData());
                    }
                }
            }
        }

        CreateFeatures(noise);
    }
    private void CreateFeatures(FastNoise noise)
    {
        // TODO: TERRAIN FEATURES
    }

    public void UpdateMesh(WorldManager worldManager)
    {
        Mesh?.Dispose();
        Mesh = new Mesh(this, worldManager);
    }

    public void UpdateBuffers()
    {
        Mesh?.CreateBuffers();
    }

    public static int GetIndex(int x, int y, int z) => x + z * WorldConstants.CHUNK_SIZE + y * WorldConstants.CHUNK_SIZE * WorldConstants.CHUNK_SIZE;
    public VoxelData GetVoxelAt(int x, int y, int z) => voxels[GetIndex(x, y, z)];
    public void SetVoxelAt(int x, int y, int z, VoxelData voxel) => voxels[GetIndex(x, y, z)] = voxel;

    internal VoxelData[] GetVoxels() => voxels;

    public void Dispose()
    {
        Mesh?.Dispose();
        GC.SuppressFinalize(this);
    }
}