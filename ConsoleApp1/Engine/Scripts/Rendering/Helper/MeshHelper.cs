using OpenTK.Mathematics;

public static class MeshHelper
{
    public static Quad GenerateQuad(float width, float height, int textureId)
    {
        return _quads[FaceDirection.Front](width, height, textureId);
    }
    
    public static Quad GenerateTextQuad(float width, float height, int textureId, int charCount, int charIndex)
    {
        Quad quad = _quads[FaceDirection.Front](width, height, textureId);

        Vector2i[] uvs = new Vector2i[4]
        {
            new Vector2i(charCount, charIndex),
            new Vector2i(charCount, charIndex),
            new Vector2i(charCount, charIndex),
            new Vector2i(charCount, charIndex),
        };
        
        quad.TextureUvs = uvs;
        
        return quad;
    }

    public static void GenerateMeshIndices(Mesh mesh)
    {
        uint count = (uint)mesh.Vertices.Count;
        
        mesh.Indices.Add(0 + count);
        mesh.Indices.Add(1 + count);
        mesh.Indices.Add(2 + count);
        mesh.Indices.Add(2 + count);
        mesh.Indices.Add(3 + count);
        mesh.Indices.Add(0 + count);
    }

    private static Dictionary<FaceDirection, Func<float, float, int, Quad>> _quads = new Dictionary<FaceDirection, Func<float, float, int, Quad>>()
    {
        { FaceDirection.Front, (width, height, t) => new Quad(new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, height, 0), new Vector3(width, height, 0), new Vector3(width, 0, 0) }, new int[] { t, t, t, t }) },
        { FaceDirection.Right, (width, height, t) => new Quad(VoxelData.PositionOffset[1](width, height), new int[] { t, t, t, t }) },
        { FaceDirection.Up,    (width, height, t) => new Quad(VoxelData.PositionOffset[2](width, height), new int[] { t, t, t, t }) },
        { FaceDirection.Left,  (width, height, t) => new Quad(VoxelData.PositionOffset[3](width, height), new int[] { t, t, t, t }) },
        { FaceDirection.Down,  (width, height, t) => new Quad(VoxelData.PositionOffset[4](width, height), new int[] { t, t, t, t }) },
        { FaceDirection.Back,  (width, height, t) => new Quad(VoxelData.PositionOffset[5](width, height), new int[] { t, t, t, t }) }
    };
}

public enum FaceDirection
{
    Front,
    Right,
    Up,
    Left,
    Down,
    Back
}