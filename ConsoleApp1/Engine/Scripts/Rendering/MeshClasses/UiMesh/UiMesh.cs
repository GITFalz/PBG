﻿using OpenTK.Mathematics;

public class UiMesh : Mesh
{
    public List<int> TextUvs;
    public VBO _textUvVbo;

    public List<Vector2> UiSizes;
    public VBO _uiSizeVbo;
    
    public int elementCount = 0;
    
    public UiMesh()
    {
        _vao = new VAO();
        
        Vertices = new List<Vector3>();
        Uvs = new List<Vector2>();
        Indices = new List<uint>();
        TextUvs = new List<int>();
        UiSizes = new List<Vector2>();
        
        transformedVertices = new List<Vector3>();
    }
    
    public override void GenerateBuffers()
    {
        _textUvVbo = new VBO(TextUvs);
        _uiSizeVbo = new VBO(UiSizes);
        
        transformedVertices.Clear();

        int i = 0;
        foreach (var t in Vertices)
        {
            transformedVertices.Add(t + Position + new Vector3(0, 0, 0.01f) * (int)(i / 36));
            i++;
        }
        
        _vertVbo = new VBO(transformedVertices);
        _uvVbo = new VBO(Uvs);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textUvVbo);
        _vao.LinkToVAO(3, 2, _uiSizeVbo);
        
        _ibo = new IBO(Indices);
    }
    
    public void AddPanel(Panel panel)
    {
        for (int i = 0; i < 4; i++)
        {
            Vertices.Add(panel.Vertices[i]);
            Uvs.Add(panel.Uvs[i]);
            TextUvs.Add(panel.TextUvs[i]);
            UiSizes.Add(panel.UiSizes[i]);
        }
        
        foreach (var t in Vertices)
        {
            transformedVertices.Add(t + Position);
        }
        
        int offset = elementCount * 4;
            
        Indices.Add((uint) (offset));
        Indices.Add((uint) (offset + 1));
        Indices.Add((uint) (offset + 2));
        Indices.Add((uint) (offset + 2));
        Indices.Add((uint) (offset + 3));
        Indices.Add((uint) (offset));
        
        elementCount++;
    }
    
    
    
    public override void Clear()
    {
        Vertices.Clear();
        Uvs.Clear();
        TextUvs?.Clear();
        Indices.Clear();
        
        transformedVertices.Clear();
        
        elementCount = 0;
    }
    
    public override void Delete()
    {
        _textUvVbo.Delete();
        
        base.Delete();
    }
}