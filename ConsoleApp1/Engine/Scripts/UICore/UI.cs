﻿using OpenTK.Mathematics;
using StbImageSharp;

public class UI
{
    public static Panel GeneratePanel(Vector3 position, float textureWidth, float textureHeight, float width, float height, float cellSize, Vector4 grid, int textureIndex)
    {
        Panel panel = new Panel();
        
        // Init
        Vector3 offsetX1 = new Vector3(cellSize, 0f, 0f);
        Vector3 offsetX2 = new Vector3(width - cellSize, 0f, 0f);
        
        Vector3 offsetY1 = new Vector3(0f, cellSize, 0f);
        Vector3 offsetY2 = new Vector3(0f, height - cellSize, 0f);
        
        float x1 = grid.X / textureWidth;
        float x2 = grid.Y / textureHeight;
        
        float y1 = grid.Z / textureWidth;
        float y2 = grid.W / textureHeight;
        
        //Bottom left corner
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetY2);
        panel.Vertices.Add(new Vector3(0, cellSize, 0) + position + offsetY2);
        panel.Vertices.Add(new Vector3(cellSize, cellSize, 0) + position + offsetY2);
        panel.Vertices.Add(new Vector3(cellSize, 0, 0) + position + offsetY2);
        
        panel.Uvs.Add(new Vector2(0, 1 - y1));
        panel.Uvs.Add(new Vector2(0, 1));
        panel.Uvs.Add(new Vector2(x1, 1));
        panel.Uvs.Add(new Vector2(x1, 1 - y1));
        
        
        //Bottom center
        float sideWidth = width - cellSize * 2;
        float sideHeight = cellSize;
        
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetX1 + offsetY2);
        panel.Vertices.Add(new Vector3(0, sideHeight, 0) + position + offsetX1 + offsetY2);
        panel.Vertices.Add(new Vector3(sideWidth, sideHeight, 0) + position + offsetX1 + offsetY2);
        panel.Vertices.Add(new Vector3(sideWidth, 0, 0) + position + offsetX1 + offsetY2);
        
        panel.Uvs.Add(new Vector2(x1, 1 - y1));
        panel.Uvs.Add(new Vector2(x1, 1));
        panel.Uvs.Add(new Vector2(1 - x2, 1));
        panel.Uvs.Add(new Vector2(1 - x2, 1 - y1));
        
        //Bottom right corner
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetX2 + offsetY2);
        panel.Vertices.Add(new Vector3(0, cellSize, 0) + position + offsetX2 + offsetY2);
        panel.Vertices.Add(new Vector3(cellSize, cellSize, 0) + position + offsetX2 + offsetY2);
        panel.Vertices.Add(new Vector3(cellSize, 0, 0) + position + offsetX2 + offsetY2);
       
        panel.Uvs.Add(new Vector2(1 - x2, 1 - y1));
        panel.Uvs.Add(new Vector2(1 - x2, 1));
        panel.Uvs.Add(new Vector2(1, 1));
        panel.Uvs.Add(new Vector2(1, 1 - y1));
        
        //Middle left
        sideWidth = cellSize;
        sideHeight = height - cellSize * 2;
        
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetY1);
        panel.Vertices.Add(new Vector3(0, sideHeight, 0) + position + offsetY1);
        panel.Vertices.Add(new Vector3(sideWidth, sideHeight, 0) + position + offsetY1);
        panel.Vertices.Add(new Vector3(sideWidth, 0, 0) + position + offsetY1);
       
        panel.Uvs.Add(new Vector2(0, y2));
        panel.Uvs.Add(new Vector2(0, 1 - y1));
        panel.Uvs.Add(new Vector2(x1, 1 - y1));
        panel.Uvs.Add(new Vector2(x1, y2));
        
        //Middle center
        sideWidth = width - cellSize * 2;
        sideHeight = height - cellSize * 2;
        
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetY1 + offsetX1);
        panel.Vertices.Add(new Vector3(0, sideHeight, 0) + position + offsetY1 + offsetX1);
        panel.Vertices.Add(new Vector3(sideWidth, sideHeight, 0) + position + offsetY1 + offsetX1);
        panel.Vertices.Add(new Vector3(sideWidth, 0, 0) + position + offsetY1 + offsetX1);
       
        panel.Uvs.Add(new Vector2(x1, y2));
        panel.Uvs.Add(new Vector2(x1, 1 - y1));
        panel.Uvs.Add(new Vector2(1 - x2, 1 - y1));
        panel.Uvs.Add(new Vector2(1 - x2, y2));
        
        //Middle right
        sideWidth = cellSize;
        sideHeight = height - cellSize * 2;
        
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetY1 + offsetX2);
        panel.Vertices.Add(new Vector3(0, sideHeight, 0) + position + offsetY1 + offsetX2);
        panel.Vertices.Add(new Vector3(sideWidth, sideHeight, 0) + position + offsetY1 + offsetX2);
        panel.Vertices.Add(new Vector3(sideWidth, 0, 0) + position + offsetY1 + offsetX2);
       
        panel.Uvs.Add(new Vector2(1 - x2, y2));
        panel.Uvs.Add(new Vector2(1 - x2, 1 - y1));
        panel.Uvs.Add(new Vector2(1, 1 - y1));
        panel.Uvs.Add(new Vector2(1, y2));
        
        //Top left corner
        panel.Vertices.Add(new Vector3(0, 0, 0) + position);
        panel.Vertices.Add(new Vector3(0, cellSize, 0) + position);
        panel.Vertices.Add(new Vector3(cellSize, cellSize, 0) + position);
        panel.Vertices.Add(new Vector3(cellSize, 0, 0) + position);
       
        panel.Uvs.Add(new Vector2(0, 0));
        panel.Uvs.Add(new Vector2(0, y2));
        panel.Uvs.Add(new Vector2(x1, y2));
        panel.Uvs.Add(new Vector2(x1, 0));
        
        //Top center
        sideWidth = width - cellSize * 2;
        sideHeight = cellSize;

        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetX1);
        panel.Vertices.Add(new Vector3(0, sideHeight, 0) + position + offsetX1);
        panel.Vertices.Add(new Vector3(sideWidth, sideHeight, 0) + position + offsetX1);
        panel.Vertices.Add(new Vector3(sideWidth, 0, 0) + position + offsetX1);
       
        panel.Uvs.Add(new Vector2(x1, 0));
        panel.Uvs.Add(new Vector2(x1, y2));
        panel.Uvs.Add(new Vector2(1 - x2, y2));
        panel.Uvs.Add(new Vector2(1 - x2, 0));
        
        //Top right corner
        panel.Vertices.Add(new Vector3(0, 0, 0) + position + offsetX2);
        panel.Vertices.Add(new Vector3(0, cellSize, 0) + position + offsetX2);
        panel.Vertices.Add(new Vector3(cellSize, cellSize, 0) + position + offsetX2);
        panel.Vertices.Add(new Vector3(cellSize, 0, 0) + position + offsetX2);
       
        panel.Uvs.Add(new Vector2(1 - x2, 0));
        panel.Uvs.Add(new Vector2(1 - x2, y2));
        panel.Uvs.Add(new Vector2(1, y2));
        panel.Uvs.Add(new Vector2(1, 0));
        
        for (int uv = 0; uv < 9; uv++)
        {
            panel.TextUvs.Add(textureIndex);
            panel.TextUvs.Add(textureIndex);
            panel.TextUvs.Add(textureIndex);
            panel.TextUvs.Add(textureIndex);
        }
        
        return panel;
    }
    
    public static StaticButton CreateStaticButton(AnchorType anchorType, PositionType positionType, Vector3 scale, Vector4 offset, UiMesh? mesh)
    {
        StaticButton button = new StaticButton();
        
        button.SetAnchorType(anchorType);
        button.SetPositionType(positionType);
        button.SetScale(scale);
        button.SetOffset(offset);
        
        if (mesh != null)
            button.SetMesh(mesh);
        
        return button;
    }
    
    public static StaticText CreateStaticText(string text, float fontSize, AnchorType? anchorType, PositionType? positionType, Vector3? scale, Vector4? offset)
    {
        StaticText Text = new StaticText(text, fontSize);
        
        if (anchorType != null)
            Text.SetAnchorType((AnchorType)anchorType);
        if (positionType != null)
            Text.SetPositionType((PositionType)positionType);
        if (scale != null)
            Text.SetScale((Vector3)scale);
        if (offset != null)
            Text.SetOffset((Vector4)offset);
        Text.SetMesh(new TextMesh());
        
        return Text;
    }
    
    public static StaticPanel CreateStaticPanel(AnchorType anchorType, PositionType? positionType, Vector3 scale, Vector4? offset, UiMesh? mesh)
    {
        StaticPanel panel = new StaticPanel();
        
        panel.SetAnchorType(anchorType);
        panel.SetScale(scale);
        if (positionType != null)
            panel.SetPositionType((PositionType)positionType);
        if (offset != null)
            panel.SetOffset((Vector4)offset);
        if (mesh != null)
            panel.SetMesh(mesh);
        
        return panel;
    }
}

public class Panel
{
    public List<Vector3> Vertices = new();
    public List<Vector2> Uvs = new();
    public List<int> TextUvs = new();
    public List<Vector2> UiSizes = new();
}