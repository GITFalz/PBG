using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public static class CWorldMultithreadNodeManager
{
    public static CWorldNodeManager MainNodeManager = new CWorldNodeManager();
    public static List<CWorldNodeManager> NodeManagers = [];

    public static CWorldOutputNode CWorldOutputNode 
    {
        get => MainNodeManager.CWorldOutputNode; 
        set => MainNodeManager.CWorldOutputNode = value; 
    }

    public static void AddNode(CWorldNode node)
    {
        MainNodeManager.AddNode(node);
    }

    public static void RemoveNode(CWorldNode node)
    {
        MainNodeManager.RemoveNode(node);
    }

    public static void Copy(int count)
    {
        Clear();

        for (int i = 0; i < count; i++)
        {
            CWorldNodeManager nodeManager = MainNodeManager.Copy();
            NodeManagers.Add(nodeManager);
        }
    }

    public static bool GetNodeManager(int index, [NotNullWhen(true)] out CWorldNodeManager? nodeManager)
    {
        if (index < 0 || index >= NodeManagers.Count)
        {
            nodeManager = null;
            return false;
        }

        nodeManager = NodeManagers[index];
        return true;
    }

    public static void Clear()
    {
        foreach (var nodeManager in NodeManagers)
            nodeManager.Delete();

        NodeManagers = [];
    }
}

public class CWorldNodeManager
{
    public bool IsBeingUsed = false;

    public List<CWorldNode> CWorldNodes = [];
    public List<CWorldInitNode> CWorldInitNodes = [];
    public CWorldOutputNode CWorldOutputNode = new();

    public Dictionary<string, CWorldNode> GetNodeDictionary()
    {
        Dictionary<string, CWorldNode> cWorldNodeDictionary = [];
        foreach (var node in CWorldNodes)
        {
            while (cWorldNodeDictionary.ContainsKey(node.Name))
                node.Name += "_1";

            cWorldNodeDictionary.Add(node.Name, node);
        }
        return cWorldNodeDictionary;
    }

    public void AddNode(CWorldNode node)
    {
        CWorldNodes.Add(node);
        node.Name = $"Node{CWorldNodes.Count}";
        if (node is CWorldInitNode initNode)
        {
            CWorldInitNodes.Add(initNode);
        }
    }

    public void RemoveNode(CWorldNode node)
    {
        CWorldNodes.Remove(node);
        if (node is CWorldInitNode initNode)
        {
            CWorldInitNodes.Remove(initNode);
        }
    }

    public void Init(Vector2 position)
    {
        foreach (var node in CWorldInitNodes)
        {
            node.Init(position);
        }
    }

    public float GetValue()
    {
        return CWorldOutputNode.GetValue();
    }

    public CWorldNodeManager Copy()
    {
        CWorldNodeManager copy = new();

        Dictionary<CWorldNode, string> nodeNameMap = [];
        Dictionary<string, CWorldNode> copiedNodes = [];

        for (int i = 0; i < CWorldNodes.Count; i++)
        {
            string name = $"Node{i}";
            nodeNameMap.Add(CWorldNodes[i], name);
            copiedNodes.Add(name, CWorldNodes[i].Copy());
        }
        
        copy.CWorldOutputNode = (CWorldOutputNode)CWorldOutputNode.Copy();

        if (CWorldOutputNode.InputNode.IsntEmpty()) 
        {
            Console.WriteLine("Type: " + CWorldOutputNode.InputNode.GetType().Name);
            string outputName = nodeNameMap[CWorldOutputNode.InputNode];
            copy.CWorldOutputNode.InputNode = (CWorldGetterNode)copiedNodes[outputName];
        }

        for (int i = 0; i < CWorldNodes.Count; i++)
        {
            CWorldNode node = CWorldNodes[i];
            string name = nodeNameMap[node];
            CWorldNode copiedNode = copiedNodes[name];

            if (node is CWorldMinMaxNode minMaxNode)
            {
                if (minMaxNode.InputNode.IsEmpty())
                    continue;

                string inputName = nodeNameMap[minMaxNode.InputNode];
                ((CWorldMinMaxNode)copiedNode).InputNode = (CWorldGetterNode)copiedNodes[inputName];
            }
            if (node is CWorldDoubleInputNode doubleInputNode)
            {
                if (doubleInputNode.InputNode1.IsntEmpty())
                {
                    string inputName1 = nodeNameMap[doubleInputNode.InputNode1];
                    ((CWorldDoubleInputNode)copiedNode).InputNode1 = (CWorldGetterNode)copiedNodes[inputName1];
                }
                if (doubleInputNode.InputNode2.IsntEmpty())
                {
                    string inputName2 = nodeNameMap[doubleInputNode.InputNode2];
                    ((CWorldDoubleInputNode)copiedNode).InputNode2 = (CWorldGetterNode)copiedNodes[inputName2];
                }
            }
        }

        foreach (var node in copiedNodes.Values)
        {
            copy.AddNode(node);
        }

        return copy;
    }

    public void Delete()
    {
        CWorldOutputNode.InputNode = new CWorldEmptyNode("Empty");
        foreach (var node in CWorldNodes)
        {
            node.Delete();
        }
        CWorldNodes = [];
        CWorldInitNodes = [];
        CWorldOutputNode.Delete();
    }
}