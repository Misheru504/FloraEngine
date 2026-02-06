using FloraEngine.Entities.Player;
using FloraEngine.Utils;
using FloraEngine.Diagnostics;
using FloraEngine.Rendering;
using System.Collections.Concurrent;
using System.Numerics;

namespace FloraEngine.World;

internal class WorldManager : IDisposable
{
    private static readonly Lazy<WorldManager> _instance = new Lazy<WorldManager>(() => new WorldManager());
    public static WorldManager Instance => _instance.Value;

    internal static Vector3 CenterPos => Player.ChunkPos;

    public int MaxLOD = 0;
    public int RenderDistance = 5;
    public bool IsWorldLoaded => World != null;
    public World? World { get; private set; }

    #region Multi-threading
    private const int GENERATION_THREAD_COUNT = 4;
    private const int MESHING_THREAD_COUNT = 4;
    private const int MAX_BUFFER_UPLOADS_PER_FRAME = 8;
    public readonly ConcurrentDictionary<Vector3, Chunk> RenderedChunks;

    private readonly ConcurrentDictionary<Vector3, byte> _chunksInProgress;

    private readonly ConcurrentQueue<Chunk> _chunksToGenerate;
    private readonly ConcurrentQueue<Chunk> _chunksToMesh;
    private readonly ConcurrentQueue<Chunk> _chunksReadyForBuffers;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task[] _generationTasks;
    private readonly Task[] _meshingTasks;
    #endregion

    public readonly FastNoise Noise;

    public WorldManager()
    {
        World = null;
        Noise = FastNoise.FromEncodedNodeTree(FastNoise.TREE_METADATA);

        RenderedChunks = new ConcurrentDictionary<Vector3, Chunk>();
        _chunksInProgress = new ConcurrentDictionary<Vector3, byte>();

        _chunksToGenerate = new ConcurrentQueue<Chunk>();
        _chunksToMesh = new ConcurrentQueue<Chunk>();
        _chunksReadyForBuffers = new ConcurrentQueue<Chunk>();

        _cancellationTokenSource = new CancellationTokenSource();
        _generationTasks = new Task[GENERATION_THREAD_COUNT];
        _meshingTasks = new Task[MESHING_THREAD_COUNT];
    }

    public void LoadWorld(WorldData worldData)
    {
        World world = new World(worldData);
        World = world;
        Noise.Seed = world.Seed;

        Logger.Print($"Loaded world '{world.Name}'. Seed: {world.Seed}");

        StartTasks();
    }

    public void SaveActiveWorld()
    {
        if (World == null) return;

        WorldData data = WorldData.FromWorld(World);
        JsonMapper.PrettySerialize(data, $"./worlds/{data.name}.json");
    }

