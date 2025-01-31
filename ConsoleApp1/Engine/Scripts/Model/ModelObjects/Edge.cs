﻿public class Edge
{
    public OldVertex V1 { get; }
    public OldVertex V2 { get; }

    public Edge(OldVertex v1, OldVertex v2)
    {
        if (v1.GetHashCode() > v2.GetHashCode())
        {
            V1 = v1;
            V2 = v2;
        }
        else
        {
            V1 = v2;
            V2 = v1;
        }
    }

    public override bool Equals(object obj)
    {
        if (obj is not Edge other) return false;
        return (V1 == other.V1 && V2 == other.V2);
    }

    public override int GetHashCode()
    {
        return V1.GetHashCode() ^ V2.GetHashCode();
    }
}