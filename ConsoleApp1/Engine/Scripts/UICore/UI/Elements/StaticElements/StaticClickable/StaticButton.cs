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
        lines.Add(gapString + "    OnClick: " + (OnClick == null ? "null" : SceneName + "." + OnClick.TargetName + "." + OnClick.MethodName));
        lines.Add(gapString + "    OnHover: " + (OnHover == null ? "null" : SceneName + "." + OnHover.TargetName + "." + OnHover.MethodName));
        lines.Add(gapString + "    OnHold: " + (OnHold == null ? "null" : SceneName + "." + OnHold.TargetName + "." + OnHold.MethodName));
        lines.Add(gapString + "    OnRelease: " + (OnRelease == null ? "null" : SceneName + "." + OnRelease.TargetName + "." + OnRelease.MethodName));
        lines.Add(gapString + "}");
        
        return lines;
    }
}