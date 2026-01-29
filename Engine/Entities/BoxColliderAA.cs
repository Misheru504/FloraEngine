using System.Drawing;
using System.Numerics;

namespace FloraEngine.Player;

internal class BoxColliderAA
{
    public Vector3 Position { get; set; }

    public float LengthX, LengthY, LengthZ;

    public float Left => Position.X;
    public float Right => Position.X + LengthX;

    public float Bottom => Position.Y;
    public float Top => Position.Y + LengthY;

    public float Front => Position.Z;
    public float Back => Position.Z + LengthZ;

    public BoxColliderAA(Vector3 pos, float lengthX, float lengthY, float lengthZ)
    {
        Position = pos; 
        LengthX = lengthX; 
        LengthY = lengthY; 
        LengthZ = lengthZ;
    }

    public static bool IsColliding(BoxColliderAA boxA, BoxColliderAA boxB)
    {
        bool x = boxA.Left <= boxB.Right && boxA.Right >= boxB.Left;
        bool y = boxA.Bottom <= boxB.Top && boxA.Top >= boxB.Bottom;
        bool z = boxA.Front <= boxB.Back && boxA.Back >= boxB.Front;

        return x && y && z;
    }

    public bool IsInside(Vector3 point)
    {
        return 
            point.X >= Left && point.X <= Right 
            &&
            point.Y >= Bottom && point.X <= Top
            &&
            point.Z >= Front && point.X <= Back;
    }
}
