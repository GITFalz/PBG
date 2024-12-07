namespace ConsoleApp1.Assets.Scripts.Inputs;

public class SimpleSwitch : Switch
{
    private Func<bool> function;
    
    public SimpleSwitch(Func<bool> function)
    {
        this.function = function;
    }

    public bool CanSwitch()
    {
        if (b && function())
        {
            b = false;
            return true;
        }
        if (!b && !function())
        {
            b = true;
        }
        return false;
    }
}