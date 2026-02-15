using FloraEngine.Core;
using FloraEngine.World;
using System.Numerics;

namespace FloraEngine.Rendering.Meshing;

public static class CulledMesher
{
    internal static void CreateCulledMesh(WorldManager worldManager, Chunk currentChunk, List<float> vertices, List<uint> indices, RenderConfig config)
    {
        uint vertexOffset = 0;
        int sideSize = WorldConstants.CHUNK_SIZE;

        for (int x = 0; x < sideSize; x++)
        {
            for (int y = 0; y < sideSize; y++)
            {
                for (int z = 0; z < sideSize; z++)
                {
                    if (currentChunk.GetVoxelAt(x, y, z).Id == Voxel.AIR.ID) continue;
                    Voxel v = Voxel.GetVoxelByID(currentChunk.GetVoxelAt(x, y, z).Id);

                    if (IsFaceVisible(worldManager, currentChunk, x, y - 1, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x, y-1, z+1), !IsFaceVisible(worldManager, currentChunk, x+1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x, y-1, z-1), !IsFaceVisible(worldManager, currentChunk, x+1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x, y-1, z-1), !IsFaceVisible(worldManager, currentChunk, x-1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x, y-1, z+1), !IsFaceVisible(worldManager, currentChunk, x-1, y-1, z+1))
                        ];

                        if (!config.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] bottomVertices = [
                            x+1, y,   z+1,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f, aos[0], v.ID,
                            x+1, y,   z,    0.0f, -1.0f,  0.0f, 0.0f, 1.0f, aos[1], v.ID,
                            x,   y,   z,    0.0f, -1.0f,  0.0f, 0.0f, 0.0f, aos[2], v.ID,
                            x,   y,   z+1,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f, aos[3], v.ID,
                        ];
                        vertices.AddRange(bottomVertices);

                        AddIndices(indices, ref vertexOffset, aos[0] + aos[2] > aos[1] + aos[3]);
                    }
                    if (IsFaceVisible(worldManager, currentChunk, x, y + 1, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x, y+1, z+1), !IsFaceVisible(worldManager, currentChunk, x-1, y+1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x, y+1, z-1), !IsFaceVisible(worldManager, currentChunk, x-1, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y+1, z), !IsFaceVisible(worldManager,   currentChunk, x, y+1, z-1), !IsFaceVisible(worldManager, currentChunk, x+1, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x, y+1, z+1), !IsFaceVisible(worldManager, currentChunk, x+1, y+1, z+1))
                        ];

                        if (!config.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] topVertices = [
                            x,   y+1, z+1,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f, aos[0], v.ID,
                            x,   y+1, z,    0.0f,  1.0f,  0.0f, 0.0f, 1.0f, aos[1], v.ID,
                            x+1, y+1, z,    0.0f,  1.0f,  0.0f, 0.0f, 0.0f, aos[2], v.ID,
                            x+1, y+1, z+1,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f, aos[3], v.ID,
                        ];
                        vertices.AddRange(topVertices);

                        AddIndices(indices, ref vertexOffset, aos[0] + aos[2] > aos[1] + aos[3]);
                    }
                    if (IsFaceVisible(worldManager, currentChunk, x - 1, y, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x-1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x-1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x-1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x-1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x-1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x-1, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x-1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x-1, y+1, z+1))
                        ];

                        if (!config.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] leftVertices = [
                            x,   y,   z+1, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f, aos[0], v.ID,
                            x,   y,   z,   -1.0f,  0.0f,  0.0f, 0.0f, 1.0f, aos[1], v.ID,
                            x,   y+1, z,   -1.0f,  0.0f,  0.0f, 0.0f, 0.0f, aos[2], v.ID,
                            x,   y+1, z+1, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f, aos[3], v.ID,
                        ];
                        vertices.AddRange(leftVertices);

                        AddIndices(indices, ref vertexOffset, aos[0] + aos[2] > aos[1] + aos[3]);
                    }
                    if (IsFaceVisible(worldManager, currentChunk, x + 1, y, z))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x+1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x+1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y-1, z), !IsFaceVisible(worldManager, currentChunk, x+1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x+1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x+1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x+1, y+1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y+1, z), !IsFaceVisible(worldManager, currentChunk, x+1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x+1, y+1, z-1))
                        ];

                        if (!config.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] rightVertices = [
                            x+1, y,   z,    1.0f,  0.0f,  0.0f, 1.0f, 1.0f, aos[0], v.ID,
                            x+1, y,   z+1,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f, aos[1], v.ID,
                            x+1, y+1, z+1,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f, aos[2], v.ID,
                            x+1, y+1, z,    1.0f,  0.0f,  0.0f, 1.0f, 0.0f, aos[3], v.ID,
                        ];
                        vertices.AddRange(rightVertices);

                        AddIndices(indices, ref vertexOffset, aos[0] + aos[2] > aos[1] + aos[3]);
                    }
                    if (IsFaceVisible(worldManager, currentChunk, x, y, z + 1))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x, y-1, z+1), !IsFaceVisible(worldManager, currentChunk, x+1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x, y-1, z+1), !IsFaceVisible(worldManager, currentChunk, x-1, y-1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x, y+1, z+1), !IsFaceVisible(worldManager, currentChunk, x-1, y+1, z+1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y, z+1), !IsFaceVisible(worldManager, currentChunk, x, y+1, z+1), !IsFaceVisible(worldManager, currentChunk, x+1, y+1, z+1))
                        ];

                        if (!config.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] frontVertices = [
                            x+1, y,   z+1,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f, aos[0], v.ID,
                            x,   y,   z+1,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f, aos[1], v.ID,
                            x,   y+1, z+1,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f, aos[2], v.ID,
                            x+1, y+1, z+1,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f, aos[3], v.ID,
                        ];
                        vertices.AddRange(frontVertices);

                        AddIndices(indices, ref vertexOffset, aos[0] + aos[2] > aos[1] + aos[3]);
                    }
                    if (IsFaceVisible(worldManager, currentChunk, x, y, z - 1))
                    {
                        float[] aos = [
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x, y-1, z-1), !IsFaceVisible(worldManager, currentChunk, x-1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x, y-1, z-1), !IsFaceVisible(worldManager, currentChunk, x+1, y-1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x+1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x, y+1, z-1), !IsFaceVisible(worldManager, currentChunk, x+1, y+1, z-1)),
                            ComputeVertexAO(!IsFaceVisible(worldManager, currentChunk, x-1, y, z-1), !IsFaceVisible(worldManager, currentChunk, x, y+1, z-1), !IsFaceVisible(worldManager, currentChunk, x-1, y+1, z-1))
                        ];

                        if (!config.IsGeneratingAOs)
                        {
                            aos[0] = 1f;
                            aos[1] = 1f;
                            aos[2] = 1f;
                            aos[3] = 1f;
                        }

                        float[] backVertices = [
                            x,   y,   z,    0.0f,  0.0f, -1.0f, 1.0f, 1.0f, aos[0], v.ID,
                            x+1, y,   z,    0.0f,  0.0f, -1.0f, 0.0f, 1.0f, aos[1], v.ID,
                            x+1, y+1, z,    0.0f,  0.0f, -1.0f, 0.0f, 0.0f, aos[2], v.ID,
                            x,   y+1, z,    0.0f,  0.0f, -1.0f, 1.0f, 0.0f, aos[3], v.ID,
                        ];
                        vertices.AddRange(backVertices);

                        AddIndices(indices, ref vertexOffset, aos[0] + aos[2] > aos[1] + aos[3]);
                    }
                }
            }
        }
    }

    private static void AddIndices(List<uint> indices, ref uint vertexOffset, bool flip = false)
    {
        uint[] indicesToAdd;
        if (flip)
            indicesToAdd  = [
                vertexOffset + 0u,
                vertexOffset + 2u,
                vertexOffset + 1u,

                vertexOffset + 0u,
                vertexOffset + 3u,
                vertexOffset + 2u,
            ];
        else
            indicesToAdd = [
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

    private static bool TryGetVoxelOut(WorldManager worldManager, Chunk currentChunk, int voxelX, int voxelY, int voxelZ, out ushort voxel)
    {
        if (voxelX < 0 || voxelX >= WorldConstants.CHUNK_SIZE || voxelY < 0 || voxelY >= WorldConstants.CHUNK_SIZE || voxelZ < 0 || voxelZ >= WorldConstants.CHUNK_SIZE)
        {
            // Voxel out of bounds
            Vector3 voxelPos = new Vector3(voxelX, voxelY, voxelZ);
            Vector3 worldTilePos = currentChunk.Position + voxelPos * currentChunk.Scale;
            voxel = worldManager.GetVoxelIdAtWorldPos((int)worldTilePos.X, (int)worldTilePos.Y, (int)worldTilePos.Z, currentChunk.LodLevel);
            return true;
        }

        voxel = currentChunk.GetVoxelAt(voxelX, voxelY, voxelZ).Id;
        return false;
    }

    private static bool IsFaceVisible(WorldManager worldManager, Chunk currentChunk, int voxelX, int voxelY, int voxelZ)
    {
        TryGetVoxelOut(worldManager, currentChunk, voxelX, voxelY, voxelZ, out ushort voxel);
        return voxel == Voxel.AIR.ID;
    }

    private static float ComputeVertexAO(bool side1, bool side2, bool corner)
    {
        if (side1 && side2) return 0;

        int ao = 3 - (side1 ? 1 : 0) - (side2 ? 1 : 0) - (corner ? 1 : 0);

        return ao / 3.0f; // Normalize
    }
}
