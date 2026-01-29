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

    #region Multi-threading
    private const int GENERATION_THREAD_COUNT = 4;
    private const int MESHING_THREAD_COUNT = 4;
    private const int MAX_BUFFER_UPLOADS_PER_FRAME = 8;
    public readonly ConcurrentDictionary<Vector3, Chunk> RenderedChunks;
    public readonly ConcurrentDictionary<(Vector3, int), Chunk> LoadedChunks;

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
        Random r = new Random();
        Noise = FastNoise.FromEncodedNodeTree(FastNoise.TREE_METADATA);
        Noise.Seed = 1444320271;
        Logger.Print($"Seed: {Noise.Seed}");

        RenderedChunks = new ConcurrentDictionary<Vector3, Chunk>();
        LoadedChunks = new ConcurrentDictionary<(Vector3, int), Chunk>();
        _chunksInProgress = new ConcurrentDictionary<Vector3, byte>();

        _chunksToGenerate = new ConcurrentQueue<Chunk>();
        _chunksToMesh = new ConcurrentQueue<Chunk>();
        _chunksReadyForBuffers = new ConcurrentQueue<Chunk>();

        _cancellationTokenSource = new CancellationTokenSource();

        _generationTasks = new Task[GENERATION_THREAD_COUNT];
        for (int i = 0; i < GENERATION_THREAD_COUNT; i++)
        {
            _generationTasks[i] = Task.Factory.StartNew(
                GenerationWorker,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default
            );
        }

        _meshingTasks = new Task[MESHING_THREAD_COUNT];
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

                    chunk.CreateBaseTerrain();

                    if (!chunk.HasFeatures)
                        chunk.CreateFeatures();

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

            if (LoadedChunks.TryGetValue((pos, 0), out Chunk? chunk))
            {
                if (chunk.Mesh != null)
                {
                    if (_chunksInProgress.TryAdd(pos, 0))
                        _chunksReadyForBuffers.Enqueue(chunk);
                }
                else
                {
                    if (_chunksInProgress.TryAdd(pos, 0))
                        _chunksToMesh.Enqueue(chunk);
                }
            }
            else
            {
                if (_chunksInProgress.TryAdd(pos, 0))
                {
                    chunk = new Chunk(pos, 0, false);
                    LoadedChunks[(pos, 0)] = chunk;
                    _chunksToGenerate.Enqueue(chunk);
                }
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
                RenderedChunks.TryRemove(v.Key, out _);
            }
        }

        foreach (var v in LoadedChunks)
        {
            if (MathUtils.OutOfDistance(v.Key.Item1, CenterPos, (RenderDistance + 1) * v.Value.WorldSize))
            {
                if (LoadedChunks.TryRemove(v.Key, out Chunk? chunk))
                {
                    chunk.Dispose();
                }
            }
        }
    }

    public ushort GetVoxelIdAtWorldPos(int x, int y, int z, byte lodLevel)
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
            c.CreateBaseTerrain();
            LoadedChunks[(chunkPos, lodLevel)] = c;
        }

        return c.GetVoxelAt((int)localTilePos.X, (int)localTilePos.Y, (int)localTilePos.Z).ID;
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        Task.WaitAll(_generationTasks.Concat(_meshingTasks).ToArray(), TimeSpan.FromSeconds(2));

        _cancellationTokenSource.Dispose();

        foreach (Chunk chunk in LoadedChunks.Values)
            chunk.Dispose();

        RenderedChunks.Clear();
        LoadedChunks.Clear();

        GC.SuppressFinalize(this);
    }
}