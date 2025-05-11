using OpenTK.Mathematics;

public class CWorldCurveNode : CWorldParameterNode
{
    public float Min = 0;
    public float Max = 1;
    public Vector2[] Spline = [];

    public CWorldGetterNode InputNode = new CWorldEmptyNode("Empty");  

    public CWorldCurveNode() : base()
    {

    }

    public override void Init(Vector2 position)
    {
        InputNode.Init(position);
        CachedValue = Mathf.Lerp(Min, Max, GetSplineVector(Spline.ToArray(), InputNode.CachedValue));
    }

    public override CWorldNode Copy()
    {
        return new CWorldCurveNode()
        {
            Name = Name,
            Min = Min,
            Max = Max,
            Spline = Spline.ToArray(),
        };
    }

    public override void Copy(CWorldNode copiedNode, Dictionary<string, CWorldNode> copiedNodes, Dictionary<CWorldNode, string> nodeNameMap)
    {
        if (InputNode.IsEmpty())
            return;

        string inputName = nodeNameMap[InputNode];
        ((CWorldCurveNode)copiedNode).InputNode = (CWorldGetterNode)copiedNodes[inputName];
        ((CWorldCurveNode)copiedNode).Spline = Spline.ToArray();
    }

    public static float GetSplineVector(Vector2[] spline, float noise)
    {
        if (spline.Length == 0)
            return 0;
    
        // Handle noise below the first spline point
        if (noise <= spline[0].X)
            return spline[0].Y;
    
        // Iterate through the spline segments
        for (int i = 0; i < spline.Length - 1; i++)
        {
            if (noise >= spline[i].X && noise <= spline[i + 1].X)
            {
                // Calculate t as the normalized position between spline[i].X and spline[i + 1].X
                float x = spline[i + 1].X - spline[i].X; x = x == 0 ? 1 : x;
                float t = (noise - spline[i].X) / x;
                return Mathf.Lerp(spline[i].Y, spline[i + 1].Y, t);
            }
        }

        // Handle noise above the last spline point
        return spline[^1].Y;
    }
}