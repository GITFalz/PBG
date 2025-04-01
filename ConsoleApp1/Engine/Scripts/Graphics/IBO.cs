using OpenTK.Graphics.OpenGL4;

public class IBO
{
    public static List<IBO> IBOs = new List<IBO>();

    public int ID;
    
    public IBO(List<uint> data)
    {
        ID = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID);
        GL.BufferData(BufferTarget.ElementArrayBuffer, data.Count * sizeof(uint), data.ToArray(), BufferUsageHint.StaticDraw);
        IBOs.Add(this);
    }
    
    public void Bind() 
    { 
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, ID); 
    }

    public void Unbind() 
    { 
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0); 
    }

    public static void Delete() 
    { 
        foreach (var ibo in IBOs) 
            GL.DeleteBuffer(ibo.ID); 
        IBOs.Clear();
    }
}