using Silk.NET.Input;

namespace FloraEngine.Player;

internal class InputManager
{
    private IKeyboard _keyboard;

    public InputManager(IKeyboard keyboard)
    {
        _keyboard = keyboard;
    }

    public bool IsKeyHeld(Key key) => _keyboard.IsKeyPressed(key);
}
