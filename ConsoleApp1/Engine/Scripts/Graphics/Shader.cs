using OpenTK.Graphics.OpenGL4;

public static class Shader
{
    public static string LoadShaderSource(string filePath)
    {
        string shaderSource = "";

        try
        {
            using (StreamReader reader = new StreamReader(Path.Combine(Game.shaderPath, filePath)))
            {
                shaderSource = reader.ReadToEnd();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to load shader source file: " + e.Message);
        }

        return shaderSource;
    }

    public static bool Error(string message = "Error: ")
    {
        ErrorCode error = GL.GetError();
        if (error != ErrorCode.NoError)
        {
            Console.WriteLine(message + error);
            return true;
        }
        return false;
    }

    public static void ErrorAndClose(string message = "Error: ")
    {
        if (Error(message))
            Game.CloseGame();
    }
}