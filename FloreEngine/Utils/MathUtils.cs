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
        );
    }

    public static bool IsOutsideBox(Vector3 point, Vector3 min, Vector3 max)
    {
        return point.X < min.X || point.X > max.X ||
            point.Y < min.Y || point.Y > max.Y ||
            point.Z < min.Z || point.Z > max.Z;
    }
}
