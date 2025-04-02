using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

public struct DrawArraysIndirectCommand
{
    public uint count;
    public uint instanceCount;
    public int first;
    public uint baseInstance;
}

public struct DrawElementsIndirectCommand
{
    public uint count;
    public uint instanceCount;
    public uint firstIndex;
    public int baseVertex;
    public uint baseInstance;
}

public class IDBOBase
{
    public static List<IDBOBase> IDBOs = new List<IDBOBase>();
    public int ID;
    
    public void Bind()
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, ID);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, 0);
    }

    public static void Delete()
    {
        foreach (var idbo in IDBOs)
            GL.DeleteBuffer(idbo.ID);
        IDBOs.Clear();
    }
}

public class ElementIDBO : IDBOBase
{
    public List<DrawElementsIndirectCommand> Commands { get; private set; }

    public ElementIDBO(List<DrawElementsIndirectCommand> commands)
    {
        Commands = commands;
        ID = GL.GenBuffer();
        Create();
        IDBOs.Add(this);
    }

    private void Create()
    {
        Bind();
        GL.BufferData(BufferTarget.DrawIndirectBuffer, Commands.Count * Marshal.SizeOf<DrawElementsIndirectCommand>(), Commands.ToArray(), BufferUsageHint.StaticDraw);
        Unbind();
    }

    public void SetCommands(List<DrawElementsIndirectCommand> commands)
    {
        Commands = commands;
        Create();
    }
}

public class ArrayIDBO : IDBOBase
{
    public List<DrawArraysIndirectCommand> Commands { get; private set; }

    public ArrayIDBO(List<DrawArraysIndirectCommand> commands)
    {
        Commands = commands;
        ID = GL.GenBuffer();
        Create();
        IDBOs.Add(this);
    }

    private void Create()
    {
        Bind();
        GL.BufferData(BufferTarget.DrawIndirectBuffer, Commands.Count * Marshal.SizeOf<DrawArraysIndirectCommand>(), Commands.ToArray(), BufferUsageHint.StaticDraw);
        Unbind();
    }

    public void SetCommands(List<DrawArraysIndirectCommand> commands)
    {
        Commands = commands;
        Create();
    }
}