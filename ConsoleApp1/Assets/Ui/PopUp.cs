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

        panel = new("PopupPanel", AnchorType.BottomCenter, PositionType.Absolute, (0, 0, 0), (500, 80), (0, 0, 0, 0), 0, 0, (10, 0.15f), null);
        text = new("PopupText", AnchorType.TopCenter, PositionType.Relative, (0, 0, 0), (100, 100), (0, 10, 0, 0), 0, 0, (10, 0.15f), null);
        button = new("PopupButton", AnchorType.TopRight, PositionType.Relative, (0, 0, 0), (30, 30), (0, 0, 0, 0), 0, 89, (0, 0), null, UIState.Interactable);

        panel.Depth = 0.5f;
        text.Depth = 0.6f;
        button.Depth = 0.6f;

        button.OnClick = new SerializableEvent(CloseCurrentPopUp);

        panel.AddChild(text);
        panel.AddChild(button);

        text.MaxCharCount = 30;
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
        if (!MovePopupEase(90, 1.5f))
            return;

        panel.Offset.Y = 85 - position.Y;
        panel.AlignAll();
        panel.UpdateAllTransformation();

        PopUpUi.UpdateMatrices();
    }

    public static void AddPopUp(string message)
    {
        Instance?.messages.Add(message);
    }

    /// <summary>
    /// Removes popups so only i are left
    /// </summary>
    /// <param name="i"></param>
    public static void Keep(int i)
    {
        if (Instance == null)
            return;

        while (Instance.messages.Count > i)
        {
            Instance.messages.RemoveAt(0);
        }
    }

    public bool MovePopupEase(float totalDistance, double duration)
    {
        if (elapsedTime < duration)
        {
            double t = elapsedTime / duration;
            double easedT = t < 0.5 
                ? 4 * t * t * t 
                : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            
            position.Y = (float)easedT * totalDistance;
        }
        else if (elapsedTime < duration * 2)
        {
            double t = (elapsedTime - duration) / duration;
            double easedT = t < 0.5 
                ? 4 * t * t * t 
                : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            
            position.Y = totalDistance - (float)(easedT * totalDistance);
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