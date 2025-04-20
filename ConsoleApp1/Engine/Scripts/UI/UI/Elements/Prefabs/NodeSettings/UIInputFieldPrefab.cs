using OpenTK.Mathematics;

public class UIInputFieldPrefab : UIPrefab
{
    public UIImage Background;
    public UIInputField InputField;

    public Vector4 BackgroundColor;
    public int CharCount;
    public int BackgroundIndex;
    public TextType TextType;

    public UIInputFieldPrefab(
        string name, 
        UIController controller, 
        Vector4 offset,
        Vector4 backgroundColor,
        int backgroundIndex,
        int charCount,
        string text,
        TextType textType = TextType.All
    ) : base(name, controller, offset)
    {
        Name = name;
        Controller = controller;
        Offset = offset;
        BackgroundColor = backgroundColor;
        CharCount = charCount;
        BackgroundIndex = backgroundIndex;
        TextType = textType;

        Collection = new UICollection(name, controller, AnchorType.TopLeft, PositionType.Relative, Vector3.Zero, Vector2.Zero, offset, 0f);
        Background = new UIImage(name + "_Background", controller, AnchorType.TopLeft, PositionType.Relative, backgroundColor, Vector3.Zero, Vector2.Zero, offset, 0f, backgroundIndex, (10, 0.05f));
        InputField = new UIInputField(name + "_InputField", controller, AnchorType.TopLeft, PositionType.Relative, Vector4.One, Vector3.Zero, Vector2.Zero, offset + (8, 8, 0, 0), 0f, backgroundIndex, (10, 0.05f));
        InputField.SetMaxCharCount(CharCount).SetText(text, 0.5f).SetTextType(TextType);

        Vector2 scale = InputField.newScale;
        Collection.SetScale(scale + (16, 16));
        Background.SetScale(scale + (16, 16));

        Collection.AddElements(Background, InputField);

        Controller.AddElements(this);
    }

    public UIText SetText(string text, float fontSize)
    {
        return InputField.SetText(text, fontSize);
    }
}