using FloreEngine.World;
using System.Diagnostics;
using System.Numerics;

namespace FloreEngine.Utils;

public static class CulledMesher
{
    internal static void CreateCulledMesh(Chunk currentChunk, List<float> vertices, List<uint> indices)
    {
        uint vertexOffset = 0;
        int sideSIZE = Chunk.SIZE;

        for (int x = 0; x < sideSIZE; x++)
        {
            for (int y = 0; y < sideSIZE; y++)
            {
                for (int z = 0; z < sideSIZE; z++)
                {
                    if (currentChunk.GetVoxelAt(x, y, z) == 0) continue;

                    if (IsFaceVisible(currentChunk, sideSIZE, x, y - 1, z))
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
                    if (IsFaceVisible(currentChunk, sideSIZE, x, y + 1, z))
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
                    if (IsFaceVisible(currentChunk, sideSIZE, x - 1, y, z))
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
                    if (IsFaceVisible(currentChunk, sideSIZE, x + 1, y, z))
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
                    if (IsFaceVisible(currentChunk, sideSIZE, x, y, z + 1))
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
                    if (IsFaceVisible(currentChunk, sideSIZE, x, y, z - 1))
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
        if (voxelX < 0 || voxelX >= Chunk.SIZE || voxelY < 0 || voxelY >= Chunk.SIZE || voxelZ < 0 || voxelZ >= Chunk.SIZE)
        {
            // Voxel out of bounds
            Vector3 voxelPos = new Vector3(voxelX, voxelY, voxelZ);
            Vector3 worldTilePos = currentChunk.Position + (voxelPos * currentChunk.Scale);
            return WorldManager.Instance.GetVoxelAtWorldPos((int)worldTilePos.X, (int)worldTilePos.Y, (int)worldTilePos.Z, currentChunk.LodLevel) == 0;
        }

        return currentChunk.GetVoxelAt(voxelX, voxelY, voxelZ) == 0;
    }

}
