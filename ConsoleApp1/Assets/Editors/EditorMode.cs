public class EditorMode
{
    public static EditorMode Instance { get; private set; }
    
    public EditorMode()
    {
        Instance = this;
    }

    public void Update()
    {
        Console.WriteLine("EditorMode Update");
    }
}