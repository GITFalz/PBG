using OpenTK.Mathematics;

public static class UiText
{
    public const float Cw = 1f / 18f;
    
    public static readonly Dictionary<Character, Vector2[]> CharacterUvs = new Dictionary<Character, Vector2[]>()
    {
        { Character.A, Char(0, 17)},
        { Character.B, Char(1, 17)},
        { Character.C, Char(2, 17)},
        { Character.D, Char(3, 17)},
        { Character.E, Char(4, 17)},
        { Character.F, Char(5, 17)},
        { Character.G, Char(6, 17)},
        { Character.H, Char(7, 17)},
        { Character.I, Char(8, 17)},
        { Character.J, Char(9, 17)},
        { Character.K, Char(10, 17)},
        { Character.L, Char(11, 17)},
        { Character.M, Char(12, 17)},
        { Character.N, Char(13, 17)},
        { Character.O, Char(14, 17)},
        { Character.P, Char(15, 17)},
        { Character.Q, Char(16, 17)},
        { Character.R, Char(17, 17)},
        { Character.S, Char(0, 16)},
        { Character.T, Char(1, 16)},
        { Character.U, Char(2, 16)},
        { Character.V, Char(3, 16)},
        { Character.W, Char(4, 16)},
        { Character.X, Char(5, 16)},
        { Character.Y, Char(6, 16)},
        { Character.Z, Char(7, 16)},
        
        { Character.a, Char(0, 15)},
        { Character.b, Char(1, 15)},
        { Character.c, Char(2, 15)},
        { Character.d, Char(3, 15)},
        { Character.e, Char(4, 15)},
        { Character.f, Char(5, 15)},
        { Character.g, Char(6, 15)},
        { Character.h, Char(7, 15)},
        { Character.i, Char(8, 15)},
        { Character.j, Char(9, 15)},
        { Character.k, Char(10, 15)},
        { Character.l, Char(11, 15)},
        { Character.m, Char(12, 15)},
        { Character.n, Char(13, 15)},
        { Character.o, Char(14, 15)},
        { Character.p, Char(15, 15)},
        { Character.q, Char(16, 15)},
        { Character.r, Char(17, 15)},
        { Character.s, Char(0, 14)},
        { Character.t, Char(1, 14)},
        { Character.u, Char(2, 14)},
        { Character.v, Char(3, 14)},
        { Character.w, Char(4, 14)},
        { Character.x, Char(5, 14)},
        { Character.y, Char(6, 14)},
        { Character.z, Char(7, 14)},
        
        
        { Character.Zero, Num(0) },
        { Character.One, Num(9) },
        { Character.Two, Num(8) },
        { Character.Three, Num(7) },
        { Character.Four, Num(6) },
        { Character.Five, Num(5) },
        { Character.Six, Num(4) },
        { Character.Seven, Num(3) },
        { Character.Eight, Num(2) },
        { Character.Nine, Num(1) },
    };
    
    public static readonly Dictionary<char, Vector2[]> CharUvs = new Dictionary<char, Vector2[]>()
    {
        { 'A', CharacterUvs[Character.A] },
        { 'B', CharacterUvs[Character.B] },
        { 'C', CharacterUvs[Character.C] },
        { 'D', CharacterUvs[Character.D] },
        { 'E', CharacterUvs[Character.E] },
        { 'F', CharacterUvs[Character.F] },
        { 'G', CharacterUvs[Character.G] },
        { 'H', CharacterUvs[Character.H] },
        { 'I', CharacterUvs[Character.I] },
        { 'J', CharacterUvs[Character.J] },
        { 'K', CharacterUvs[Character.K] },
        { 'L', CharacterUvs[Character.L] },
        { 'M', CharacterUvs[Character.M] },
        { 'N', CharacterUvs[Character.N] },
        { 'O', CharacterUvs[Character.O] },
        { 'P', CharacterUvs[Character.P] },
        { 'Q', CharacterUvs[Character.Q] },
        { 'R', CharacterUvs[Character.R] },
        { 'S', CharacterUvs[Character.S] },
        { 'T', CharacterUvs[Character.T] },
        { 'U', CharacterUvs[Character.U] },
        { 'V', CharacterUvs[Character.V] },
        { 'W', CharacterUvs[Character.W] },
        { 'X', CharacterUvs[Character.X] },
        { 'Y', CharacterUvs[Character.Y] },
        { 'Z', CharacterUvs[Character.Z] },
        
        { 'a', CharacterUvs[Character.a] },
        { 'b', CharacterUvs[Character.b] },
        { 'c', CharacterUvs[Character.c] },
        { 'd', CharacterUvs[Character.d] },
        { 'e', CharacterUvs[Character.e] },
        { 'f', CharacterUvs[Character.f] },
        { 'g', CharacterUvs[Character.g] },
        { 'h', CharacterUvs[Character.h] },
        { 'i', CharacterUvs[Character.i] },
        { 'j', CharacterUvs[Character.j] },
        { 'k', CharacterUvs[Character.k] },
        { 'l', CharacterUvs[Character.l] },
        { 'm', CharacterUvs[Character.m] },
        { 'n', CharacterUvs[Character.n] },
        { 'o', CharacterUvs[Character.o] },
        { 'p', CharacterUvs[Character.p] },
        { 'q', CharacterUvs[Character.q] },
        { 'r', CharacterUvs[Character.r] },
        { 's', CharacterUvs[Character.s] },
        { 't', CharacterUvs[Character.t] },
        { 'u', CharacterUvs[Character.u] },
        { 'v', CharacterUvs[Character.v] },
        { 'w', CharacterUvs[Character.w] },
        { 'x', CharacterUvs[Character.x] },
        { 'y', CharacterUvs[Character.y] },
        { 'z', CharacterUvs[Character.z] },
    };
    
    public static readonly List<Vector2[]> IntUvs = new List<Vector2[]>()
    {
        Num(17), Num(8), Num(9), Num(10), Num(11), Num(12), Num(13), Num(14), Num(15), Num(16)
    };

    private static Vector2[] Num(int i)
    {
        return new Vector2[]
        {
            new Vector2(Cw * i, Cw),
            new Vector2(Cw * (i + 1), Cw),
            new Vector2(Cw * (i + 1), 0),
            new Vector2(Cw * i, 0)
        };
    }
    
    private static Vector2[] Char(int w, int h)
    {
        return new Vector2[]
        {
            new Vector2(Cw * w, Cw * (h + 1)),
            new Vector2(Cw * (w + 1), Cw * (h + 1)),
            new Vector2(Cw * (w + 1), Cw * h),
            new Vector2(Cw * w, Cw * h)
        };
    }
}

public enum Character
{
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z,
    a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p, q, r, s, t, u, v, w, x, y, z,
    Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine,
    Exclamation, Question, Period, Comma, Colon, Semicolon, Apostrophe, Quotation, Dash, Underscore, Plus, Minus, Equals, ForwardSlash, BackSlash, Pipe, Tilde, At, Hash, Dollar, Percent, Caret, Ampersand, Asterisk, OpenParenthesis, CloseParenthesis, OpenBracket, CloseBracket, OpenBrace, CloseBrace, LessThan, GreaterThan,
    Space, Backspace, Enter, 
    None,
}