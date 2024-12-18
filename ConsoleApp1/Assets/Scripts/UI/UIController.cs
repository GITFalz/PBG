using ConsoleApp1.Assets.Scripts.Inputs;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class UIController
{
    private List<UI_Base> uiElements = new List<UI_Base>();
    
    private List<UI_Button> buttons = new List<UI_Button>();

    
    public void Update(MouseState mouse)
    {
        if (mouse.IsAnyButtonDown)
        {
            CheckButtonOnClick(mouse);
            CheckButtonOnHold(mouse);
        }
        
        if (mouse.IsButtonReleased(MouseButton.Left))
        {
            CheckButtonOnRelease(mouse);
        }
    }
    
    
    public void CheckButtonOnClick(MouseState mouse)
    {
        foreach (UI_Button button in buttons)
        {
            if (button.IsButtonPressed(mouse))
            {
                button.OnClick?.Invoke();
            }
        }
    }
    
    public void CheckButtonOnHold(MouseState mouse)
    {
        foreach (UI_Button button in buttons)
        {
            if (button.IsButtonHeld(mouse))
            {
                button.OnHold?.Invoke(mouse);
            }
        }
    }
    
    public void CheckButtonOnRelease(MouseState mouse)
    {
        foreach (UI_Button button in buttons)
        {
            if (button.IsButtonReleased(mouse))
            {
                button.OnRelease?.Invoke();
            }
        }
    }
    
    public void AddElement(UI_Base element)
    {
        uiElements.Add(element);
        
        if (element is UI_Button button)
        {
            buttons.Add(button);
        }
    }
    
    public List<UI_Base> GetElements()
    {
        return uiElements;
    }
}