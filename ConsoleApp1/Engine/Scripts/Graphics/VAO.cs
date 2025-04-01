using OpenTK.Graphics.OpenGL4;

public class VAO
{
    public static List<VAO> VAOs = new List<VAO>();

    public int ID;
    
    public VAO()
    {
        ID = GL.GenVertexArray();
        GL.BindVertexArray(ID);
        VAOs.Add(this);
    }
    
    public void LinkToVAO<T>(int location, int size, VBO<T> vbo) where T : struct
    {
        Bind();
        vbo.Bind();
        GL.VertexAttribPointer(location, size, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(location);
        Unbind();
    }
    
    public void Bind() { GL.BindVertexArray(ID); }
    public void Unbind() { GL.BindVertexArray(0); }
    public static void Delete()
    {
        foreach (var vao in VAOs)
        {
            GL.DeleteVertexArray(vao.ID);
        }
        VAOs.Clear();
    }
}