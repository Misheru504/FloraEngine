using FloraEngine.Rendering;
using FloraEngine.World;
using System.Numerics;

namespace FloraEngine.Utils;

public static class GreedyMesher
{
    enum Face
    {
        Left,
        Right,
        Top,
        Bottom,
        Back,
        Front
    }

    private struct FaceMask
    {
        public ushort VoxelId;
        public float AO0, AO1, AO2, AO3;

        public static FaceMask Air => new FaceMask { VoxelId = Voxel.AIR.ID, AO0 = 1f, AO1 = 1f, AO2 = 1f, AO3 = 1f };

        public bool IsAir => VoxelId == Voxel.AIR.ID;

        public bool CanMergeWith(FaceMask other)
        {
            return VoxelId == other.VoxelId &&
                   AO0 == other.AO0 && AO1 == other.AO1 &&
                   AO2 == other.AO2 && AO3 == other.AO3;
        }
    }

    private static float[] GetNormals(Face type)
    {
        return type switch
        {
            Face.Left => [-1.0f, 0.0f, 0.0f],
            Face.Right => [1.0f, 0.0f, 0.0f],
            Face.Bottom => [0.0f, -1.0f, 0.0f],
            Face.Top => [0.0f, 1.0f, 0.0f],
            Face.Back => [0.0f, 0.0f, -1.0f],
            Face.Front => [0.0f, 0.0f, 1.0f],
            _ => [0.0f, 0.0f, 0.0f]
        };
    }

    private static float[] GetUVs(Face type, int width, int height)
    {
        return type switch
        {
            Face.Left => [height, 0, 0, 0, 0, width, height, width],
            Face.Right => [height, 0, 0, 0, 0, width, height, width],

            Face.Front => [0, 0, 0, height, width, height, width, 0],
            Face.Back => [0, 0, 0, height, width, height, width, 0],

            Face.Top => [width, height, width, 0, 0, 0, 0, height],
            Face.Bottom => [width, height, width, 0, 0, 0, 0, height],

            _ => [0, 0, 0, 0, 0, 0, 0, 0]
        };
    }

