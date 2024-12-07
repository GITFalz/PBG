using System.Diagnostics;
using ImGuiNET;
using ClickableTransparentOverlay;
using ConsoleApp1.Assets.Scripts.Inputs;
using ConsoleApp1.Assets.Scripts.Rendering;
using ConsoleApp1.Assets.Scripts.UI;
using ConsoleApp1.Assets.Scripts.World.Blocks;
using ConsoleApp1.Assets.Scripts.World.Chunk;
using ConsoleApp1.Engine.Scripts.Core;
using ConsoleApp1.Engine.Scripts.Core.Graphics;
using ConsoleApp1.Engine.Scripts.Core.MathLibrary;
using ConsoleApp1.Engine.Scripts.Core.Rendering;
using ConsoleApp1.Engine.Scripts.Core.UI;
using ConsoleApp1.Engine.Scripts.Core.UI.UILib;
using ConsoleApp1.Engine.Scripts.Core.Voxel;
using ConsoleApp1.Engine.Scripts.UI.UITextData;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using StbImageSharp;

namespace ConsoleApp1;

public class Game : GameWindow
{
    private readonly int _width;
    private readonly int _height;

    private Camera _mainCamera;
    
    // World
    private ChunkManager _chunkManager;
    private ShaderProgram _shaderProgram;
    private TextureArray _textureArray;
    
    
    // UI
    private VAO _uIvao;
    private VBO _uIvbo;
    private VBO _uItextureVbo;
    private IBO _uIibo;
    
    private ShaderProgram _uiShader;
    private Texture _uItexture;
    
    private MeshData _uiMeshData;
    
    private Matrix4 _orthographicProjection;
    
    
    // Text
    private VAO _textVao;
    private VBO _textVbo;
    private VBO _textTextureVbo;
    private IBO _textIbo;
    
    private ShaderProgram _textShader;
    private Texture _textTexture;
    
    private MeshData _textMeshData;
    
    
    // Input
    private bool _isTyping = false;
    private Character _currentCharacter;
    
    private List<Character> _inputField = new List<Character>()
    {
        Character.A,
        Character.B,
        Character.C,
        Character.D,
        Character.E,
        Character.F,
        Character.G,
        Character.H,
        Character.I,
        Character.I,
        Character.I,
        Character.I,
        Character.I,
    };
    
    
    // Events
    private Action? _updateText;
    
    
    
    public List<GameObject> GameObjects = new List<GameObject>();
    
    private bool _visibleCursor = false;
    private KeyboardSwitch _visibleCursorSwitch;



    private int frameCount = 0;
    private float elapsedTime = 0;
    private Stopwatch stopwatch;
    
    
    public Game(int width, int height) : base(GameWindowSettings.Default, NativeWindowSettings.Default)
    {
        CenterWindow(new Vector2i(width, height));
        this._width = width;
        this._height = height;
    }
    
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        
        _orthographicProjection = Matrix4.CreateOrthographicOffCenter(0, e.Width, e.Height, 0, -1, 1);
        
