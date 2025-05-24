using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class FileManager : ScriptingNode
{
    public UIController UIController;
    public UIScrollView PathScrollView;
    public UIInputField PathInput;
    private int _pathCharacterCount => PathInput.Text.TrimEnd().Length;
    private float _charWidth = 0;

    public UIController FilesUIController;
    public UIScrollView UIScrollView;

    public Vector3 Position
    {
        get => _position;
        set
        {
            _position = value;
            UIController.SetPosition(value);
            FilesUIController.SetPosition(value);
        }
    }
    private Vector3 _position = new Vector3(0, 0, 0);

    public string DefaultPath;

    public List<string> SelectedPaths = [];

    private List<string> CurrentPaths = [];
    private List<UIButton> _buttons = [];

    private bool _regen = false;
    private string _wantedPath;
    private bool _doubleClick = false;
    private bool _clickedButton = false;
    private int _clickedIndex = -1;
    
    private float _timer = -1;
    private int _start = -1;
    private int _end = -1;

    public bool IsVisible { get => _visible; }
    private bool _visible = false;

    public FileManager()
    {
        UIController = new UIController();
        FilesUIController = new UIController();
        DefaultPath = Game.mainPath;
        _wantedPath = DefaultPath;

        UICollection backgroundCollection = new UICollection("FileBackgroundCollection", UIController, AnchorType.TopLeft, PositionType.Absolute, (0, 0, 0), (800, 500), (0, 0, 0, 0), 0);

        UIButton moveButton = new UIButton("MoveButton", UIController, AnchorType.TopLeft, PositionType.Relative, (0.65f, 0.65f, 0.65f, 1f), (0, 0, 0), (800, 20), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
        moveButton.SetOnHold(() =>
        {
            Vector2 mouseDelta = Input.GetMouseDelta();
            if (mouseDelta == Vector2.Zero)
                return;

            Position += new Vector3(mouseDelta.X, mouseDelta.Y, 0);
        });

        UIImage background = new UIImage("FileBackground", UIController, AnchorType.ScaleFull, PositionType.Relative, (0.65f, 0.65f, 0.65f, 1f), (0, 0, 0), (800, 500), (0, 20, 0, 0), 0, 10, (7.5f, 0.05f));

        UICollection pathCollection = new UICollection("PathCollection", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (790, 25), (5, 25, 0, 0), 0);

        UIImage pathBackground = new UIImage("PathBackground", UIController, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (800, 500), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        PathScrollView = new UIScrollView("PathScrollView", UIController, AnchorType.TopLeft, PositionType.Relative, CollectionType.Horizontal, (775, 25), (5, 0, 0, 0));
        PathInput = new UIInputField("PathInput", UIController, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (800, 500), (5, 0, 0, 0), 0, 0, (0, 0));
        PathInput.SetMaxCharCount(300).SetText(DefaultPath, 0.8f).TextType = TextType.All;
        _charWidth = PathInput.Scale[0] / 300f;

        PathScrollView.AddElement(PathInput);

        pathCollection.AddElements(pathBackground, PathScrollView);

        UICollection manageCollection = new UICollection("ManageCollection", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (190, 460), (5, 55, 0, 0), 0);

        UIImage manageBackground = new UIImage("ManageBackground", UIController, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (800, 500), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIVerticalCollection manageVerticalCollection = new UIVerticalCollection("ManageVerticalCollection", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (190, 460), (5, 5, 0, 0), (0, 0, 0, 0), 2, 0);

        UITextButton backButton = new UITextButton("BackButton", UIController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (180, 25), (5, 0, 0, 0), 0, 10, (7.5f, 0.05f));
        backButton.SetText("Back", 0.8f);
        backButton.SetOnClick(() =>
        {
            _wantedPath = Path.GetDirectoryName(GetPath()) ?? DefaultPath;
            _regen = true;
        });

        manageVerticalCollection.AddElement(backButton.Collection);

        manageCollection.AddElements(manageBackground, manageVerticalCollection);

        UICollection folderCollection = new UICollection("FolderCollection", UIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (595, 460), (200, 55, 0, 0), 0);

        UIImage folderBackground = new UIImage("FolderBackground", UIController, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (800, 500), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));

        folderCollection.AddElement(folderBackground);

        backgroundCollection.AddElements(moveButton, background, pathCollection, manageCollection, folderCollection);

        UIController.AddElement(backgroundCollection);

        PathInput.SetOnTextChange(() =>
        {
            if (_pathCharacterCount > 94)
            {
                PathScrollView.ScrollPosition = (95 - _pathCharacterCount) * _charWidth + 5;
                PathScrollView.SubElements.Align();
                PathScrollView.SubElements.UpdateTransformation();
            }
        });

        Position = (200, 200, 0);
    }

    public string[] GetSelectedFiles()
    {
        return SelectedPaths.ToArray();
    }

    public void ToggleOn()
    {
        if (_visible)
            return;

        _wantedPath = DefaultPath;
        _regen = true;
        _visible = true;
    }

    public void ToggleOff()
    {
        if (!_visible)
            return;

        _visible = false;
    }

    public void ClearElements()
    {
        UIScrollView.Delete();
    }

    public void GenerateElements(string path)
    {
        CurrentPaths = [];
        _buttons = [];

        string[] directories;
        string[] files;

        try
        {
            directories = Directory.GetDirectories(path);
            files = Directory.GetFiles(path);
        }
        catch (Exception e)
        {
            return;
        }

        Array.Sort(directories);
        Array.Sort(files);

        List<string> allFiles = [];

        for (int i = 0; i < directories.Length; i++)
        {
            string? directoryName = Path.GetFileName(directories[i]);
            if (directoryName != null)
            {
                CurrentPaths.Add(directories[i]);
                allFiles.Add(directoryName);
            }
        }

        int directoryCount = allFiles.Count;

        for (int i = 0; i < files.Length; i++)
        {
            string? fileName = Path.GetFileName(files[i]);
            if (fileName != null)
            {
                CurrentPaths.Add(files[i]);
                allFiles.Add(fileName);
            }
        }

        int elementCount = allFiles.Count;

        UIScrollView = new UIScrollView("FileVerticalCollection", FilesUIController, AnchorType.TopLeft, PositionType.Absolute, CollectionType.Vertical, (595, 450), (200, 60, 0, 0));
        UIScrollView.SetBorder((5, 5, 5, 5));
        UIScrollView.SetScrollSpeed(10f);

        for (int i = 0; i < elementCount; i++)
        {
            bool isDirectory = i < directoryCount;
            string newPath = CurrentPaths[i];
            int index = i;

            UICollection fileCollection = new UICollection("FileCollection" + i, FilesUIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 25), (100, 0, 0, 0), 0);

            UIButton fileButton = new UIButton("FileButton" + i, FilesUIController, AnchorType.TopLeft, PositionType.Relative, UINoiseNodePrefab.SELECTION_COLOR, (0, 0, 0), (300, 25), (0, 0, 0, 0), 0, 12, (7.5f, 0.05f), UIState.Interactable);
            _buttons.Add(fileButton);
            fileButton.SetVisibility(false);
            fileButton.CanTest = true;

            UICollection nameCollection = new UICollection("NameCollection" + i, FilesUIController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (300, 25), (0, 0, 0, 0), 0);

            UIImage fileBackground = new UIImage("FileBackground" + i, FilesUIController, AnchorType.TopLeft, PositionType.Relative, isDirectory ? (0.4f, 0.5f, 0.7f, 1f) : (0.2f, 0.7f, 0.6f, 1f), (0, 0, 0), (25, 25), (0, 0, 0, 0), 0, isDirectory ? 90 : 91, (-1, -1));
            UIText fileText = new UIText("FileText" + i, FilesUIController, AnchorType.MiddleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (100, 20), (30, 0, 0, 0), 0);
            fileText.SetMaxCharCount(50).SetText(allFiles[i], 0.8f);

            nameCollection.AddElements(fileBackground, fileText);

            fileCollection.AddElements(fileButton, nameCollection);

            UIScrollView.AddElement(fileCollection);

            fileButton.SetOnClick(() =>
            {
                _clickedButton = true;
                if (_doubleClick && isDirectory && _clickedIndex == index)
                {
                    _wantedPath = newPath;
                    _regen = true;
                    _doubleClick = false;
                }

                if (Input.LeftShiftDown)
                {
                    if (_start == -1)
                    {
                        _start = index;
                        _end = index + 1;
                    }
                    else
                    {
                        _end = index + 1;
                    }

                    SetSelected(_start, _end);
                }
                else if (Input.LeftControlDown)
                {
                    if (index < _start)
                        _start = index;
                    else
                        _end = index + 1;
                    AddSelection(index);
                }
                else
                {
                    SetSelected(index, index + 1);
                    _start = index;
                    _end = index + 1;
                }
                _clickedIndex = index;
            });
        }

        FilesUIController.AddElement(UIScrollView);

        PathInput.SetText(path, 0.8f).UpdateCharacters();
    }

    public void SetNoSelection()
    {
        SelectedPaths = [];
        for (int i = 0; i < _buttons.Count; i++)
        {
            UIButton button = _buttons[i];
            button.SetVisibility(false);
            button.CanTest = true;
        }
    }

    public void SetSelected(int start, int end)
    {
        if (end <= start)
        {
            (end, start) = (start, end);
            start--; end++;
        }

        SetNoSelection();
        for (int i = start; i < end; i++)
        {
            if (i < 0 || i >= _buttons.Count)
                continue;

            UIButton button = _buttons[i];
            button.SetVisibility(true);
            SelectedPaths.Add(CurrentPaths[i]);
        }
    }

    public void AddSelection(int index)
    {
        if (index < 0 || index >= _buttons.Count)
            return;

        UIButton button = _buttons[index];
        button.SetVisibility(!button.Visible);
        if (button.Visible)
            SelectedPaths.Add(CurrentPaths[index]);
        else
            SelectedPaths.Remove(CurrentPaths[index]);
    }

    public void RemoveSelection(int index)
    {
        if (index < 0 || index >= _buttons.Count)
            return;

        UIButton button = _buttons[index];
        button.SetVisibility(false);
        SelectedPaths.Remove(CurrentPaths[index]);
    }

    public string GetPath()
    {
        string[] paths = PathInput.Text.Split(['\'', '/']);
        if (paths.Length == 0)
            return DefaultPath;


        string path = paths[0];
        for (int i = 1; i < paths.Length; i++)
        {
            string p = Path.Combine(path, paths[i]);
            if (i == paths.Length - 1 && File.Exists(p)) // return only the folder path
                break;

            path = p;
        }

        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? '/' + path : path;
    }

    void Awake()
    {
        GenerateElements(DefaultPath);
    }

    void Resize()
    {
        UIController.Resize();
        FilesUIController.Resize();
    }

    void Update()
    {
        if (!_visible)
            return;

        if (Input.IsKeyPressed(Keys.Enter))
        {
            string path = GetPath();
            if (!Directory.Exists(path))
                return;

            _wantedPath = path;
            _regen = true;
        }

        if (Input.IsKeyAndControlPressed(Keys.B))
        {
            FilesUIController.PrintMemory();
        }

        bool left = Input.IsMousePressed(MouseButton.Left);

        if (left)
        {
            if (_timer < 0)
            {
                _doubleClick = false;
                _timer = 0.3f;
            }
            else if (_timer >= 0)
            {
                _doubleClick = true;
                _timer = -1;
            }
        }

        if (_timer >= 0)
        {
            _timer -= GameTime.DeltaTime;
        }

        UIController.Update();
        FilesUIController.Update();

        if (_regen)
        {
            ClearElements();
            GenerateElements(_wantedPath);
            _regen = false;
        }
        else if (left && !_clickedButton)
        {
            SetNoSelection();
        }
        
        _clickedButton = false;
    }

    void Render()
    {
        if (!_visible)
            return;

        UIController.RenderNoDepthTest();
        FilesUIController.RenderNoDepthTest();
    }

    void Exit()
    {
        ClearElements();
    }
}