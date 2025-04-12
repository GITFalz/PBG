using OpenTK.Graphics.OpenGL4;

public class ShaderProgram : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;

    public ShaderProgram(string vertexShaderFilePath) : base()
    {
        CreateShader(vertexShaderFilePath);
        _bufferCount++;
    }
    
    public ShaderProgram(string vertexShaderFilePath, string fragmentShaderFilePath) : base()
    {
        CreateShader(vertexShaderFilePath, fragmentShaderFilePath);
        _bufferCount++;
    }

    public ShaderProgram(string vertexShaderFilePath, string geometryShaderFilePath, string fragmentShaderFilePath) : base()
    {
        CreateShader(vertexShaderFilePath, geometryShaderFilePath, fragmentShaderFilePath);
        _bufferCount++;
    }

    public void Renew(string vertexShaderFilePath) => CreateShader(vertexShaderFilePath);
    public void Renew(string vertexShaderFilePath, string fragmentShaderFilePath) => CreateShader(vertexShaderFilePath, fragmentShaderFilePath);
    public void Renew(string vertexShaderFilePath, string geometryShaderFilePath, string fragmentShaderFilePath) => CreateShader(vertexShaderFilePath, geometryShaderFilePath, fragmentShaderFilePath);
    public void Bind() => GL.UseProgram(ID);
    public void Unbind() => GL.UseProgram(0);

    private void CreateShader(string vertexShaderFilePath)
    {
        ID = GL.CreateProgram();
        
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, Shader.LoadShaderSource(vertexShaderFilePath));
        GL.CompileShader(vertexShader);
        
        GL.AttachShader(ID, vertexShader);
        
        GL.LinkProgram(ID);
        
        GL.DeleteShader(vertexShader);
    }

    private void CreateShader(string vertexShaderFilePath, string fragmentShaderFilePath)
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

    private void CreateShader(string vertexShaderFilePath, string geometryShaderFilePath, string fragmentShaderFilePath)
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
    }


    public override void DeleteBuffer()
    {
        GL.DeleteProgram(ID);
        _bufferCount--;
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "ShaderProgram";
    }
}

public enum ShaderProgramType 
{
    VertexFragmentShader,
    VertexGeometryFragmentShader
}