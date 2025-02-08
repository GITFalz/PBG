using OpenTK.Graphics.OpenGL4;

public class ShaderProgram
{
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
    }
    
    public void Bind() { GL.UseProgram(ID); }
    public void Unbind() { GL.UseProgram(0); }
    public void Delete() { GL.DeleteShader(ID); }
}