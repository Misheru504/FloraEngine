using FloreEngine.Diagnostics;
using FloreEngine.Rendering;
using FloreEngine.Utils;
using FloreEngine.World;
using Silk.NET.Input;
using System.Numerics;

namespace FloreEngine;

/// <summary>
/// Simple camera controller
/// </summary>
internal class Controller
{
    private static readonly Lazy<Controller> _instance = new Lazy<Controller>(() => new Controller());
    public static Controller Instance => _instance.Value;
    private static Camera Camera => Camera.Instance;
    private static IKeyboard Keyboard => Program.Keyboard;
    public static Vector3 ChunkPos => MathUtils.WorldToChunkCoord(Camera.Position, Chunk.Size);

    private static ICursor Cursor => Program.InputContext.Mice[0].Cursor;

    private Vector2 mousePosition;
    internal float Speed = 2f;
    private double deltaTime;

    internal void Update(double deltaTime)
    {
        this.deltaTime = deltaTime;
        var moveSpeed = Speed * (float)deltaTime;
        var camFrontAndBack = moveSpeed * Camera.Forward;
        var camSides = Vector3.Normalize(Vector3.Cross(Camera.Forward, Camera.Up)) * moveSpeed;
        var camUpAndDown = moveSpeed * Camera.Up;

        if (Keyboard.IsKeyPressed(Key.AltLeft))
            Cursor.CursorMode = CursorMode.Normal;
        else
            Cursor.CursorMode = CursorMode.Raw;

        if (Keyboard.IsKeyPressed(Key.W))
            Camera.Position += camFrontAndBack;

        if (Keyboard.IsKeyPressed(Key.S))
            Camera.Position -= camFrontAndBack;

        if (Keyboard.IsKeyPressed(Key.D))
            Camera.Position += camSides;

        if (Keyboard.IsKeyPressed(Key.A))
            Camera.Position -= camSides;

        if(Keyboard.IsKeyPressed(Key.Space))
            Camera.Position += camUpAndDown;

        if (Keyboard.IsKeyPressed(Key.ShiftLeft))
            Camera.Position -= camUpAndDown;
    }

    internal void MouseMove(IMouse mouse, Vector2 position)
    {
        if (Cursor.CursorMode == CursorMode.Normal) return;
        float lookSensitivity = 0.1f;
        if(mousePosition == default) mousePosition = position;
        else
        {
            var xOffset = (position.X - mousePosition.X) * lookSensitivity;
            var yOffset = (position.Y - mousePosition.Y) * lookSensitivity;
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
