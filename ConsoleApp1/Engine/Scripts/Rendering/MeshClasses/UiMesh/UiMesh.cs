using OpenTK.Mathematics;

public class UiMesh : Mesh
{
    public List<int> TextUvs;
    public VBO _textUvVbo;
    
    public int elementCount = 0;
    
    public UiMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextUvs = new List<int>();
        
        transformedVertices = new List<Vector3>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextUvs);
        
        base.GenerateBuffers();
        
        _vao.LinkToVAO(2, 1, _textUvVbo);
    }
    
    public void AddUiElement(Panel panel)
    {
        for (int i = 0; i < 36; i++)
        {
            Vertices.Add(panel.Vertices[i]);
            Uvs.Add(panel.Uvs[i]);
            TextUvs.Add(panel.TextUvs[i]);
            
            //Console.WriteLine("Vertex : " + panel.Vertices[i] + " Uv : " + panel.Uvs[i] + " TextUv : " + panel.TextUvs[i]);
        }
        
        foreach (var t in Vertices)
        {
            transformedVertices.Add(t + Position);
        }

        for (int i = 0; i < 9; i++)
        {
            int index = i * 4;
            int offset = elementCount * 36;
            
            int indexOffset = index + offset;
            
            Indices.Add((uint) (indexOffset));
            Indices.Add((uint) (indexOffset + 1));
            Indices.Add((uint) (indexOffset + 2));
            Indices.Add((uint) (indexOffset + 2));
            Indices.Add((uint) (indexOffset + 3));
            Indices.Add((uint) (indexOffset));
            
            //Console.WriteLine("Index : " + Indices[offset + i * 6] + " " + Indices[offset + i * 6 + 1] + " " + Indices[offset + i * 6 + 2] + " " + Indices[offset + i * 6 + 3] + " " + Indices[offset + i * 6 + 4] + " " + Indices[offset + i * 6 + 5]);
        }
        
        elementCount++;
    }

    public override void RenderMesh()
    {
        //Console.WriteLine("Vertices : " + transformedVertices.Count + " Indices : " + Indices.Count + " Uvs : " + Uvs.Count);
        base.RenderMesh();
    }

    public void MoveUiElement(int index, Vector3 position)
    {
        index *= 36;
        
        for (int i = 0; i < 36; i++)
        {
            transformedVertices[index + i] = Vertices[index + i] + position;
        }
        
        UpdateMesh();
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}