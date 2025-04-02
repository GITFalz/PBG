using OpenTK.Graphics.OpenGL4;

public class ShaderProgram
{
    public static List<ShaderProgram> ShaderPrograms = new List<ShaderProgram>();

    public int ID;
    
    public ShaderProgram(string vertexShaderFilePath, string fragmentShaderFilePath)
    {
        ID = GL.CreateProgram();
        
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, Shader.LoadShaderSource(vertexShaderFilePath));
        GL.CompileShader(vertexShader);
        
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, Shader.LoadShaderSource(fragmentShaderFilePath));
        GL.CompileShader(fragmentShader);
        
        GL.AttachShader(ID, vertexShader);
        GL.AttachShader(ID, fragmentShader);
        
        GL.LinkProgram(ID);
        
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);

        ShaderPrograms.Add(this);
    }

    public ShaderProgram(string vertexShaderFilePath, string geometryShaderFilePath, string fragmentShaderFilePath)
    {
        ID = GL.CreateProgram();
        
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, Shader.LoadShaderSource(vertexShaderFilePath));
        GL.CompileShader(vertexShader);

        int geometryShader = GL.CreateShader(ShaderType.GeometryShader);
        GL.ShaderSource(geometryShader, Shader.LoadShaderSource(geometryShaderFilePath));
        GL.CompileShader(geometryShader);
        
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, Shader.LoadShaderSource(fragmentShaderFilePath));
        GL.CompileShader(fragmentShader);
        
        GL.AttachShader(ID, vertexShader);
        GL.AttachShader(ID, geometryShader);
        GL.AttachShader(ID, fragmentShader);
        
        GL.LinkProgram(ID);
        
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(geometryShader);
        GL.DeleteShader(fragmentShader);

        ShaderPrograms.Add(this);
    }
    
    public void Bind() { GL.UseProgram(ID); }
    public void Unbind() { GL.UseProgram(0); }
    public static void Delete() 
    { 
        foreach (var shaderProgram in ShaderPrograms) 
        { 
            GL.DeleteProgram(shaderProgram.ID); 
        }
        ShaderPrograms.Clear();
    }
}

public enum ShaderProgramType 
{
    VertexFragmentShader,
    VertexGeometryFragmentShader
}