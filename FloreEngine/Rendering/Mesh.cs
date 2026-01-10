using FloreEngine.Utils;
using FloreEngine.World;

namespace FloreEngine.Rendering;

public class Mesh : IDisposable
{
    public float[] Vertices;
    public uint[] Indices;
    public uint IndexCount;
    public int VertexCount;

    public Mesh(ushort[] voxels, int sideSize)
    {
        List<float> vertices = new List<float>();
        List< uint> indices = new List<uint>();

        CulledMesher.CreateCulledMesh(voxels, sideSize, vertices, indices);
        //BinaryGreedyMesher.GenerateMesh(voxels, sideSize, vertices, indices);

        VertexCount = vertices.Count / MainRenderer.VertexStride;
        IndexCount = (uint)indices.Count;

        Vertices = [..vertices];
        Indices = [..indices];
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
