using FloreEngine.Rendering;
using Silk.NET.Input;
using System.Numerics;

namespace FloraEngine.Player;

internal static class FreecamMovement
{
    public static Vector3 GetVelocity(float deltaTime, IKeyboard keyboard)
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

        if (keyboard.IsKeyPressed(Key.Space))
            velocity += camUpAndDown;

        if (keyboard.IsKeyPressed(Key.ShiftLeft))
            velocity -= camUpAndDown;

        return velocity;
    }
}
