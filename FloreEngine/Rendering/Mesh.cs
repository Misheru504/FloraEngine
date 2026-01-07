using FloreEngine.World;

namespace FloreEngine.Rendering;

public class Mesh : IDisposable
{
    public float[]? Vertices;
    public uint[]? Indices;

    public void CreateMesh(ref ushort[] voxels, int sideSize)
    {
        List<float> vertices = new List<float>();
        List<uint> indices = new List<uint>();
        uint vertexOffset = 0;

        for (int x = 0; x < sideSize; x++)
        {
            for (int y = 0; y < sideSize; y++)
            {
                for (int z = 0; z < sideSize; z++)
                {
                    if (voxels[Chunk.Index(x,y,z)] == 0) continue;

                    if (IsFaceVisible(ref voxels, sideSize, x, y - 1, z))
                    {
                        float[] bottomVertices = [
                            x+1, y,   z+1, 1.0f, 1.0f,
                            x+1, y,   z,   0.0f, 1.0f,
                            x,   y,   z,   0.0f, 0.0f,
                            x,   y,   z+1, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(bottomVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(ref voxels, sideSize, x, y + 1, z))
                    {
                        float[] topVertices = [
                            x,   y+1, z+1, 1.0f, 1.0f,
                            x,   y+1, z,   0.0f, 1.0f,
                            x+1, y+1, z,   0.0f, 0.0f,
                            x+1, y+1, z+1, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(topVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(ref voxels, sideSize, x - 1, y, z))
                    {
                        float[] leftVertices = [
                            x,   y,   z+1, 1.0f, 1.0f,
                            x,   y,   z,   0.0f, 1.0f,
                            x,   y+1, z,   0.0f, 0.0f,
                            x,   y+1, z+1, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(leftVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(ref voxels, sideSize, x + 1, y, z))
                    {
                        float[] rightVertices = [
                            x+1, y,   z,   1.0f, 1.0f,
                            x+1, y,   z+1, 0.0f, 1.0f,
                            x+1, y+1, z+1, 0.0f, 0.0f,
                            x+1, y+1, z,   1.0f, 0.0f,
                        ];
                        vertices.AddRange(rightVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(ref voxels, sideSize, x, y, z + 1))
                    {
                        float[] rightVertices = [
                            x+1, y,   z+1, 1.0f, 1.0f,
                            x,   y,   z+1, 0.0f, 1.0f,
                            x,   y+1, z+1, 0.0f, 0.0f,
                            x+1, y+1, z+1, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(rightVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(ref voxels, sideSize, x, y, z - 1))
                    {
                        float[] frontVertices = [
                            x,   y,   z,   1.0f, 1.0f,
                            x+1, y,   z,   0.0f, 1.0f,
                            x+1, y+1, z,   0.0f, 0.0f,
                            x,   y+1, z,   1.0f, 0.0f,
                        ];
                        vertices.AddRange(frontVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                }
            }
        }

        Vertices = [..vertices];
        Indices = [..indices];
    }

    private static void AddIndices(List<uint> indices, ref uint vertexOffset)
    {
        uint[] bottomIndices = [
            vertexOffset + 0u,
            vertexOffset + 3u,
            vertexOffset + 1u,

            vertexOffset + 1u,
            vertexOffset + 3u,
            vertexOffset + 2u,
        ];

        vertexOffset += 4;
        indices.AddRange(bottomIndices);
    }

    private static bool IsFaceVisible(ref ushort[] voxels, int sideSize, int x, int y, int z)
    {
        if (x < 0 || x >= sideSize) return true;
        if (y < 0 || y >= sideSize) return true;
        if (z < 0 || z >= sideSize) return true;

        return voxels[Chunk.Index(x, y, z)] == 0;
    }

    public void Dispose()
    {
        Vertices = null;
        Indices = null;
        GC.SuppressFinalize(this);
    }
}
