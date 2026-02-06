using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FloraEngine.World;

[Serializable]
public struct VoxelData
{
    public ushort id;
    public string data;

    public VoxelData()
    {
        id = 0;
        data = "";
    }

    public VoxelData(ushort id, string data)
    {
        this.id = id;
        this.data = data;
    }

    public void SetData(string data)
    {
        this.data = data;
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

