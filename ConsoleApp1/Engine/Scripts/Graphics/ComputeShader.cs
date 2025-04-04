using OpenTK.Graphics.OpenGL4;

public class ComputeShader
{
    public static List<ComputeShader> ComputeShaders = new List<ComputeShader>();

    public int ID;

    public ComputeShader(string computeShaderFilePath)
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

        ComputeShaders.Add(this);
    }

    public void Bind() { GL.UseProgram(ID); }

    public void Unbind() { GL.UseProgram(0); }

    public void DispatchCompute(int workGroupX, int workGroupY, int workGroupZ)
    {
        GL.DispatchCompute(workGroupX, workGroupY, workGroupZ);
        GL.MemoryBarrier(MemoryBarrierFlags.ShaderStorageBarrierBit);
    }

    public void Unload()
    {
        GL.DeleteProgram(ID);
        ComputeShaders.Remove(this);
    }

    public static void Delete()
    {
        foreach (var computeShader in ComputeShaders)
        {
            GL.DeleteProgram(computeShader.ID);
        }
        ComputeShaders.Clear();
    }
}