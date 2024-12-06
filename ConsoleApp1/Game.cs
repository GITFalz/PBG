using ConsoleApp1.Assets.Scripts.World.Blocks;
using ConsoleApp1.Assets.Scripts.World.Chunk;
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
    
    private VoxelManager voxelManager;
    private BlockManager blockManager;
    private Chunk chunk;
    
    private List<VAO> vao;
    private List<IBO> ibo;
    private List<int> indicesCount;
    
    private ShaderProgram shaderProgram;
    private TextureArray textureArray;
    
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
        
        
        voxelManager = new VoxelManager();
        blockManager = new BlockManager();
        chunk = new Chunk();
        
        blockManager.Init();
        
        vao = new List<VAO>(9);
        ibo = new List<IBO>(9);
        indicesCount = new List<int>(9);
        
        for (int x = 0; x < 3; x++)
        {
            for (int z = 0; z < 3; z++)
            {
                chunk.GenerateChunk(new Vector3(x * Chunk.WIDTH, 0, z * Chunk.DEPTH));
                chunk.RenderChunk();
                
                vao[x * 3 + z] = chunk.chunkVao;
                ibo[x * 3 + z] = chunk.chunkIbo;
                
                indicesCount[x * 3 + z] = chunk.voxelManager.indices.Count;
            }
        }
        
        shaderProgram = new ShaderProgram("Default.vert", "Default.frag");
        textureArray = new TextureArray("Test_TextureAtlas.png", 32, 32);
        
        GL.Enable(EnableCap.DepthTest);
        
        _mainCamera = new Camera(width, height, new Vector3(0, 0, 0));
    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        foreach (var v in vao)
            v.Delete();
        
        foreach (var i in ibo)
            i.Delete();
            
        shaderProgram.Delete();
        textureArray.Delete();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0.6f, 0.3f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        foreach (var v in vao)
            v.Bind();
        
        foreach (var i in ibo) 
            i.Bind();

        shaderProgram.Bind();
        textureArray.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = _mainCamera.GetViewMatrix();
        Matrix4 projection = _mainCamera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        GL.DrawElements(PrimitiveType.Triangles, chunk.voxelManager.indices.Count, DrawElementsType.UnsignedInt, 0);

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