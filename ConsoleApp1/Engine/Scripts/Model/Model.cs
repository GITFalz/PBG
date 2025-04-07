using OpenTK.Mathematics;

public class Model
{
    public string Name = "Model";
    public bool IsShown = true;
    public bool IsSelected = false;
    

    public ModelMesh Mesh = new();

    public void Render()
    {

    }
    
    public static List<Edge> GetFullSelectedEdges(List<Vertex> selectedVertices)
    {
        HashSet<Edge> edges = [];
                
        foreach (var vert in selectedVertices)
        {
            foreach (var edge in vert.ParentEdges)
            {
                if (selectedVertices.Contains(edge.Not(vert)))
                    edges.Add(edge);
            }
        }

        return edges.ToList();
    }

    public static List<Triangle> GetFullSelectedTriangles(List<Vertex> selectedVertices)
    {
        HashSet<Triangle> triangles = [];
                
        foreach (var triangle in GetSelectedTriangles(selectedVertices))
        {
            if (IsTriangleFullySelected(selectedVertices, triangle))
                triangles.Add(triangle);
        }
        
        return triangles.ToList();
    }

    public static HashSet<Triangle> GetSelectedTriangles(List<Vertex> selectedVertices)
    {
        HashSet<Triangle> triangles = [];
                
        foreach (var vert in selectedVertices)
        {
            foreach (var triangle in vert.ParentTriangles)
            {
                triangles.Add(triangle);
            }
        }
        
        return triangles;
    }

    public static bool IsTriangleFullySelected(List<Vertex> selectedVertices, Triangle triangle)
    {
        return selectedVertices.Contains(triangle.A) &&
               selectedVertices.Contains(triangle.B) &&
               selectedVertices.Contains(triangle.C);
    }

    public static List<Vertex> GetVertices(List<Triangle> triangles)
    {
        List<Vertex> vertices = [];

        foreach (var triangle in triangles)
        {
            if (!vertices.Contains(triangle.A))
                vertices.Add(triangle.A);

            if (!vertices.Contains(triangle.B))
                vertices.Add(triangle.B);

            if (!vertices.Contains(triangle.C))
                vertices.Add(triangle.C);
        }

        return vertices;
    }

    public static List<Edge> GetEdges(List<Triangle> triangles)
    {
        List<Edge> edges = [];

        foreach (var triangle in triangles)
        {
            if (!edges.Contains(triangle.AB))
                edges.Add(triangle.AB);

            if (!edges.Contains(triangle.BC))
                edges.Add(triangle.BC);

            if (!edges.Contains(triangle.CA))
                edges.Add(triangle.CA);
        }

        return edges;
    }

    public static Vector3 GetSelectedCenter(List<Vertex> selectedVertices)
    {
        Vector3 center = Vector3.Zero;
        if (selectedVertices.Count == 0)
            return center;

        foreach (var vert in selectedVertices)
        {
            center += vert;
        }
        return center / selectedVertices.Count;
    }

    public static Vector3 GetAverageNormal(List<Triangle> triangles)
    {
        Vector3 normal = (0, 1, 0);
        if (triangles.Count == 0)
            return normal;

        foreach (var triangle in triangles)
        {
            normal += triangle.Normal;
        }
        return normal / triangles.Count;
    }

    public static void MoveSelectedVertices(Vector3 move, List<Vertex> selectedVertices)
    {
        foreach (var vert in selectedVertices)
        {
            if (ModelSettings.GridAligned && ModelSettings.Snapping)
                vert.SnapPosition(move, ModelSettings.SnappingFactor);
            else
                vert.MovePosition(move);
        }
    }

    public static void Handle_Flattening(List<Triangle> triangles)
    {
        if (triangles.Count == 0)
            return;

        Triangle first = triangles[0];

        Vector3 rotationAxis = Vector3.Cross(first.Normal, (0, 1, 0));

        if (rotationAxis.Length != 0)
        {
            float angle = MathHelper.RadiansToDegrees(Vector3.CalculateAngle(first.Normal, (0, 1, 0)));
            Vector3 center = first.GetCenter();
            Vector3 rotatedNormal = Mathf.RotatePoint(first.Normal, Vector3.Zero, rotationAxis, angle);

            if (Vector3.Dot(rotatedNormal, (0, 1, 0)) < 0)
                angle += 180f;
            
            foreach (var vert in Model.GetVertices(triangles))
                vert.SetPosition(Mathf.RotatePoint(vert, center, rotationAxis, angle));
        }

        first.FlattenRegion(triangles);
    }
}