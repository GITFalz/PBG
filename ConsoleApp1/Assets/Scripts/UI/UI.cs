using ConsoleApp1.Assets.Scripts.Rendering;
using ConsoleApp1.Engine.Scripts.UI.UITextData;
using OpenTK.Mathematics;
using StbImageSharp;

namespace ConsoleApp1.Assets.Scripts.UI;

public class UI
{
    private static Vector3 lastCharacterPosition = Vector3.Zero;
    private static Vector3 nextCharacterPosition = new Vector3(7, 0, 0);
    
    /*
     * position: The position of the ui element
     * texture: The texture of the ui element (to reference the size of the texture)
     * width/height: The width and height of the ui element
     * cellSize: The size of the grid cells
     * grid: values indicate the pixel distance from the edge of the texture to the start of the grid
     */
    public static MeshData Generate9Slice(Vector3 position, float textureWidth, float textureHeight, float width, float height, float cellSize, Vector4 grid)
    {
        MeshData meshData = new MeshData();
        
        Vector3 offsetX1 = new Vector3(cellSize, 0f, 0f);
        Vector3 offsetX2 = new Vector3(width - cellSize, 0f, 0f);
        
        Vector3 offsetY1 = new Vector3(0f, cellSize, 0f);
        Vector3 offsetY2 = new Vector3(0f, height - cellSize, 0f);
        
        float x1 = grid.X / textureWidth;
        float x2 = grid.Y / textureHeight;
        
        float y1 = grid.Z / textureWidth;
        float y2 = grid.W / textureHeight;
        
        float middle = 1f - x1 - x2;
        
        //Bottom left corner
        GenerateBasicQuad(position, cellSize, meshData);
        
        //Bottom center
        float sideWidth = width - cellSize * 2;
        float sideHeight = cellSize;

        GenerateQuad(position + offsetX1, sideWidth, sideHeight, meshData);
        
        //Bottom right corner
        GenerateBasicQuad(position + offsetX2, cellSize, meshData);
        
        //Middle left
        sideWidth = cellSize;
        sideHeight = height - cellSize * 2;
        
        GenerateQuad(position + offsetY1, sideWidth, sideHeight, meshData);
        
        //Middle center
        sideWidth = width - cellSize * 2;
        sideHeight = height - cellSize * 2;
        
        GenerateQuad(position + offsetX1 + offsetY1, sideWidth, sideHeight, meshData);
        
        //Middle right
        sideWidth = cellSize;
        sideHeight = height - cellSize * 2;
        
        GenerateQuad(position + offsetX2 + offsetY1, sideWidth, sideHeight, meshData);
        
        //Top left corner
        GenerateBasicQuad(position + offsetY2, cellSize, meshData);
        
        //Top center
        sideWidth = width - cellSize * 2;
        sideHeight = cellSize;
        
        GenerateQuad(position + offsetX1 + offsetY2, sideWidth, sideHeight, meshData);
        
        //Top right corner
        GenerateBasicQuad(position + offsetX2 + offsetY2, cellSize, meshData);
        
        for (int uv = 0; uv < 9; uv++)
            AddUvs(meshData, uvMaps[uv](x1, x2, y1, y2, middle));
        
        return meshData;
    }

    public static void GenerateCharacters(Vector3 position, float size, List<Character> characters, MeshData meshData)
    {
        Vector3 offset = Vector3.Zero;
        foreach (Character character in characters)
        {
            GenerateCharacter(position + offset, size, character, meshData);
            offset.X += size * 7;
        }
    }

    public static void GenerateCharacter(Vector3 position, float size, Character character, MeshData meshData)
    {
        lastCharacterPosition = position;
        nextCharacterPosition = position + new Vector3(7 * size, 0, 0);
        
        if (character == Character.Space)
            return;
        
        meshData.verts.Add((quadFunc[0](7, 9) * size) + position);
        meshData.verts.Add((quadFunc[1](7, 9) * size) + position);
        meshData.verts.Add((quadFunc[2](7, 9) * size) + position);
        meshData.verts.Add((quadFunc[3](7, 9) * size) + position);
        
        AddTris(meshData);
        AddUvs(meshData, UIText.CharacterUvs[character]);
        
        Console.WriteLine("Character: " + character + " Position: " + position);
    }
    
    public static void GenerateCharacterAtLastPosition(float size, Character character, MeshData meshData)
    {
        GenerateCharacter(nextCharacterPosition, size, character, meshData);
    }
    
    
    
    public static void GenerateCharacters(Vector3 position, float size, int[] intArray, MeshData meshData)
    {
        Vector3 offset = Vector3.Zero;
        foreach (var i in intArray)
        {
            GenerateCharacter(position + offset, size, i, meshData);
            offset.X += size * 7;
        }
    }
    
