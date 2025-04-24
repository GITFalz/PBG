using OpenTK.Mathematics;


public static class VoronoiLib
{
    private static Vector2i[] offsets = [(1, 1),(1, 0),(1, -1),(0, -1),(-1, -1),(-1, 0),(-1, 1),(0, 1)];

    private static float Hash(Vector2 p) {
        return Mathf.Fraction(Mathf.Sin(Vector2.Dot(p , new Vector2(127.1f, 311.7f))) * 43758.5453f);
    }

    private static Vector2 Hash2(Vector2 p) {
        p = new Vector2(Vector2.Dot(p, new Vector2(127.1f, 311.7f)), Vector2.Dot(p, new Vector2(269.5f, 183.3f)));
        return Mathf.Fraction(Mathf.Sin(p) * 43758.5453f);
    }

    private static Vector3 Hash3(Vector2 p) {
        Vector3 c = new Vector3(Vector2.Dot(p, new Vector2(127.1f, 311.7f)), Vector2.Dot(p, new Vector2(269.5f, 183.3f)), Vector2.Dot(p, new Vector2(419.2f, 271.6f)));
        return Mathf.Fraction(Mathf.Sin(c) * 43758.5453f);
    }

    private static Vector2i[] GetP(Vector2i p) {
        return [p,p+offsets[0],p+offsets[1],p+offsets[2],p+offsets[3],p+offsets[4],p+offsets[5],p+offsets[6],p+offsets[7]];
    }

    private static Vector2[] GetG(Vector2i[] p) {
        return [Hash2(p[0]),Hash2(p[1])+offsets[0],Hash2(p[2])+offsets[1],Hash2(p[3])+offsets[2],Hash2(p[4])+offsets[3],Hash2(p[5])+offsets[4],Hash2(p[6])+offsets[5],Hash2(p[7])+offsets[6],Hash2(p[8])+offsets[7]];
    }

    // Mathf.Single color voronoi
    public static float Voronoi(Vector2 p) {
        Vector2i[] ps = GetP(Mathf.FloorToInt(p)); // the floored position and its neighbors
        Vector2[] gs = GetG(ps); // the random positions of the neighbors
        p = Mathf.Fraction(p);
        float d = Vector2.Distance(p, gs[0]);
        float c = Hash(ps[0]);
        for (int i = 1; i < 9; i++) {
            float d1 = Vector2.Distance(p, gs[i]);
            if (d1 < d) {
                d = d1; c = Hash(ps[i]);
            }
        }
        return c;
    }

    // Edge voronoi
    public static float VoronoiF2(Vector2 p) {
        Vector2i[] ps = GetP(Mathf.FloorToInt(p)); // the floored position and its neighbors
        Vector2[] gs = GetG(ps); // the random positions of the neighbors
        p = Mathf.Fraction(p);
        float d = Vector2.Distance(p, gs[0]);
        float d2 = 999.0f;
        for (int i = 1; i < 9; i++) {
            float dist = Vector2.Distance(p, gs[i]);
            if (dist < d) {
                d2 = d; d = dist;
            } else if (dist < d2) {
                d2 = dist;
            }
        }
        return d2 - d;
    }

    // Color voronoi
    public static Vector3 Voronoi3(Vector2 p) {
        Vector2i[] ps = GetP(Mathf.FloorToInt(p)); // the floored position and its neighbors
        Vector2[] gs = GetG(ps); // the random positions of the neighbors
        p = Mathf.Fraction(p);
        float d = Vector2.Distance(p, gs[0]);
        Vector3 c = Hash3(ps[0]);
        for (int i = 1; i < 9; i++) {
            float d1 = Vector2.Distance(p, gs[i]);
            if (d1 < d) {
                d = d1; c = Hash3(ps[i]);
            }
        }
        return c;
    }

    // Distance to voronoi cell
    public static float VoronoiDistance(Vector2 p) {
        Vector2i[] ps = GetP(Mathf.FloorToInt(p)); // the floored position and its neighbors
        Vector2[] gs = GetG(ps); // the random positions of the neighbors
        p = Mathf.Fraction(p);
        float d = Vector2.Distance(p, gs[0]);
        for (int i = 1; i < 9; i++) {
            float d1 = Vector2.Distance(p, gs[i]);
            if (d1 < d) {
                d = d1;
            }
        }
        return d;
    }

    // Checkerboard voronoi
    public static float VoronoiChecker(Vector2 p) {
        Vector2i[] ps = GetP(Mathf.FloorToInt(p)); // the floored position and its neighbors
        Vector2[] gs = GetG(ps); // the random positions of the neighbors
        p = Mathf.Fraction(p);
        float d = Vector2.Distance(p, gs[0]);
        float c = Hash(ps[0]);
        for (int i = 1; i < 9; i++) {
            float d1 = Vector2.Distance(p, gs[i]);
            if (d1 < d) {
                d = d1; c = Hash(ps[i]);
            }
        }
        return Mathf.Mod(Mathf.Floor(c * 10.0f), 2.0f);
    }

    // Worley Flow voronoi
    public static Vector2 VoronoiWF(Vector2 p) {
        Vector2i[] ps = GetP(Mathf.FloorToInt(p)); // the floored position and its neighbors
        Vector2[] gs = GetG(ps); // the random positions of the neighbors
        p = Mathf.Fraction(p);
        Vector2 g = gs[0];
        float d = Vector2.Distance(p, gs[0]);
        for (int i = 1; i < 9; i++) {
            float d1 = Vector2.Distance(p, gs[i]);
            if (d1 < d) {
                d = d1; g = gs[i];
            }
        }
        Vector2 dir = (p - g).Normalized(); // flow from feature to pixel
        return dir;
    }
}