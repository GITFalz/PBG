using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class PopUp : Component
{
    public static PopUp? Instance = null;
    public List<string> messages = [];

    public double elapsedTime = 0;
    public Vector3 position = new(0, 0, 0);

    public UIController PopUpUi = new();

    public UIPanel panel;
    public UIText text;
    public UIButton button;

    private bool isShowing = false;

    public PopUp()
    {
        Instance ??= this;

        panel = new("PopupPanel", AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (500, 80), (10, 10, 0, 0), 0, 0, null);
        text = new("PopupText", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 100), (10, 10, 0, 0), 0, 0, null);
        button = new("PopupButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (80, 30), (10, 10, 0, 0), 0, 0, null, UIState.Interactable);

        button.OnClick = new SerializableEvent(CloseCurrentPopUp);

        panel.AddChild(text);
        panel.AddChild(button);

        text.MaxCharCount = 100;
        text.SetText("Hello", 0.7f);

        PopUpUi.AddElement(panel);

        PopUpUi.GenerateBuffers();
    }

    public override void Update()
    {
        if (!isShowing && !ShowNextPopUp()) {
            return;
        }

        MovePopup();
        PopUpUi.Update();
    }

    public override void Render()
    {
        if (isShowing)
            PopUpUi.Render();
    }

    public void CloseCurrentPopUp()
    {
        if (!ShowNextPopUp())
        {
            elapsedTime = 0;
            isShowing = false;
        }
    }

    public bool ShowNextPopUp()
    {
        if (messages.Count > 0)
        {
            text.SetText(messages[0], 0.7f);
            text.GenerateChars();
            text.UpdateText();
            messages.RemoveAt(0);
            isShowing = true;
            elapsedTime = 0;
            return true;
        }
        return false;
    }

    public void MovePopup()
    {
        if (!MovePopupEase(510, 2))
            return;

        panel.Offset.X = -500 + position.X;
        panel.AlignAll();
        panel.UpdateAllTransformation();

        PopUpUi.UpdateMatrices();
    }

    public static void AddPopUp(string message)
    {
        Instance?.messages.Add(message);
    }

    public bool MovePopupEase(float totalDistance, double duration)
    {
        if (elapsedTime < duration)
        {
            double t = elapsedTime / duration;
            double easedT = t < 0.5 
                ? 4 * t * t * t 
                : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            
            position.X = (float)easedT * totalDistance;
        }
        else if (elapsedTime < duration * 2)
        {
            double t = (elapsedTime - duration) / duration;
            double easedT = t < 0.5 
                ? 4 * t * t * t 
                : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            
            position.X = totalDistance - (float)(easedT * totalDistance);
        }
        else
        {
            elapsedTime = 0;
            isShowing = false;
            return false;
        }

        elapsedTime += GameTime.DeltaTime;
        return true;
    }
}