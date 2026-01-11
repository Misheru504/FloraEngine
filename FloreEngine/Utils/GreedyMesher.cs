using FloreEngine.World;

namespace FloreEngine.Utils;

public static class BinaryGreedyMesher
{
    enum FaceType
    {
        Left, 
        Right, 
        Top, 
        Bottom, 
        Back, 
        Front
    }

    private static float[] GetNormals(FaceType type)
    {
        return type switch
        {
            FaceType.Left => [-1.0f, 0.0f, 0.0f],
            FaceType.Right => [1.0f, 0.0f, 0.0f],
            FaceType.Bottom => [0.0f, -1.0f, 0.0f],
            FaceType.Top => [0.0f, 1.0f, 0.0f],
            FaceType.Back => [0.0f, 0.0f, -1.0f],
            FaceType.Front => [0.0f, 0.0f, 1.0f],
            _ => [0.0f, 0.0f, 0.0f]
        };
    }

    internal static void CreateGreedyMesh(Chunk currentChunk, List<float> vertices, List<uint> indices)
    {
        if(currentChunk.Voxels == null) return;

        // TODO: Chunk culling

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

                ushort[] mask = new ushort[Chunk.Size * Chunk.Size];
                q[d] = 1;

                FaceType face = FaceType.Front;
                if (d == 0) face = b == 0 ? FaceType.Left : FaceType.Right;
                else if (d == 1) face = b == 0 ? FaceType.Bottom : FaceType.Top;
                else if (d == 2) face = b == 0 ? FaceType.Back : FaceType.Front;

                pos[d] = -1;

                while (pos[d] < Chunk.Size)
                {
                    int maskIndex = 0;
                    pos[v] = 0;
                    while (pos[v] < Chunk.Size)
                    {
                        pos[u] = 0;
                        while (pos[u] < Chunk.Size)
                        {
                            ushort current;
                            ushort compare;

                            if (pos[d] >= 0) current = currentChunk.Voxels[Chunk.Index(pos[0], pos[1], pos[2])];
                            else current = 0;

                            if (pos[d] < Chunk.Size - 1) compare = currentChunk.Voxels[Chunk.Index(pos[0] + q[0], pos[1] + q[1], pos[2] + q[2])];
                            else compare = 0;

                            if (b == 0)
                            {
                                if (current == 0 && compare != 0) mask[maskIndex] = compare;
                                else mask[maskIndex] = 0;
                            }
                            else
                            {
                                if (current != 0 && compare == 0) mask[maskIndex] = current;
                                else mask[maskIndex] = 0;
                            }

                            maskIndex++;
                            pos[u]++;
                        }
                        pos[v]++;
                    }
                    pos[d]++;
                    maskIndex = 0;

                    for (int j = 0; j < Chunk.Size; j++)
                    {
                        int i = 0;
                        while (i < Chunk.Size)
                        {
                            ushort voxel = mask[maskIndex];

                            if (voxel == 0)
                            {
                                maskIndex += 1;
                                i += 1;
                                continue;
                            }

                            width = 1;
                            while (i + width < Chunk.Size && voxel == mask[maskIndex + width])
                            {
                                width++;
                            }

                            bool done = false;
                            height = 1;

                            while (height + j < Chunk.Size)
                            {
                                for(int k = 0; k < width; k++)
                                {
                                    if (voxel != mask[maskIndex + k + height * Chunk.Size])
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

                            AddFace(pos, deltaU, deltaV, width, height, face, vertices, indices, ref vertexOffset, b == 0);

                            for (int l = 0; l < height; l++)
                                for (int k = 0; k < width; k++)
                                {
                                    mask[maskIndex + k + l * Chunk.Size] = 0;
                                }

                            i += width;
                            maskIndex += width;
                        }
                    }
                }
            }
        }
    }

    private static void AddFace(int[] pos, int[] deltaU, int[] deltaV, int width, int height, FaceType face, List<float> vertices, List<uint> indices, ref uint vertexOffset, bool flip)
    {
        int x = pos[0], y = pos[1], z = pos[2];
        float[] normals = GetNormals(face);

        // Fix UV coordinates
        float[] quadVertices = [
            x + deltaU[0] + deltaV[0], y + deltaU[1] + deltaV[1], z + deltaU[2] + deltaV[2], normals[0], normals[1], normals[2], width, height,
            x + deltaU[0],             y + deltaU[1],             z + deltaU[2],             normals[0], normals[1], normals[2], width, 0.0f,
            x,                         y,                         z,                         normals[0], normals[1], normals[2], 0.0f, 0.0f,
            x + deltaV[0],             y + deltaV[1],             z + deltaV[2],             normals[0], normals[1], normals[2], 0.0f, height,
        ];
        vertices.AddRange(quadVertices);

        AddIndices(indices, ref vertexOffset, flip);
    }

    private static void AddIndices(List<uint> indices, ref uint vertexOffset, bool flip)
    {
        uint[] indicesToAdd;
        if (flip)
        {
            indicesToAdd = [
                vertexOffset + 0u,
                vertexOffset + 1u,
                vertexOffset + 3u,
                vertexOffset + 1u,
                vertexOffset + 2u,
                vertexOffset + 3u,
            ];
        }
        else
        {
            indicesToAdd = [
                vertexOffset + 0u,
                vertexOffset + 3u,
                vertexOffset + 1u,
                vertexOffset + 1u,
                vertexOffset + 3u,
                vertexOffset + 2u,
            ];
        }

        vertexOffset += 4;
        indices.AddRange(indicesToAdd);
    }
}