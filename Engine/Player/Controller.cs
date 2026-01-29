using FloreEngine;
using FloreEngine.Rendering;
using FloreEngine.Utils;
using FloreEngine.World;
using Silk.NET.Input;
using Silk.NET.Maths;
using System.Numerics;

namespace FloraEngine.Player;

internal class Controller
{
    private static readonly Lazy<Controller> _instance = new Lazy<Controller>(() => new Controller());
    public static Controller Instance => _instance.Value;
    private static Camera Camera => Camera.Instance;
    private static IKeyboard Keyboard => Program.Keyboard;
    public static Vector3 ChunkPos => MathUtils.WorldToChunkCoord(Camera.Position, Chunk.SIZE);
    public static Vector3 LocalVoxelPos => MathUtils.WorldToTilePosition(Camera.Instance.Position);

    private static ICursor Cursor => Program.InputContext.Mice[0].Cursor;

    private Vector2 mousePosition;
    internal float Speed = 2f;

    public BoxColliderAA Collider { get; private set; }
    public Vector3 Size { get; private set; }

    private Controller()
    {
        Size = new Vector3(1,1.5f,1);
        Vector3 position = Camera.Position - (Size * 0.5f);
        Collider = new BoxColliderAA(position, Size.X, Size.Y, Size.Z);
    }

    internal void Update(double deltaTime)
    {
        float moveSpeed = Speed * (float)deltaTime;
        Vector3 camFrontAndBack = moveSpeed * Camera.Forward;
        Vector3 camSides = Vector3.Normalize(Vector3.Cross(Camera.Forward, Camera.Up)) * moveSpeed;
        Vector3 camUpAndDown = moveSpeed * Camera.Up;
        Vector3 velocity = Vector3.Zero;

        if (Keyboard.IsKeyPressed(Key.AltLeft))
            Cursor.CursorMode = CursorMode.Normal;
        else
            Cursor.CursorMode = CursorMode.Raw;

        if (Keyboard.IsKeyPressed(Key.W))
            velocity += camFrontAndBack;

        if (Keyboard.IsKeyPressed(Key.S))
            velocity -= camFrontAndBack;

        if (Keyboard.IsKeyPressed(Key.D))
            velocity += camSides;

        if (Keyboard.IsKeyPressed(Key.A))
            velocity -= camSides;

        if(Keyboard.IsKeyPressed(Key.Space))
            velocity += camUpAndDown * 5;

        if (Keyboard.IsKeyPressed(Key.ShiftLeft))
            velocity -= camUpAndDown;

        if (!Keyboard.IsKeyPressed(Key.ControlLeft))
        {
            Collider.Position = Camera.Position - (Size * 0.5f);
            velocity -= Vector3.UnitY * (10f * (float)deltaTime);
            Camera.Position = ResolveCollision(velocity);
        }
        else
        {
            Camera.Position += velocity;
        }
    }

    private Vector3 ResolveCollision(Vector3 velocity)
    {
        Vector3 newPos = Camera.Position;

        newPos.X += velocity.X;
        if (CheckCollision(newPos, Size))
            newPos.X = Camera.Position.X;

        newPos.Y += velocity.Y;
        if (CheckCollision(newPos, Size))
            newPos.Y = Camera.Position.Y;

        newPos.Z += velocity.Z;
        if (CheckCollision(newPos, Size))
            newPos.Z = Camera.Position.Z;

        return newPos;
    }

    private bool CheckCollision(Vector3 position, Vector3 size)
    {
        // Check all voxels the player's AABB overlaps
        int minX = (int)MathF.Floor(position.X - size.X / 2);
        int maxX = (int)MathF.Floor(position.X + size.X / 2);
        int minY = (int)MathF.Floor(position.Y - size.Y);
        int maxY = (int)MathF.Floor(position.Y);
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

    internal void MouseMove(IMouse mouse, Vector2 position)
    {
        if (Cursor.CursorMode == CursorMode.Normal) return;
        float lookSensitivity = 0.1f;
        if(mousePosition == default) mousePosition = position;
        else
        {
            float xOffset = (position.X - mousePosition.X) * lookSensitivity;
            float yOffset = (position.Y - mousePosition.Y) * lookSensitivity;
            mousePosition = position;

            Camera.Yaw += xOffset;
            Camera.Pitch -= yOffset;

            //We don't want to be able to look behind us by going over our head or under our feet so make sure it stays within these bounds
            Camera.Pitch = Math.Clamp(Camera.Pitch, -89.0f, 89.0f);

            Camera.Direction.X = MathF.Cos(MathUtils.DegreesToRadians(Camera.Yaw)) * MathF.Cos(MathUtils.DegreesToRadians(Camera.Pitch));
            Camera.Direction.Y = MathF.Sin(MathUtils.DegreesToRadians(Camera.Pitch));
            Camera.Direction.Z = MathF.Sin(MathUtils.DegreesToRadians(Camera.Yaw)) * MathF.Cos(MathUtils.DegreesToRadians(Camera.Pitch));
            Camera.Forward = Vector3.Normalize(Camera.Direction);
        }
    }

    internal void MouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        float y = scrollWheel.Y < 0 ? -1 : 1;
        Speed *= Math.Abs(y + 0.05f);
    }
}
