using ConsoleApp1.Engine.Scripts.UI.UITextData;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Veldrid;

namespace ConsoleApp1.Assets.Scripts.Inputs;

public static class InputManager
{
    public static bool IsKeyPressed(KeyboardState keyboard, Keys key)
    {
        return keyboard.IsKeyDown(key);
    }

    public static Character GetPressedKey(KeyboardState keyboard)
    {
        foreach (var key in _pressedCharacters)
        {
            if (keyboard.IsKeyDown(key.Key))
            {
                return key.Value;
            }
        }

        return Character.None;
    }

    private static Dictionary<Keys, Character> _pressedCharacters = new Dictionary<Keys, Character>()
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