using OpenTK.Mathematics;

public static class ModelData
{
    public static readonly Vector3[] Corners =
    [
        (0, 0, 0),
        (1, 0, 0),
        (1, 1, 0),
        (0, 1, 0),
        (0, 0, 1),
        (1, 0, 1),
        (1, 1, 1),
        (0, 1, 1),
    ];
    
    public static readonly Vector3i[] Positions = new Vector3i[]
    {
        new Vector3i(-1, -1,  1), // 0 : bottom front left
        new Vector3i( 0, -1,  1), // 1 : bottom front center
        new Vector3i( 1, -1,  1), // 2 : bottom front right
        new Vector3i(-1, -1,  0), // 3 : bottom middle left
        new Vector3i( 0, -1,  0), // 4 : bottom middle center
        new Vector3i( 1, -1,  0), // 5 : bottom middle right
        new Vector3i(-1, -1, -1), // 6 : bottom back left
        new Vector3i( 0, -1, -1), // 7 : bottom back center
        new Vector3i( 1, -1, -1), // 8 : bottom back right
        new Vector3i(-1,  0,  1), // 9 : middle front left
        new Vector3i( 0,  0,  1), // 10 : middle front center
        new Vector3i( 1,  0,  1), // 11 : middle front right
        new Vector3i(-1,  0,  0), // 12 : middle middle left
        new Vector3i( 1,  0,  0), // 13 : middle middle right
        new Vector3i(-1,  0, -1), // 14 : middle back left
        new Vector3i( 0,  0, -1), // 15 : middle back center
        new Vector3i( 1,  0, -1), // 16 : middle back right
        new Vector3i(-1,  1,  1), // 17 : top front left
        new Vector3i( 0,  1,  1), // 18 : top front center
        new Vector3i( 1,  1,  1), // 19 : top front right
        new Vector3i(-1,  1,  0), // 20 : top middle left
        new Vector3i( 0,  1,  0), // 21 : top middle center
        new Vector3i( 1,  1,  0), // 22 : top middle right
        new Vector3i(-1,  1, -1), // 23 : top back left
        new Vector3i( 0,  1, -1), // 24 : top back center
        new Vector3i( 1,  1, -1)  // 25 : top back right
    };

    public static readonly int[][] SideNodeIndexes =
    [
        [6],            // 0
        [6, 7],         // 1
        [7],            // 2
        [2, 6],         // 3
        [2, 3, 6, 7],   // 4
        [3, 7],         // 5
        [2],            // 6
        [2, 3],         // 7
        [3],            // 8
        [5, 6],         // 9 
        [4, 5, 6, 7],   // 10   
        [4, 7],         // 11
        [1, 2, 5, 6],   // 12
        [0, 3, 4, 7],   // 13
        [1, 2],         // 14
        [0, 1, 2, 3],   // 15
        [0, 3],         // 16
        [5],            // 17
        [4, 5],         // 18
        [4],            // 19
        [1, 5],         // 20
        [0, 1, 4, 5],   // 21
        [0, 4],         // 22
        [1],            // 23
        [0, 1],         // 24
        [0]             // 25
    ];
    
    public static readonly int[][] OppositeSideNodeIndexes =
    [
        [0],            // 0
        [1, 0],         // 1
        [1],            // 2
        [0, 4],         // 3
        [1, 0, 5, 4],   // 4
        [1, 5],         // 5
        [4],            // 6
        [5, 4],         // 7
        [5],            // 8
        [0, 3],         // 9 
        [0, 1, 2, 3],   // 10   
        [1, 2],         // 11
        [0, 3, 4, 7],   // 12
        [1, 2, 5, 6],   // 13
        [4, 7],         // 14
        [4, 5, 6, 7],   // 15
        [5, 6],         // 16
        [3],            // 17
        [3, 2],         // 18
        [2],            // 19
        [3, 7],         // 20
        [3, 2, 7, 6],   // 21
        [2, 6],         // 22
        [7],            // 23
        [7, 6],         // 24
        [6]             // 25
    ];

    public static readonly int[][][] SymmetryHelper =
    [
        [[1],[3],[4],[1,2,3],[1,4,5],[3,4,7],[1,2,3,4,5,6,7]],
        [[0],[2],[5],[0,2,3],[0,4,5],[2,5,6],[0,2,3,4,5,6,7]],
        [[3],[1],[6],[0,1,3],[3,6,7],[1,5,6],[0,1,3,4,5,6,7]],
        [[2],[0],[7],[0,1,2],[2,6,7],[0,4,7],[0,1,2,4,5,6,7]],
        [[5],[7],[0],[5,6,7],[0,1,5],[0,3,7],[0,1,2,3,5,6,7]],
        [[4],[6],[1],[4,6,7],[0,1,4],[1,2,6],[0,1,2,3,4,6,7]],
        [[7],[5],[2],[4,5,7],[2,3,7],[1,2,5],[0,1,2,3,4,5,7]],
        [[6],[4],[3],[4,5,6],[2,3,6],[0,3,4],[0,1,2,3,4,5,6]]
    ];
}