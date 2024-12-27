using OpenTK.Mathematics;

public class ModelGrid
{
    public static ModelGrid Instance = new ModelGrid();
        
    public Dictionary<Vector3i, ModelNode> Grid = new Dictionary<Vector3i, ModelNode>();
    
    public int AddModelNode(ModelNode node)
    {
        Vector3i position = node.GridCell.Position;
        
        if (!Grid.TryAdd(position, node))
            return -1;

        Vector3i front = position + new Vector3i(0, 0, 1);
        Vector3i right = position + new Vector3i(1, 0, 0);
        Vector3i top = position + new Vector3i(0, 1, 0);
        Vector3i left = position + new Vector3i(-1, 0, 0);
        Vector3i bottom = position + new Vector3i(0, -1, 0);
        Vector3i back = position + new Vector3i(0, 0, -1);
        
        Vector3i bfl = new Vector3i(left.X, bottom.Y, front.Z); // 0 : bottom front left
        Vector3i bfc = new Vector3i(position.X, bottom.Y, front.Z); // 1 : bottom front center
        Vector3i bfr = new Vector3i(right.X, bottom.Y, front.Z); // 2 : bottom front right
        
        Vector3i bml = new Vector3i(left.X, bottom.Y, position.Z); // 3 : bottom middle left
        Vector3i bmc = new Vector3i(position.X, bottom.Y, position.Z); // 4 : bottom middle center
        Vector3i bmr = new Vector3i(right.X, bottom.Y, position.Z); // 5 : bottom middle right
        
        Vector3i bbl = new Vector3i(left.X, bottom.Y, back.Z); // 6 : bottom back left
        Vector3i bbc = new Vector3i(position.X, bottom.Y, back.Z); // 7 : bottom back center
        Vector3i bbr = new Vector3i(right.X, bottom.Y, back.Z); // 8 : bottom back right
        
        Vector3i mfl = new Vector3i(left.X, position.Y, front.Z); // 9 : middle front left
        Vector3i mfc = new Vector3i(position.X, position.Y, front.Z); // 10 : middle front center
        Vector3i mfr = new Vector3i(right.X, position.Y, front.Z); // 11 : middle front right
        
        Vector3i mml = new Vector3i(left.X, position.Y, position.Z); // 12 : middle middle left
        Vector3i mmr = new Vector3i(right.X, position.Y, position.Z); // 13 : middle middle right
        
        Vector3i mbl = new Vector3i(left.X, position.Y, back.Z); // 14 : middle back left
        Vector3i mbc = new Vector3i(position.X, position.Y, back.Z); // 15 : middle back center
        Vector3i mbr = new Vector3i(right.X, position.Y, back.Z); // 16 : middle back right
        
        Vector3i tfl = new Vector3i(left.X, top.Y, front.Z); // 17 : top front left
        Vector3i tfc = new Vector3i(position.X, top.Y, front.Z); // 18 : top front center
        Vector3i tfr = new Vector3i(right.X, top.Y, front.Z); // 19 : top front right
        
        Vector3i tml = new Vector3i(left.X, top.Y, position.Z); // 20 : top middle left
        Vector3i tmc = new Vector3i(position.X, top.Y, position.Z); // 21 : top middle center
        Vector3i tmr = new Vector3i(right.X, top.Y, position.Z); // 22 : top middle right
        
        Vector3i tbl = new Vector3i(left.X, top.Y, back.Z); // 23 : top back left
        Vector3i tbc = new Vector3i(position.X, top.Y, back.Z); // 24 : top back center
        Vector3i tbr = new Vector3i(right.X, top.Y, back.Z); // 25 : top back right
        
        if (Grid.TryGetValue(bfl, out ModelNode? bflNode))
        {
            node.GridCell.ModelNodes[0] = bflNode;
            bflNode.GridCell.ModelNodes[25] = node;
        }
        if (Grid.TryGetValue(bfc, out ModelNode? bfcNode))
        {
            node.GridCell.ModelNodes[1] = bfcNode;
            bfcNode.GridCell.ModelNodes[24] = node;
        }
        if (Grid.TryGetValue(bfr, out ModelNode? bfrNode))
        {
            node.GridCell.ModelNodes[2] = bfrNode;
            bfrNode.GridCell.ModelNodes[23] = node;
        }
        if (Grid.TryGetValue(bml, out ModelNode? bmlNode))
        {
            node.GridCell.ModelNodes[3] = bmlNode;
            bmlNode.GridCell.ModelNodes[22] = node;
        }
        if (Grid.TryGetValue(bmc, out ModelNode? bmcNode))
        {
            node.GridCell.ModelNodes[4] = bmcNode;
            bmcNode.GridCell.ModelNodes[21] = node;
        }
        if (Grid.TryGetValue(bmr, out ModelNode? bmrNode))
        {
            node.GridCell.ModelNodes[5] = bmrNode;
            bmrNode.GridCell.ModelNodes[20] = node;
        }
        if (Grid.TryGetValue(bbl, out ModelNode? bblNode))
        {
            node.GridCell.ModelNodes[6] = bblNode;
            bblNode.GridCell.ModelNodes[19] = node;
        }
        if (Grid.TryGetValue(bbc, out ModelNode? bbcNode))
        {
            node.GridCell.ModelNodes[7] = bbcNode;
            bbcNode.GridCell.ModelNodes[18] = node;
        }
        if (Grid.TryGetValue(bbr, out ModelNode? bbrNode))
        {
            node.GridCell.ModelNodes[8] = bbrNode;
            bbrNode.GridCell.ModelNodes[17] = node;
        }
        if (Grid.TryGetValue(mfl, out ModelNode? mflNode))
        {
            node.GridCell.ModelNodes[9] = mflNode;
            mflNode.GridCell.ModelNodes[16] = node;
        }
        if (Grid.TryGetValue(mfc, out ModelNode? mfcNode))
        {
            node.GridCell.ModelNodes[10] = mfcNode;
            mfcNode.GridCell.ModelNodes[15] = node;
        }
        if (Grid.TryGetValue(mfr, out ModelNode? mfrNode))
        {
            node.GridCell.ModelNodes[11] = mfrNode;
            mfrNode.GridCell.ModelNodes[14] = node;
        }
        if (Grid.TryGetValue(mml, out ModelNode? mmlNode))
        {
            node.GridCell.ModelNodes[12] = mmlNode;
            mmlNode.GridCell.ModelNodes[13] = node;
        }
        if (Grid.TryGetValue(mmr, out ModelNode? mmrNode))
        {
            node.GridCell.ModelNodes[13] = mmrNode;
            mmrNode.GridCell.ModelNodes[12] = node;
        }
        if (Grid.TryGetValue(mbl, out ModelNode? mblNode))
        {
            node.GridCell.ModelNodes[14] = mblNode;
            mblNode.GridCell.ModelNodes[11] = node;
        }
        if (Grid.TryGetValue(mbc, out ModelNode? mbcNode))
        {
            node.GridCell.ModelNodes[15] = mbcNode;
            mbcNode.GridCell.ModelNodes[10] = node;
        }
        if (Grid.TryGetValue(mbr, out ModelNode? mbrNode))
        {
            node.GridCell.ModelNodes[16] = mbrNode;
            mbrNode.GridCell.ModelNodes[9] = node;
        }
        if (Grid.TryGetValue(tfl, out ModelNode? tflNode))
        {
            node.GridCell.ModelNodes[17] = tflNode;
            tflNode.GridCell.ModelNodes[8] = node;
        }
        if (Grid.TryGetValue(tfc, out ModelNode? tfcNode))
        {
            node.GridCell.ModelNodes[18] = tfcNode;
            tfcNode.GridCell.ModelNodes[7] = node;
        }
        if (Grid.TryGetValue(tfr, out ModelNode? tfrNode))
        {
            node.GridCell.ModelNodes[19] = tfrNode;
            tfrNode.GridCell.ModelNodes[6] = node;
        }
        if (Grid.TryGetValue(tml, out ModelNode? tmlNode))
        {
            node.GridCell.ModelNodes[20] = tmlNode;
            tmlNode.GridCell.ModelNodes[5] = node;
        }
        if (Grid.TryGetValue(tmc, out ModelNode? tmcNode))
        {
            node.GridCell.ModelNodes[21] = tmcNode;
            tmcNode.GridCell.ModelNodes[4] = node;
        }
        if (Grid.TryGetValue(tmr, out ModelNode? tmrNode))
        {
            node.GridCell.ModelNodes[22] = tmrNode;
            tmrNode.GridCell.ModelNodes[3] = node;
        }
        if (Grid.TryGetValue(tbl, out ModelNode? tblNode))
        {
            node.GridCell.ModelNodes[23] = tblNode;
            tblNode.GridCell.ModelNodes[2] = node;
        }
        if (Grid.TryGetValue(tbc, out ModelNode? tbcNode))
        {
            node.GridCell.ModelNodes[24] = tbcNode;
            tbcNode.GridCell.ModelNodes[1] = node;
        }
        if (Grid.TryGetValue(tbr, out ModelNode? tbrNode))
        {
            node.GridCell.ModelNodes[25] = tbrNode;
            tbrNode.GridCell.ModelNodes[0] = node;
        }

        return 0;
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