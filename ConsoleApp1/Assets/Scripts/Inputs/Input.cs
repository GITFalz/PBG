using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

public static class Input
{
    private static KeyboardState _previousKeyboardState;
    private static MouseState _previousMouseState;
    private static CursorState cursorState = CursorState.Normal;
    private static HashSet<Keys> _pressedKeys = new HashSet<Keys>();

    public static void Start(KeyboardState keyboard, MouseState mouse)
    {
        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;
    }
    
    public static void Update(KeyboardState keyboard, MouseState mouse)
    {
        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;
    }
    
    public static bool AddPressedKey(Keys key)
    {
        return _pressedKeys.Add(key);
    }
    
    public static void RemovePressedKey(Keys key)
    {
        _pressedKeys.Remove(key);
    }

    public static bool IsKeyPressed(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyDown(key);
    }
    
    public static bool IsMousePressed(MouseButton button)
    {
        return _previousMouseState.IsButtonPressed(button);
    }
    
    public static bool IsMouseDown(MouseButton button)
    {
        return _previousMouseState.IsButtonDown(button);
    }
    
    public static bool IsMouseReleased(MouseButton button)
    {
        return _previousMouseState.IsButtonReleased(button);
    }
    
    public static Vector2 GetMouseScroll()
    {
        return _previousMouseState.Scroll;
    }
    
    public static bool IsKeyPressed(Keys key)
    {
        return _previousKeyboardState.IsKeyPressed(key);
    }
    
    public static bool IsKeyDown(Keys key)
    {
        return _previousKeyboardState.IsKeyDown(key);
    }
    
    public static bool AreKeysDown(out int index, params Keys[] keys)
    {
        index = 0;
        foreach (var k in keys)
        {
            if (_previousKeyboardState.IsKeyDown(k)) 
                return true;
            index++;
        }

        index = -1;
        return false;
    }
    
    public static bool AreKeysDown(out Keys? key, params Keys[] keys)
    {
        if (AreKeysDown(out int index, keys))
        {
            key = keys[index];
            return true;
        }
        
        key = null;
        return false;
    }
    
    public static bool AreKeysDown(params Keys[] keys)
    {
        return AreKeysDown(out int _, keys);
    }
    
    public static bool AreAllKeysDown(params Keys[] keys)
    {
        return keys.All(k => _previousKeyboardState.IsKeyDown(k));
    }

    public static Character GetPressedKey(KeyboardState keyboard)
    {
        foreach (var key in PressedCharacters)
        {
            if (keyboard.IsKeyDown(key.Key))
            {
                return key.Value;
            }
        }

        return Character.None;
    }
    
    public static bool IsKeyReleased(Keys key)
    {
        return _previousKeyboardState.IsKeyReleased(key);
    }
    
    public static Vector2 GetMousePosition()
    {
        return new Vector2(_previousMouseState.X, _previousMouseState.Y);
    }
    
    public static Vector2 GetMouseDelta()
    {
        return _previousMouseState.Delta;
    }
    
    public static void SwitchCursor(CursorState state)
    {
        Game.SetCursorState(state);
    }
    
    
    //Player Inputs
    public static Vector2 GetMovementInput()
    {
        Vector2 input = Vector2.Zero;

        if (IsKeyDown(Keys.W))
            input.Y += 1;
        if (IsKeyDown(Keys.S))
            input.Y -= 1;
        
        if (IsKeyDown(Keys.A))
            input.X += 1;
        if (IsKeyDown(Keys.D))
            input.X -= 1;
        
        return input;
    }
    

    private static readonly Dictionary<Keys, Character> PressedCharacters = new Dictionary<Keys, Character>()
    {
        { Keys.A, Character.A },
        { Keys.B, Character.B },
        { Keys.C, Character.C },
        { Keys.D, Character.D },
        { Keys.E, Character.E },
        { Keys.F, Character.F },
        { Keys.G, Character.G },
        { Keys.H, Character.H },
        { Keys.I, Character.I },
        { Keys.J, Character.J },
        { Keys.K, Character.K },
        { Keys.L, Character.L },
        { Keys.M, Character.M },
        { Keys.N, Character.N },
        { Keys.O, Character.O },
        { Keys.P, Character.P },
        { Keys.Q, Character.Q },
        { Keys.R, Character.R },
        { Keys.S, Character.S },
        { Keys.T, Character.T },
        { Keys.U, Character.U },
        { Keys.V, Character.V },
        { Keys.W, Character.W },
        { Keys.X, Character.X },
        { Keys.Y, Character.Y },
        { Keys.Z, Character.Z },
        { Keys.Space, Character.Space },
        { Keys.Backspace, Character.Backspace },
        { Keys.Enter, Character.Enter }
    };
}