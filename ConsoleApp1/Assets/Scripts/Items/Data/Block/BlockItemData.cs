using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class BlockItemData : ItemData
{
    public uint BlockIndex;
    public Block Block;

    public BlockItemData(CWorldBlock block)
    {
        Name = block.blockName;
        BlockIndex = block.index;
        Block = block.GetBlock();
        MaxStackSize = 999;
        Base(); 
    }

    public override void GenerateIcon()
    {   
        int[] indices = BlockManager.GetBlock(BlockIndex).GetIndices();

        GL.ClearColor(0.0f, 0.0f, 0.0f, 0.0f);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.Uniform1(CubeIndicesLocation, 6, indices);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

        ItemDataManager.Data.Add(FBO.GetPixels(64, 64)); 
    }

    public override void RenderIcon(Vector2 position, float scale) { RenderIcon((position.X, position.Y, 0), scale); }
    public override void RenderIcon(Vector3 position, float scale) 
    {
        IconShader.Bind();
        ItemDataManager.Image.Bind(TextureUnit.Texture0);
        IconVAO.Bind();

        Matrix4 model = Matrix4.CreateScale(scale) * Matrix4.CreateTranslation(position.X, position.Y, position.Z);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -1, 1);

        GL.UniformMatrix4(IconModelLocation, true, ref model);
        GL.UniformMatrix4(IconProjectionLocation, true, ref projection);
        GL.Uniform2(IconSizeLocation, new Vector2(100, 100));
        GL.Uniform1(IconTextureLocation, 0);
        GL.Uniform1(IconIndexLocation, Index);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        Shader.Error("Error rendering icon: ");

        IconVAO.Unbind();
        ItemDataManager.Image.Unbind();
        IconShader.Unbind();
    }

    public override void LeftClick(ItemSlot slot) 
    { 
        if (RemoveBlock(PlayerData.LookingAtBlockPosition, out Block swappedBlock) && BlockManager.GetBlock(swappedBlock.BlockId(), out var block))
        {
            PlayerInventoryManager.AddBlock(block);
        }
    }
    public override void RightClick(ItemSlot slot) 
    { 
        if (slot.Amount <= 0 || slot.Inventory == null)
            return;

        if (PlaceBlock(PlayerData.LookingAtBlockPlacementPosition, Block))
        {
            slot.Inventory.Remove(slot, 1);
        }
    }
}