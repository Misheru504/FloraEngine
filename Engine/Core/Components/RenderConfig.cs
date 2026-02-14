namespace FloraEngine.Core.Components;

public enum RenderMode
{
    Default = 0,
    Depth = 1,
    Normals = 2,
    UV = 3,
    AO = 4,
    Layer = 5,
}

public class RenderConfig
{
    public bool IsGeneratingAOs;
    public bool IsUsingGreedyMeshing;
    public bool IsWireframe;
    public bool IsFreecam;
    public int VertexCount { get; set; }
    public RenderMode RenderMode { get; set; }
}
