using OpenTK.Graphics.OpenGL4;

public class ComputeShader : BufferBase
{
    public int ID;

    private static int _bufferCount = 0;

    public ComputeShader(string computeShaderFilePath) : base()
    {
        Create(computeShaderFilePath);
        _bufferCount++;
    }

    public void Bind() => GL.UseProgram(ID); 
    public void Unbind() => GL.UseProgram(0); 

    public void DispatchCompute(int workGroupX, int workGroupY, int workGroupZ)
    {
        GL.DispatchCompute(workGroupX, workGroupY, workGroupZ);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
    }

    public void Renew(string computeShaderFilePath) 
    {
        GL.DeleteProgram(ID); // The program needs to be deleted before creating a new one
        Create(computeShaderFilePath);
    }

    public void Create(string computeShaderFilePath)
    {
        ID = GL.CreateProgram();

        int computeShader = GL.CreateShader(ShaderType.ComputeShader);
        GL.ShaderSource(computeShader, Shader.LoadShaderSource(computeShaderFilePath));
        GL.CompileShader(computeShader);

        string compileStatus = GL.GetShaderInfoLog(computeShader);
        if (!string.IsNullOrEmpty(compileStatus))
        {
            Console.WriteLine("Error compiling compute shader: " + compileStatus);
        }

        GL.AttachShader(ID, computeShader);
        GL.LinkProgram(ID);

        string linkStatus = GL.GetProgramInfoLog(ID);
        if (!string.IsNullOrEmpty(linkStatus))
        {
            Console.WriteLine("Error linking compute shader program: " + linkStatus);
        }

        GL.DeleteShader(computeShader);
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
        return "ComputeShader";
    }
}