    internal static void CreateGreedyMesh(Chunk currentChunk, List<float> vertices, List<uint> indices)
    {
        uint vertexOffset = 0;

        for (int b = 0; b < 2; b++)
        {
            for (int d = 0; d < 3; d++)
            {
                int width, height;
                int u = (d + 1) % 3;
                int v = (d + 2) % 3;
                int[] pos = [0, 0, 0];
                int[] q = [0, 0, 0];

                FaceMask[] mask = new FaceMask[Chunk.SIZE * Chunk.SIZE];
                q[d] = 1;

                Face face = Face.Front;
                if (d == 0) face = b == 0 ? Face.Left : Face.Right;
                else if (d == 1) face = b == 0 ? Face.Bottom : Face.Top;
                else if (d == 2) face = b == 0 ? Face.Back : Face.Front;

                pos[d] = -1;

                while (pos[d] < Chunk.SIZE)
                {
                    int maskIndex = 0;
                    pos[v] = 0;
                    while (pos[v] < Chunk.SIZE)
                    {
                        pos[u] = 0;
                        while (pos[u] < Chunk.SIZE)
                        {
                            ushort current;
                            ushort compare;

                            Vector3 voxelPos = new Vector3(pos[0], pos[1], pos[2]);
                            Vector3 comparePos = new Vector3(pos[0] + q[0], pos[1] + q[1], pos[2] + q[2]);
                            Vector3 worldVoxelPos = currentChunk.Position + (voxelPos * currentChunk.Scale);
                            Vector3 worldComparePos = currentChunk.Position + (comparePos * currentChunk.Scale);

                            if (pos[d] >= 0) current = currentChunk.GetVoxelAt(pos[0], pos[1], pos[2]).id;
                            else current = WorldManager.Instance.GetVoxelIdAtWorldPos((int)worldVoxelPos.X, (int)worldVoxelPos.Y, (int)worldVoxelPos.Z, currentChunk.LodLevel);

                            if (pos[d] < Chunk.SIZE - 1) compare = currentChunk.GetVoxelAt(pos[0] + q[0], pos[1] + q[1], pos[2] + q[2]).id;
                            else compare = WorldManager.Instance.GetVoxelIdAtWorldPos((int)worldComparePos.X, (int)worldComparePos.Y, (int)worldComparePos.Z, currentChunk.LodLevel);

                            if (b == 0)
                            {
                                if (current == Voxel.AIR.ID && compare != Voxel.AIR.ID)
                                {
                                    int faceX = pos[0] + q[0];
                                    int faceY = pos[1] + q[1];
                                    int faceZ = pos[2] + q[2];
                                    float[] aos = ComputeFaceAOs(currentChunk, faceX, faceY, faceZ, face);
                                    mask[maskIndex] = new FaceMask { VoxelId = compare, AO0 = aos[0], AO1 = aos[1], AO2 = aos[2], AO3 = aos[3] };
                                }
                                else
                                    mask[maskIndex] = FaceMask.Air;
                            }
                            else
                            {
                                if (current != Voxel.AIR.ID && compare == Voxel.AIR.ID)
                                {
                                    float[] aos = ComputeFaceAOs(currentChunk, pos[0], pos[1], pos[2], face);
                                    mask[maskIndex] = new FaceMask { VoxelId = current, AO0 = aos[0], AO1 = aos[1], AO2 = aos[2], AO3 = aos[3] };
                                }
                                else
                                    mask[maskIndex] = FaceMask.Air;
                            }

                            maskIndex++;
                            pos[u]++;
                        }
                        pos[v]++;
                    }
                    pos[d]++;
                    maskIndex = 0;

                    for (int j = 0; j < Chunk.SIZE; j++)
                    {
                        int i = 0;
                        while (i < Chunk.SIZE)
                        {
                            FaceMask faceMask = mask[maskIndex];

                            if (faceMask.IsAir)
                            {
                                maskIndex += 1;
                                i += 1;
                                continue;
                            }

                            width = 1;
                            while (i + width < Chunk.SIZE && faceMask.CanMergeWith(mask[maskIndex + width]))
                            {
                                width++;
                            }

                            bool done = false;
                            height = 1;

                            while (height + j < Chunk.SIZE)
                            {
                                for (int k = 0; k < width; k++)
                                {
                                    if (!faceMask.CanMergeWith(mask[maskIndex + k + height * Chunk.SIZE]))
                                    {
                                        done = true;
                                        break;
                                    }
                                }

                                if (done) break;
                                height++;
                            }

                            int[] deltaU = new int[3] { 0, 0, 0 };
                            int[] deltaV = new int[3] { 0, 0, 0 };

                            deltaU[u] = width;
                            deltaV[v] = height;

                            pos[u] = i;
                            pos[v] = j;

                            float[] aos = [faceMask.AO0, faceMask.AO1, faceMask.AO2, faceMask.AO3];
                            AddFace(currentChunk, pos, deltaU, deltaV, width, height, face, vertices, indices, ref vertexOffset, b == 0, aos);

                            for (int l = 0; l < height; l++)
                                for (int k = 0; k < width; k++)
                                {
                                    mask[maskIndex + k + l * Chunk.SIZE] = FaceMask.Air;
                                }

                            i += width;
                            maskIndex += width;
                        }
                    }
                }
            }
        }
    }

    private static void AddFace(Chunk currentChunk, int[] pos, int[] deltaU, int[] deltaV, int width, int height, Face face, List<float> vertices, List<uint> indices, ref uint vertexOffset, bool windingFlip, float[] aos)
    {
        int x = pos[0], y = pos[1], z = pos[2];
        float[] normals = GetNormals(face);
        float[] UVs = GetUVs(face, width, height);
        float u0 = UVs[0], v0 = UVs[1], u1 = UVs[2], v1 = UVs[3], u2 = UVs[4], v2 = UVs[5], u3 = UVs[6], v3 = UVs[7];

        float[] quadVertices = [
            x + deltaU[0] + deltaV[0], y + deltaU[1] + deltaV[1], z + deltaU[2] + deltaV[2], normals[0], normals[1], normals[2], u0, v0, aos[0],
            x + deltaU[0],             y + deltaU[1],             z + deltaU[2],             normals[0], normals[1], normals[2], u1, v1, aos[1],
            x,                         y,                         z,                         normals[0], normals[1], normals[2], u2, v2, aos[2],
            x + deltaV[0],             y + deltaV[1],             z + deltaV[2],             normals[0], normals[1], normals[2], u3, v3, aos[3]
        ];
        vertices.AddRange(quadVertices);

        bool aoFlip = aos[0] + aos[2] > aos[1] + aos[3];
        AddIndices(indices, ref vertexOffset, windingFlip, aoFlip);
    }

