using OpenTK.Mathematics;

public static class MeshDataHelper
{
    public static bool MoveQuad(MeshData meshData, int index, Vector3 offset)
    {
        return meshData.MoveQuad(index, offset);
    }
    
    public static bool MoveQuads(MeshData meshData, int index, int size, Vector3 offset)
    {
        for (int i = 0; i < size; i++)
        {
            if (!MoveQuad(meshData, index + i, offset))
                return false;
        }

        return true;
    }
}