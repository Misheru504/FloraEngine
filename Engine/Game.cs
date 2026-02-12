using FloraEngine.Player;
using Silk.NET.Input;
using Silk.NET.OpenGL;

namespace FloraEngine;

public class Game
{
    private static readonly Game _instance = new Game();
    public static Game Instance => _instance;

    private GL _graphics;
    private InputManager _inputManager;

    public void Initialize(GL grahics, IKeyboard keyboard)
    {
        _graphics = grahics;
        _inputManager = new InputManager(keyboard);
    }

    public void Update(double deltaTime)
    {

    }

    public void Draw(double deltaTime)
    {

    }

    public void Closing()
    {

    }
}