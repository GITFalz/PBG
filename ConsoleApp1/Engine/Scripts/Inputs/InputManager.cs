using OpenTK.Windowing.GraphicsLibraryFramework;

public static class InputManager
{
    public static Dictionary<HashSet<Keys>, ToggleableAction> KeyActions = new Dictionary<HashSet<Keys>, ToggleableAction>(new HashKeyComparer())
    {
        {[Keys.Space], new(() => {})}, // Jump
    };

    public static Dictionary<HashSet<MouseButton>, ToggleableAction> MouseActions = new Dictionary<HashSet<MouseButton>, ToggleableAction>(new HashMouseComparer())
    {
        {[MouseButton.Left], new(() => {})}, // Attack
    };

    public static bool CallAction()
    {
        return CallKeys() || CallMouse();
    }

    public static bool CallKeys()
    {
        return CallAction(Input.PressedKeys);
    }

    public static bool CallMouse()
    {
        return CallAction(Input.PressedButtons);
    }

    public static bool CallAction(HashSet<Keys> keys)
    {
        if (KeyActions.TryGetValue(keys, out ToggleableAction? value))
        {
            return value.Invoke();
        }
        return false;
    }

    public static bool CallAction(HashSet<MouseButton> buttons)
    {
        if (MouseActions.TryGetValue(buttons, out ToggleableAction? value))
        {
            return value.Invoke();
        }
        return false;
    }

    public static void SetAction(HashSet<Keys> keys, Action action)
    {
        if (KeyActions.TryGetValue(keys, out ToggleableAction? value))
        {
            value.SetAction(action);
        }
    }

    public static void SetAction(HashSet<MouseButton> buttons, Action action)
    {
        if (MouseActions.TryGetValue(buttons, out ToggleableAction? value))
        {
            value.SetAction(action);
        }
    }

    public static void SetAction(HashSet<Keys> keys, Func<bool> action)
    {
        if (KeyActions.TryGetValue(keys, out ToggleableAction? value))
        {
            value.SetAction(action);
        }
    }

    public static void SetAction(HashSet<MouseButton> buttons, Func<bool> action)
    {
        if (MouseActions.TryGetValue(buttons, out ToggleableAction? value))
        {
            value.SetAction(action);
        }
    }


    public static void AddKeyAction(HashSet<Keys> keys)
    {
        AddKeyAction(keys, () => { return false; });
    }

    public static void AddMouseAction(HashSet<MouseButton> buttons)
    {
        AddMouseAction(buttons, () => { return false; });
    }

    public static ToggleableAction AddKeyAction(HashSet<Keys> keys, Action action)
    {
        return AddKeyAction(keys, () => { action.Invoke(); return true; });
    }

    public static ToggleableAction AddMouseAction(HashSet<MouseButton> buttons, Action action)
    {
        return AddMouseAction(buttons, () => { action.Invoke(); return true; });
    }

    public static ToggleableAction AddKeyAction(HashSet<Keys> keys, Func<bool> action)
    {
        ToggleableAction toggleableAction = new(action);
        KeyActions.Add(keys, toggleableAction);
        return toggleableAction;
    }

    public static ToggleableAction AddMouseAction(HashSet<MouseButton> buttons, Func<bool> action)
    {
        ToggleableAction toggleableAction = new(action);
        MouseActions.Add(buttons, toggleableAction);
        return toggleableAction;
    }

    public static void RemoveAction(HashSet<Keys> keys)
    {
        KeyActions.Remove(keys);
    }

    public static void RemoveAction(HashSet<MouseButton> buttons)
    {
        MouseActions.Remove(buttons);
    }

    public static void ChangeKeys(HashSet<Keys> oldKeys, HashSet<Keys> newKeys)
    {
        if (KeyActions.TryGetValue(oldKeys, out ToggleableAction? value))
        {
            KeyActions.Remove(oldKeys);
            KeyActions.Add(newKeys, value);
        }
    }

    public static void ChangeButtons(HashSet<MouseButton> oldButtons, HashSet<MouseButton> newButtons)
    {
        if (MouseActions.TryGetValue(oldButtons, out ToggleableAction? value))
        {
            MouseActions.Remove(oldButtons);
            MouseActions.Add(newButtons, value);
        }
    }

    public static void Disable(HashSet<Keys> keys)
    {
        if (KeyActions.TryGetValue(keys, out ToggleableAction? value))
        {
            value.Disable();
        }
    }

    public static void Disable(HashSet<MouseButton> buttons)
    {
        if (MouseActions.TryGetValue(buttons, out ToggleableAction? value))
        {
            value.Disable();
        }
    }

    public static void Enable(HashSet<Keys> keys)
    {
        if (KeyActions.TryGetValue(keys, out ToggleableAction? value))
        {
            value.Enable();
        }
    }

    public static void Enable(HashSet<MouseButton> buttons)
    {
        if (MouseActions.TryGetValue(buttons, out ToggleableAction? value))
        {
            value.Enable();
        }
    }
}

public class HashKeyComparer : IEqualityComparer<HashSet<Keys>>
{
    public bool Equals(HashSet<Keys>? x, HashSet<Keys>? y)
    {
        if (x is null || y is null)
            return false;
        return x.SetEquals(y);
    }

    public int GetHashCode(HashSet<Keys> obj)
    {
        int hash = 0;
        foreach (var item in obj)
        {
            hash ^= item!.GetHashCode();
        }
        return hash;
    }
}

public class HashMouseComparer : IEqualityComparer<HashSet<MouseButton>>
{
    public bool Equals(HashSet<MouseButton>? x, HashSet<MouseButton>? y)
    {
        if (x is null || y is null)
            return false;
        return x.SetEquals(y);
    }

    public int GetHashCode(HashSet<MouseButton> obj)
    {
        int hash = 0;
        foreach (var item in obj)
        {
            hash ^= item!.GetHashCode();
        }
        return hash;
    }
}

public class ToggleableAction
{
    internal Func<bool> savedAction = () => { return false; };
    internal bool isOn = false;

    private Func<bool> action = () => { return false; };

    public ToggleableAction(Action action)
    {
        SetAction(action);
        this.action = Call;
    }

    public ToggleableAction(Func<bool> action)
    {
        SetAction(action);
        this.action = Call;
    }

    public void SetAction(Action action)
    {
        savedAction = () => { action.Invoke(); return true; };
    }

    public void SetAction(Func<bool> action)
    {
        savedAction = action;
    }

    public bool Invoke()
    {
        return action.Invoke();
    }

    public void Disable()
    {
        action = () => { return false; };
        isOn = false;
    }

    public void Enable()
    {
        action = Call;
        isOn = true;
    }

    internal bool Call()
    {
        return savedAction.Invoke();
    }
}