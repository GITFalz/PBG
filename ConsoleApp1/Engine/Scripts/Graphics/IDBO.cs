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

public abstract class IDBOBase : BufferBase
{
    public int ID;
    public int ElementCount;

    private static int _bufferCount = 0;

    public IDBOBase() : base() { _bufferCount++; }
    
    public void Bind()
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, ID);
    }

    public void Unbind()
    {
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, 0);
    }

    public override int GetBufferCount()
    {
        return _bufferCount;
    }

    public override string GetTypeName()
    {
        return "IDBO";
    }
}

public class ElementIDBO : IDBOBase
{
    public ElementIDBO(List<DrawElementsIndirectCommand> commands) : base()
    {
        ID = GL.GenBuffer();
        Create(commands.ToArray());
    }

    private void Create(DrawElementsIndirectCommand[] commands)
    {
        Bind();
        ElementCount = commands.Length;
        GL.BufferData(BufferTarget.DrawIndirectBuffer, commands.Length * Marshal.SizeOf<DrawElementsIndirectCommand>(), commands, BufferUsageHint.StaticDraw);
        Unbind();
    }

    public void Renew(List<DrawElementsIndirectCommand> commands) => Renew(commands.ToArray());
    public void Renew(DrawElementsIndirectCommand[] commands)
    {
        GL.DeleteBuffer(ID);
        Create(commands);
    }
}

public class ArrayIDBO : IDBOBase
{
    public ArrayIDBO(List<DrawArraysIndirectCommand> commands) : base()
    {
        ID = GL.GenBuffer();
        Create(commands.ToArray());
    }

    private void Create(DrawArraysIndirectCommand[] commands)
    {
        Bind();
        ElementCount = commands.Length;
        GL.BufferData(BufferTarget.DrawIndirectBuffer, commands.Length * Marshal.SizeOf<DrawArraysIndirectCommand>(), commands, BufferUsageHint.StaticDraw);
        Unbind();
    }

    public void Renew(List<DrawArraysIndirectCommand> commands) => Renew(commands.ToArray());
    public void Renew(DrawArraysIndirectCommand[] commands)
    {
        GL.DeleteBuffer(ID);
        Create(commands);
    }
}