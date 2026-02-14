using System.Numerics;

namespace FloraEngine.Core.Components;

public class DiagnosticsData
{
    // Rendering
    public RenderConfig RenderConfig { get; set; } = null!;
    public float RenderTimeMs { get; set; }

    // World
    public int WorldSeed { get; set; }
    public int ChunksLoaded { get; set; }

    // Player
    public Transform PlayerTransform { get; set; } = null!;
    public float MoveSpeed { get; set; }
    public Vector3 RespawnPosition { get; set; }

    // Frame
    public float FrameTimeMs { get; set; }
    public int FPS { get; set; }
}
