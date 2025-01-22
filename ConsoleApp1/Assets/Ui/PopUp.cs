using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class PopUp : Updateable
{
    public static PopUp? Instance = null;
    public List<string> messages = [];

    public UIController PopUpUi = new();
    private static ShaderProgram _uiShader = new ShaderProgram("NewUI/UI.vert", "NewUI/UI.frag");
    private static TextureArray _uItexture = OldUIController._uItexture;

    private bool isShowing = false;

    public PopUp()
    {
        if (Instance != null)
            return;

        Instance = this;

        UIPanel panel = new("testPanel", AnchorType.MiddleCenter, PositionType.Absolute, (0, 0, 0), (100, 100), (10, 10, 10, 10), 0, 0, null);
        UIInputField inputField = new("testInputfield", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 100), (10, 10, 10, 10), 0, 0, null);

        panel.AddChild(inputField);

        inputField.MaxCharCount = 5;
        inputField.SetText("Hello", 0.7f);

        PopUpUi.AddElement(panel);
        PopUpUi.AddElement(inputField);

        PopUpUi.GenerateBuffers();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void Render()
    {
        if (isShowing)
            PopUpUi.Render();
    }


    public void ShowNextPopUp()
    {
        if (messages.Count > 0)
        {
            //PopUpUi.ShowPopUp(messages[0]);
            messages.RemoveAt(0);
        }
    }

    public void ShowPopUp(string message)
    {
        
    }

    public static void AddPopUp(string message)
    {
        Instance?.messages.Add(message);
    }
}