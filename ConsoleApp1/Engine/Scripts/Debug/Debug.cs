using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class Debug
{
    private static DebugMesh _mesh;
    
    private static ShaderProgram _shaderProgram;
    
    public static void Start()
    {
        _shaderProgram = new ShaderProgram("Debug/Entity.vert", "Debug/Entity.frag");
        
        _mesh = new DebugMesh();
        
        
        
        _mesh.GenerateBuffers();
    }
    
    public static void Update(Vector3 position)
    {
        _mesh.Position = position;
        
        _mesh.UpdatePosition();
        _mesh.UpdateScale(new Vector3(1, 1, 1));
        _mesh.UpdateRotation(Vector3.Zero, new Vector3(0, 1, 0), 0);
        _mesh.UpdateMesh();
    }

    /// <summary>
    /// NOTE: This method is not optimized and should only be used for debugging purposes, also this function should only be called once!!!
    /// </summary>
    /// <param name="position"></param>
    /// <param name="size"></param>
    public static void DrawBox(Vector3 position, Vector3 size)
    {
        _mesh.Position = position;
        
        _mesh.UpdatePosition();
        _mesh.UpdateScale(size);
        _mesh.UpdateMesh();
    }
    
    public static void DrawHitbox(Hitbox hitbox)
    {
        _mesh.Position = hitbox.Position + hitbox.Min;
        
        _mesh.UpdatePosition();
        _mesh.UpdateScale(new Vector3(hitbox.Width, hitbox.Height, hitbox.Depth));
        _mesh.UpdateMesh();
    }

    public static void Render()
    {
        _shaderProgram.Bind();
        
        Matrix4 model = Matrix4.Identity;
        Matrix4 view = Camera.viewMatrix;
        Matrix4 projection = Camera.projectionMatrix;

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        _mesh.RenderMesh();
        
        _shaderProgram.Unbind();
    }
}