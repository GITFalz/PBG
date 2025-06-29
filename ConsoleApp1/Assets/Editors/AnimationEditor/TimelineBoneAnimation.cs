using System.Diagnostics.CodeAnalysis;

public class TimelineBoneAnimation(string name, NewBoneAnimation animation)
{
    public string Name = name;
    public int Index;
    public NewBoneAnimation Animation = animation;

    /* Old
    public Dictionary<UIButton, IndividualKeyframe> ButtonKeyframes = [];
    public Dictionary<IndividualKeyframe, UIButton> KeyframeButtons = [];
    */

    public Dictionary<UIButton, PositionKeyframe> PositionButtonKeyframes = [];
    public Dictionary<PositionKeyframe, UIButton> PositionKeyframeButtons = [];

    public Dictionary<UIButton, RotationKeyframe> RotationButtonKeyframes = [];
    public Dictionary<RotationKeyframe, UIButton> RotationKeyframeButtons = [];

    public Dictionary<UIButton, ScaleKeyframe> ScaleButtonKeyframes = [];
    public Dictionary<ScaleKeyframe, UIButton> ScaleKeyframeButtons = [];



    public void Add(UIButton button, IndividualKeyframe keyframe)
    {
        if (keyframe is PositionKeyframe positionKeyframe)
            Add(button, positionKeyframe);
        else if (keyframe is RotationKeyframe rotationKeyframe)
            Add(button, rotationKeyframe);
        else if (keyframe is ScaleKeyframe scaleKeyframe)
            Add(button, scaleKeyframe);
    }

    public void Add(UIButton button, PositionKeyframe keyframe)
    {
        if (!PositionButtonKeyframes.ContainsKey(button))
        {
            PositionButtonKeyframes.Add(button, keyframe);
            PositionKeyframeButtons.Add(keyframe, button);
        }
    }

    public void Add(UIButton button, RotationKeyframe keyframe)
    {
        if (!RotationButtonKeyframes.ContainsKey(button))
        {
            RotationButtonKeyframes.Add(button, keyframe);
            RotationKeyframeButtons.Add(keyframe, button);
        }
    }

    public void Add(UIButton button, ScaleKeyframe keyframe)
    {
        if (!ScaleButtonKeyframes.ContainsKey(button))
        {
            ScaleButtonKeyframes.Add(button, keyframe);
            ScaleKeyframeButtons.Add(keyframe, button);
        }
    }

    public bool ContainsIndex(int type, int index)
    {
        if (type == 0)
            return PositionButtonKeyframes.Any(k => k.Value.Index == index);
        else if (type == 1)
            return RotationButtonKeyframes.Any(k => k.Value.Index == index);
        else if (type == 2)
            return ScaleButtonKeyframes.Any(k => k.Value.Index == index);
        return false;
    }

    public void Remove(UIButton button)
    {
        if (PositionButtonKeyframes.TryGetValue(button, out PositionKeyframe? positionKeyframe))
        {
            PositionButtonKeyframes.Remove(button);
            PositionKeyframeButtons.Remove(positionKeyframe);
        }
        if (RotationButtonKeyframes.TryGetValue(button, out RotationKeyframe? rotationKeyframe))
        {
            RotationButtonKeyframes.Remove(button);
            RotationKeyframeButtons.Remove(rotationKeyframe);
        }
        if (ScaleButtonKeyframes.TryGetValue(button, out ScaleKeyframe? scaleKeyframe))
        {
            ScaleButtonKeyframes.Remove(button);
            ScaleKeyframeButtons.Remove(scaleKeyframe);
        }
    }

    public void Remove(IndividualKeyframe keyframe)
    {
        if (keyframe is PositionKeyframe positionKeyframe)
            Remove(positionKeyframe);
        else if (keyframe is RotationKeyframe rotationKeyframe)
            Remove(rotationKeyframe);
        else if (keyframe is ScaleKeyframe scaleKeyframe)
            Remove(scaleKeyframe);
    }

    public void Remove(PositionKeyframe keyframe)
    {
        if (PositionKeyframeButtons.TryGetValue(keyframe, out UIButton? button))
        {
            PositionButtonKeyframes.Remove(button);
            PositionKeyframeButtons.Remove(keyframe);
        }
    }

    public void Remove(RotationKeyframe keyframe)
    {
        if (RotationKeyframeButtons.TryGetValue(keyframe, out UIButton? button))
        {
            RotationButtonKeyframes.Remove(button);
            RotationKeyframeButtons.Remove(keyframe);
        }
    }

