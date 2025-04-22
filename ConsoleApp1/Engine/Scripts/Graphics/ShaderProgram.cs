using OpenTK.Graphics.OpenGL4;

public class ShaderProgram : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;

    private Action _recompileAction = () => { };

    public ShaderProgram(string vertexShaderFilePath) : base()
    {
        CreateShader(vertexShaderFilePath);
        _bufferCount++;
        _recompileAction = () => Renew(vertexShaderFilePath);
    }
    
    public ShaderProgram(string vertexShaderFilePath, string fragmentShaderFilePath) : base()
    {
        CreateShader(vertexShaderFilePath, fragmentShaderFilePath);
        _bufferCount++;
        _recompileAction = () => Renew(vertexShaderFilePath, fragmentShaderFilePath);
    }

    public ShaderProgram(string vertexShaderFilePath, string geometryShaderFilePath, string fragmentShaderFilePath) : base()
    {
        CreateShader(vertexShaderFilePath, geometryShaderFilePath, fragmentShaderFilePath);
        _bufferCount++;
        _recompileAction = () => Renew(vertexShaderFilePath, geometryShaderFilePath, fragmentShaderFilePath);
    }

    public void Recompile()
    {
        _recompileAction.Invoke();
    }

    public void Bind() => GL.UseProgram(ID);
    public void Unbind() => GL.UseProgram(0);

    public void Renew(string vertexShaderFilePath) 
    {
        GL.DeleteProgram(ID); // The program needs to be deleted before creating a new one
        CreateShader(vertexShaderFilePath);
    }

    public void Renew(string vertexShaderFilePath, string fragmentShaderFilePath)
    {
        GL.DeleteProgram(ID); // The program needs to be deleted before creating a new one
        CreateShader(vertexShaderFilePath, fragmentShaderFilePath);
    }

    public void Renew(string vertexShaderFilePath, string geometryShaderFilePath, string fragmentShaderFilePath) 
    {
        GL.DeleteProgram(ID); // The program needs to be deleted before creating a new one
        CreateShader(vertexShaderFilePath, geometryShaderFilePath, fragmentShaderFilePath);
    }
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
        base.DeleteBuffer();
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "ShaderProgram";
    }

    public int GetLocation(string name)
    {
        return GL.GetUniformLocation(ID, name);
    }
}

public enum ShaderProgramType 
{
    VertexFragmentShader,
    VertexGeometryFragmentShader
}