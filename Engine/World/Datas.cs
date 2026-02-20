using System;

namespace FloraEngine.World;

[Serializable]
public struct VoxelData
{
    public ushort Id { get; }
    public byte Data { get; set; }

    public VoxelData()
    {
        Id = 0;
        Data = 0;
    }

    public VoxelData(ushort id, byte data)
    {
        Id = id;
        Data = data;
    }
}

[Serializable]
public struct ChunkData
{
    public int x, y, z;
    public byte lodLevel;
    public VoxelData[] voxels;

    public static ChunkData FromChunk(Chunk chunk)
    {
        return new ChunkData
        {
            x = (int)chunk.Position.X,
            y = (int)chunk.Position.Y,
            z = (int)chunk.Position.Z,
            lodLevel = chunk.LodLevel,
            voxels = chunk.GetVoxels()
        };
    }
}

[Serializable]
public struct WorldData
{
    public string name;
    public int seed;
    public ChunkData[] chunks;

    public static WorldData FromWorld(World world)
    {
        return new WorldData
        {
            name = world.Name,
            seed = world.Seed,
            chunks = world.Chunks.ToArray()
        };
    }
}

