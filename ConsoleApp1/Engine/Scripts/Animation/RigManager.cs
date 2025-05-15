public static class RigManager
{
    public static Dictionary<string, Rig> Rigs = [];

    public static bool Add(Rig rig)
    {
        return Rigs.TryAdd(rig.Name, rig);
    }
}