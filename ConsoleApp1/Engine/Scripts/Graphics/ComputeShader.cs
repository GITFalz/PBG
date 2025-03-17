using OpenTK.Graphics.OpenGL4;

public class ComputeShader
{
    public int ID;

    public ComputeShader(string data, bool isFile = true)
    {
        ID = GL.CreateProgram();

        int computeShader = GL.CreateShader(ShaderType.ComputeShader);       

        if (isFile) data = Shader.LoadShaderSource(data);

        GL.ShaderSource(computeShader, data);
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

    public void Bind() { GL.UseProgram(ID); }

    public void Unbind() { GL.UseProgram(0); }

    public void DispatchCompute(int workGroupX, int workGroupY, int workGroupZ)
    {
        GL.DispatchCompute(workGroupX, workGroupY, workGroupZ);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
        GL.Finish();
    }

    public void Delete() { GL.DeleteProgram(ID); }
}