using FloraEngine.Core;
using FloraEngine.Core.Components;
using FloraEngine.Physics;
using Silk.NET.Input;
using System.Numerics;

namespace FloraEngine.Player;

public class PlayerController
{
    private readonly InputManager _inputManager;
    private readonly Transform _transform;
    private readonly Rigidbody _rigidbody;
    private readonly IMouse _mouse;
    private readonly ICursor _cursor;
    private Vector2 _mousePosition;
    private float _speed;

    public bool IsFreecam { get; set; }

    public float Speed { get { return _speed; } set { _speed = value < 0 ? 0 : value; } }

    public Vector3 SpawnPosition { get; set; }

    public PlayerController(InputManager inputManager, IMouse mouse, Transform transform)
    {
        Vector3 size = new Vector3(0.7f, 2f, 0.7f);

        _inputManager = inputManager;

        _transform = transform;

        _rigidbody = new Rigidbody(Vector3.Zero, 1.0f, size);
        _speed = 5.0f;

        _mouse = mouse;
        _cursor = mouse.Cursor;
        _cursor.CursorMode = CursorMode.Raw;

        _mouse.MouseMove += MouseMove;
        _mouse.Scroll += MouseWheel;

        SpawnPosition = new Vector3(0.5f, 15, 0.5f);
        SetPosition(SpawnPosition);

        _inputManager.RegisterKeyPress(Key.Escape, Program.EngineWindow.Close);
        _inputManager.RegisterKeyPress(Key.T,  () => IsFreecam = !IsFreecam);
        _inputManager.RegisterKeyPress(Key.R, () => SetPosition(SpawnPosition));
    }

    public void SetPosition(Vector3 position)
    {
        _transform.Position = position + Vector3.UnitY * 1.6f;
        _rigidbody.Position = position;
        _rigidbody.Velocity = Vector3.Zero;
    }

    public void Update(double deltaTime)
    {
        if (_inputManager.IsKeyHeld(Key.AltLeft))
            _cursor.CursorMode = CursorMode.Normal;
        else
            _cursor.CursorMode = CursorMode.Raw;

        if (IsFreecam)
        {
            _rigidbody.Position += FreecamMovement.GetVelocity(deltaTime, _inputManager, _speed, _transform);
            _rigidbody.Velocity = Vector3.Zero;
        }
        else
        {
            HumanMovement.ComputeVelocity(_inputManager, _rigidbody, _transform);
            _rigidbody.Update((float) deltaTime);
        }

        _transform.Position = _rigidbody.Position + Vector3.UnitY * 1.6f;
    }

    private void MouseMove(IMouse mouse, Vector2 position)
    {
        if (_cursor.CursorMode == CursorMode.Normal) return;
        float lookSensitivity = 0.1f;
        if (_mousePosition == default) _mousePosition = position;
        else
        {
            float xOffset = (position.X - _mousePosition.X) * lookSensitivity;
            float yOffset = (position.Y - _mousePosition.Y) * lookSensitivity;
            _mousePosition = position;

            _transform.Yaw += xOffset;
            _transform.Pitch -= yOffset;

            Vector3 direction = new Vector3(
                MathF.Cos(MathUtils.DegreesToRadians(_transform.Yaw)) * MathF.Cos(MathUtils.DegreesToRadians(_transform.Pitch)),
                MathF.Sin(MathUtils.DegreesToRadians(_transform.Pitch)),
                MathF.Sin(MathUtils.DegreesToRadians(_transform.Yaw)) * MathF.Cos(MathUtils.DegreesToRadians(_transform.Pitch))
            );

            _transform.Direction = direction;

            _transform.Forward = Vector3.Normalize(direction);
        }
    }

    private void MouseWheel(IMouse mouse, ScrollWheel scrollWheel)
    {
        float y = scrollWheel.Y < 0 ? -1 : 1;
        Speed *= Math.Abs(y + 0.05f);
    }
}
