using FloraEngine.Utils;
using FloreEngine.Rendering;
using FloreEngine.World;
using Silk.NET.Input;
using System.Drawing;
using System.Numerics;

namespace FloraEngine.Entities.Player;

internal static class HumanMovement
{
    public static float MoveSpeed { get; set; } = 5f;
    public static float SprintMultiplier { get; set; } = 1.5f;
    public static float JumpForce { get; set; } = 8f;
    public static float AirControl { get; set; } = 0.3f;
    public static void GetNextPosition(float deltaTime, IKeyboard keyboard, Rigidbody rigidbody)
    {
        ComputeVelocity(deltaTime, keyboard, rigidbody);
    }

    private static void ComputeVelocity(float deltaTime, IKeyboard keyboard, Rigidbody rigidbody)
    {
        Vector3 forward = Vector3.Normalize(new Vector3(Camera.Instance.Forward.X, 0, Camera.Instance.Forward.Z));
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, Camera.Instance.Up));


        Vector3 moveDir = Vector3.Zero;

        if (keyboard.IsKeyPressed(Key.W)) moveDir += forward;
        if (keyboard.IsKeyPressed(Key.A)) moveDir -= right;
        if (keyboard.IsKeyPressed(Key.S)) moveDir -= forward;
        if (keyboard.IsKeyPressed(Key.D)) moveDir += right;

        if(moveDir != Vector3.Zero) moveDir = Vector3.Normalize(moveDir);

        float speed = MoveSpeed;

        float control = rigidbody.IsGrounded ? 1f : AirControl;

        Vector3 targetVelocity = moveDir * speed;
        rigidbody.Velocity = new Vector3(
            MathUtils.Lerp(rigidbody.Velocity.X, targetVelocity.X, control),
            rigidbody.Velocity.Y,
            MathUtils.Lerp(rigidbody.Velocity.Z, targetVelocity.Z, control)
        );

        if (keyboard.IsKeyPressed(Key.Space))
            rigidbody.Jump(JumpForce);
    }
}
