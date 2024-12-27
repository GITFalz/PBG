using OpenTK.Mathematics;

public class ModelNode(ModelGridCell gridCell)
{
    public ModelQuad[] Quads = new ModelQuad[6];
    public Vector3[] Offsets = new Vector3[8];
    
    public ModelGridCell GridCell = gridCell;

    public Symmetry Symmetry => AnimationEditor.symmetry;

    public void MoveVert(int index, Vector3 offset)
    {
        foreach (var multiplier in SymmetryOffsets[0][Symmetry])
        {
            int vertIndex = VertexIndex[index][multiplier.w];
            Offsets[vertIndex] += offset * multiplier.vector;

            ModelNode? A = GridCell.GetModelNode(SideNodeIndexes[vertIndex][0]);
            ModelNode? B = GridCell.GetModelNode(SideNodeIndexes[vertIndex][1]);
            ModelNode? C = GridCell.GetModelNode(SideNodeIndexes[vertIndex][2]);
            
            int AIndex = SideNodeVertIndexes[vertIndex][0];
            int BIndex = SideNodeVertIndexes[vertIndex][1];
            int CIndex = SideNodeVertIndexes[vertIndex][2];
            
            if (A != null)
                A.Offsets[AIndex] += offset * multiplier.vector;
            
            if (B != null)
                B.Offsets[BIndex] += offset * multiplier.vector;
            
            if (C != null)
                C.Offsets[CIndex] += offset * multiplier.vector;
        }
    }

    public void CreateNewHexahedron(Vector3 offset)
    {
        Quads[0] = new ModelQuad(new Vector3(0, 0, 0), new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1, 0, 0));
        Quads[1] = new ModelQuad(new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(1, 1, 1), new Vector3(1, 0, 1));
        Quads[2] = new ModelQuad(new Vector3(0, 1, 0), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, 0));
        Quads[3] = new ModelQuad(new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 1, 0), new Vector3(0, 0, 0));
        Quads[4] = new ModelQuad(new Vector3(1, 0, 0), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(0, 0, 0));
        Quads[5] = new ModelQuad(new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 1));
        
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
        [5, 1, 4],
        [5, 1, 2],
        [5, 3, 2],
        [0, 3, 4],
        [0, 1, 4],
        [0, 1, 2],
        [0, 3, 2],
    ];
    
    private int[][] SideNodeVertIndexes =
    [
        [6, 7, 2],
        [5, 0, 2],
        [6, 3, 1],
        [7, 2, 0],
        [0, 5, 7],
        [1, 4, 6],
        [2, 7, 5],
        [3, 6, 4],
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

public struct ModelQuad
{
    public Vector3[] Vertices = new Vector3[4];
    
    public ModelQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vertices[0] = a;
        Vertices[1] = b;
        Vertices[2] = c;
        Vertices[3] = d;
    }
    
    public ModelQuad Copy()
    {
        ModelQuad quad = new ModelQuad(Vertices[0], Vertices[1], Vertices[2], Vertices[3]);
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