    public void Remove(ScaleKeyframe keyframe)
    {
        if (ScaleKeyframeButtons.TryGetValue(keyframe, out UIButton? button))
        {
            ScaleButtonKeyframes.Remove(button);
            ScaleKeyframeButtons.Remove(keyframe);
        }
    }

    public bool Get(UIButton button, [NotNullWhen(true)] out PositionKeyframe? keyframe)
    {
        if (PositionButtonKeyframes.TryGetValue(button, out PositionKeyframe? value))
        {
            keyframe = value;
            return true;
        }
        keyframe = null;
        return false;
    }

    public bool Get(UIButton button, [NotNullWhen(true)] out RotationKeyframe? keyframe)
    {
        if (RotationButtonKeyframes.TryGetValue(button, out RotationKeyframe? value))
        {
            keyframe = value;
            return true;
        }
        keyframe = null;
        return false;
    }

    public bool Get(UIButton button, [NotNullWhen(true)] out ScaleKeyframe? keyframe)
    {
        if (ScaleButtonKeyframes.TryGetValue(button, out ScaleKeyframe? value))
        {
            keyframe = value;
            return true;
        }
        keyframe = null;
        return false;
    }

    public bool Get(IndividualKeyframe keyframe, [NotNullWhen(true)] out UIButton? button)
    {
        if (keyframe is PositionKeyframe positionKeyframe)
            return Get(positionKeyframe, out button);
        else if (keyframe is RotationKeyframe rotationKeyframe)
            return Get(rotationKeyframe, out button);
        else if (keyframe is ScaleKeyframe scaleKeyframe)
            return Get(scaleKeyframe, out button);
        button = null;
        return false;
    }

    public bool Get(PositionKeyframe keyframe, [NotNullWhen(true)] out UIButton? button)
    {
        if (PositionKeyframeButtons.TryGetValue(keyframe, out UIButton? value))
        {
            button = value;
            return true;
        }
        button = null;
        return false;
    }

    public bool Get(RotationKeyframe keyframe, [NotNullWhen(true)] out UIButton? button)
    {
        if (RotationKeyframeButtons.TryGetValue(keyframe, out UIButton? value))
        {
            button = value;
            return true;
        }
        button = null;
        return false;
    }

    public bool Get(ScaleKeyframe keyframe, [NotNullWhen(true)] out UIButton? button)
    {
        if (ScaleKeyframeButtons.TryGetValue(keyframe, out UIButton? value))
        {
            button = value;
            return true;
        }
        button = null;
        return false;
    }

    public void RegenerateBoneKeyframes()
    {
        ResetKeyframes();
        List<PositionKeyframe> positionKeyframes = [];
        List<RotationKeyframe> rotationKeyframes = [];
        List<ScaleKeyframe> scaleKeyframes = [];
        for (int i = 0; i < Animation.Length; i++)
        {
            var frame = Animation.GetKeyframe(i);
            positionKeyframes.Add(frame.Position);
            rotationKeyframes.Add(frame.Rotation);
            scaleKeyframes.Add(frame.Scale);
        }
        Animation.SetKeyframes(positionKeyframes);
        Animation.SetKeyframes(rotationKeyframes);
        Animation.SetKeyframes(scaleKeyframes);
    }

    public void ResetKeyframes()
    {
        List<PositionKeyframe> positionKeyframes = [.. PositionButtonKeyframes.Values];
        List<RotationKeyframe> rotationKeyframes = [.. RotationButtonKeyframes.Values];
        List<ScaleKeyframe> scaleKeyframes = [.. ScaleButtonKeyframes.Values];
        Animation.SetKeyframes(positionKeyframes);
        Animation.SetKeyframes(rotationKeyframes);
        Animation.SetKeyframes(scaleKeyframes);
    }

    public void OrderKeyframes(List<IndividualKeyframe> keyframes)
    {
        if (keyframes.Count > 1)
        {
            keyframes = [.. keyframes.OrderBy(k => k.Time)];
        }
    }

    public void Clear()
    {
        foreach (var button in PositionButtonKeyframes.Keys)
        {
            button.Delete();
        }
        foreach (var button in RotationButtonKeyframes.Keys)
        {
            button.Delete();
        }
        foreach (var button in ScaleButtonKeyframes.Keys)
        {
            button.Delete();
        }
        PositionButtonKeyframes = [];
        RotationButtonKeyframes = [];
        ScaleButtonKeyframes = [];
    }

    public void ClearFull()
    {
        Clear();
    }
}