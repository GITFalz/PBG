using OpenTK.Graphics.OpenGL;

public static class Debug
{
    public static void CheckGLError(string context)
    {
        ErrorCode error;
        while ((error = GL.GetError()) != ErrorCode.NoError)
        {
            string errorMessage = $"OpenGL Error in {context}: {error}";
            Console.WriteLine(errorMessage);
        }
    }
}