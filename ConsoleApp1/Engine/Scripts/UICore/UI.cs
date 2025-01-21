using OpenTK.Mathematics;
using StbImageSharp;

public class UI
{
    public static StaticButton CreateStaticButton(string name, AnchorType anchorType, PositionType positionType, Vector3 scale, Vector4 offset, OldUiMesh? mesh)
    {
        StaticButton button = new StaticButton(name);
        
        button.SetAnchorType(anchorType);
        button.SetPositionType(positionType);
        button.SetScale(scale);
        button.SetOffset(offset);
        
        if (mesh != null)
            button.SetMesh(mesh);
        
        return button;
    }
    
    public static StaticText CreateStaticText(string name, string text, float fontSize, AnchorType? anchorType, PositionType? positionType, Vector3? scale, Vector4? offset)
    {
        StaticText Text = new StaticText(name, text, fontSize);
        
        if (anchorType != null)
            Text.SetAnchorType((AnchorType)anchorType);
        if (positionType != null)
            Text.SetPositionType((PositionType)positionType);
        if (scale != null)
            Text.SetScale((Vector3)scale);
        if (offset != null)
            Text.SetOffset((Vector4)offset);
        Text.SetMesh(new OldTextMesh());
        
        return Text;
    }
    
    public static StaticInputField CreateStaticInputField(string name, string text, float fontSize, AnchorType? anchorType, PositionType? positionType, Vector3? scale, Vector4? offset)
    {
        StaticInputField inputField = new StaticInputField(name, text, fontSize);
        
        if (anchorType != null)
            inputField.SetAnchorType((AnchorType)anchorType);
        if (positionType != null)
            inputField.SetPositionType((PositionType)positionType);
        if (scale != null)
            inputField.SetScale((Vector3)scale);
        if (offset != null)
            inputField.SetOffset((Vector4)offset);
        
        return inputField;
    }
    
    public static StaticPanel CreateStaticPanel(string name, AnchorType anchorType, PositionType? positionType, Vector3 scale, Vector4? offset, OldUiMesh? mesh)
    {
        StaticPanel panel = new StaticPanel(name);
        
        panel.SetAnchorType(anchorType);
        panel.SetScale(scale);
        if (positionType != null)
            panel.SetPositionType((PositionType)positionType);
        if (offset != null)
            panel.SetOffset((Vector4)offset);
        if (mesh != null)
            panel.SetMesh(mesh);
        
        return panel;
    }
}

public class Panel
{
    public List<Vector3> Vertices = [];
    public List<Vector2> Uvs = [];
    public List<int> TextUvs = [];
    public List<Vector2> UiSizes = [];
}