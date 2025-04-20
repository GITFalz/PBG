using OpenTK.Mathematics;

public class UITextButton : UIPrefab
{
    public UICollection TextCollection;
    public UIText Text;
    public UIButton Button;

    public UITextButton(
        string name, 
        UIController controller, 
        AnchorType anchorType, 
        PositionType positionType, 
        Vector3? color,
        Vector3? pivot, 
        Vector2? scale, 
        Vector4? offset, 
        float? rotation, 
        int? textureIndex, 
        Vector2? slice,
        UIState state = UIState.Interactable
    ) : base(name, controller, offset ?? (0, 0, 0, 0))
    {
        UIMesh uiMesh = Controller.UiMesh;
        TextMesh textMesh = Controller.TextMesh;

        Collection = new UICollection($"{name}Collection", controller, anchorType, positionType, pivot ?? (0, 0, 0), scale ?? (100, 100), Offset, rotation ?? 0);
        TextCollection = new UICollection($"{name}TextCollection", controller, AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), scale ?? (100, 100), (0, 0, 0, 0), 0);
        Text = new UIText($"{name}Text", controller, AnchorType.MiddleCenter, PositionType.Relative, (0, 0, 0), (0, 0), (0, 0, 0, 0), 0, textMesh);
        Button = new UIButton($"{name}Button", controller, AnchorType.MiddleCenter, PositionType.Relative, color ?? (0.6f, 0.6f, 0.6f), (0, 0, 0), scale ?? (100, 100), (0, 0, 0, 0), 0, textureIndex ?? 0, slice ?? (10, 0.05f), uiMesh, state);

        TextCollection.AddElement(Text);
        Collection.AddElements(Button, TextCollection);

        Controller.AddElements(this);
    }

    public UIText SetMaxCharCount(int maxCharCount)
    {
        return Text.SetMaxCharCount(maxCharCount);
    }

    public UIText SetTextCharCount(string text, float fontSize)
    {
        return Text.SetTextCharCount(text, fontSize);
    }

    public UIText SetText(string text, float fontSize)
    {
        return Text.SetText(text, fontSize);
    }

    public UIText SetText(string text)
    {
        return SetText(text, 0.5f);
    }

    public void SetOnClick(Action action) => Button.SetOnClick(action);
    public void SetOnHover(Action action) => Button.SetOnHover(action);
    public void SetOnHold(Action action) => Button.SetOnHold(action);
    public void SetOnRelease(Action action) => Button.SetOnRelease(action);
}