using FloreEngine.Rendering;
using FloreEngine.World;
using Silk.NET.Input;
using System.Drawing;
using System.Numerics;

namespace FloraEngine.Player;

internal static class HumanMovement
{
    public static Vector3 GetNextPosition(float deltaTime, IKeyboard keyboard)
    {
        Vector3 velocity = ComputeVelocity(deltaTime, keyboard);

        velocity -= Vector3.UnitY * (10f * (float)deltaTime);
        return ResolveCollision(velocity);
    }

    private static Vector3 ComputeVelocity(float deltaTime, IKeyboard keyboard)
    {
        float speed = Player.Instance.Speed;
        float moveSpeed = speed * deltaTime;
        Vector3 camFrontAndBack = moveSpeed * Camera.Instance.Forward;
        Vector3 camSides = Vector3.Normalize(Vector3.Cross(Camera.Instance.Forward, Camera.Instance.Up)) * moveSpeed;
        Vector3 camUpAndDown = moveSpeed * Camera.Instance.Up;
        Vector3 velocity = Vector3.Zero;

        if (keyboard.IsKeyPressed(Key.W))
            velocity += camFrontAndBack;

        if (keyboard.IsKeyPressed(Key.S))
            velocity -= camFrontAndBack;

        if (keyboard.IsKeyPressed(Key.D))
            velocity += camSides;

        if (keyboard.IsKeyPressed(Key.A))
            velocity -= camSides;

        if (keyboard.IsKeyPressed(Key.Space) && Player.Instance.IsOnGround)
            velocity += camUpAndDown * 5;

        if (keyboard.IsKeyPressed(Key.ShiftLeft))
            velocity -= camUpAndDown;

        return velocity;
    }

    internal static Vector3 ResolveCollision(Vector3 velocity)
    {
        Vector3 newPos = Camera.Instance.Position;
        Vector3 Size = Player.Instance.Size;

        newPos.X += velocity.X;
        if (CheckCollision(newPos, Size))
            newPos.X = Camera.Instance.Position.X;

        newPos.Y += velocity.Y;
        if (CheckCollision(newPos, Size))
            newPos.Y = Camera.Instance.Position.Y;

        newPos.Z += velocity.Z;
        if (CheckCollision(newPos, Size))
            newPos.Z = Camera.Instance.Position.Z;

        return newPos;
    }

    private static bool CheckCollision(Vector3 position, Vector3 size)
    {
        // Check all voxels the player's AABB overlaps
        int minX = (int)MathF.Floor(position.X - size.X / 2);
        int maxX = (int)MathF.Floor(position.X + size.X / 2);
        int minY = (int)MathF.Floor(position.Y - size.Y);
        int maxY = (int)MathF.Floor(position.Y + 0.5f);
        int minZ = (int)MathF.Floor(position.Z - size.Z / 2);
        int maxZ = (int)MathF.Floor(position.Z + size.Z / 2);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    ushort voxelId = WorldManager.Instance.GetVoxelIdAtWorldPos(x, y, z, 0);
                    if (Voxel.Voxels[voxelId].IsSolid)
                        return true;
                }
        return false;
    }
}
