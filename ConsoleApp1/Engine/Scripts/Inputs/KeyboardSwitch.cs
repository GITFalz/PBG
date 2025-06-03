using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ConsoleApp1.Assets.Scripts.Inputs;

public class KeyboardSwitch : Switch
{
    private Func<KeyboardState, Keys, bool> function;
    
    public KeyboardSwitch(Func<KeyboardState, Keys, bool> function)
    {
        this.function = function;
    }

    public bool CanSwitch(KeyboardState keyboardState, Keys key)
    {
        switch (b)
        {
            case true when function(keyboardState, key):
                b = false;
                return true;
            case false when !function(keyboardState, key):
                b = true;
                break;
        }

        return false;
    }
}