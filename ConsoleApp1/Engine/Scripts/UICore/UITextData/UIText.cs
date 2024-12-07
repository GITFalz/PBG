using OpenTK.Mathematics;

public static class UIText
{
    public const float Cw = 1f / 18f;

    public static readonly Dictionary<Character, Vector2[]> CharacterUvs = new Dictionary<Character, Vector2[]>()
    {
        { Character.Q, Char(0, 17)},
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
        { Character.A, Char(16, 17)},
        { Character.R, Char(17, 17)},
        { Character.S, Char(0, 16)},
        { Character.T, Char(1, 16)},
        { Character.U, Char(2, 16)},
        { Character.V, Char(3, 16)},
        { Character.Z, Char(4, 16)},
        { Character.X, Char(5, 16)},
        { Character.Y, Char(6, 16)},
        { Character.W, Char(7, 16)},
        
        
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