using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class LodChunk : LodTreeBase
{
    public Vector3 position = Vector3.Zero;

    private VAO _vao;
    private SSBO<Vector2i> _vertexSSBO;
    private List<Vector2i> _vertices = [];

    public LodChunk()
    {
        AddFace(position, 16, 16, 0, 2, 0);
        _vao.Renew();
        _vertexSSBO.Renew(_vertices);
    }

    public void AddFace(byte posX, byte posY, byte posZ, byte width, byte height, int blockIndex, byte side, byte mutliplier)
    {
        Vector3i size = ChunkData.FaceVertices[side];
        int vertex = posX | (posY << 5) | (posZ << 10) | (width << 15) | (height << 20);
        int blockData = blockIndex | (side << 16) | (size.X << 19) | (size.Y << 21) | (size.Z << 23) | (mutliplier << 25);

        _vertices.Add(new Vector2i(vertex, blockData));
    }

    public void AddFace(Vector3 position, byte width, byte height, int blockIndex, byte side, byte mutliplier)
    {
        AddFace((byte)position.X, (byte)position.Y, (byte)position.Z, width, height, blockIndex, side, mutliplier);
    }

    public override void Render()
    {
        _vao.Bind();
        _vertexSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Count * 6);
        
        _vertexSSBO.Unbind();
        _vao.Unbind();
    }
}