using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public static class ChunkLODManager
{
    public static LODChunkGrid[] LODChunks = [];

    public static void Initialize(int x, int y, int z, int startX, int startY, int startZ, int resolution)
    {
        int scale = (int)Mathf.Pow(2, resolution) * 32;
        LODChunks = new LODChunkGrid[x * y * z];
        int index = 0;
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    Vector3i position = new Vector3i(startX + i, startY + j, startZ + k) * scale;
                    LODChunkGrid newGrid = new LODChunkGrid(position, scale, resolution - 1);
                    LODChunks[index] = newGrid;
                    index++;
                }
            }
        }
    }

    public static void CheckChunkResolution(Vector3i position)
    {
        for (int i = 0; i < LODChunks.Length; i++)
        {
            LODChunkGrid chunk = LODChunks[i];
            chunk.UpdateResolution(position);
        }

        for (int i = 0; i < LODChunks.Length; i++)
        {
            LODChunkGrid chunk = LODChunks[i];
            chunk.GenerateChunk();
        }
    }

    public static void RenderChunks()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);
        GL.Enable(EnableCap.CullFace);

        Matrix4 model = Matrix4.Identity; 
        Matrix4 projection = Game.Camera.ProjectionMatrix;
        Matrix4 view = Game.Camera.ViewMatrix;

        WorldManager.newTestShader.Bind();

        int modelLocation = WorldManager.newTestShader.GetLocation("model");
        int viewLocation = WorldManager.newTestShader.GetLocation("view");
        int projectionLocation = WorldManager.newTestShader.GetLocation("projection");
        int textureArrayLocation = WorldManager.newTestShader.GetLocation("textureArray");

        GL.UniformMatrix4(viewLocation, false, ref view);
        GL.UniformMatrix4(projectionLocation, false, ref projection);
        GL.Uniform1(textureArrayLocation, 0);

        //Shader.Error("Setting uniforms: ");

        WorldShader.Textures.Bind(TextureUnit.Texture0);

        GL.DepthMask(true);
        GL.Disable(EnableCap.Blend);

        foreach (var chunk in LODChunks)
        {
            chunk.RenderChunk(modelLocation); 
            //Shader.Error("Rendering chunk at position: " + chunk.Position);
        }

        /*
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.DepthMask(false);

        foreach (var (key, chunk) in ChunkManager.TransparentChunks)
        {   
            model = Matrix4.CreateTranslation(key);
            GL.UniformMatrix4(modelLocation, false, ref model);
            chunk.RenderChunkTransparent(); 
        }
        */

        WorldShader.Textures.Unbind();

        WorldManager.newTestShader.Unbind();

        //Shader.Error("After Render End: ");

        model = Matrix4.Identity;
    }

    public static void Clear()
    {
        foreach (var chunk in LODChunks)
        {
            chunk.Clear();
        }
        LODChunks = [];
    }
}