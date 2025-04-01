using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

public class AnimationMesh : Meshes
{
    public List<Vertex> VertexList = new List<Vertex>();
    public List<Vector3> Vertices = new List<Vector3>();
    public List<Vector2> Uvs = new List<Vector2>();
    public List<uint> Indices = new List<uint>();
    public List<int> TextureIndices = new List<int>();
    public List<Vector3> Normals = new List<Vector3>();
    public List<int> BoneIndices = new List<int>();
    public List<Matrix4> BoneMatrices = new List<Matrix4>();  
    private VAO _vao = new VAO();
    private IBO _ibo = new IBO([]);
    private VBO<Vector3> _vertVbo = new(new List<Vector3>());
    private VBO<Vector2> _uvVbo = new(new List<Vector2>());
    private VBO<int> _textureVbo = new(new List<int>());
    private VBO<Vector3> _normalVbo = new(new List<Vector3>());
    private VBO<int> _boneVbo = new(new List<int>());
    private SSBO<Matrix4> _boneSSBO = new(new List<Matrix4>());

    private VAO _boneVao = new VAO();
    private VBO<Vector3> _boneVertexVbo = new(new List<Vector3>());
    private VBO<Vector3> _boneColorVbo = new(new List<Vector3>());
    private VBO<float> _boneSizeVbo = new(new List<float>());
    public List<Vector3> BoneVertices = new List<Vector3>();
    public List<Vector3> BoneColors = new List<Vector3>();
    public List<float> BoneSizes = new List<float>();

    public Bone RootBone;
    public int BoneCount;

    public AnimationMesh(Bone rootBone, int boneCount)
    {
        RootBone = rootBone;
        BoneCount = boneCount;
    }

    public AnimationMesh()
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
        GenerateBasicBoneMatrices();

        _vertVbo = new(Vertices);
        _uvVbo = new(Uvs);
        _textureVbo = new(TextureIndices);
        _normalVbo = new(Normals);
        _boneVbo = new(BoneIndices);
        _boneSSBO = new(BoneMatrices);
        
        _vao.LinkToVAO(0, 3, _vertVbo);
        _vao.LinkToVAO(1, 2, _uvVbo);
        _vao.LinkToVAO(2, 1, _textureVbo);
        _vao.LinkToVAO(3, 3, _normalVbo);
        _vao.LinkToVAO(4, 1, _boneVbo);

        GenerateRigBuffers();
        
        _ibo = new IBO(Indices);
    }

    public void GenerateRigBuffers()
    {
        _boneVertexVbo = new(BoneVertices);
        _boneColorVbo = new(BoneColors);
        _boneSizeVbo = new(BoneSizes);

        _boneVao.LinkToVAO(0, 3, _boneVertexVbo);
        _boneVao.LinkToVAO(1, 3, _boneColorVbo);
        _boneVao.LinkToVAO(2, 1, _boneSizeVbo);
    }

    public void GenerateIndices()
    {
        Indices.Clear();
        for (uint i = 0; i < Vertices.Count; i++)
        {
            Indices.Add(i);
        }
    }

    public void GenerateBasicBoneMatrices()
    {
        BoneMatrices.Clear();
        for (int i = 0; i < BoneCount; i++)
        {
            BoneMatrices.Add(Matrix4.Identity);
        }
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
        _boneVertexVbo.Update(BoneVertices);
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
        _boneSSBO.Bind(0);
        
        GL.DrawElements(PrimitiveType.Triangles, Indices.Count, DrawElementsType.UnsignedInt, 0);
        
        _vao.Unbind();
        _ibo.Unbind();
        _boneSSBO.Unbind();
    }

    public void UpdateBoneMatrices()
    {
        _boneSSBO.Update(BoneMatrices, 0);
    }

    public override void SaveModel(string modelName)
    {
        SaveModel(modelName, Game.modelPath);
    }

    public override void SaveModel(string modelName, string basePath)
    {

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
        }

        for (int i = edgeTotal + 3; i <= triangleTotal + 2; i++)
        {
            string[] values = lines[i].Split(' ');

            Vertex a = VertexList[int.Parse(values[1])];
            Vertex b = VertexList[int.Parse(values[2])];
            Vertex c = VertexList[int.Parse(values[3])];

            Vertices.AddRange(a, b, c);
            BoneIndices.AddRange(a.Index, b.Index, c.Index);
        }

        for (int i = vertexCount + edgeCount + triangleCount + 4; i <= vertexCount + edgeCount + triangleCount + uvCount + 3; i++)
        {
            string[] values = lines[i].Split(' ');
            Uvs.Add(new Vector2(float.Parse(values[1]), float.Parse(values[2])));
            TextureIndices.Add(0);
        }

        for (int i = uvTotal + 5; i <= normalTotal + 4; i++)
        {
            Normals.Add(Parse(lines[i], (1, 2, 3)));
        }

        BoneCount = 0;
        for (int i = normalTotal + 6; i <= boneTotal + 5; i++)
        {
            string line = lines[i];
            Vector3 pivot = Parse(line, (1, 2, 3));
            Vector3 end = Parse(line, (4, 5, 6));
            
            if (BoneCount == 0)
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

            BoneCount++;
        }

        Init();
        GenerateBuffers();

        return true;
    }

    public override void Unload()
    {
        VertexList.Clear();
        Vertices.Clear();
        Indices.Clear();
        BoneMatrices.Clear();
        BoneVertices.Clear();
        BoneColors.Clear();
        BoneSizes.Clear();
        BoneIndices.Clear();
        Normals.Clear();
        TextureIndices.Clear();
        BoneCount = 0;
    }
}