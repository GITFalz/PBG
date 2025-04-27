using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Inventory
{
    public static List<Inventory> InventoryList = new List<Inventory>();

    public static ShaderProgram InventoryShader = new ShaderProgram("Inventory/Items/Items.vert", "Utils/ArrayImages.frag");
    public static VAO InventoryVAO = new VAO();

    public static int InventoryModelLocation = -1;
    public static int InventoryProjectionLocation = -1;
    public static int InventoryTextureLocation = -1;

    static Inventory()
    {
        InventoryShader.Bind();

        InventoryModelLocation = InventoryShader.GetLocation("model");
        InventoryProjectionLocation = InventoryShader.GetLocation("projection");
        InventoryTextureLocation = InventoryShader.GetLocation("textureArray");

        InventoryShader.Unbind();
    }
    

    public SSBO<IconData> IconSSBO;
    public IconData[] IconDataList = [];
    public ItemSlot[] Items;
    public UIText[] IconCountText = [];

    public int Width = 0;
    public int Height = 0;
    public Vector2 Size = Vector2.Zero;
    public Vector2 HalfSize = Vector2.Zero;

    public Vector2 Offset = Vector2.Zero;

    public Vector2 Position = new Vector2(100, 100);

    public int VisibleItems = 0;

    private bool _updateVisibility = false;
    private bool _updateData = false;

    public UIController UIController;
    public UICollection Collection;

    public AnchorType AnchorType = AnchorType.TopLeft;

    public int currentIndex = 0;

    public double leftTimer = 0;
    public double rightTimer = 0;
 
    public Inventory(int width, int height)
    {
        InventoryList.Add(this);

        Items = new ItemSlot[width * height];
        IconDataList = new IconData[width * height];
        IconSSBO = new(new IconData[width * height]);
        IconCountText = new UIText[width * height];

        Width = width;
        Height = height;
        Size = (width * 50 + 20, height * 50 + 20);
        HalfSize = Size / 2;

        for (int i = 0; i < Items.Length; i++)
        {
            var item = ItemDataManager.GetItem("grass_block");
            Items[i] = new ItemSlot(item, 1);
            Items[i].Visible = true;
            Items[i].Amount = 1;
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = x + y * width;
                IconData iconData = new IconData
                {
                    Position = new Vector2(x * 50, y * 50),
                    Size = new Vector2(50, 50),
                    Data = new Vector2i(0, VisibleItems)
                };
                IconDataList[index] = iconData;

                VisibleItems++;
                index++;
            }
        }

        _updateVisibility = true;

        UIController = new();

        Collection = new UICollection("Inventory", UIController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (Width * 50 + 20, Height * 50 + 20), (-10, -10, 0, 0), 0);

        UIButton movingBar = new UIButton("MovingBar", UIController, AnchorType.TopLeft, PositionType.Relative, (0.8f, 0.8f, 0.8f, 1f), (0, 0, 0), (Width * 50 + 20, 20), (0, -20, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);
        movingBar.SetOnHold(MoveBar); 

        UIVerticalCollection verticalCollection = new UIVerticalCollection("InventoryVerticalCollection", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (Width * 50 + 20, Height * 50 + 20), (0, 0, 0, 0), (10, 10, 10, 10), 0, 10);

        for (int y = 0; y < height; y++)
        {
            UIHorizontalCollection horizontalCollection = new UIHorizontalCollection("InventoryHorizontalCollection" + y, UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (Width * 50 + 20, 50), (0, 0, 0, 0), (-1f, -1f, -1f, -1f), -2, 0);
        
            for (int x = 0; x < width; x++)
            {
                int index = x + y * width;
                UICollection slotCollection = new UICollection("SlotCollection" + x + "" + y, UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (52, 52), (0, 0, 0, 0), 0);
                UIImage slotImage = new UIImage("Slot" + x + "" + y, UIController, AnchorType.TopLeft, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), (52, 52), (0, 0, 0, 0), 0, 11, (10, 0.05f));
                UIText iconCount = new UIText("IconCount" + x + "" + y, UIController, AnchorType.BottomLeft, PositionType.Relative, (1f, 1f, 1f, 1f), (0, 0, 0), (50, 50), (5, -5, 0, 0), 0);
                iconCount.SetMaxCharCount(3).SetText(Items[index].GetAmountToString(), 1.2f).SetTextType(TextType.Numeric);
                iconCount.Depth = 20;
                slotCollection.AddElements(slotImage, iconCount);
                horizontalCollection.AddElement(slotCollection); 
                IconCountText[index] = iconCount;
            }

            verticalCollection.AddElement(horizontalCollection);
        }

        UIImage background = new UIImage("Background", UIController, AnchorType.ScaleFull, PositionType.Relative, (0.8f, 0.8f, 0.8f, 1f), (0, 0, 0), (Width * 50 + 10, Height * 50 + 10), (0, 0, 0, 0), 0, 10, (10, 0.05f));
    
        Collection.AddElements(movingBar, background, verticalCollection);

        UIController.AddElement(Collection);

        UIController.SetPosition(new Vector3(Position.X, Position.Y, 0));
    }

    private void MoveBar()
    {
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta != Vector2.Zero)
        {
            SetPosition(Position + mouseDelta);
        }
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
        UIController.SetPosition(position);
    }

    public void Center(Vector2 position)
    {
        SetPosition(position - HalfSize);
    }

    public void Align()
    {
        if ((int)AnchorType >= 9)
            AnchorType = AnchorType.MiddleCenter;

        SetPosition(_getPosition[(int)AnchorType](Size, Offset));
    }

    public void SetVisibility(int index, bool visible)
    {
        if (index < 0 || index >= IconDataList.Length)
            return;

        Items[index].Visible = visible;
        _updateVisibility = true;
    }

    public void ExchangeItem(int index)
    {
        if (index < 0 || index >= IconDataList.Length)
            return;

        Items[index].Exchange(SelectedItemManager.SelectedItem);
        IconCountText[index].SetText(Items[index].GetAmountToString(), 1.2f).UpdateCharacters();
        SelectedItemManager.UpdateSelectedItemText.Invoke();
        _updateVisibility = true;
    }

    public void Transfer(int index)
    {
        if (index < 0 || index >= IconDataList.Length)
            return;

        Items[index].Transfer(SelectedItemManager.SelectedItem);
        IconCountText[index].SetText(Items[index].GetAmountToString(), 1.2f).UpdateCharacters();
        SelectedItemManager.UpdateSelectedItemText.Invoke();
        _updateVisibility = true;
    }

    public void GatherAll()
    {
        for (int i = 0; i < IconDataList.Length; i++)
        {
            if (Items[i].item.IsEmpty())
                continue;

            if (Items[i].SameItem(SelectedItemManager.SelectedItem))
            {
                SelectedItemManager.SelectedItem.Add(Items[i]);
                IconCountText[i].SetText(Items[i].GetAmountToString(), 1.2f).UpdateCharacters();
                if (SelectedItemManager.SelectedItem.HasMaxStackSize())
                    break;
            }
        }
        SelectedItemManager.UpdateSelectedItemText.Invoke();
        _updateVisibility = true;
    }

    public void AddTo(int index)
    {
        if (index < 0 || index >= IconDataList.Length)
            return;

        if (Items[index].Amount >= 990)
            return;

        if (Items[index].Amount <= 0)
        {
            Items[index].Visible = true;
            IconCountText[index].SetVisibility(true);
            _updateVisibility = true;
        }

        Items[index].Amount++;
        IconCountText[index].SetText(Items[index].GetAmountToString(), 1.2f).UpdateCharacters();
    }

    public void RemoveFrom(int index)
    {
        if (index < 0 || index >= IconDataList.Length)
            return;

        if (Items[index].Amount <= 0)
            return;

        Items[index].Amount--;
        
        if (Items[index].Amount <= 0)
        {
            Items[index].Visible = false;
            IconCountText[index].SetVisibility(false);
            _updateVisibility = true;
        }
        else
        {
            IconCountText[index].SetText(Items[index].GetAmountToString(), 1.2f).UpdateCharacters();
        }   
    }

    public void UpdateData()
    {
        VisibleItems = 0;
        for (int i = 0; i < Items.Length; i++)
        {
            if (!Items[i].item.IsEmpty() && Items[i].Visible)
            {
                IconDataList[VisibleItems].Data.Y = i;
                VisibleItems++;
            }
        }
        _updateData = true;
    }

    public void UpdateIconData()
    {
        IconSSBO.Update(IconDataList, 0);
    }

    public void Resize()
    {
        UIController.Resize();
        Align();
    }

    public static void ResizeAll()
    {
        foreach (var inventory in InventoryList)
        {
            inventory.Resize();
        }
    }

    public void Update()
    {
        if (Input.IsMousePressed(MouseButton.Right))
        {
            rightTimer = 0;
        }
        if (Input.IsMouseDown(MouseButton.Right))
        {
            Vector2 mousePos = Input.GetMousePosition() - Position;
            mousePos = Mathf.FloorToInt(mousePos /= 50);
            if (mousePos.X >= 0 && mousePos.Y >= 0 && mousePos.X < Width && mousePos.Y < Height)
            {
                int index = (int)mousePos.X + (int)mousePos.Y * Width;
                if (index != currentIndex)
                    Transfer(index);
                currentIndex = index;
            }
        }
        if (Input.IsMousePressed(MouseButton.Left))
        {
            Vector2 mousePos = Input.GetMousePosition() - Position;
            mousePos = Mathf.FloorToInt(mousePos /= 50);
            if (mousePos.X >= 0 && mousePos.Y >= 0 && mousePos.X < Width && mousePos.Y < Height)
            {
                int index = (int)mousePos.X + (int)mousePos.Y * Width;
                if (index >= 0 || index < IconDataList.Length)
                {
                    if (leftTimer < 0.2f)
                    {
                        GatherAll();
                    }
                    else
                    {
                        ExchangeItem(index);
                    }
                }
            }
            leftTimer = 0;
        }

        if (_updateVisibility)
        {
            UpdateData();
            _updateVisibility = false;
        }

        if (_updateData)
        {
            UpdateIconData();
            _updateData = false;
        }

        UIController.Test();

        rightTimer += GameTime.DeltaTime;
        leftTimer += GameTime.DeltaTime;
    }

    public void Render()
    {   
        UIController.RenderDepthTest();

        InventoryShader.Bind();
        IconSSBO.Bind(0);
        ItemDataManager.Image.Bind(TextureUnit.Texture0);
        InventoryVAO.Bind();

        Matrix4 model = Matrix4.CreateTranslation(Position.X, Position.Y, 0.1f);
        Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0, Game.Width, Game.Height, 0, -1, 1);

        GL.UniformMatrix4(InventoryModelLocation, true, ref model);
        GL.UniformMatrix4(InventoryProjectionLocation, true, ref projection);
        GL.Uniform1(InventoryTextureLocation, 0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, VisibleItems * 6);

        InventoryVAO.Unbind();
        ItemDataManager.Image.Unbind();
        IconSSBO.Unbind();
        InventoryShader.Unbind();
    }


    private static readonly Func<Vector2, Vector2, Vector2>[] _getPosition =
    [
        (size, offset) => (offset.X,                                offset.Y),                                  // TopLeft
        (size, offset) => (offset.X + Game.Width / 2 - size.X / 2,  offset.Y),                                  // TopCenter
        (size, offset) => (offset.X + Game.Width - size.X,          offset.Y),                                  // TopRight
        (size, offset) => (offset.X,                                offset.Y + Game.Height / 2 - size.Y / 2),   // MiddleLeft
        (size, offset) => (offset.X + Game.Width / 2 - size.X / 2,  offset.Y + Game.Height / 2 - size.Y / 2),   // MiddleCenter
        (size, offset) => (offset.X + Game.Width - size.X,          offset.Y + Game.Height / 2 - size.Y / 2),   // MiddleRight
        (size, offset) => (offset.X,                                offset.Y + Game.Height - size.Y),           // BottomLeft
        (size, offset) => (offset.X + Game.Width / 2 - size.X / 2,  offset.Y + Game.Height - size.Y),           // BottomCenter
        (size, offset) => (offset.X + Game.Width - size.X,          offset.Y + Game.Height - size.Y),           // BottomRight
    ];
}

public struct IconData {
    public Vector2 Position;
    public Vector2 Size;
    public Vector2i Data;
};
