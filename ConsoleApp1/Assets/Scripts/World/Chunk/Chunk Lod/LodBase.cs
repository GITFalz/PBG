public static class LodBase
{
    public static List<LodTreeBase> Trees = [new LodChunk()];

    public static void Update()
    {

    }

    public static void Render()
    {
        foreach (var tree in Trees)
        {
            tree.Render();
        }
    }
}