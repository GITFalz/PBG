using OpenTK.Mathematics;

public class ModelNode(ModelGridCell gridCell)
{
    public OldModelQuad[] Quads = new OldModelQuad[6];
    public Vector3[] Offsets = new Vector3[8];
    
    public ModelGridCell GridCell = gridCell;

    public static Symmetry Symmetry => ModelingEditor.symmetry;

    public void MoveVert(int index, Vector3 offset)
    {
        foreach (var multiplier in SymmetryOffsets[0][Symmetry])
        {
            Vector3 vector = multiplier.vector;
            
            int vertIndex = VertexIndex[index][multiplier.w];
            Offsets[vertIndex] += offset * vector;

            ModelNode? A = GridCell.GetModelNode(SideNodeIndexes[vertIndex][0]);
            ModelNode? B = GridCell.GetModelNode(SideNodeIndexes[vertIndex][1]);
            ModelNode? C = GridCell.GetModelNode(SideNodeIndexes[vertIndex][2]);
            ModelNode? D = GridCell.GetModelNode(SideNodeIndexes[vertIndex][3]);
            ModelNode? E = GridCell.GetModelNode(SideNodeIndexes[vertIndex][4]);
            ModelNode? F = GridCell.GetModelNode(SideNodeIndexes[vertIndex][5]);
            ModelNode? G = GridCell.GetModelNode(SideNodeIndexes[vertIndex][6]);
            
            int AIndex = SideNodeVertIndexes[vertIndex][0];
            int BIndex = SideNodeVertIndexes[vertIndex][1];
            int CIndex = SideNodeVertIndexes[vertIndex][2];
            int DIndex = SideNodeVertIndexes[vertIndex][3];
            int EIndex = SideNodeVertIndexes[vertIndex][4];
            int FIndex = SideNodeVertIndexes[vertIndex][5];
            int GIndex = SideNodeVertIndexes[vertIndex][6];

            Vector3 vertPosition = GetVertPosition(vertIndex);

            A?.SetVertPosition(AIndex, vertPosition);
            B?.SetVertPosition(BIndex, vertPosition);
            C?.SetVertPosition(CIndex, vertPosition);
            D?.SetVertPosition(DIndex, vertPosition);
            E?.SetVertPosition(EIndex, vertPosition);
            F?.SetVertPosition(FIndex, vertPosition);
            G?.SetVertPosition(GIndex, vertPosition);
        }
    }

    public Vector3 GetVertPosition(int index)
    {
        return ModelData.Corners[index] + Offsets[index];
    }
    
    public void SetVertPosition(int index, Vector3 position)
    {
        Offsets[index] = position - ModelData.Corners[index];
    }

    public void CreateNewHexahedron(Vector3 offset)
    {
        offset.Z = -offset.Z;
        
        Quads[0] = new OldModelQuad(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0));
        Quads[1] = new OldModelQuad(new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1));
        Quads[2] = new OldModelQuad(new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0));
        Quads[3] = new OldModelQuad(new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        Quads[4] = new OldModelQuad(new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 0));
        Quads[5] = new OldModelQuad(new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1));
        
        for (int i = 0; i < 8; i++) 
            Offsets[i] = offset;
    }

    private int[][] VertexIndex =
    [
        [0, 1, 2, 3, 4, 5, 6, 7],
        [1, 0, 3, 2, 5, 4, 7, 6],
        [2, 3, 0, 1, 6, 7, 4, 5],
        [3, 2, 1, 0, 7, 6, 5, 4],
        [4, 5, 6, 7, 0, 1, 2, 3],
        [5, 4, 7, 6, 1, 0, 3, 2],
        [6, 7, 4, 5, 2, 3, 0, 1],
        [7, 6, 5, 4, 3, 2, 1, 0],
    ];
    
    private int[][] SideNodeIndexes =
    [
        [0, 1, 3, 4, 9, 10, 12],
        [1, 2, 4, 5, 10, 11, 13],
        [10, 11, 13, 18, 19, 21, 22],
        [9, 10, 12, 17, 18, 20, 21],
        [3, 4, 6, 7, 12, 14, 15],
        [4, 5, 7, 8, 13, 15, 16],
        [13, 15, 16, 21, 22, 24, 25],
        [12, 14, 15, 20, 21, 23, 24],
    ];
    
    private int[][] SideNodeVertIndexes =
    [
        [6, 7, 2, 3, 5, 4, 1],
        [6, 7, 2,3, 5, 4, 0],
        [6, 7, 3, 5, 4, 0, 1],
        [6, 7, 2, 5, 4, 0, 1],
        [6, 7, 2, 3, 5, 1, 0],
        [6, 7, 2, 3, 4, 0, 1],
        [7, 2, 3, 5, 4, 1, 0],
        [6, 2, 3, 5, 4, 1, 0],
    ];

    private Dictionary<Symmetry, Vector3_1i[]>[] SymmetryOffsets =
    [
        new Dictionary<Symmetry, Vector3_1i[]>()
        {
            { Symmetry.None, 
                [
                    ((1, 1, 1), 0)
                ] 
            },
            { Symmetry.X,
                [
                    ((1, 1, 1), 0), 
                    ((-1, 1, 1), 1)
                ] 
            },
            { Symmetry.Y, 
                [
                    ((1, 1, 1), 0), 
                    ((1, -1, 1), 3)
                ] 
            },
            { Symmetry.Z, 
                [
                    ((1, 1, 1), 0), 
                    ((1, 1, -1), 4)
                ] 
            },
            { Symmetry.XY, 
                [
                    ((1, 1, 1), 0), 
                    ((-1, 1, 1), 1), 
                    ((-1, -1, 1), 2), 
                    ((1, -1, 1), 3)
                ] 
            },
            { Symmetry.XZ, 
                [
                    ((1, 1, 1), 0), 
                    ((-1, 1, 1), 1), 
                    ((1, 1, -1), 4), 
                    ((-1, 1, -1), 5)
                ] 
            },
            { Symmetry.YZ, 
                [
                    ((1, 1, 1), 0), 
                    ((1, -1, 1), 3), 
                    ((1, 1, -1), 4), 
                    ((1, -1, -1), 7)
                ] 
            },
            { Symmetry.XYZ, 
                [
                    ((1, 1, 1), 0), 
                    ((-1, 1, 1), 1), 
                    ((-1, -1, 1), 2), 
                    ((1, -1, 1), 3),
                    ((1, 1, -1), 4), 
                    ((-1, 1, -1), 5), 
                    ((-1, -1, -1), 6), 
                    ((1, -1, -1), 7)
                ] 
            },
        },
    ];
    
    public ModelNode Copy()
    {
        ModelNode node = new ModelNode(GridCell);
        GridCell.MainNode = node;
        for (int i = 0; i < 6; i++) node.Quads[i] = Quads[i].Copy();
        for (int i = 0; i < 8; i++) node.Offsets[i] = Offsets[i];
        return node;
    }
}

public struct OldModelQuad
{
    public Vector3[] Vertices = new Vector3[4];
    
    public OldModelQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vertices[0] = a;
        Vertices[1] = b;
        Vertices[2] = c;
        Vertices[3] = d;
    }
    
    public OldModelQuad Copy()
    {
        OldModelQuad quad = new OldModelQuad(Vertices[0], Vertices[1], Vertices[2], Vertices[3]);
        return quad;
    }
}

public enum Symmetry
{
    None,
    X,
    Y,
    Z,
    XY,
    XZ,
    YZ,
    XYZ
}

public enum SymmetryMode
{
    None,
    Object,
    World,
}