using FloreEngine.Utils;
using System.Numerics;

namespace FloreEngine.Rendering;

/// <summary>
/// A camera contains methods to transform the world into the screen
/// </summary>
public sealed class Camera
{
    private static readonly Lazy<Camera> _instance = new Lazy<Camera>(() => new Camera());
    public static Camera Instance => _instance.Value;

    public Vector3 Position;
    public Vector3 Up;
    public Vector3 Forward;
    public Vector3 Direction;

    public float Yaw, Pitch, FoV;

    /// <summary>
    /// Uses absolute coordinates for rendering (may cause issues with floating point errors for large distances)
    /// </summary>
    internal Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Forward, Up);

    /// <summary>
    /// Uses the position relative to the camera for rendering 
    /// </summary>
    /// <remarks>
    /// (every rendered objects position needs to be sustracted with Camera.Position)
    /// </remarks>
    internal Matrix4x4 RelativeViewMatrix => Matrix4x4.CreateLookAt(Vector3.Zero, Forward, Up);
    internal Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(MathUtils.DegreesToRadians(FoV), Program.AspectRatio, 0.1f, 1000f);

    internal Matrix4x4 FarProjectionMatrix => CreateReversedZPerspective(MathUtils.DegreesToRadians(FoV), Program.AspectRatio, 0.1f);   

    private Camera()
    {
        Position = Vector3.Zero;
        Up = Vector3.UnitY;
        Forward = -Vector3.UnitZ;
        Direction = Vector3.Zero;

        Yaw = 0;
        Pitch = 0;
        FoV = 70;
    }

    public Vector3 RelativePosition(Vector3 absolutePosition) => absolutePosition - Position;

    public static Matrix4x4 CreateReversedZPerspective(float fov, float aspect, float nearPlane)
    {
        float f = 1.0f / MathF.Tan(fov * 0.5f);

        return new Matrix4x4(
            f / aspect, 0, 0, 0,
            0, f, 0, 0,
            0, 0, 0, -1,
            0, 0, nearPlane, 0
        );
    }
}