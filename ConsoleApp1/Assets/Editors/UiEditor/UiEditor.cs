using OpenTK.Mathematics;

namespace ConsoleApp1.Assets.Editors.UiEditor;

public class UiEditor : Updateable
{
    public UIController MainUi = new UIController();
    
    public override void Awake()
    {
        Console.WriteLine("UiEditor Awake");
    }
    
    public override void Start()
    {
        Console.WriteLine("UiEditor Start");

        gameObject.Scene.UiController = MainUi;
        gameObject.Scene.LoadUi();
    }
    
    public override void Update()
    {
        
    }

    public override void Render()
    {
        
    }

    public override void Exit()
    {
        base.Exit();
    }
}