using OpenTK.Mathematics;

/// <summary>
/// This class is used to define inbuilt functions for the UI system.
/// </summary>
public static class UIFunction
{
    /// <summary>
    /// This function is used on a button to act as a "tab bar" to move UI elements around the screen.
    /// </summary>
    /// <param name="element"></param>
    public static void MoveBar(UIElement element)
    {
        Vector2 mouseDelta = Input.GetMouseDelta();

        Vector4 offset = element.Offset;
        offset.X += mouseDelta.X;
        offset.Y += mouseDelta.Y;

        element.SetOffset(offset);

        element.Align();
        element.UpdateTransformation();
    }
}