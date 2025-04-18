using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class WorldShader
{   
    public static int Collums = 2;
    public static int Rows = 2;

    public static ShaderProgram PullingShader = new ShaderProgram("World/Pulling.vert", "World/Pulling.frag");
    public static TextureArray Textures = new TextureArray("Test_TextureAtlas.png", Collums, Rows, TextureArrayLoadType.AtlasSizeFlipped);  

    private int _modelLocation = -1;
    private int _viewLocation = -1;
    private int _projectionLocation = -1;
    private int _doAmbientOcclusionLocation = -1;
    private int _doShadowLocation = -1;
    private int _lightViewLocation = -1;
    private int _lightProjectionLocation = -1;
    private int _textureArrayLocation = -1;
    private int _shadowMapLocation = -1;

    public WorldShader()
    {
        PullingShader.Bind();

        _modelLocation = GL.GetUniformLocation(PullingShader.ID, "model");
        _viewLocation = GL.GetUniformLocation(PullingShader.ID, "view");
        _projectionLocation = GL.GetUniformLocation(PullingShader.ID, "projection");
        _doAmbientOcclusionLocation = GL.GetUniformLocation(PullingShader.ID, "doAmbientOcclusion");
        _doShadowLocation = GL.GetUniformLocation(PullingShader.ID, "doShadow");
        _lightViewLocation = GL.GetUniformLocation(PullingShader.ID, "lightView");
        _lightProjectionLocation = GL.GetUniformLocation(PullingShader.ID, "lightProjection");
        _textureArrayLocation = GL.GetUniformLocation(PullingShader.ID, "textureArray");
        _shadowMapLocation = GL.GetUniformLocation(PullingShader.ID, "shadowMap");
        
        PullingShader.Unbind();
    }

    public void Bind()
    {
        PullingShader.Bind();
        Textures.Bind();
    }

    public void UniformGeneral()
    {
        Camera camera = Game.Camera;

        Matrix4 view = camera.ViewMatrix;
        Matrix4 projection = camera.ProjectionMatrix;
        Matrix4 lightView = Matrix4.Identity;
        Matrix4 lightProjection = Matrix4.Identity;

        GL.UniformMatrix4(_viewLocation, true, ref view);
        GL.UniformMatrix4(_projectionLocation, true, ref projection);
        GL.Uniform1(_doAmbientOcclusionLocation, WorldManager.DoAmbientOcclusion ? 1 : 0);
        GL.Uniform1(_doShadowLocation, WorldManager.DoRealtimeShadows ? 1 : 0);
        GL.UniformMatrix4(_lightViewLocation, true, ref lightView);
        GL.UniformMatrix4(_lightProjectionLocation, true, ref lightProjection);
        GL.Uniform1(_textureArrayLocation, 0);
        GL.Uniform1(_shadowMapLocation, 1); 
    }

    public void UniformModel(ref Matrix4 model)
    {
        GL.UniformMatrix4(_modelLocation, true, ref model);
    }

    public void Unbind()
    {
        Textures.Unbind();
        PullingShader.Unbind();
    }
}