using FloraEngine.Core.Components;
using Silk.NET.Input;
using System.Numerics;

namespace FloraEngine.Player;

internal static class FreecamMovement
{
    public static Vector3 GetVelocity(double deltaTime, InputManager inputManager, float speed, Transform transform)
    {
        float moveSpeed = (float) (speed * deltaTime);
        Vector3 camFrontAndBack = moveSpeed * transform.Forward;
        Vector3 camSides = Vector3.Normalize(Vector3.Cross(transform.Forward, transform.Up)) * moveSpeed;
        Vector3 camUpAndDown = moveSpeed * transform.Up;
        Vector3 velocity = Vector3.Zero;

        if (inputManager.IsKeyHeld(Key.W))
            velocity += camFrontAndBack;

        if (inputManager.IsKeyHeld(Key.S))
            velocity -= camFrontAndBack;

        if (inputManager.IsKeyHeld(Key.D))
            velocity += camSides;

        if (inputManager.IsKeyHeld(Key.A))
            velocity -= camSides;

        if (inputManager.IsKeyHeld(Key.Space))
            velocity += camUpAndDown;

        if (inputManager.IsKeyHeld(Key.ShiftLeft))
            velocity -= camUpAndDown;

        return velocity;
    }
}