    private static bool IsFaceVisible(Chunk currentChunk, int voxelX, int voxelY, int voxelZ)
    {
        if (voxelX < 0 || voxelX >= Chunk.SIZE || voxelY < 0 || voxelY >= Chunk.SIZE || voxelZ < 0 || voxelZ >= Chunk.SIZE)
        {
            Vector3 voxelPos = new Vector3(voxelX, voxelY, voxelZ);
            Vector3 worldTilePos = currentChunk.Position + (voxelPos * currentChunk.Scale);
            return WorldManager.Instance.GetVoxelIdAtWorldPos((int)worldTilePos.X, (int)worldTilePos.Y, (int)worldTilePos.Z, currentChunk.LodLevel) == Voxel.AIR.ID;
        }

        return currentChunk.GetVoxelAt(voxelX, voxelY, voxelZ).id == Voxel.AIR.ID;
    }

    private static float ComputeVertexAO(bool side1, bool side2, bool corner)
    {
        if (side1 && side2) return 0;

        int ao = 3 - (side1 ? 1 : 0) - (side2 ? 1 : 0) - (corner ? 1 : 0);

        return (float)ao / 3.0f;
    }

    private static float[] ComputeFaceAOs(Chunk currentChunk, int x, int y, int z, Face face)
    {
        if (!Renderer.IsGeneratingAOs)
        {
            return [1f, 1f, 1f, 1f];
        }

        float[] aos = new float[4];

        switch (face)
        {
            case Face.Bottom: // -Y, d=1, u=Z, v=X
                // Vertices: V0(x+1,y,z+1), V1(x,y,z+1), V2(x,y,z), V3(x+1,y,z)
                aos[0] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y - 1, z), !IsFaceVisible(currentChunk, x, y - 1, z + 1), !IsFaceVisible(currentChunk, x + 1, y - 1, z + 1));
                aos[1] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y - 1, z), !IsFaceVisible(currentChunk, x, y - 1, z + 1), !IsFaceVisible(currentChunk, x - 1, y - 1, z + 1));
                aos[2] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y - 1, z), !IsFaceVisible(currentChunk, x, y - 1, z - 1), !IsFaceVisible(currentChunk, x - 1, y - 1, z - 1));
                aos[3] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y - 1, z), !IsFaceVisible(currentChunk, x, y - 1, z - 1), !IsFaceVisible(currentChunk, x + 1, y - 1, z - 1));
                break;

            case Face.Top: // +Y, d=1, u=Z, v=X
                // Vertices: V0(x+1,y+1,z+1), V1(x,y+1,z+1), V2(x,y+1,z), V3(x+1,y+1,z)
                aos[0] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y + 1, z), !IsFaceVisible(currentChunk, x, y + 1, z + 1), !IsFaceVisible(currentChunk, x + 1, y + 1, z + 1));
                aos[1] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y + 1, z), !IsFaceVisible(currentChunk, x, y + 1, z + 1), !IsFaceVisible(currentChunk, x - 1, y + 1, z + 1));
                aos[2] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y + 1, z), !IsFaceVisible(currentChunk, x, y + 1, z - 1), !IsFaceVisible(currentChunk, x - 1, y + 1, z - 1));
                aos[3] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y + 1, z), !IsFaceVisible(currentChunk, x, y + 1, z - 1), !IsFaceVisible(currentChunk, x + 1, y + 1, z - 1));
                break;

            case Face.Left: // -X, d=0, u=Y, v=Z
                // Vertices: V0(x,y+1,z+1), V1(x,y+1,z), V2(x,y,z), V3(x,y,z+1)
                aos[0] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y + 1, z), !IsFaceVisible(currentChunk, x - 1, y, z + 1), !IsFaceVisible(currentChunk, x - 1, y + 1, z + 1));
                aos[1] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y + 1, z), !IsFaceVisible(currentChunk, x - 1, y, z - 1), !IsFaceVisible(currentChunk, x - 1, y + 1, z - 1));
                aos[2] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y - 1, z), !IsFaceVisible(currentChunk, x - 1, y, z - 1), !IsFaceVisible(currentChunk, x - 1, y - 1, z - 1));
                aos[3] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y - 1, z), !IsFaceVisible(currentChunk, x - 1, y, z + 1), !IsFaceVisible(currentChunk, x - 1, y - 1, z + 1));
                break;

            case Face.Right: // +X, d=0, u=Y, v=Z
                // Vertices: V0(x+1,y+1,z+1), V1(x+1,y+1,z), V2(x+1,y,z), V3(x+1,y,z+1)
                aos[0] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y + 1, z), !IsFaceVisible(currentChunk, x + 1, y, z + 1), !IsFaceVisible(currentChunk, x + 1, y + 1, z + 1));
                aos[1] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y + 1, z), !IsFaceVisible(currentChunk, x + 1, y, z - 1), !IsFaceVisible(currentChunk, x + 1, y + 1, z - 1));
                aos[2] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y - 1, z), !IsFaceVisible(currentChunk, x + 1, y, z - 1), !IsFaceVisible(currentChunk, x + 1, y - 1, z - 1));
                aos[3] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y - 1, z), !IsFaceVisible(currentChunk, x + 1, y, z + 1), !IsFaceVisible(currentChunk, x + 1, y - 1, z + 1));
                break;

            case Face.Back: // -Z, d=2, u=X, v=Y
                // Vertices: V0(x+1,y+1,z), V1(x+1,y,z), V2(x,y,z), V3(x,y+1,z)
                aos[0] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y, z - 1), !IsFaceVisible(currentChunk, x, y + 1, z - 1), !IsFaceVisible(currentChunk, x + 1, y + 1, z - 1));
                aos[1] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y, z - 1), !IsFaceVisible(currentChunk, x, y - 1, z - 1), !IsFaceVisible(currentChunk, x + 1, y - 1, z - 1));
                aos[2] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y, z - 1), !IsFaceVisible(currentChunk, x, y - 1, z - 1), !IsFaceVisible(currentChunk, x - 1, y - 1, z - 1));
                aos[3] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y, z - 1), !IsFaceVisible(currentChunk, x, y + 1, z - 1), !IsFaceVisible(currentChunk, x - 1, y + 1, z - 1));
                break;

            case Face.Front: // +Z, d=2, u=X, v=Y
                // Vertices: V0(x+1,y+1,z+1), V1(x+1,y,z+1), V2(x,y,z+1), V3(x,y+1,z+1)
                aos[0] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y, z + 1), !IsFaceVisible(currentChunk, x, y + 1, z + 1), !IsFaceVisible(currentChunk, x + 1, y + 1, z + 1));
                aos[1] = ComputeVertexAO(!IsFaceVisible(currentChunk, x + 1, y, z + 1), !IsFaceVisible(currentChunk, x, y - 1, z + 1), !IsFaceVisible(currentChunk, x + 1, y - 1, z + 1));
                aos[2] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y, z + 1), !IsFaceVisible(currentChunk, x, y - 1, z + 1), !IsFaceVisible(currentChunk, x - 1, y - 1, z + 1));
                aos[3] = ComputeVertexAO(!IsFaceVisible(currentChunk, x - 1, y, z + 1), !IsFaceVisible(currentChunk, x, y + 1, z + 1), !IsFaceVisible(currentChunk, x - 1, y + 1, z + 1));
                break;
        }

        return aos;
    }

    private static void AddIndices(List<uint> indices, ref uint vertexOffset, bool windingFlip, bool aoFlip)
    {
        uint[] indicesToAdd;

        // windingFlip: controls triangle winding order (for backface culling)
        // aoFlip: controls which diagonal to use (for AO interpolation quality)

        if (!windingFlip && !aoFlip)
        {
            // CCW winding, diagonal 1-3
            indicesToAdd = [
                vertexOffset + 0u,
                vertexOffset + 3u,
                vertexOffset + 1u,
                vertexOffset + 1u,
                vertexOffset + 3u,
                vertexOffset + 2u,
            ];
        }
        else if (windingFlip && !aoFlip)
        {
            // CW winding, diagonal 1-3
            indicesToAdd = [
                vertexOffset + 0u,
                vertexOffset + 1u,
                vertexOffset + 3u,
                vertexOffset + 1u,
                vertexOffset + 2u,
                vertexOffset + 3u,
            ];
        }
        else if (!windingFlip && aoFlip)
        {
            // CCW winding, diagonal 0-2
            indicesToAdd = [
                vertexOffset + 0u,
                vertexOffset + 2u,
                vertexOffset + 1u,
                vertexOffset + 0u,
                vertexOffset + 3u,
                vertexOffset + 2u,
            ];
        }
        else // windingFlip && aoFlip
        {
            // CW winding, diagonal 0-2
            indicesToAdd = [
                vertexOffset + 0u,
                vertexOffset + 1u,
                vertexOffset + 2u,
                vertexOffset + 0u,
                vertexOffset + 2u,
                vertexOffset + 3u,
            ];
        }

        vertexOffset += 4;
        indices.AddRange(indicesToAdd);
    }
}