using System.Numerics;

namespace FloraEngine.Rendering;

internal struct Frustum
{
    public Plane[] Planes; // 6 planes

    public Frustum(Matrix4x4 viewProjection)
    {
        Planes = new Plane[6];

        // Left
        Planes[0] = Plane.Normalize(new Plane(
            viewProjection.M14 + viewProjection.M11,
            viewProjection.M24 + viewProjection.M21,
            viewProjection.M34 + viewProjection.M31,
            viewProjection.M44 + viewProjection.M41));

        // Right
        Planes[1] = Plane.Normalize(new Plane(
            viewProjection.M14 - viewProjection.M11,
            viewProjection.M24 - viewProjection.M21,
            viewProjection.M34 - viewProjection.M31,
            viewProjection.M44 - viewProjection.M41));

        // Bottom
        Planes[2] = Plane.Normalize(new Plane(
            viewProjection.M14 + viewProjection.M12,
            viewProjection.M24 + viewProjection.M22,
            viewProjection.M34 + viewProjection.M32,
            viewProjection.M44 + viewProjection.M42));

        // Top
        Planes[3] = Plane.Normalize(new Plane(
            viewProjection.M14 - viewProjection.M12,
            viewProjection.M24 - viewProjection.M22,
            viewProjection.M34 - viewProjection.M32,
            viewProjection.M44 - viewProjection.M42));

        // Near
        Planes[4] = Plane.Normalize(new Plane(
            viewProjection.M13,
            viewProjection.M23,
            viewProjection.M33,
            viewProjection.M43));

        // Far
        Planes[5] = Plane.Normalize(new Plane(
            viewProjection.M14 - viewProjection.M13,
            viewProjection.M24 - viewProjection.M23,
            viewProjection.M34 - viewProjection.M33,
            viewProjection.M44 - viewProjection.M43));
    }
}
