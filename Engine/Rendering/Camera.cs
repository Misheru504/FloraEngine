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

    public float NearPlane;
    public float FarPlane;

    public float Yaw, Pitch, FoV;

    /// <summary>
    /// Uses absolute coordinates for rendering (may cause issues with floating point errors for large distances)
    /// </summary>
    internal Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Position, Position + Vector3.Normalize(Direction), Up);

    /// <summary>
    /// Uses the position relative to the camera for rendering 
    /// </summary>
    /// <remarks>
    /// (every rendered objects position needs to be sustracted with Camera.Position)
    /// </remarks>
    internal Matrix4x4 RelativeViewMatrix => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.Normalize(Direction), Up);
    internal Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(MathUtils.DegreesToRadians(FoV), Program.AspectRatio, NearPlane, FarPlane);
    internal Frustum Frustum => new Frustum(ViewMatrix * ProjectionMatrix);

    private Camera()
    {
        Position = Vector3.Zero;
        Up = Vector3.UnitY;
        Forward = -Vector3.UnitZ;
        Direction = Vector3.Zero;

        Yaw = 0;
        Pitch = 0;
        FoV = 100;

        NearPlane = 0.1f;
        FarPlane = 5000.0f;
    }

    public Vector3 RelativePosition(Vector3 absolutePosition) => absolutePosition - Position;
}