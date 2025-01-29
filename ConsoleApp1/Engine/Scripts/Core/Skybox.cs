using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class Skybox
{
    private static ShaderProgram _skyboxShader = new ShaderProgram("Sky/Default.vert", "Sky/Default.frag");
    private static SkyboxMesh _skyboxMesh = new SkyboxMesh();

    public static void Render()
    {
        GL.Enable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Lequal);
       
        _skyboxShader.Bind();
        
        Matrix4 model = Matrix4.CreateTranslation(Game.camera.position);
        Matrix4 view = Game.camera.GetViewMatrix();
        Matrix4 projection = Game.camera.GetProjectionMatrix();

        int sml = GL.GetUniformLocation(_skyboxShader.ID, "model");
        int svl = GL.GetUniformLocation(_skyboxShader.ID, "view");
        int spl = GL.GetUniformLocation(_skyboxShader.ID, "projection");
        
        GL.UniformMatrix4(sml, true, ref model);
        GL.UniformMatrix4(svl, true, ref view);
        GL.UniformMatrix4(spl, true, ref projection);
        
        _skyboxMesh.Render();

        _skyboxShader.Unbind();

        GL.DepthMask(true);
        GL.DepthFunc(DepthFunction.Less);
    }
}