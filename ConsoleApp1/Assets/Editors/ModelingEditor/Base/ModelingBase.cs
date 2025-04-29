using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenTK.Mathematics;

public abstract class ModelingBase
{
    public ModelingEditor Editor;
    public Model? Model => Editor.Model;
    public bool CanStash {
        get => Editor.CanStash;
        set => Editor.CanStash = value;
    }
    public bool CanGenerateBuffers {
        get => Editor.CanGenerateBuffers;
        set => Editor.CanGenerateBuffers = value;
    }
    public bool FreeCamera => Editor.Editor.freeCamera;

    public ModelingBase(ModelingEditor editor)
    {
        Editor = editor;
    }

    public abstract void Start();
    public abstract void Update();
    public abstract void Render();
    public abstract void Exit();

    protected struct BoundingBoxRegion
    {
        public Vector3 Min;
        public Vector3 Max;
        public Vector3 OriginalMin;
        public Vector3 Size;
        public List<Vertex> Vertices;

        public BoundingBoxRegion(Vector3 min, Vector3 max, List<Vertex> vertices)
        {
            Min = min;
            Max = max;
            OriginalMin = min;
            Size = max - min;
            Vertices = vertices;
        }

        public void SetMin(Vector3 min)
        {
            Min = min;
            Max = Min + Size;
        }

        public bool Intersects(BoundingBoxRegion other)
        {
            return (Mathf.Min(Max.X, other.Max.X) >= Mathf.Max(Min.X, other.Min.X)) &&
                (Mathf.Min(Max.Y, other.Max.Y) >= Mathf.Max(Min.Y, other.Min.Y)) &&
                (Mathf.Min(Max.Z, other.Max.Z) >= Mathf.Max(Min.Z, other.Min.Z));
        }

        /// Check if two bounding boxes intersect
        public static bool operator &(BoundingBoxRegion a, BoundingBoxRegion b)
        {
            return a.Intersects(b);
        }
    }
}