        try
        {
            _mainCamera.UpdateProjectionMatrix(e.Width, e.Height);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    
    protected override void OnLoad()
    {
        base.OnLoad();

        stopwatch = new Stopwatch();
        stopwatch.Start();
        
        _visibleCursorSwitch = new KeyboardSwitch(InputManager.IsKeyPressed);
        
        // World setup
        _chunkManager = new ChunkManager();
        
        UVmaps grassUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });
        UVmaps dirtUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });
        UVmaps stoneUvs = new UVmaps( new int[] { 0, 0, 1, 0, 2, 0 });

        CWorldBlock grass = new CWorldBlock("grass_block", 0, 1, grassUvs);
        CWorldBlock dirt = new CWorldBlock("dirt_block", 1, 2, dirtUvs);
        CWorldBlock stone = new CWorldBlock("stone_block", 2, 3, stoneUvs);

        BlockManager.Add(grass);
        BlockManager.Add(dirt);
        BlockManager.Add(stone);

        for (int i = 0; i < 3; i++)
        {
            _chunkManager.GenerateChunk(new Vector3i(i * Chunk.WIDTH, 0, 0));
        }

        _shaderProgram = new ShaderProgram("World/Default.vert", "World/Default.frag");
        _textureArray = new TextureArray("Test_TextureAtlas.png", 32, 32);
        
        GL.Enable(EnableCap.DepthTest);
        
        _mainCamera = new Camera(_width, _height, new Vector3(0, 0, 0));
        
        
        // Ui setup
        _uiShader = new ShaderProgram("UI/UI.vert", "UI/UI.frag");
        
        _uItexture = new Texture("UI_Element_1.png");
        _uiMeshData = UI.Generate9Slice(new Vector3(50f, 50f, 0f), _uItexture.Width, _uItexture.Height, 150f, 150f, 10f, new Vector4(10f, 10f, 10f, 10f));
        
        _uIvao = new VAO();
        
        _uIvbo = new VBO(_uiMeshData.verts);
        _uItextureVbo = new VBO(_uiMeshData.uvs);
        
        _uIvao.LinkToVAO(0, 3, _uIvbo);
        _uIvao.LinkToVAO(1, 2, _uItextureVbo);
        
        _uIibo = new IBO(_uiMeshData.tris);
        
        // Text setup
        _textShader = new ShaderProgram("Text/Text.vert", "Text/Text.frag");

        _textTexture = new Texture("text.png");
        _textMeshData = new MeshData();
        
        UI.GenerateCharacter(new Vector3(55f, 55f, 0.001f), 1, Character.C, _textMeshData);

        _textVao = new VAO();

        _textVbo = new VBO(_textMeshData.verts);
        _textTextureVbo = new VBO(_textMeshData.uvs);

        _textVao.LinkToVAO(0, 3, _textVbo);
        _textVao.LinkToVAO(1, 2, _textTextureVbo);

        _textIbo = new IBO(_textMeshData.tris);

    }
    
    protected override void OnUnload()
    {
        base.OnUnload();
        
        _chunkManager.Delete();
            
        _shaderProgram.Delete();
        _uiShader.Delete();
        
        _textureArray.Delete();
    }
    
    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(0.6f, 0.3f, 1f, 1f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        // World
        _shaderProgram.Bind();
        _textureArray.Bind();

        Matrix4 model = Matrix4.Identity;
        Matrix4 view = _mainCamera.GetViewMatrix();
        Matrix4 projection = _mainCamera.GetProjectionMatrix();

        int modelLocation = GL.GetUniformLocation(_shaderProgram.ID, "model");
        int viewLocation = GL.GetUniformLocation(_shaderProgram.ID, "view");
        int projectionLocation = GL.GetUniformLocation(_shaderProgram.ID, "projection");
        
        GL.UniformMatrix4(modelLocation, true, ref model);
        GL.UniformMatrix4(viewLocation, true, ref view);
        GL.UniformMatrix4(projectionLocation, true, ref projection);
        
        _chunkManager.RenderChunks();

        // UI
        UpdateUi();
        
        // Text
        UpdateText();
        
        Context.SwapBuffers();
        
        base.OnRenderFrame(args);
    }
    
    private void UpdateUi()
    {
        _uiShader.Bind();
        _uItexture.Bind();
        
        int projectionLoc = GL.GetUniformLocation(_uiShader.ID, "projection");
        GL.UniformMatrix4(projectionLoc, true, ref _orthographicProjection);
        
        _uIvao.Bind();
        _uIibo.Bind();

        _uItextureVbo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, _uiMeshData.tris.Count, DrawElementsType.UnsignedInt, 0);
        
        _uIvao.Unbind();
        _uIibo.Unbind();

        _uItextureVbo.Unbind();
    }

    private void UpdateText()
    {
        //FpsCalculation();

        _updateText?.Invoke();
        _updateText = null;

        _textShader.Bind();
        _textTexture.Bind();

        int textProjectionLoc = GL.GetUniformLocation(_textShader.ID, "projection");
        GL.UniformMatrix4(textProjectionLoc, true, ref _orthographicProjection);

        _textVao.Bind();
        _textIbo.Bind();

        _textTextureVbo.Bind();

        GL.DrawElements(PrimitiveType.Triangles, _textMeshData.tris.Count, DrawElementsType.UnsignedInt, 0);

        _textVao.Unbind();
        _textIbo.Unbind();

        _textTextureVbo.Unbind();

    }
    
    private void FpsCalculation()
    {
        frameCount++;
        elapsedTime += stopwatch.ElapsedMilliseconds / 100.0f;
        stopwatch.Restart();
        
        if (elapsedTime >= 1.0f)
        {
            int fps = Mathf.FloorToInt(frameCount / elapsedTime);
            frameCount = 0;
            elapsedTime -= 1f;
            
            _textMeshData.Clear();

            int[] fpsArray = IntToArray(fps);
            
            UI.GenerateCharacters(new Vector3(55f, 55f, 0.001f), 1, fpsArray, _textMeshData);

            _textVbo.Bind();
            _textVbo.Update(_textMeshData.verts);
            
            _textTextureVbo.Bind();
            _textTextureVbo.Update(_textMeshData.uvs);
            
            _textIbo.Bind();
            _textIbo.Update(_textMeshData.tris);
        }
    }

    private int[] IntToArray(int value)
    {
        int digitCount = (int)Math.Floor(Math.Log10(value) + 1);
        int[] digits;
        
        try
        {
            digits = new int[digitCount];
        }
        catch (OverflowException)
        {
            Console.WriteLine("Overflow");
            return [0];
        }
        
        for (int i = digitCount - 1; i >= 0; i--)
        {
            digits[i] = value % 10;
            value /= 10; 
        }

        return digits;
    }

    private void RemoveLastCharacter()
    {
        UI.RemoveLastQuad(_textMeshData);
        
        _textVbo.Bind();
        _textVbo.Update(_textMeshData.verts);
            
        _textTextureVbo.Bind();
        _textTextureVbo.Update(_textMeshData.uvs);
            
        _textIbo.Bind();
        _textIbo.Update(_textMeshData.tris);

        _textVbo.Unbind();
        _textTextureVbo.Unbind();
        _textIbo.Unbind();
    }
    
    private void AddCharacter()
    {
        UI.GenerateCharacterAtLastPosition(1, _currentCharacter, _textMeshData);
        
        _textVao = new VAO();

        _textVbo = new VBO(_textMeshData.verts);
        _textTextureVbo = new VBO(_textMeshData.uvs);

        _textVao.LinkToVAO(0, 3, _textVbo);
        _textVao.LinkToVAO(1, 2, _textTextureVbo);

        _textIbo = new IBO(_textMeshData.tris);
    }
    
    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        MouseState mouse = MouseState;
        KeyboardState keyboard = KeyboardState;
        
        base.OnUpdateFrame(args);

        if (keyboard.IsAnyKeyDown && !_isTyping)
        {
            _currentCharacter = InputManager.GetPressedKey(keyboard);
            
            if (_currentCharacter == Character.Backspace)
            {
                _updateText = RemoveLastCharacter;
            }
            else if (_currentCharacter != Character.None)
            {
                _updateText = AddCharacter;
            }
            
            _isTyping = true;
        }
        else if (!keyboard.IsAnyKeyDown && _isTyping)
        {
            _isTyping = false;
        }
        
        _mainCamera.Update(keyboard, mouse, args);
        
        if (_visibleCursorSwitch.CanSwitch(keyboard, Keys.Escape))
        {
            _visibleCursor = !_visibleCursor;
            CursorState = !_visibleCursor ? CursorState.Grabbed : CursorState.Normal;
        }
    }
}