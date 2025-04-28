using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class RigMesh : Meshes
{
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector3> _vertVbo = new VBO<Vector3>([]);
    private VBO<Vector3> _normalVbo = new VBO<Vector3>([]);

    public List<Vertex> VertexList = new List<Vertex>();
    public List<uint> Indices = new List<uint>();
    public List<Vector3> MeshVertices = new List<Vector3>();
    public List<Vector3> Normals = new List<Vector3>();


    private VAO _vertexVao = new VAO();
    private VBO<Vector3> _vertexVbo = new VBO<Vector3>([]);
    private VBO<Vector3> _colorVbo = new VBO<Vector3>([]);
    private VBO<float> _vertexSizeVbo = new VBO<float>([]);

    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector3> VertexColors = new List<Vector3>();
    public List<float> VertexSizes = new List<float>();


    private VAO _boneVao = new VAO();
    private VBO<Vector3> _boneVbo = new VBO<Vector3>([]);
    private VBO<Vector3> _boneColorVbo = new VBO<Vector3>([]);
    private VBO<float> _boneSizeVbo = new VBO<float>([]);
    public List<Vector3> BoneVertices = new List<Vector3>();
    public List<Vector3> BoneColors = new List<Vector3>();
    public List<float> BoneSizes = new List<float>();

    public Bone RootBone;
    public int BoneCount;

    public RigMesh(Bone rootBone, int boneCount)
    {
        RootBone = rootBone;
        BoneCount = boneCount;
    }

    public RigMesh()
    {
        RootBone = new Bone("Root");
        BoneCount = 1;
    }

    public void Init()
    {
        BoneVertices.Clear();
        BoneColors.Clear();
        BoneSizes.Clear();
        List<Bone> bones = RootBone.GetBones();
        foreach (Bone bone in bones)
        {
            BoneVertices.AddRange(bone.Pivot.Position, bone.End.Position);
            BoneColors.AddRange((0.1f, 0.03f, 0.025f), (0.1f, 0.03f, 0.025f));
            BoneSizes.AddRange(15f, 15f);
        }
    }

    public void GenerateBuffers()
    {
        GenerateIndices();
        
        _vertVbo.Renew(MeshVertices);
        _normalVbo.Renew(Normals);

        _vertexVbo.Renew(Vertices);
        _colorVbo.Renew(VertexColors);
        _vertexSizeVbo.Renew(VertexSizes); 

        _vao.Bind();
        
        _vao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _vertVbo);
        _vao.LinkToVAO(1, 3, VertexAttribPointerType.Float, 0, 0, _normalVbo);

        _vao.Unbind();  

        _vertexVao.Bind();

        _vertexVao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _vertexVbo);
        _vertexVao.LinkToVAO(1, 3, VertexAttribPointerType.Float, 0, 0, _colorVbo);
        _vertexVao.LinkToVAO(2, 1, VertexAttribPointerType.Float, 0, 0, _vertexSizeVbo);

        _vertexVao.Unbind();

        GenerateRigBuffers();
        
        _ibo.Renew(Indices);
    }

    public void GenerateRigBuffers()
    {
        _boneVbo.Renew(BoneVertices);
        _boneColorVbo.Renew(BoneColors);
        _boneSizeVbo.Renew(BoneSizes);

        _boneVao.Bind();

        _boneVao.LinkToVAO(0, 3, VertexAttribPointerType.Float, 0, 0, _boneVbo);
        _boneVao.LinkToVAO(1, 3, VertexAttribPointerType.Float, 0, 0, _boneColorVbo);
        _boneVao.LinkToVAO(2, 1, VertexAttribPointerType.Float, 0, 0, _boneSizeVbo);

        _boneVao.Unbind();
    }

    public void GenerateIndices()
    {
        Indices.Clear();
        for (uint i = 0; i < MeshVertices.Count; i++)
        {
            Indices.Add(i);
        }
    }

    public void UpdateVertexColors()
    {
        _colorVbo.Update(VertexColors);
    }


    public void UpdateBoneColors()
    {
        _boneColorVbo.Update(BoneColors);
    }

    public void UpdateBonePositions()
    {
        BoneVertices.Clear();
        List<Bone> bones = RootBone.GetBones();
        foreach (Bone bone in bones)
        {
            BoneVertices.AddRange(bone.Pivot.Position, bone.End.Position);
        }
        _boneVbo.Update(BoneVertices);
    }

    public override void SaveModel(string modelName)
    {
        SaveModel(modelName, Game.modelPath);
    }

    public override void SaveModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName}.model");
        List<string> oldLines = File.ReadAllLines(path).ToList();
        List<string> newLines = new List<string>();

        int oldVertexCount = int.Parse(oldLines[0]);
        int oldEdgeCount = int.Parse(oldLines[oldVertexCount + 1]);
        int oldTriangleCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + 2]);
        int oldTextureCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldTriangleCount + 3]);
        int oldNormalCount = int.Parse(oldLines[oldVertexCount + oldEdgeCount + oldTriangleCount + oldTextureCount + 4]);
        int rigStart = oldVertexCount + oldEdgeCount + oldTriangleCount + oldTextureCount + oldNormalCount + 5;

        int j = 0;
        for (int i = 0; i < rigStart; i++)
        {
            string line = oldLines[i];
            if (line.StartsWith("v"))
            {
                line = $"v {VertexList[j].X} {VertexList[j].Y} {VertexList[j].Z} {VertexList[j].Index}";
                j++;
            }
            newLines.Add(line);
        }

        List<Bone> bones = RootBone.GetBones();
        newLines.Add(bones.Count.ToString());
        for (int i = 0; i < bones.Count; i++)
        {
            Bone bone = bones[i];
            Vector3 pivot = bone.Pivot.Position;
            Vector3 end = bone.End.Position;
            int index = i == 0 ? -1 : bones.IndexOf(bone.Parent);
            newLines.Add($"b {pivot.X} {pivot.Y} {pivot.Z} {end.X} {end.Y} {end.Z} {index}");
        }

        for (int i = rigStart + int.Parse(oldLines[rigStart]) + 1; i < oldLines.Count; i++)
        {
            newLines.Add(oldLines[i]);
        }

        File.WriteAllLines(path, newLines);
    }

    public override bool LoadModel(string modelName)
    {
        return LoadModel(modelName, Game.modelPath);
    }

    public override bool LoadModel(string modelName, string basePath)
    {
        string path = Path.Combine(basePath, $"{modelName.Trim()}.model");
        if (!File.Exists(path))
        {
            PopUp.AddPopUp("The model does not exist.");
            return false;
        }

        Unload();

        string[] lines = File.ReadAllLines(path);

        int vertexCount = int.Parse(lines[0]);
        int edgeCount = int.Parse(lines[vertexCount + 1]);
        int triangleCount = int.Parse(lines[vertexCount + edgeCount + 2]);
        int uvCount = int.Parse(lines[vertexCount + edgeCount + triangleCount + 3]);
        int normalCount = int.Parse(lines[vertexCount + edgeCount + triangleCount + uvCount + 4]);
        int boneCount = int.Parse(lines[vertexCount + edgeCount + triangleCount + uvCount + normalCount + 5]);

        int edgeTotal = vertexCount + edgeCount;
        int triangleTotal = edgeTotal + triangleCount;
        int uvTotal = triangleTotal + uvCount;
        int normalTotal = uvTotal + normalCount;
        int boneTotal = normalTotal + boneCount;

        for (int i = 1; i <= vertexCount; i++)
        {
            string[] values = lines[i].Split(' ');
            Vector3 position = new Vector3(float.Parse(values[1]), float.Parse(values[2]), float.Parse(values[3]));
            Vertex vertex = new Vertex(position);
            vertex.Index = int.Parse(values[4]);
            VertexList.Add(vertex);
            Vertices.Add(position);
            VertexColors.Add(new Vector3(0f, 0f, 0f));
            VertexSizes.Add(10f);
        }

        for (int i = edgeTotal + 3; i <= triangleTotal + 2; i++)
        {
            string[] values = lines[i].Split(' ');
            MeshVertices.AddRange(VertexList[int.Parse(values[1])], VertexList[int.Parse(values[2])], VertexList[int.Parse(values[3])]);
        }

        for (int i = uvTotal + 5; i <= normalTotal + 4; i++)
        {
            Normals.Add(Parse(lines[i], (1, 2, 3)));
        }

        int j = 0;
        for (int i = normalTotal + 6; i <= boneTotal + 5; i++)
        {
            string line = lines[i];
            Vector3 pivot = Parse(line, (1, 2, 3));
            Vector3 end = Parse(line, (4, 5, 6));
            
            if (j == 0)
            {
                RootBone = new Bone("Root");
                RootBone.Pivot.Position = pivot;
                RootBone.End.Position = end;
            }
            else
            {
                int boneIndex = int.Parse(line.Split(' ')[7]);
                Bone bone = new Bone(RootBone, "ChildBone" + boneIndex);
                bone.Pivot.Position = pivot;
                bone.End.Position = end;
                RootBone.GetBones()[boneIndex].AddChild(bone);
            }
            
            j++;
        }

        Init();
        GenerateBuffers();

        return true;
    }

    public override void Unload()
    {
        VertexList.Clear();
        Indices.Clear();
        Normals.Clear();
        VertexColors.Clear();
        Vertices.Clear();
        MeshVertices.Clear();
        BoneVertices.Clear();
        BoneColors.Clear();
        BoneSizes.Clear();
    }

    public void RenderEdges()
    {
        _boneVao.Bind();

        GL.LineWidth(5.0f);
        GL.DrawArrays(PrimitiveType.Lines, 0, BoneVertices.Count);

        _boneVao.Unbind();
    }

    public void RenderVertices()
    {
        GL.Enable(EnableCap.PointSprite);
        GL.Enable(EnableCap.ProgramPointSize);

        _vertexVao.Bind();

        GL.PointSize(10.0f);
        GL.DrawArrays(PrimitiveType.Points, 0, Vertices.Count);

        _vertexVao.Unbind();

        _boneVao.Bind();

        GL.PointSize(15.0f);
        GL.DrawArrays(PrimitiveType.Points, 0, BoneVertices.Count);

        _boneVao.Unbind();

        GL.Disable(EnableCap.PointSprite);
        GL.Disable(EnableCap.ProgramPointSize);
    }

    public void RenderMesh()
    {
        _vao.Bind();
        _ibo.Bind();
        
        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        
        _vao.Unbind();
        _ibo.Unbind();
    }
}