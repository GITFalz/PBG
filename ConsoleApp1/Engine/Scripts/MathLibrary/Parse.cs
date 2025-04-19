using OpenTK.Mathematics;

public static class Parse
{
    public static Vector4 Vec4(string str)
    {
        str = str.Trim(['(',')']);
        string[] parts = str.Split(',');
        if (parts.Length != 4)
            return Vector4.Zero;

        float x = float.Parse(parts[0].Trim());
        float y = float.Parse(parts[1].Trim());
        float z = float.Parse(parts[2].Trim());
        float w = float.Parse(parts[3].Trim());

        return new Vector4(x, y, z, w);
    }

    public static Vector3 Vec3(string str)
    {
        str = str.Trim(['(',')']);
        string[] parts = str.Split(',');
        if (parts.Length != 3)
            return Vector3.Zero;

        float x = float.Parse(parts[0].Trim());
        float y = float.Parse(parts[1].Trim());
        float z = float.Parse(parts[2].Trim());

        return new Vector3(x, y, z);
    }

    public static Vector2 Vec2(string str)
    {
        str = str.Trim(['(',')']);
        string[] parts = str.Split(',');
        if (parts.Length != 2)
            return Vector2.Zero;

        float x = float.Parse(parts[0].Trim());
        float y = float.Parse(parts[1].Trim());

        return new Vector2(x, y);
    }

    public static float Float(string str)
    {
        str = str.Trim(['(',')']);
        return float.Parse(str);
    }
}