using FloreEngine.Rendering;
using FloreEngine.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace FloraEngine.Entities;

internal class Rigidbody
{
    private static readonly Vector3 Gravity = new Vector3(0, -20f, 0);
    private WorldManager WorldManager => WorldManager.Instance;

    public Vector3 Position { get; set; }

    public Vector3 Velocity { get; set; }
    public Vector3 Acceleration { get; set; }

    public float Mass { get; private set; }
    public float Drag { get; set; }
    public float GravityScale { get; set; }

    public Vector3 Size { get; set; }
    public bool IsGrounded { get; private set; }

    public Rigidbody(Vector3 position, float mass, Vector3 size)
    {
        Position = position;
        Velocity = Vector3.Zero;
        Acceleration = Vector3.Zero;
        Mass = mass;
        Drag = 0.1f;
        GravityScale = 1.0f;
        Size = size;
        IsGrounded = false;
    }

    public void Update(float deltaTime)
    {
        Velocity += Gravity * GravityScale * deltaTime; // Applying gravity
        Velocity *= 1f - Drag * deltaTime; // Applying drag

        Velocity += Acceleration * deltaTime;
        Acceleration = Vector3.Zero;

        MoveWithCollision(deltaTime);
    }

    public void MoveWithCollision(float deltaTime)
    {
        Vector3 movement = Velocity * deltaTime;

        // Resolve Y first (gravity/jumping)
        Position = new Vector3(Position.X, Position.Y + movement.Y, Position.Z);
        if (CheckCollision())
        {
            Position = new Vector3(Position.X, Position.Y - movement.Y, Position.Z);

            if (Velocity.Y < 0)
                IsGrounded = true;

            Velocity = new Vector3(Velocity.X, 0, Velocity.Z);
        }
        else
        {
            IsGrounded = false;
        }

        // Resolve X
        Position = new Vector3(Position.X + movement.X, Position.Y, Position.Z);
        if (CheckCollision())
        {
            Position = new Vector3(Position.X - movement.X, Position.Y, Position.Z);
            Velocity = new Vector3(0, Velocity.Y, Velocity.Z);
        }

        // Resolve Z
        Position = new Vector3(Position.X, Position.Y, Position.Z + movement.Z);
        if (CheckCollision())
        {
            Position = new Vector3(Position.X, Position.Y, Position.Z - movement.Z);
            Velocity = new Vector3(Velocity.X, Velocity.Y, 0);
        }
    }

    private bool CheckCollision()
    {
        // Get the AABB bounds
        float halfX = Size.X / 2f;
        float halfZ = Size.Z / 2f;

        int minX = (int)MathF.Floor(Position.X - halfX);
        int maxX = (int)MathF.Floor(Position.X + halfX);
        int minY = (int)MathF.Floor(Position.Y);
        int maxY = (int)MathF.Floor(Position.Y + Size.Y);
        int minZ = (int)MathF.Floor(Position.Z - halfZ);
        int maxZ = (int)MathF.Floor(Position.Z + halfZ);

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                for (int z = minZ; z <= maxZ; z++)
                {
                    if (Voxel.Voxels[WorldManager.GetVoxelIdAtWorldPos(x, y, z, 0)].IsSolid)
                        return true;
                }

        return false;
    }

    public void AddForce(Vector3 force) => Acceleration += force / Mass;
    public void AddImpulse(Vector3 impulse) => Velocity += impulse / Mass;

    public void Jump(float jumpForce)
    {
        if (IsGrounded)
        {
            Velocity = new Vector3(Velocity.X, jumpForce, Velocity.Z);
            IsGrounded = false;
        }
    }
}
