using OpenTK.Mathematics;

namespace ConsoleApp1.Engine.Scripts.Core.Voxel;

public static class VoxelData
{
    public static readonly List<List<Vector3>> voxelVerts = new List<List<Vector3>>()
    {
        new List<Vector3>()
        {
            //Font face
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0),
        },

        new List<Vector3>()
        {
            //Right face
            new Vector3(1, 0, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 1, 1),
            new Vector3(1, 0, 1),
        },

        new List<Vector3>()
        {
            //Top face
            new Vector3(0, 1, 0),
            new Vector3(0, 1, 1),
            new Vector3(1, 1, 1),
            new Vector3(1, 1, 0),
        },

        new List<Vector3>()
        {
            //Left face
            new Vector3(0, 0, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 1, 0),
            new Vector3(0, 0, 0),
        },

        new List<Vector3>()
        {
            //Bottom face
            new Vector3(0, 0, 1),
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(1, 0, 1),
        },

        new List<Vector3>()
        {
            //Back face
            new Vector3(1, 0, 1),
            new Vector3(1, 1, 1),
            new Vector3(0, 1, 1),
            new Vector3(0, 0, 1),
        }
    };
}