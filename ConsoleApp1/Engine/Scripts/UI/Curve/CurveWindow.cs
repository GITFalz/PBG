using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class CurveWindow
{
    private static ShaderProgram _curvePanelShader = new ShaderProgram("Painting/Rectangle.vert", "Painting/Curve.frag");

    private static VAO _curvePanelVao = new VAO();

    private static int _modelLocation = -1;
    private static int _projectionLocation = -1;
    private static int _sizeLocation = -1;
    private static int _pointCountLocation = -1;

    static CurveWindow()
    {
        _modelLocation = _curvePanelShader.GetLocation("model");
        _projectionLocation = _curvePanelShader.GetLocation("projection");
        _sizeLocation = _curvePanelShader.GetLocation("size");
        _pointCountLocation = _curvePanelShader.GetLocation("pointCount");
    }


    public CurveConnectorNode? CurveNode = null;

    private SSBO<Vector2> _pointSSBO = new(new Vector2[]{(0, 0), (1,0.2f)});
    public List<Vector2> Points = [(0, 0), (1, 0.2f)];
    public List<UIButton> Buttons = [];
    public Dictionary<UIButton, int> ButtonIndex = new();

    public Vector2 Position { 
        get => _position;
        set
        {
            _position = value;
            ModelMatrix = Matrix4.CreateTranslation(value.X, value.Y, 0.1f);
        }
    }
    private Vector2 _position;
    public Vector2 _oldMouseButtonPosition = new Vector2(0, 0);
    public Vector2 Size;

    public Matrix4 ModelMatrix = Matrix4.Identity;
    public Matrix4 ProjectionMatrix;

    public UIController Controller;

    public UICollection Collection;
    public UICollection ButtonCollection;

    public UIButton X0Button;
    public UIButton X1Button;

    public UIButton? SelectedButton = null;

    private bool _isHoveringOver = false;

    public CurveWindow(UIController controller, Vector2 position, Vector4 offset, Vector2 size)
    {
        Controller = controller;
        Position = position;
        size -= (14, 14);
        Size = size;

        ProjectionMatrix = UIController.OrthographicProjection;

        Collection = new UICollection("CurveCollection", controller, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), size + (14, 14), offset, 0);

        UIImage background = new UIImage("Background", controller, AnchorType.ScaleFull, PositionType.Relative, (0.6f, 0.6f, 0.6f, 1f), (0, 0, 0), size, (0, 0, 0, 0), 0, 11, (10, 0.05f));
        background.SetOnHover(() => { _isHoveringOver = true; });
        ButtonCollection = new UICollection("ButtonCollection", controller, AnchorType.MiddleCenter, PositionType.Absolute, (0, 0, 0), size, (0, 0, 0, 0), 0) 
        {
            Depth = 10
        };

        X0Button = new UIButton("X0Button", controller, AnchorType.TopLeft, PositionType.Absolute, (0.7f, 0.7f, 0.7f, 1f), (0, 0, 0), (20, 20), (-10, size.Y - 10, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);
        X1Button = new UIButton("X1Button", controller, AnchorType.TopLeft, PositionType.Absolute, (0.7f, 0.7f, 0.7f, 1f), (0, 0, 0), (20, 20), (size.X - 10, -10, 0, 0), 0, 10, (10, 0.05f), UIState.Interactable);

        Buttons.AddRange(X0Button, X1Button);
        ButtonIndex.Add(X0Button, 0);
        ButtonIndex.Add(X1Button, 1);

        X0Button.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        X0Button.SetOnHold(() => 
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta != Vector2.Zero)
            {
                Vector4 offset = X0Button.Offset + (mouseDelta.X, mouseDelta.Y, 0, 0);
                offset = Mathf.Clamp((-10, -10, 0, 0), (-10, size.Y - 10, 0, 0), offset);
                X0Button.Offset = offset;
                X0Button.Align();
                X0Button.UpdateTransformation();
                UpdatePoints();
            }
        });
        X0Button.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        X1Button.SetOnClick(() => Game.SetCursorState(CursorState.Grabbed));
        X1Button.SetOnHold(() => 
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta != Vector2.Zero)
            {
                Vector4 offset = X1Button.Offset + (mouseDelta.X, mouseDelta.Y, 0, 0);
                offset = Mathf.Clamp((size.X - 10, -10, 0, 0), (size.X - 10, size.Y - 10, 0, 0), offset);
                X1Button.Offset = offset;
                X1Button.Align();
                X1Button.UpdateTransformation();
                UpdatePoints();
            }
        });
        X1Button.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));

        ButtonCollection.AddElements(X0Button, X1Button);

        Collection.AddElements(background, ButtonCollection);

        UpdatePoints();
    }

    public void MoveNode(Vector2 delta)
    {
        Position += delta;
    }

    public void UpdateButton(UIButton button)
    {
        bool updatePoints = false;
        Vector2 mouseDelta = Input.GetMouseDelta();
        if (mouseDelta != Vector2.Zero)
        {
            Vector4 offset = button.Offset + (mouseDelta.X, mouseDelta.Y, 0, 0);
            offset = Mathf.Clamp((-10, -10, 0, 0), (Size.X - 10, Size.Y - 10, 0, 0), offset);
            button.Offset = offset;
            button.Align();
            button.UpdateTransformation();
            updatePoints = true;
        }

        int index = ButtonIndex[button];
        if (index == 0 || index == Buttons.Count - 1) 
            return;

        if (mouseDelta.X > 0)
        {
            int nextIndex = index + 1;
            UIButton swapButton = Buttons[index + 1];
            int swapIndex = index + 1;
            bool swap = false;
            while (nextIndex != Buttons.Count - 1) // ignore if it is the last button
            {
                UIButton nextButton = Buttons[nextIndex];
                if (button.Offset.X > nextButton.Offset.X)
                {
                    swapButton = nextButton;
                    swapIndex = nextIndex;
                    swap = true;
                }
                else
                {
                    break;
                }
                nextIndex++;
            }
            if (swap)
            {
                SwapButton(index, swapIndex, button, swapButton);
                updatePoints = true;
            }

        }
        else if (mouseDelta.X < 0)
        {
            int prevIndex = index - 1;
            UIButton swapButton = Buttons[index - 1];
            int swapIndex = index - 1;
            bool swap = false;
            while (prevIndex != 0) // ignore if it is the first button
            {
                UIButton prevButton = Buttons[prevIndex];
                if (button.Offset.X < prevButton.Offset.X)
                {
                    swapButton = prevButton;
                    swapIndex = prevIndex;
                    swap = true;
                }
                else
                {
                    break;
                }
                prevIndex--;
            }
            if (swap)
            {
                SwapButton(index, swapIndex, button, swapButton);
                updatePoints = true;
            }
        }

        if (updatePoints)
        {
            UpdatePoints();
        }
    }

    public void SwapButton(int index1, int index2, UIButton button1, UIButton button2)
    {
        Buttons.Remove(button1);
        Buttons.Insert(index2, button1);
        ButtonIndex[button1] = index2;
        ButtonIndex[button2] = index1;
    }

    public void UpdatePoints()
    {
        Points.Clear();
        foreach (UIButton button in Buttons)
        {
            Vector2 point = new Vector2((button.Offset.X + 10) / Size.X, 1f - ((button.Offset.Y + 10) / Size.Y));
            Points.Add(point);
        }
        _pointSSBO.Update(Points.ToArray(), 0);
        CurveNode?.UpdateCurve(this);
    }

    public UIButton AddButton()
    {
        return AddButton((Size.X / 2 - 10, Size.Y / 2 - 10, 0, 0));
    }

    public UIButton AddButton(Vector4 offset)
    {
        UIButton button = new UIButton("Button", Controller, AnchorType.TopLeft, PositionType.Absolute, (0.7f, 0.7f, 0.7f, 1f), (0, 0, 0), (20, 20), offset, 0, 10, (10, 0.05f), UIState.Interactable);
        int index = 1;
        while (Buttons.Count - 1 > index && Buttons[index].Offset.X < button.Offset.X)
        {
            index++;
        }
        Buttons.Insert(index, button);
        ButtonIndex.Add(button, index);
        button.SetOnClick(() => { Game.SetCursorState(CursorState.Grabbed); SelectedButton = button; });
        button.SetOnHold(() => UpdateButton(button));
        button.SetOnRelease(() => Game.SetCursorState(CursorState.Normal));
        ButtonCollection.AddElement(button);
        return button;
    }

    public void UpdateButtons()
    {
        GenerateButtons();
        NoiseNodeManager.Compile();
        UpdatePoints();
    }

    public void RemoveButton(UIButton button)
    {
        int index = ButtonIndex[button];
        Buttons.Remove(button);
        ButtonIndex.Remove(button);
        for (int i = index; i < Buttons.Count - 1; i++)
        {
            ButtonIndex[Buttons[i]]--;
        }
        button.Delete();
        ButtonCollection.RemoveElement(button);
        Controller.RemoveElement(button);
        GenerateButtons();
    }

    public void GenerateButtons()
    {
        Points.Clear();
        foreach (UIButton button in Buttons)
        {
            Vector2 point = new Vector2((button.Offset.X + 10) / Size.X, 1f - ((button.Offset.Y + 10) / Size.Y));
            Points.Add(point);
        }
        _pointSSBO.Renew(Points);
    }

    public void Resize()
    {
        Controller.Resize();
    }

    public void Update()
    {
        if (!_isHoveringOver)
            return;

        if (Input.IsKeyPressed(Keys.A))
        {
            Controller.AddElement(AddButton());
            UpdateButtons();
        }

        if (Input.IsKeyPressed(Keys.D) && SelectedButton != null)
        {
            RemoveButton(SelectedButton);
            SelectedButton = null;
        }

        _isHoveringOver = false;
    }

    public void Render(Matrix4 modelMatrix, Matrix4 projectionMatrix)
    {
        modelMatrix = ModelMatrix * modelMatrix;
 
        _curvePanelShader.Bind();

        Matrix4 model = modelMatrix;
        Matrix4 projection = projectionMatrix;

        GL.Enable(EnableCap.DebugOutput);

        GL.UniformMatrix4(_modelLocation, true, ref model);
        GL.UniformMatrix4(_projectionLocation, true, ref projection);
        GL.Uniform2(_sizeLocation, Size);
        GL.Uniform1(_pointCountLocation, Points.Count);
        
        _curvePanelVao.Bind();
        _pointSSBO.Bind(0);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

        Shader.Error("Error while drawing curve panel: ");

        _pointSSBO.Unbind();
        _curvePanelVao.Unbind();

        _curvePanelShader.Unbind();
    }

    public void Destroy()
    {
        _pointSSBO.DeleteBuffer();
        Points = [];
        Buttons = [];
        ButtonIndex = [];
        CurveNode = null;
    }
}