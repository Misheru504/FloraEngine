using Silk.NET.Maths;
using System.Numerics;

namespace FloraEngine.Core;

public class DiagnosticsData
{
    // Rendering
    public RenderConfig RenderConfig { get; set; } = null!;
    public float RenderTimeMs { get; set; }
    public int ChunksRendered { get; set; }

    // World
    public int WorldSeed { get; set; }
    public int MaxLod { get; set; }
    public int RenderDistance { get; set; }

    // Player
    public Transform PlayerTransform { get; set; } = null!;
    public float MoveSpeed { get; set; }
    public Vector3 RespawnPosition { get; set; }

    // Frame
    public float FrameTimeMs { get; set; }
    public int FPS { get; set; }
    
    // Window
    public Vector2D<int> WindowSize { get; set; }
    public Vector2D<int> WindowFrameBufferSize { get; set; }
}
