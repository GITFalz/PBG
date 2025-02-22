using OpenTK.Mathematics;

public abstract class Meshes
{
    public abstract void SaveModel(string modelName);
    public abstract void SaveModel(string modelName, string basePath);
    public abstract bool LoadModel(string modelName);
    public abstract bool LoadModel(string modelName, string basePath);
    public abstract void Unload();

    public Vector3 Parse(string line, Vector3i indices)
    {
        try 
        {
            string[] parts = line.Split(' ');
            return new Vector3(
                float.Parse(parts[indices.X].Trim()),
                float.Parse(parts[indices.Y].Trim()),
                float.Parse(parts[indices.Z].Trim())
            );
        }
        catch (Exception)
        {
            return Vector3.Zero;
        }
    }
}