    private void StartTasks()
    {
        for (int i = 0; i < GENERATION_THREAD_COUNT; i++)
        {
            _generationTasks[i] = Task.Factory.StartNew(
                GenerationWorker,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        for (int i = 0; i < MESHING_THREAD_COUNT; i++)
        {
            _meshingTasks[i] = Task.Factory.StartNew(
                MeshingWorker,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }
    }

    private void GenerationWorker()
    {
        var token = _cancellationTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            if (_chunksToGenerate.TryDequeue(out Chunk? chunk))
            {
                try
                {
                    if (MathUtils.OutOfDistance(chunk.Position, CenterPos, RenderDistance * chunk.WorldSize))
                    {
                        _chunksInProgress.TryRemove(chunk.Position, out _);
                        continue;
                    }

                    chunk.CreateTerrain();

                    _chunksToMesh.Enqueue(chunk);
                }
                catch (Exception ex)
                {
                    Logger.Print($"Error generating chunk at {chunk.Position}: {ex.Message}");
                    _chunksInProgress.TryRemove(chunk.Position, out _);
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    private void MeshingWorker()
    {
        var token = _cancellationTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            if (_chunksToMesh.TryDequeue(out Chunk? chunk))
            {
                try
                {
                    if (MathUtils.OutOfDistance(chunk.Position, CenterPos, RenderDistance * chunk.WorldSize))
                    {
                        _chunksInProgress.TryRemove(chunk.Position, out _);
                        continue;
                    }

                    chunk.UpdateMesh();

                    _chunksReadyForBuffers.Enqueue(chunk);
                }
                catch (Exception ex)
                {
                    Logger.Print($"Error meshing chunk at {chunk.Position}: {ex.Message}");
                    _chunksInProgress.TryRemove(chunk.Position, out _);
                }
            }
            else
            {
                Thread.Sleep(1);
            }
        }
    }

    public unsafe void Update(double deltaTime)
    {
        if (!IsWorldLoaded) return;

        QueueChunksForGeneration();
        ProcessReadyChunks();
        UnloadFarChunks();
    }

    private void QueueChunksForGeneration()
    {
        var positions = new List<Vector3>();

        for (int y = -RenderDistance; y < RenderDistance; y++)
            for (int z = -RenderDistance; z < RenderDistance; z++)
                for (int x = -RenderDistance; x < RenderDistance; x++)
                {
                    Vector3 pos = CenterPos + new Vector3(x, y, z) * Chunk.SIZE;
                    positions.Add(pos);
                }

        foreach (var pos in positions)
        {
            if (RenderedChunks.ContainsKey(pos) || _chunksInProgress.ContainsKey(pos))
                continue;
            
            if (_chunksInProgress.TryAdd(pos, 0))
            {
                Chunk chunk = new Chunk(pos, 0);
                RenderedChunks[pos] = chunk;
                _chunksToGenerate.Enqueue(chunk);
            }
            
        }
    }

    private void ProcessReadyChunks()
    {
        int processed = 0;

        while (processed < MAX_BUFFER_UPLOADS_PER_FRAME && _chunksReadyForBuffers.TryDequeue(out Chunk? chunk))
        {
            try
            {
                chunk.UpdateBuffers();
                RenderedChunks[chunk.Position] = chunk;
            }
            finally
            {
                _chunksInProgress.TryRemove(chunk.Position, out _);
            }

            processed++;
        }
    }

    private void UnloadFarChunks()
    {
        foreach (var v in RenderedChunks)
        {
            if (MathUtils.OutOfDistance(v.Key, CenterPos, RenderDistance * v.Value.WorldSize))
            {
                RenderedChunks.TryRemove(v.Key, out Chunk? c);
                c?.Dispose();
            }
        }
    }

    public ushort GetVoxelIdAtWorldPos(int x, int y, int z, byte lodLevel)
    {
        if (!IsWorldLoaded) return Voxel.AIR.ID;

        int scale = 1 << lodLevel;
        int chunkSize = Chunk.SIZE * (1 << lodLevel);

        Vector3 worldTilePos = new Vector3(x, y, z);
        Vector3 localTilePos = MathUtils.WorldToTilePosition(worldTilePos / scale);
        Vector3 chunkPos = MathUtils.WorldToChunkCoord(worldTilePos, chunkSize);

        RenderedChunks.TryGetValue(chunkPos, out Chunk? c);

        if (c == null)
        {
            c = new Chunk(chunkPos, lodLevel);
            c.CreateTerrain();
            RenderedChunks[chunkPos] = c;
            _chunksToMesh.Enqueue(c);
        }

        return c.GetVoxelAt((int)localTilePos.X, (int)localTilePos.Y, (int)localTilePos.Z).id;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        Task.WaitAll(_generationTasks.Concat(_meshingTasks).ToArray(), TimeSpan.FromSeconds(2));

        _cancellationTokenSource.Dispose();

        foreach (Chunk chunk in RenderedChunks.Values)
            chunk.Dispose();

        RenderedChunks.Clear();

        GC.SuppressFinalize(this);
    }

    public void UpdateChunksMeshes()
    {
        foreach(Chunk chunk in RenderedChunks.Values)
        {
            if(chunk.Mesh != null)
            {
                chunk.UpdateMesh();
                chunk.UpdateBuffers();
            }
        }
    }
}