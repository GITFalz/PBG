using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using MouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

public static class Input
{
    private static KeyboardState _previousKeyboardState;
    private static MouseState _previousMouseState;
    
    private static Vector2 _oldMousePosition;

    public static HashSet<Keys> PressedKeys = new HashSet<Keys>();
    public static HashSet<MouseButton> PressedButtons = new HashSet<MouseButton>();

    public static void Start(KeyboardState keyboard, MouseState mouse)
    {
        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;
    }
    
    public static void Update(KeyboardState keyboard, MouseState mouse)
    {
        _oldMousePosition = GetMousePosition();
        
        _previousKeyboardState = keyboard;
        _previousMouseState = mouse;
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

    public static bool IsKeyAndControlPressed(Keys key)
    {
        return IsKeyDown(Keys.LeftControl) && IsKeyPressed(key);
    }

    public static bool IsAnyKeyPressed(params Keys[] keys)
    {
        foreach (var k in keys)
        {
            if (IsKeyPressed(k)) 
                return true;
        }
        return false;
    }
    
    public static bool AreKeysPressed(params Keys[] keys)
    {
        return keys.All(IsKeyPressed);
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
    
    public static bool IsKeyReleased(Keys key)
    {
        return _previousKeyboardState.IsKeyReleased(key);
    }
    
    public static Vector2 GetMousePosition()
    {
        return new Vector2(_previousMouseState.X, _previousMouseState.Y);
    }

    public static Vector3 GetMousePosition3()
    {
        return new Vector3(_previousMouseState.X, _previousMouseState.Y, 0f);
    }
    
    public static Vector2 GetMouseDelta()
    {
        return _previousMouseState.Delta;
    }

    public static Vector2 GetOldMousePosition()
    {
        return _oldMousePosition;
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

    public static Vector2 GetMouseScrollDelta()
    {
        return _previousMouseState.ScrollDelta;
    }

    public static bool AnyKeysReleased(params Keys[] keys)
    {
        foreach (var k in keys)
        {
            if (IsKeyReleased(k)) 
                return true;
        }
        return false;
    }
}