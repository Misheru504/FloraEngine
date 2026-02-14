using FloraEngine.Core;
using FloraEngine.Core.Components;
using System.Numerics;

namespace FloraEngine.Rendering;

/// <summary>
/// A camera contains methods to transform the world into the screen
/// </summary>
public sealed class Camera
{
    public float NearPlane;
    public float FarPlane;

    public float FoV;

    public Transform Transform;

    internal Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Transform.Position, Transform.Position + Vector3.Normalize(Transform.Direction), Transform.Up);
    internal Matrix4x4 RelativeViewMatrix => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.Normalize(Transform.Direction), Transform.Up);

    internal Matrix4x4 ProjectionMatrix => Matrix4x4.CreatePerspectiveFieldOfView(MathUtils.DegreesToRadians(FoV), Program.AspectRatio, NearPlane, FarPlane);
    internal Frustum Frustum => new Frustum(ViewMatrix * ProjectionMatrix);

    public Camera(Transform transform)
    {
        Transform = transform;
        FoV = 100;

        NearPlane = 0.1f;
        FarPlane = 1000.0f;
    }

    public Vector3 RelativePosition(Vector3 absolutePosition) => absolutePosition - Transform.Position;
}