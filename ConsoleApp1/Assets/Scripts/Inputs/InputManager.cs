using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Veldrid;

public static class InputManager
{
    private static KeyboardState _previousKeyboardState;
    private static MouseState _previousMouseState;

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

    public static bool IsKeyPressed(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyDown(key);
    }
    
    public static bool IsKeyPressed(Keys key)
    {
        return _previousKeyboardState.IsKeyPressed(key);
    }
    
    public static bool IsDown(Keys key)
    {
        return _previousKeyboardState.IsKeyDown(key);
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
    
    public static List<Character> GetPressedKeys(KeyboardState keyboard)
    {
        List<Character> pressedKeys = new List<Character>();
        
        foreach (var key in PressedCharacters)
        {
            if (keyboard.IsKeyDown(key.Key))
            {
                pressedKeys.Add(key.Value);
            }
        }

        return pressedKeys;
    }
    
    
    //Player Inputs
    public static Vector2 GetMovementInput()
    {
        Vector2 input = Vector2.Zero;

        if (IsDown(Keys.W))
            input.Y += 1;
        if (IsDown(Keys.S))
            input.Y -= 1;
        
        if (IsDown(Keys.A))
            input.X += 1;
        if (IsDown(Keys.D))
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