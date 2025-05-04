using OpenTK.Mathematics;

public static class ChunkData
{
    public static readonly Vector3i[] FaceVertices =
    [
        (0, 1, 2),
        (2, 1, 0),
        (0, 2, 1),
        (2, 1, 0),
        (0, 2, 1),
        (0, 1, 2),
    ];

    public static readonly Dictionary<Vector3i, int> SideChunkIndices = new Dictionary<Vector3i, int>
    {
        { (-1, -1, -1),  0 },
        { (-1, -1,  0),  1 },
        { (-1, -1,  1),  2 },

        { (-1,  0, -1),  3 },
        { (-1,  0,  0),  4 },
        { (-1,  0,  1),  5 },

        { (-1,  1, -1),  6 },
        { (-1,  1,  0),  7 },
        { (-1,  1,  1),  8 },


        { ( 0, -1, -1),  9 },
        { ( 0, -1,  0), 10 },
        { ( 0, -1,  1), 11 },

        { ( 0,  0, -1), 12 },
        { ( 0,  0,  1), 13 },

        { ( 0,  1, -1), 14 },
        { ( 0,  1,  0), 15 },
        { ( 0,  1,  1), 16 },


        { ( 1, -1, -1), 17 },
        { ( 1, -1,  0), 18 },
        { ( 1, -1,  1), 19 },

        { ( 1,  0, -1), 20 },
        { ( 1,  0,  0), 21 },
        { ( 1,  0,  1), 22 },

        { ( 1,  1, -1), 23 },
        { ( 1,  1,  0), 24 },
        { ( 1,  1,  1), 25 },
    };

    public static readonly Vector3i[] SidePositions =
    [
        (-1, -1, -1),
        (-1, -1,  0),
        (-1, -1,  1),

        (-1,  0, -1),
        (-1,  0,  0),
        (-1,  0,  1),

        (-1,  1, -1),
        (-1,  1,  0),
        (-1,  1,  1),


        ( 0, -1, -1),
        ( 0, -1,  0),
        ( 0, -1,  1),

        ( 0,  0, -1),
        ( 0,  0,  1),

        ( 0,  1, -1),
        ( 0,  1,  0),
        ( 0,  1,  1),


        ( 1, -1, -1),
        ( 1, -1,  0),
        ( 1, -1,  1),

        ( 1,  0, -1),
        ( 1,  0,  0),
        ( 1,  0,  1),

        ( 1,  1, -1),
        ( 1,  1,  0),
        ( 1,  1,  1)
    ];
}

public enum ChunkStage
{
    Empty = -1,
    Instance = 0,
    Generated = 1,
    WaitingToBePopulated = 2,
    Populated = 3,
    WaitingToBeRendered = 4,
    Rendered = 5,
    WaitingToBeCreated = 6,
}

public enum ChunkStatus
{
    Empty = -1,
    Active = 0, // chunks that the player is in and nearby
    Inactive = 1, // Chunks that were active but have gone out of range
    Independent = 2, // Chunks that have merged and are not dependent on the player anymore
}