    public static void GenerateCharacter(Vector3 position, float size, int i, MeshData meshData)
    {
        meshData.verts.Add((quadFunc[0](7, 9) * size) + position);
        meshData.verts.Add((quadFunc[1](7, 9) * size) + position);
        meshData.verts.Add((quadFunc[2](7, 9) * size) + position);
        meshData.verts.Add((quadFunc[3](7, 9) * size) + position);
        
        AddTris(meshData);
        AddUvs(meshData, UIText.IntUvs[i]);
    }

    public static void RemoveLastQuad(MeshData meshData)
    {
        meshData.RemoveLastQuad();
        lastCharacterPosition.X -= 7;
        nextCharacterPosition.X -= 7;
    }
    
    
    private static void GenerateBasicQuad(Vector3 position, float size, MeshData meshData)
    {
        meshData.verts.Add((quad[0] * size) + position);
        meshData.verts.Add((quad[1] * size) + position);
        meshData.verts.Add((quad[2] * size) + position);
        meshData.verts.Add((quad[3] * size) + position);
        
        AddTris(meshData);
    }
    
    private static void GenerateQuad(Vector3 position, float width, float height, MeshData meshData)
    {
        meshData.verts.Add(quadFunc[0](width, height) + position);
        meshData.verts.Add(quadFunc[1](width, height) + position);
        meshData.verts.Add(quadFunc[2](width, height) + position);
        meshData.verts.Add(quadFunc[3](width, height) + position);

        AddTris(meshData);
    }
    
    private static void AddUvs(MeshData meshData, Vector2[] uvs)
    {
        meshData.uvs.Add(uvs[0]);
        meshData.uvs.Add(uvs[1]);
        meshData.uvs.Add(uvs[2]);
        meshData.uvs.Add(uvs[3]);
    }
    
    //Only if verts have been added (otherwise Count - 4 could be negative)
    private static void AddTris(MeshData meshData)
    {
        uint index = (uint)meshData.verts.Count - 4;
        
        meshData.tris.Add(0 + index);
        meshData.tris.Add(1 + index);
        meshData.tris.Add(2 + index);
        meshData.tris.Add(2 + index);
        meshData.tris.Add(3 + index);
        meshData.tris.Add(0 + index);
    }
    
    private static readonly List<Vector3> quad = new List<Vector3>
    {
        new Vector3(0f, 0f, 0f), 
        new Vector3(1f, 0f, 0f), 
        new Vector3(1f, 1f, 0f), 
        new Vector3(0f, 1f, 0f) 
    };
    
    private static readonly Func<float, float, Vector3>[] quadFunc = new Func<float, float, Vector3>[]
    {
        (width, height) => new Vector3(0f, 0f, 0f),
        (width, height) => new Vector3(width, 0f, 0f),
        (width, height) => new Vector3(width, height, 0f),
        (width, height) => new Vector3(0f, height, 0f)
    };

    private static readonly Func<float, float, float, float, float, Vector2[]>[] uvMaps = new Func<float, float, float, float, float, Vector2[]>[]
    {
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(0, 1),
            new Vector2(x1, 1),
            new Vector2(x1, 1 - y2),
            new Vector2(0, 1 - y2)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(x1, 1),
            new Vector2(x1 + middle, 1),
            new Vector2(x1 + middle, 1 - y2),
            new Vector2(x1, 1 - y2)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(1 - x2, 1),
            new Vector2(1, 1),
            new Vector2(1, 1 - y2),
            new Vector2(1 - x2, 1 - y2)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(0, 1 - y2),
            new Vector2(x1, 1 - y2),
            new Vector2(x1, y1),
            new Vector2(0, y1)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(x1, 1 - y2),
            new Vector2(x1 + middle, 1 - y2),
            new Vector2(x1 + middle, y1),
            new Vector2(x1, y1)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(1 - x2, 1 - y2),
            new Vector2(1, 1 - y2),
            new Vector2(1, y1),
            new Vector2(1 - x2, y1)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(0, y1),
            new Vector2(x1, y1),
            new Vector2(x1, 0),
            new Vector2(0, 0)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(x1, y1),
            new Vector2(x1 + middle, y1),
            new Vector2(x1 + middle, 0),
            new Vector2(x1, 0)
        },
        (x1, x2, y1, y2, middle) => new Vector2[]
        {
            new Vector2(1 - x2, y1),
            new Vector2(1, y1),
            new Vector2(1, 0),
            new Vector2(1 - x2, 0)
        }
    };
}