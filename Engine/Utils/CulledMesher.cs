using FloraEngine.Rendering;
using FloraEngine.World;
using System.Numerics;

namespace FloraEngine.Utils;

public static class CulledMesher
{
    internal static void CreateCulledMesh(Chunk currentChunk, List<float> vertices, List<uint> indices)
    {
        uint vertexOffset = 0;
        int sideSize = Chunk.SIZE;

        for (int x = 0; x < sideSize; x++)
        {
            for (int y = 0; y < sideSize; y++)
            {
                for (int z = 0; z < sideSize; z++)
                {
                    if (currentChunk.GetVoxelAt(x, y, z).ID == Voxel.AIR.ID) continue;

                    if (IsFaceVisible(currentChunk, x, y - 1, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y, z), !IsFaceVisible(currentChunk, x, y, z+1), !IsFaceVisible(currentChunk, x+1, y, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y, z), !IsFaceVisible(currentChunk, x, y, z-1), !IsFaceVisible(currentChunk, x+1, y, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y, z), !IsFaceVisible(currentChunk, x, y, z-1), !IsFaceVisible(currentChunk, x-1, y, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y, z), !IsFaceVisible(currentChunk, x, y, z+1), !IsFaceVisible(currentChunk, x-1, y, z+1))
                        ];

                        if (!Renderer.Instance.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] bottomVertices = [
                            x+1, y,   z+1,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f, aos[0],
                            x+1, y,   z,    0.0f, -1.0f,  0.0f, 0.0f, 1.0f, aos[1],
                            x,   y,   z,    0.0f, -1.0f,  0.0f, 0.0f, 0.0f, aos[2],
                            x,   y,   z+1,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f, aos[3]
                        ];
                        vertices.AddRange(bottomVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, x, y + 1, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y+1, z), !IsFaceVisible(currentChunk, x, y+1, z+1), !IsFaceVisible(currentChunk, x-1, y+1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y+1, z), !IsFaceVisible(currentChunk, x, y+1, z-1), !IsFaceVisible(currentChunk, x-1, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y+1, z), !IsFaceVisible(currentChunk, x, y+1, z-1), !IsFaceVisible(currentChunk, x+1, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y+1, z), !IsFaceVisible(currentChunk, x, y+1, z+1), !IsFaceVisible(currentChunk, x+1, y+1, z+1))
                        ];

                        if (!Renderer.Instance.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] topVertices = [
                            x,   y+1, z+1,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f, aos[0],
                            x,   y+1, z,    0.0f,  1.0f,  0.0f, 0.0f, 1.0f, aos[1],
                            x+1, y+1, z,    0.0f,  1.0f,  0.0f, 0.0f, 0.0f, aos[2],
                            x+1, y+1, z+1,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f, aos[3],
                        ];
                        vertices.AddRange(topVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, x - 1, y, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x, y-1, z), !IsFaceVisible(currentChunk, x, y, z+1), !IsFaceVisible(currentChunk, x, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x, y-1, z), !IsFaceVisible(currentChunk, x, y, z-1), !IsFaceVisible(currentChunk, x, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x, y+1, z), !IsFaceVisible(currentChunk, x, y, z-1), !IsFaceVisible(currentChunk, x, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x, y+1, z), !IsFaceVisible(currentChunk, x, y, z+1), !IsFaceVisible(currentChunk, x, y+1, z+1))
                        ];

                        if (!Renderer.Instance.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] leftVertices = [
                            x,   y,   z+1, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f, aos[0],
                            x,   y,   z,   -1.0f,  0.0f,  0.0f, 0.0f, 1.0f, aos[1],
                            x,   y+1, z,   -1.0f,  0.0f,  0.0f, 0.0f, 0.0f, aos[2],
                            x,   y+1, z+1, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f, aos[3],
                        ];
                        vertices.AddRange(leftVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, x + 1, y, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y-1, z), !IsFaceVisible(currentChunk, x+1, y, z-1), !IsFaceVisible(currentChunk, x+1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y-1, z), !IsFaceVisible(currentChunk, x+1, y, z+1), !IsFaceVisible(currentChunk, x+1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y+1, z), !IsFaceVisible(currentChunk, x+1, y, z+1), !IsFaceVisible(currentChunk, x+1, y+1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y+1, z), !IsFaceVisible(currentChunk, x+1, y, z-1), !IsFaceVisible(currentChunk, x+1, y+1, z-1))
                        ];

                        if (!Renderer.Instance.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] rightVertices = [
                            x+1, y,   z,    1.0f,  0.0f,  0.0f, 1.0f, 1.0f, aos[0],
                            x+1, y,   z+1,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f, aos[1],
                            x+1, y+1, z+1,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f, aos[2],
                            x+1, y+1, z,    1.0f,  0.0f,  0.0f, 1.0f, 0.0f, aos[3],
                        ];
                        vertices.AddRange(rightVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, x, y, z + 1))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y, z+1), !IsFaceVisible(currentChunk, x, y-1, z+1), !IsFaceVisible(currentChunk, x+1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y, z+1), !IsFaceVisible(currentChunk, x, y-1, z+1), !IsFaceVisible(currentChunk, x-1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y, z+1), !IsFaceVisible(currentChunk, x, y+1, z+1), !IsFaceVisible(currentChunk, x-1, y+1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y, z+1), !IsFaceVisible(currentChunk, x, y+1, z+1), !IsFaceVisible(currentChunk, x+1, y+1, z+1))
                        ];

                        if (!Renderer.Instance.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] frontVertices = [
                            x+1, y,   z+1,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f, aos[0],
                            x,   y,   z+1,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f, aos[1],
                            x,   y+1, z+1,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f, aos[2],
                            x+1, y+1, z+1,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f, aos[3],
                        ];
                        vertices.AddRange(frontVertices);

                        AddIndices(indices, ref vertexOffset);
                    }
                    if (IsFaceVisible(currentChunk, x, y, z - 1))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y, z), !IsFaceVisible(currentChunk, x, y-1, z), !IsFaceVisible(currentChunk, x-1, y-1, z)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y, z), !IsFaceVisible(currentChunk, x, y-1, z), !IsFaceVisible(currentChunk, x+1, y-1, z)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x+1, y, z), !IsFaceVisible(currentChunk, x, y+1, z), !IsFaceVisible(currentChunk, x+1, y+1, z)),
                            ComputeVertexAO(!IsFaceVisible(currentChunk, x-1, y, z), !IsFaceVisible(currentChunk, x, y+1, z), !IsFaceVisible(currentChunk, x-1, y+1, z))
                        ];

                        if (!Renderer.Instance.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] backVertices = [
                            x,   y,   z,    0.0f,  0.0f, -1.0f, 1.0f, 1.0f, aos[0],
                            x+1, y,   z,    0.0f,  0.0f, -1.0f, 0.0f, 1.0f, aos[1],
                            x+1, y+1, z,    0.0f,  0.0f, -1.0f, 0.0f, 0.0f, aos[2],
                            x,   y+1, z,    0.0f,  0.0f, -1.0f, 1.0f, 0.0f, aos[3],
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
        uint[] indicesToAdd  = [
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

    private static bool TryGetVoxelOut(Chunk currentChunk, int voxelX, int voxelY, int voxelZ, out ushort voxel)
    {
        if (voxelX < 0 || voxelX >= Chunk.SIZE || voxelY < 0 || voxelY >= Chunk.SIZE || voxelZ < 0 || voxelZ >= Chunk.SIZE)
        {
            // Voxel out of bounds
            Vector3 voxelPos = new Vector3(voxelX, voxelY, voxelZ);
            Vector3 worldTilePos = currentChunk.Position + (voxelPos * currentChunk.Scale);
            voxel = WorldManager.Instance.GetVoxelIdAtWorldPos((int)worldTilePos.X, (int)worldTilePos.Y, (int)worldTilePos.Z, currentChunk.LodLevel);
            return true;
        }

        voxel = currentChunk.GetVoxelAt(voxelX, voxelY, voxelZ).ID;
        return false;
    }

    private static bool IsFaceVisible(Chunk currentChunk, int voxelX, int voxelY, int voxelZ)
    {
        TryGetVoxelOut(currentChunk, voxelX, voxelY, voxelZ, out ushort voxel);
        return voxel == Voxel.AIR.ID;
    }

    private static float ComputeVertexAO(bool side1, bool side2, bool corner)
    {
        if (side1 && side2) return 0;

        int ao = 3 - (side1 ? 1 : 0) - (side2 ? 1 : 0) - (corner ? 1 : 0);

        return (float)ao / 3.0f; // Normalize
    }
}
