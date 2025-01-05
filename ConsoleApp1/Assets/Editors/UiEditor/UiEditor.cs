using OpenTK.Mathematics;

namespace ConsoleApp1.Assets.Editors.UiEditor;

public class UiEditor : Updateable
{
    public override void Awake()
    {
        Console.WriteLine("UiEditor Awake");

        string fileName = "panelTest";
        string modelPath = Path.Combine(Game.modelPath, fileName);
        
        string[] lines = File.ReadAllLines(modelPath);
        
        Scene? scene = Game.Instance.GetScene(lines[0].Split(":")[1].Trim());
        if (scene == null)
            return;
    }
    
    public override void Start()
    {
        Console.WriteLine("UiEditor Start");
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