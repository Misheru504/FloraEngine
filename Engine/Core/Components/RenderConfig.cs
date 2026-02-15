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
    public bool IsGeneratingAOs { get; set; }
    public bool IsUsingGreedyMeshing { get; set; }
    public bool IsWireframe { get; set; }
    public bool IsFreecam { get; set; }
    public int VertexCount { get; set; }
    public RenderMode RenderMode { get; set; }
}
