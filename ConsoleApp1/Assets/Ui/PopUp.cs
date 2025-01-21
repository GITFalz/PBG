using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class PopUp : Updateable
{
    public static PopUp? Instance = null;
    public List<string> messages = [];

    public UIController PopUpUi = new();
    private static ShaderProgram _uiShader = new ShaderProgram("NewUI/UI.vert", "NewUI/UI.frag");
    private static TextureArray _uItexture = OldUIController._uItexture;

    private bool isShowing = true;

    public PopUp()
    {
        if (Instance != null)
            return;

        Instance = this;

        //UIPanel panel = new(AnchorType.MiddleCenter, 0, (0, 0, 0), (100, 100), (10, 10, 10, 10), 0, 0, null);
        UIText text = new(AnchorType.MiddleCenter, 0, (0, 0, 0), (100, 100), (10, 10, 10, 10), 0, 0, null);

        text.SetText("Test", 2);

        //PopUpUi.AddElement(panel);
        PopUpUi.AddElement(text);

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