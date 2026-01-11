using FloreEngine.World;
using System.Numerics;

namespace FloreEngine.Utils;

public static class MathUtils
{
    public static float DegreesToRadians(float degrees) => MathF.PI / 180f * degrees;
    public static int ManhattanDistance(Vector3 pointA, Vector3 pointB)
    {
        return (int) (Math.Abs(pointB.X - pointA.X) + Math.Abs(pointB.Y - pointA.Y) + Math.Abs(pointB.Z - pointA.Z));
    }

    public static Vector3 WorldToChunkCoord(Vector3 worldCoord, int chunkSize)
    {
        return new Vector3(
            MathF.Floor(worldCoord.X / chunkSize),
            MathF.Floor(worldCoord.Y / chunkSize),
            MathF.Floor(worldCoord.Z / chunkSize)
        ) * chunkSize;
    }

    public static Vector3 WorldToTilePosition(Vector3 worldPostion)
    {
        int localX = (int)Math.Floor(worldPostion.X) % Chunk.Size;
        int localY = (int)Math.Floor(worldPostion.Y) % Chunk.Size;
        int localZ = (int)Math.Floor(worldPostion.Z) % Chunk.Size;

        if (localX < 0) localX += Chunk.Size;
        if (localY < 0) localY += Chunk.Size;
        if (localZ < 0) localZ += Chunk.Size;

        return new Vector3(localX, localY, localZ);
    }

    public static bool IsOutsideBox(Vector3 point, Vector3 min, Vector3 max)
    {
        return point.X < min.X || point.X > max.X ||
            point.Y < min.Y || point.Y > max.Y ||
            point.Z < min.Z || point.Z > max.Z;
    }
}
