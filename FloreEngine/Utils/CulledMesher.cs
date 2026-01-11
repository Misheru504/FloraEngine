using FloreEngine.World;
using System.Diagnostics;
using System.Numerics;

namespace FloreEngine.Utils;

public static class CulledMesher
{
    internal static void CreateCulledMesh(Chunk currentChunk, List<float> vertices, List<uint> indices)
    {
        uint vertexOffset = 0;
        int sideSize = Chunk.Size;

        if (currentChunk.Voxels == null) return;

        for (int x = 0; x < sideSize; x++)
        {
            for (int y = 0; y < sideSize; y++)
            {
                for (int z = 0; z < sideSize; z++)
                {
                    if (currentChunk.Voxels[Chunk.Index(x, y, z)] == 0) continue;

                    if (IsFaceVisible(currentChunk, sideSize, x, y - 1, z))
                    {
                        float[] bottomVertices = [
                            x+1, y,   z+1,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f,
                            x+1, y,   z,    0.0f, -1.0f,  0.0f, 0.0f, 1.0f,
                            x,   y,   z,    0.0f, -1.0f,  0.0f, 0.0f, 0.0f,
                            x,   y,   z+1,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(bottomVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, sideSize, x, y + 1, z))
                    {
                        float[] topVertices = [
                            x,   y+1, z+1,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f,
                            x,   y+1, z,    0.0f,  1.0f,  0.0f, 0.0f, 1.0f,
                            x+1, y+1, z,    0.0f,  1.0f,  0.0f, 0.0f, 0.0f,
                            x+1, y+1, z+1,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(topVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, sideSize, x - 1, y, z))
                    {
                        float[] leftVertices = [
                            x,   y,   z+1, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
                            x,   y,   z,   -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
                            x,   y+1, z,   -1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
                            x,   y+1, z+1, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(leftVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, sideSize, x + 1, y, z))
                    {
                        float[] rightVertices = [
                            x+1, y,   z,    1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
                            x+1, y,   z+1,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
                            x+1, y+1, z+1,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
                            x+1, y+1, z,    1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(rightVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, sideSize, x, y, z + 1))
                    {
                        float[] frontVertices = [
                            x+1, y,   z+1,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f,
                            x,   y,   z+1,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,
                            x,   y+1, z+1,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f,
                            x+1, y+1, z+1,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(frontVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, sideSize, x, y, z - 1))
                    {
                        float[] backVertices = [
                            x,   y,   z,    0.0f,  0.0f, -1.0f, 1.0f, 1.0f,
                            x+1, y,   z,    0.0f,  0.0f, -1.0f, 0.0f, 1.0f,
                            x+1, y+1, z,    0.0f,  0.0f, -1.0f, 0.0f, 0.0f,
                            x,   y+1, z,    0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
                        ];
                        vertices.AddRange(backVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                }
            }
        }
    }

    private static void AddIndices(List<uint> indices, ref uint vertexOffset)
    {
        uint[] indicesToAdd = [
            vertexOffset + 0u,
            vertexOffset + 3u,
            vertexOffset + 1u,

            vertexOffset + 1u,
            vertexOffset + 3u,
            vertexOffset + 2u,
        ];

        vertexOffset += 4;
        indices.AddRange(indicesToAdd);
    }

    private static bool IsFaceVisible(Chunk currentChunk, int sideSize, int voxelX, int voxelY, int voxelZ)
    {
        if (voxelX < 0 || voxelX >= Chunk.Size || voxelY < 0 || voxelY >= Chunk.Size || voxelZ < 0 || voxelZ >= Chunk.Size)
        {
            // Voxel out of bounds
            Vector3 voxelPos = new Vector3(voxelX, voxelY, voxelZ);
            Vector3 worldTilePos = currentChunk.Position + (voxelPos * currentChunk.Scale);
            return WorldManager.Instance.GetVoxelAtWorldPos((int)worldTilePos.X, (int)worldTilePos.Y, (int)worldTilePos.Z, currentChunk.WorldSize, currentChunk.Scale) == 0;
        }

        return currentChunk.Voxels?[Chunk.Index(voxelX, voxelY, voxelZ)] == 0;
    }

}
