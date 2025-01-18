using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Text.Json;

public class StaticButton : StaticClickable
{
    public StaticButton(string name)
    {
        Name = name;
        
        AnchorType = AnchorType.MiddleCenter;
        PositionType = PositionType.Absolute;
    }
    
    public override List<string> ToLines(int gap)
    {
        List<string> lines = new List<string>();
        string gapString = "";
        for (int i = 0; i < gap; i++)
        {
            gapString += "    ";
        }
        
        lines.Add(gapString + "Static Button");
        lines.Add(gapString + "{");
        lines.Add(gapString + "    Name: " + Name);
        lines.Add(gapString + "    Position: " + Position);
        lines.Add(gapString + "    Scale: " + Scale);
        lines.Add(gapString + "    Offset: " + Offset);
        lines.Add(gapString + "    AnchorType: " + (int)AnchorType);
        lines.Add(gapString + "    PositionType: " + (int)PositionType);
        lines.Add(gapString + "    TextureIndex: " + TextureIndex);
        lines.Add(gapString + "    OnClick: " + GetMethodString(OnClick));
        lines.Add(gapString + "    OnHover: " + GetMethodString(OnHover));
        lines.Add(gapString + "    OnHold: " + GetMethodString(OnHold));
        lines.Add(gapString + "    OnRelease: " + GetMethodString(OnRelease));
        lines.Add(gapString + "}");
        
        return lines;
    }
}