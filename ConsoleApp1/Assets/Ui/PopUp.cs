using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class PopUp
{
    public static PopUp? Instance = null;
    public List<string> messages = [];
    public Dictionary<string, Actions> confirmations = new();
    public Action RenderAction = () => { };
    public Action UpdateAction = () => { };

    public double elapsedTime = 0;
    public Vector3 position = new(0, 0, 0);

    public UIController MessageUI = new();
    public UIController ConfirmationUI = new();

    public UIImage MessagePanel;
    public UICollection MessageCollection;
    public UIText Message;
    public UIButton MessageCloseButton;

    public UIImage ConfirmationPanel;
    public UICollection ConfirmationCollection;
    public UIText Confirmation;
    public UIButton ConfirmationAcceptButton;
    public UIButton ConfirmationDeclineButton;
    public UIText AcceptText;
    private bool isShowing = false;
    private bool isDone = false;

    public PopUp()
    {
        Instance ??= this;

        RenderAction = RenderMessage;
        UpdateAction = UpdateMessage;

        MessagePanel = new("PopupPanel", MessageUI, AnchorType.MiddleCenter, PositionType.Absolute, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (500, 80), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        MessageCollection = new("MessageCollection", MessageUI, AnchorType.BottomCenter, PositionType.Absolute, (0, 0, 0), (500, 80), (0, 80, 0, 0), 0);
        Message = new("PopupText", MessageUI, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 100), (10, 10, 0, 0), 0);
        MessageCloseButton = new("PopupButton", MessageUI, AnchorType.TopRight, PositionType.Relative, (0.7f, 0.7f, 0.7f, 1f), (0, 0, 0), (30, 30), (0, 0, 0, 0), 0, 89, (0, 0), UIState.Interactable);

        MessageCloseButton.SetOnClick(CloseCurrentMessage);

        MessageCollection.AddElement(MessagePanel);
        MessageCollection.AddElement(Message);
        MessageCollection.AddElement(MessageCloseButton);

        Message.MaxCharCount = 30;
        Message.SetText("Hello", 0.7f);

        MessageUI.AddElement(MessageCollection);


        ConfirmationPanel = new("AlertPanel", ConfirmationUI, AnchorType.MiddleCenter, PositionType.Absolute, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (500, 80), (0, 0, 0, 0), 0, 0, (10, 0.05f));
        ConfirmationCollection = new("AlertCollection", ConfirmationUI, AnchorType.BottomCenter, PositionType.Absolute, (0, 0, 0), (500, 80), (0, 80, 0, 0), 0);
        Confirmation = new("AlertText", ConfirmationUI, AnchorType.TopLeft, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 100), (10, 10, 0, 0), 0);
        ConfirmationAcceptButton = new("AlertButton", ConfirmationUI, AnchorType.BottomCenter, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (480, 35), (0, -10, 0, 0), 0, 0, (10, 0.05f), UIState.Interactable);
        ConfirmationDeclineButton = new("AlertButton", ConfirmationUI, AnchorType.TopRight, PositionType.Relative, (0.7f, 0.7f, 0.7f, 1f), (0, 0, 0), (30, 30), (0, 0, 0, 0), 0, 89, (0, 0), UIState.Interactable);
        AcceptText = new("AcceptText", ConfirmationUI, AnchorType.BottomCenter, PositionType.Relative, Vector4.One, (0, 0, 0), (100, 100), (0, -20, 0, 0), 0);

        ConfirmationAcceptButton.SetOnClick(AcceptCurrentConfirmation);
        ConfirmationDeclineButton.SetOnClick(DeclineCurrentConfirmation);

        ConfirmationCollection.AddElement(ConfirmationPanel);
        ConfirmationCollection.AddElement(Confirmation);
        ConfirmationCollection.AddElement(ConfirmationAcceptButton);
        ConfirmationCollection.AddElement(ConfirmationDeclineButton);
        ConfirmationCollection.AddElement(AcceptText);

        Confirmation.MaxCharCount = 30;
        Confirmation.SetText("Hello", 0.7f);
        AcceptText.MaxCharCount = 6;
        AcceptText.SetText("Accept", 1f);

        ConfirmationUI.AddElement(ConfirmationCollection);
    }

    public void Resize()
    {
        MessageUI.Resize();
        ConfirmationUI.Resize();
    }

    public void Update()
    {
        if (!isShowing)
            return;
        UpdateAction.Invoke();
    }

    public void UpdateMessage()
    {
        if (!isShowing)
            return;

        if (isDone)
            ShowNextMessage();

        MoveMessage();
        MessageUI.Update();
    }

    public void UpdateConfirmation()
    {
        if (!isShowing)
            return;

        if (isDone)
            ShowNextConfirmation();

        MoveConfirmation();
        ConfirmationUI.Update();
    }


    public void Render()
    {
        if (!isShowing)
            return;
        RenderAction.Invoke();
    }

    public void RenderMessage()
    {
        MessageUI.RenderDepthTest();      
    }

    public void RenderConfirmation()
    {
        ConfirmationUI.RenderDepthTest();
    }


    public void CloseCurrentMessage()
    {
        if (!ShowNextMessage())
        {
            elapsedTime = 0;
            isShowing = false;
        }
    }

    public void AcceptCurrentConfirmation()
    {
        confirmations.First().Value.Accept();
        CloseCurrentConfirmation();
    }

    public void DeclineCurrentConfirmation()
    {
        confirmations.First().Value.Decline();
        CloseCurrentConfirmation();
    }

    public void CloseCurrentConfirmation()
    {
        confirmations.Remove(confirmations.First().Key);
        isDone = true;
    }
    

    public bool ShowNextMessage()
    {
        isDone = false;

        if (messages.Count > 0)
        {
            Message.SetText(messages[0], 0.7f);
            Message.GenerateChars();
            Message.UpdateText();
            messages.RemoveAt(0);
            elapsedTime = 0;
            isShowing = true;
            return true;
        }

        elapsedTime = 0;
        isShowing = false;
        return false;
    }

    public bool ShowNextConfirmation()
    {
        Console.WriteLine("ShowNextConfirmation");
        isDone = false;

        if (confirmations.Count > 0)
        {
            Confirmation.SetText(confirmations.First().Key, 0.7f);
            Confirmation.GenerateChars();
            Confirmation.UpdateText();
            elapsedTime = 0;
            isShowing = true;
            return true;
        }

        UpdateAction = UpdateMessage;
        RenderAction = RenderMessage;

        elapsedTime = 0;
        isShowing = false;
        return false;
    }


    public void MoveMessage()
    {
        if (!MoveMessageEase(90, 1.5f))
            return;

        MessageCollection.Offset.Y = 85 - position.Y;
        MessageCollection.Align();
        MessageCollection.UpdateTransformation();
    }

    public void MoveConfirmation()
    {
        if (!MoveConfirmationEaseIn(90, 1.5f))
            return;

        ConfirmationCollection.Offset.Y = 85 - position.Y;
        ConfirmationCollection.Align();
        ConfirmationCollection.UpdateTransformation();
    }



    public static void AddPopUp(string message)
    {
        if (Instance == null)
            return;
        Instance.messages.Add(message);
        Instance.isShowing = true;
        Instance.isDone = true;
    }

    public static void AddConfirmation(string message, Action? accept, Action? decline)
    {
        if (Instance == null || Instance.confirmations.ContainsKey(message))
            return;

        Instance.confirmations.Add(message, new(accept, decline));  
        Instance.isShowing = true;
        Instance.isDone = true;

        Instance.UpdateAction = Instance.UpdateConfirmation;
        Instance.RenderAction = Instance.RenderConfirmation;
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


    public bool MoveMessageEase(float totalDistance, double duration)
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
            isDone = true;
            return false;
        }

        elapsedTime += GameTime.DeltaTime;
        return true;
    }

    public bool MoveConfirmationEaseIn(float totalDistance, double duration)
    {
        if (elapsedTime < duration)
        {
            double t = elapsedTime / duration;
            double easedT = t < 0.5 
                ? 4 * t * t * t 
                : 1 - Mathf.Pow(-2 * t + 2, 3) / 2;
            
            position.Y = (float)easedT * totalDistance;
        }
        else
        {
            return false;
        }

        elapsedTime += GameTime.DeltaTime;
        return true;
    }

    public class Actions { 
        Action? accept = null; 
        Action? decline = null;

        public Actions(Action? accept, Action? decline)
        {
            this.accept = accept;
            this.decline = decline;
        }

        public void Accept()
        {
            accept?.Invoke();
        }

        public void Decline()
        {
            decline?.Invoke();
        } 
    }
}