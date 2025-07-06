public static class NNS_NodeHelper
{
    public const float SlideSpeed = 10f;

    public static void SetSlideValue(ref float value, UIInputField inputField, float speed, int index)
    {
        float delta = Input.GetMouseDelta().X * speed * GameTime.DeltaTime;
        if (delta == 0f) return;
        value += delta;
        inputField.SetText(value.ToString()).UpdateCharacters();
        NoiseGlslNodeManager.UpdateValue(index, value);
    }

    public static void SetSlideValue(ref int value, UIInputField inputField, float speed, int index)
    {
        float delta = Input.GetMouseDelta().X * speed * GameTime.DeltaTime;
        if (delta == 0f) return;
        value += (int)delta;
        inputField.SetText(value.ToString()).UpdateCharacters();
        NoiseGlslNodeManager.UpdateValue(index, value);
    }

    public static void SetValue(ref float value, UIInputField inputField, float replacement, int index)
    {
        value = inputField.ParseFloat(replacement);
        NoiseGlslNodeManager.UpdateValue(index, value);
    }

    public static void SetValue(ref int value, UIInputField inputField, int replacement, int index)
    {
        value = inputField.ParseInt(replacement);
        NoiseGlslNodeManager.UpdateValue(index, value);
    }
}