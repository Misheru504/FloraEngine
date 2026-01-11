using FloreEngine.World;

namespace FloreEngine.Utils;


public static class BinaryGreedyMesher
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

                Face face = Face.Front;
                if (d == 0) face = b == 0 ? Face.Left : Face.Right;
                else if (d == 1) face = b == 0 ? Face.Bottom : Face.Top;
                else if (d == 2) face = b == 0 ? Face.Back : Face.Front;

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

    private static void AddFace(int[] pos, int[] deltaU, int[] deltaV, int width, int height, Face face, List<float> vertices, List<uint> indices, ref uint vertexOffset, bool flip)
    {
        int x = pos[0], y = pos[1], z = pos[2];
        float[] normals = GetNormals(face);
        float[] UVs = GetUVs(face, width, height);
        float u0 = UVs[0], v0 = UVs[1], u1 = UVs[2], v1 = UVs[3], u2 = UVs[4], v2 = UVs[5], u3 = UVs[6], v3 = UVs[7];

        float[] quadVertices = [
            x + deltaU[0] + deltaV[0], y + deltaU[1] + deltaV[1], z + deltaU[2] + deltaV[2], normals[0], normals[1], normals[2], u0, v0,
            x + deltaU[0],             y + deltaU[1],             z + deltaU[2],             normals[0], normals[1], normals[2], u1, v1,
            x,                         y,                         z,                         normals[0], normals[1], normals[2], u2, v2,
            x + deltaV[0],             y + deltaV[1],             z + deltaV[2],             normals[0], normals[1], normals[2], u3, v3,
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