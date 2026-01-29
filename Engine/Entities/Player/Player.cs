using FloraEngine.Player;
using FloraEngine.Utils;
using FloreEngine;
using FloreEngine.Rendering;
using FloreEngine.World;
using Silk.NET.Input;
using System.Numerics;

namespace FloraEngine.Entities.Player;

internal class Player
{
    private static readonly Lazy<Player> _instance = new Lazy<Player>(() => new Player());
    public static Player Instance => _instance.Value;

    private static Camera Camera => Camera.Instance;
    public static Vector3 ChunkPos => MathUtils.WorldToChunkCoord(Camera.Position, Chunk.SIZE);
    public static Vector3 LocalVoxelPos => MathUtils.WorldToTilePosition(Camera.Instance.Position);

    public BoxColliderAA Collider { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Vector3 Size { get; private set; }
    private static IKeyboard Keyboard => Program.Keyboard;
    private static ICursor Cursor => Program.InputContext.Mice[0].Cursor;

    private Vector2 mousePosition;
    internal float Speed = 2f;

    public bool IsFreecamMovement;

    private Player()
    {
        Size = new Vector3(0.7f, 2f, 0.7f);
        Vector3 position = Camera.Position - Size * 0.5f;
        Collider = new BoxColliderAA(position, Size.X, Size.Y, Size.Z);
        Rigidbody = new Rigidbody(Vector3.Zero, 1f, Size);
    }

    public void Update(float deltaTime)
    {
        if (Keyboard.IsKeyPressed(Key.AltLeft))
            Cursor.CursorMode = CursorMode.Normal;
        else
            Cursor.CursorMode = CursorMode.Raw;

        if (IsFreecamMovement)
        {
            Rigidbody.Position += FreecamMovement.GetVelocity(deltaTime, Keyboard);
        }
        else
        {
            HumanMovement.GetNextPosition(deltaTime, Keyboard, Rigidbody);
            Rigidbody.Update(deltaTime);
        }

        Camera.Position = Rigidbody.Position + (Vector3.UnitY * 1.5f);
    }

    internal void MouseMove(IMouse mouse, Vector2 position)
    {
        if (Cursor.CursorMode == CursorMode.Normal) return;
        float lookSensitivity = 0.1f;
        if (mousePosition == default) mousePosition = position;
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
            Camera.Forward = Vector3.Normalize(new Vector3(Camera.Direction.X, Camera.Direction.Y, Camera.Direction.Z));
        }
    }

    internal void MouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        float y = scrollWheel.Y < 0 ? -1 : 1;
        Speed *= Math.Abs(y + 0.05f);
    }
}
