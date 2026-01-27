using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using FloreEngine.Utils;
using Silk.NET.Maths;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;

namespace FloreEngine.World;

internal class WorldManager : IDisposable
{
    private static readonly Lazy<WorldManager> _instance = new Lazy<WorldManager>(() => new WorldManager());
    public static WorldManager Instance => _instance.Value;

    public readonly ConcurrentDictionary<Vector3, Chunk> RenderedChunks;
    public readonly ConcurrentDictionary<(Vector3, int), Chunk> LoadedChunks;

    public static bool centerWorldGen = true;

    internal Vector3 centerPos = Camera.Instance.Position;

    public int MaxLOD = 0;
    public int RenderDistance = 4;

    public readonly FastNoise Noise;

    public WorldManager()
    {
        Random r = new Random();
        Noise = FastNoise.FromEncodedNodeTree(FastNoise.TREE_METADATA);
        //Noise.Seed = r.Next(int.MinValue, int.MaxValue);
        Noise.Seed = 1444320271;
        Logger.Print($"Seed: {Noise.Seed}");

        RenderedChunks = new ConcurrentDictionary<Vector3, Chunk>();
        LoadedChunks = new ConcurrentDictionary<(Vector3, int), Chunk>();
    }


    public unsafe void Update(double deltaTime)
    {
        centerPos = Controller.ChunkPos;

        for (int y = -RenderDistance; y < RenderDistance; y++)
            for (int z = -RenderDistance; z < RenderDistance; z++)
                for (int x = -RenderDistance; x < RenderDistance; x++)
                {
                    Vector3 pos = centerPos + new Vector3(x, y, z) * Chunk.SIZE;

                    if (RenderedChunks.TryGetValue(pos, out Chunk? chunk)) continue;

                    if (LoadedChunks.TryGetValue((pos, 0), out chunk))
                    {
                        if (chunk.HasFeatures == false)
                            chunk.CreateFeatures();

                        if (chunk.Mesh == null)
                            chunk.UpdateMesh();
                    }
                    else
                    {
                        chunk = new Chunk(pos, 0, true);
                    }

                    RenderedChunks[pos] = chunk;
                }

        UnloadFarChunks();
    }

    private void UnloadFarChunks()
    {
        foreach (var v in RenderedChunks)
        {
            if (MathUtils.OutOfDistance(v.Key, centerPos, RenderDistance * v.Value.WorldSize))
            {
                RenderedChunks.Remove(v.Key, out _);
            }
        }

        foreach (var v in LoadedChunks)
        {
            if (MathUtils.OutOfDistance(v.Key.Item1, centerPos, (RenderDistance + 1) * v.Value.WorldSize))
            {
                LoadedChunks.Remove(v.Key, out _);
            }
        }
    }

    public ushort GetVoxelAtWorldPos(int x, int y, int z, int lodLevel)
    {
        int scale = 1 << lodLevel;
        int chunkSize = Chunk.SIZE * (1 << lodLevel);

        Vector3 worldTilePos = new Vector3(x, y, z);
        Vector3 localTilePos = MathUtils.WorldToTilePosition(worldTilePos / scale);
        Vector3 chunkPos = MathUtils.WorldToChunkCoord(worldTilePos, chunkSize);

        LoadedChunks.TryGetValue((chunkPos, lodLevel), out Chunk? c);

        if (c == null)
        {
            c = new Chunk(chunkPos, lodLevel, false);
            LoadedChunks[(chunkPos, lodLevel)] = c;
        }

        // if (level != 0) return 0;
        return c.GetVoxelAt((int)localTilePos.X, (int)localTilePos.Y, (int)localTilePos.Z);
    }

    public void Dispose()
    {
        foreach (Chunk chunk in LoadedChunks.Values)
            chunk.Dispose();

        RenderedChunks.Clear();
        LoadedChunks.Clear();

        GC.SuppressFinalize(this);
    }
}
