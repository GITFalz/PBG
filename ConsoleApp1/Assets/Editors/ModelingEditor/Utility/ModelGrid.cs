using OpenTK.Mathematics;

public class ModelGrid
{
    public static ModelGrid Instance = new ModelGrid();
        
    public Dictionary<Vector3i, ModelNode> Grid = new Dictionary<Vector3i, ModelNode>();
    
    public Vector3?[] Offsets = [null, null, null, null, null, null, null, null];
    
    public void AddModelNode(ModelNode node)
    {
        Vector3i position = node.GridCell.Position;

        if (!Grid.TryAdd(position, node))
            return;

        for (int i = 0; i < 26; i++)
        {
            SetGridCells(position, i, node);
        }
        
        for (int i = 0; i < 8; i++)
        {
            Vector3? offset = Offsets[i];
            if (offset != null)
            {
                node.Offsets[i] += (Vector3)offset;
            }
            
            Console.WriteLine(node.Offsets[i]);
        }
    }

    public void RemoveModelNode(ModelNode node)
    {
        Vector3i position = node.GridCell.Position;
        
        if (!Grid.Remove(node.GridCell.Position))
            return;

        for (int i = 0; i < 26; i++)
        {
            RemoveGridCells(position, i);
        }
    }

    public void SetGridCells(Vector3i position, int index, ModelNode node)
    {
        if (index < 0 || index > 25) 
            return;
        
        if (!Grid.TryGetValue(position + ModelData.Positions[index], out ModelNode? Node)) 
            return;
        Console.WriteLine("Index: " + index);
        int[] indexes = ModelData.SideNodeIndexes[index];
        int[] oppositeIndexes = ModelData.OppositeSideNodeIndexes[index];
        for (int i = 0; i < indexes.Length; i++)
        {
            Vector3 nodePos = node.GetVertPosition(oppositeIndexes[i]);
            Console.WriteLine(oppositeIndexes[i]);
            Vector3 NodePos = Node.GetVertPosition(indexes[i]);
            
            Offsets[oppositeIndexes[i]] = NodePos - nodePos;
            
            Console.WriteLine("nodePos: " + nodePos + " NodePos: " + NodePos + " index: " + indexes[i] + " Offset: " + Offsets[oppositeIndexes[i]]);
        }

        node.GridCell.ModelNodes[index] = Node;
        Node.GridCell.ModelNodes[-index + 25] = node;
    }
    
    public void RemoveGridCells(Vector3i position, int index)
    {
        if (index < 0 || index > 25) 
            return;
        
        if (!Grid.TryGetValue(position + ModelData.Positions[index], out ModelNode? Node)) 
            return;
        
        Node.GridCell.ModelNodes[-index + 25] = null;
    }
}

public class ModelGridCell(ModelNode mainNode, Vector3i position)
{
    public ModelNode MainNode = mainNode;
    public Vector3i Position = position;

    public List<ModelNode?> ModelNodes = [null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null];

    public ModelNode? GetModelNode(int index)
    {
        index = Mathf.Clamp(0, 25, index);
        return ModelNodes[index];
    }
}