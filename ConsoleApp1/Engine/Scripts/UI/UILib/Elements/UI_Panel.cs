using ConsoleApp1.Engine.Scripts.Core.MathLibrary;
using ImGuiNET;
using OpenTK.Mathematics;

namespace ConsoleApp1.Engine.Scripts.Core.UI.UILib;

public class UI_Panel : UI_Base
{
    public UI_Panel(Vector2 position, Vector2 size)
    {
        name = "UI_Panel";
        
        this.position = Mathf.ToNumericsVector2(position);
        this.size = Mathf.ToNumericsVector2(size);
    }
    
    public override void RenderUI()
    {
        ImGui.GetWindowDrawList().AddRect(
            position, 
            position + size, 
            ImGui.GetColorU32(color)
        );
    }
}