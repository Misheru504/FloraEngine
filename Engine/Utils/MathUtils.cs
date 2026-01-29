using FloreEngine.World;
using System.Numerics;

namespace FloraEngine.Utils;

public static class MathUtils
{
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;

    public static float DegreesToRadians(float degrees) => MathF.PI / 180f * degrees;
    public static int ManhattanDistance(Vector3 pointA, Vector3 pointB)
    {
        return (int) (Math.Abs(pointB.X - pointA.X) + Math.Abs(pointB.Y - pointA.Y) + Math.Abs(pointB.Z - pointA.Z));
    }

    public static int ChebyshevDistance(Vector3 pointA, Vector3 pointB)
    {
        return Math.Max((int)Math.Abs(pointB.X - pointA.X), Math.Max((int)Math.Abs(pointB.Y - pointA.Y), (int)Math.Abs(pointB.Z - pointA.Z)));
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
        int localX = (int)Math.Floor(worldPostion.X) % Chunk.SIZE;
        int localY = (int)Math.Floor(worldPostion.Y) % Chunk.SIZE;
        int localZ = (int)Math.Floor(worldPostion.Z) % Chunk.SIZE;

        if (localX < 0) localX += Chunk.SIZE;
        if (localY < 0) localY += Chunk.SIZE;
        if (localZ < 0) localZ += Chunk.SIZE;

        return new Vector3(localX, localY, localZ);
    }

    public static bool IsOutsideBox(Vector3 point, Vector3 min, Vector3 max)
    {
        return point.X < min.X || point.X > max.X ||
            point.Y < min.Y || point.Y > max.Y ||
            point.Z < min.Z || point.Z > max.Z;
    }

    public static bool OutOfDistance(Vector3 vecA, Vector3 vecB, int distance)
    {
        float dX = Math.Abs(vecB.X - vecA.X);
        float dY = Math.Abs(vecB.Y - vecA.Y);
        float dZ = Math.Abs(vecB.Z - vecA.Z);

        return dX > distance || dY > distance || dZ > distance;
    }
}
