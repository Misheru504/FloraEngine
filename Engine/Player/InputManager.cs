using System;
using System.Collections.Generic;
using Silk.NET.Input;

namespace FloraEngine.Player;

public class InputManager
{
    private readonly IKeyboard _keyboard;
    private readonly Dictionary<Key, Action> _pressedKeys;

    public InputManager(IKeyboard keyboard)
    {
        _keyboard = keyboard;
        _keyboard.KeyDown += OnKeyDown;

        _pressedKeys = new Dictionary<Key, Action>();
    }

    public void OnKeyDown(IKeyboard keyboard, Key key, int keyCode)
    {
        if (_pressedKeys.ContainsKey(key))
        {
            _pressedKeys[key]();
        }
    }

    public bool IsKeyHeld(Key key) => _keyboard.IsKeyPressed(key);

    public void RegisterKeyPress(Key key, Action action) => _pressedKeys[key] = action;

    public void RemoveKeyPress(Key key) => _pressedKeys.Remove(key);
}
