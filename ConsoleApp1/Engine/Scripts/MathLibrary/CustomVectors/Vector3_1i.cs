using OpenTK.Mathematics;

public struct Vector3_1i(Vector3i vector, int w)
{
    public Vector3i vector = vector;
    public int w = w;
    
    public static implicit operator Vector3_1i((Vector3i vector, int w) tuple)
    {
        return new Vector3_1i(tuple.vector, tuple.w);
    }
}