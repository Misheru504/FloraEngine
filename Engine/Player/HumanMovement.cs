using Silk.NET.Input;
using System.Numerics;
using FloraEngine.Physics;
using FloraEngine.Core;

namespace FloraEngine.Player;

internal static class HumanMovement
{
    private static readonly float _jumpForce = 8f;
    public static readonly float _airControl = 0.3f;

    public static void ComputeVelocity(InputManager inputManager, Rigidbody rigidbody, Transform transform, float speed)
    {
        Vector3 forward = Vector3.Normalize(new Vector3(transform.Forward.X, 0, transform.Forward.Z));
        Vector3 right = Vector3.Normalize(Vector3.Cross(forward, transform.Up));


        Vector3 moveDir = Vector3.Zero;

        if (inputManager.IsKeyHeld(Key.W)) moveDir += forward;
        if (inputManager.IsKeyHeld(Key.A)) moveDir -= right;
        if (inputManager.IsKeyHeld(Key.S)) moveDir -= forward;
        if (inputManager.IsKeyHeld(Key.D)) moveDir += right;

        if (moveDir != Vector3.Zero) moveDir = Vector3.Normalize(moveDir);

        float control = rigidbody.IsGrounded ? 1f : _airControl;

        Vector3 targetVelocity = moveDir * speed;
        rigidbody.Velocity = new Vector3(
            MathUtils.Lerp(rigidbody.Velocity.X, targetVelocity.X, control),
            rigidbody.Velocity.Y,
            MathUtils.Lerp(rigidbody.Velocity.Z, targetVelocity.Z, control)
        );

        if (inputManager.IsKeyHeld(Key.Space))
            rigidbody.Jump(_jumpForce);
    }
}
