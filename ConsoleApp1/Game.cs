using ConsoleApp1.Engine.Scripts.Core.Graphics;
using ConsoleApp1.Engine.Scripts.Core.Rendering;
using ConsoleApp1.Engine.Scripts.Core.Voxel;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ConsoleApp1;

public class Game : GameWindow
{
    private int width, height;
    
    private Camera _mainCamera;

    private Mesh mesh;
    private VoxelManager voxelManager;

    private VAO vao;
    private IBO ibo;
    ShaderProgram shaderProgram;
    Texture texture;
    
    public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        this.width = width;
        this.height = height;
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();
        
        mesh = new Mesh();
        voxelManager = new VoxelManager();
        
        voxelManager.GenerateBlock(Vector3.Zero);
        
        mesh.vertices = voxelManager.vertices;
        mesh.uvs = voxelManager.uvs;
        mesh.indices = voxelManager.indices;

        try
        {
            mesh.CheckConformity();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Environment.Exit(1);
        }
        
        mesh.RecalculateBounds();
        mesh.SeperateUvs();

        vao = new VAO();
        
        VBO vbo = new VBO(mesh.vertices);
        VBO uvVBO = new VBO(mesh.uvs2D);
        VBO texIndexVBO = new VBO(mesh.texIndex);
        
        vao.LinkToVAO(0, 3, vbo);
        vao.LinkToVAO(1, 2, uvVBO);
        vao.LinkToVAO(2, 1, texIndexVBO);

        ibo = new IBO(mesh.indices);
        
        shaderProgram = new ShaderProgram("Default.vert", "Default.frag");
        
        texture = new Texture("dirt_block.png");
        
        GL.Enable(EnableCap.DepthTest);
        
        _mainCamera = new Camera(width, height, new Vector3(0, 0, 0));
    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        vao.Delete();
        ibo.Delete();
        shaderProgram.Delete();
        texture.Delete();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0.6f, 0.3f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        shaderProgram.Bind();
        vao.Bind();
        ibo.Bind();
        texture.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = _mainCamera.GetViewMatrix();
        Matrix4 projection = _mainCamera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        GL.DrawElements(PrimitiveType.Triangles, mesh.indices.Count, DrawElementsType.UnsignedInt, 0);

        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        base.OnUpdateFrame(args);
        
        _mainCamera.Update(keyboard, mouse, args);
        CursorState = CursorState.Grabbed;
    }
}