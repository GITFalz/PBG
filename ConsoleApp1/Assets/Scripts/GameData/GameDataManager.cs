using OpenTK.Windowing.GraphicsLibraryFramework;
using SharpGen.Runtime.Win32;

public class GameDataManager : ScriptingNode
{
    public UIController GameDataController;
    public UIScrollView SelectionScrollView;
    public FileManager FileManager;

    public UIScrollView ImportedScrollView;
    public UIInputField ImportNameField;
    
    public List<ImportedFileData> ImportedFiles = new List<ImportedFileData>();
    public ImportedFileData? SelectedFileData;

    public GameDataManager(FileManager fileManager)
    {
        GameDataController = new UIController();
        FileManager = fileManager;

        UICollection mainCollection = new UICollection("MainCollection", GameDataController, AnchorType.ScaleFull, PositionType.Absolute, (0, 0, 0), (Game.Width, Game.Height), (0, 0, 0, 0), 0);

        UICollection selectionCollection = new UICollection("SelectionCollection", GameDataController, AnchorType.ScaleLeft, PositionType.Relative, (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0);

        UIImage selectionBackground = new UIImage("SelectionBackground", GameDataController, AnchorType.ScaleLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        SelectionScrollView = new UIScrollView("SelectionScrollView", GameDataController, AnchorType.ScaleLeft, PositionType.Relative, CollectionType.Vertical, (290, Game.Height - 10), (5, 5, 0, 0));

        selectionCollection.AddElements(selectionBackground, SelectionScrollView);


        UICollection centerCollection = new UICollection("CenterCollection", GameDataController, AnchorType.ScaleFull, PositionType.Relative, (0, 0, 0), (Game.Width, Game.Height), (300, 0, 300, 0), 0);

        UIImage centerBackground = new UIImage("CenterBackground", GameDataController, AnchorType.ScaleFull, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (Game.Width - 300, Game.Height), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));

        // Import info panel
        UICollection centerImportInfoCollection = new UICollection("CenterImportInfoCollection", GameDataController, AnchorType.ScaleFull, PositionType.Relative, (0, 0, 0), (Game.Width - 300, Game.Height), (20, 20, 20, 20), 0);

        UICollection importNameFieldCollection = new UICollection("ImportNameFieldCollection", GameDataController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (290, 25), (0, 0, 0, 0), 0);
        UIImage importFieldBackground = new UIImage("ImportFieldBackground", GameDataController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (290, 25), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        ImportNameField = new UIInputField("ImportNameField", GameDataController, AnchorType.MiddleCenter, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (290, 25), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f));
        ImportNameField.SetMaxCharCount(50).SetText("Import Name", 1f);
        ImportNameField.SetOnTextChange(() =>
        {
            if (SelectedFileData == null)
                return;

            SelectedFileData.Name = ImportNameField.Text;
        });
        importFieldBackground.SetScale(ImportNameField.Scale + (14, 14));
        importNameFieldCollection.SetScale(ImportNameField.Scale + (14, 14));

        UITextButton confirmButton = new UITextButton("ConfirmButton", GameDataController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (290, 25), (0, 30, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
        confirmButton.SetTextCharCount("Import", 1f);
        confirmButton.SetOnClick(() =>
        {
            if (SelectedFileData == null)
                return;

            SelectedFileData.Import();
            PopUp.AddPopUp("Import Successful");
        });

        UITextButton deleteImportButton = new UITextButton("DeleteImportButton", GameDataController, AnchorType.BottomLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (290, 25), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
        deleteImportButton.SetTextCharCount("Delete", 1f);

        importNameFieldCollection.AddElements(importFieldBackground, ImportNameField, confirmButton.Collection);

        centerImportInfoCollection.AddElements(importNameFieldCollection, deleteImportButton.Collection);

        centerCollection.AddElements(centerBackground, centerImportInfoCollection);


        UICollection importCollection = new UICollection("ImportCollection", GameDataController, AnchorType.ScaleRight, PositionType.Relative, (0, 0, 0), (300, Game.Height), (0, 0, 0, 0), 0);

        UIImage importBackground = new UIImage("ImportBackground", GameDataController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, 210), (0, 0, 0, 0), 0, 11, (7.5f, 0.05f));
        UIVerticalCollection importButtons = new UIVerticalCollection("ImportButtons", GameDataController, AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (290, 200), (5, 5, 0, 0), (0, 0, 0, 0), 0, 0);

        UITextButton importButton = new UITextButton("ImportButton", GameDataController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (290, 25), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
        importButton.SetTextCharCount("Import", 1f);
        importButton.SetOnClick(() =>
        {
            if (FileManager.IsVisible)
            {
                FileManager.ToggleOff();
            }
            else
            {
                FileManager.Position = (0, 0, 0);
                FileManager.ToggleOn();
            }
        });

        UIImage importedBackground = new UIImage("ImportedBackground", GameDataController, AnchorType.ScaleRight, PositionType.Relative, (0.5f, 0.5f, 0.5f, 1f), (0, 0, 0), (300, Game.Height - 210), (0, 210, 0, 0), 0, 11, (7.5f, 0.05f));
        ImportedScrollView = new UIScrollView("ImportedScrollView", GameDataController, AnchorType.ScaleRight, PositionType.Relative, CollectionType.Vertical, (290, Game.Height - 220), (-5, 215, 5, 0));
        ImportedScrollView.SetSpacing(0);

        importButtons.AddElements(importButton);

        importCollection.AddElements(importBackground, importedBackground, importButtons, ImportedScrollView);

        mainCollection.AddElements(selectionCollection, centerCollection, importCollection);

        GameDataController.AddElement(mainCollection);


        // Set up here because of null dereference error
        deleteImportButton.SetOnClick(() =>
        {
            if (SelectedFileData == null || SelectedFileData.Button == null)
                return;

            ImportedFiles.Remove(SelectedFileData);
            SelectedFileData.Button.Collection.Delete();
            ImportedScrollView.ResetInit();
            ImportedScrollView.QueueUpdateTransformation();
            ImportNameField.SetText("Import Name").UpdateCharacters();
            SelectedFileData = null;
        });
    }

    public void ImportFiles()
    {
        if (!FileManager.IsVisible)
            return;

        string[] files = FileManager.GetSelectedFiles();

        foreach (var file in files)
        {
            if (!File.Exists(file))
                continue;

            string fileName = Path.GetFileName(file).Split('.')[0];
            string fileExtension = Path.GetExtension(file);

            ImportedFileData importedFileData;

            switch (fileExtension)
            {
                case ".model":
                    importedFileData = new ImportedModelData(file, fileName, null);
                    break;
                case ".rig":
                    importedFileData = new ImportedRigData(file, fileName, null);
                    break;
                case ".anim":
                    importedFileData = new ImportedAnimationData(file, fileName, null);
                    break;
                default:
                    continue; // Skip unsupported file types
            }

            UITextButton fileButton = new UITextButton(fileName, GameDataController, AnchorType.TopLeft, PositionType.Relative, (0.5f, 0.5f, 0.5f), (0, 0, 0), (290, 25), (0, 0, 0, 0), 0, 10, (7.5f, 0.05f), UIState.Interactable);
            fileButton.SetTextCharCount(fileName, 1f);
            fileButton.SetOnClick(() =>
            {
                SelectedFileData = importedFileData;
                UpdateImportedFileInfo();
            });

            ImportedScrollView.AddElement(fileButton.Collection);
            GameDataController.AddElement(fileButton.Collection);
            
            ImportedFiles.Add(importedFileData);
            importedFileData.Button = fileButton;
        }

        ImportedScrollView.ResetInit();

        FileManager.ToggleOff();
    }

    private void UpdateImportedFileInfo()
    {
        if (SelectedFileData == null)
            return;

        ImportNameField.SetText(SelectedFileData.Name).UpdateCharacters();
    }

    void Start()
    {

    }

    void Awake()
    {

    }

    void Resize()
    {
        GameDataController.Resize();
    }

    void Update()
    {
        GameDataController.Update();

        if (Input.IsKeyPressed(Keys.Enter))
        {
            ImportFiles();
        }
    }

    void Render()
    {
        GameDataController.RenderNoDepthTest();
    }

    void Exit()
    {

    }
}

public abstract class ImportedFileData
{
    public string Path;
    public string Name;
    public UITextButton? Button;

    public ImportedFileData(string path, string name, UITextButton? button)
    {
        Path = path;
        Name = name;
        Button = button;
    }

    public abstract void Import();
}

public class ImportedModelData : ImportedFileData
{
    public ImportedModelData(string path, string name, UITextButton? button) : base(path, name, button) { }
    public override void Import()
    {
        Model model = new Model();
        model.LoadModelFromPath(Path);
        model.Name = Name;
        GameData.ModelSaveData modelSaveData = new GameData.ModelSaveData(model, Path);
        GameData.Add(modelSaveData);
    }
}

public class ImportedRigData : ImportedFileData
{
    public ImportedRigData(string path, string name, UITextButton? button) : base(path, name, button) { }
    public override void Import()
    {
        if (!Rig.LoadFromPath(Path, out Rig? rig)) 
            return;

        rig.Name = Name; 
        GameData.RigSaveData rigSaveData = new GameData.RigSaveData(rig, Path);
        GameData.Add(rigSaveData);
    }
}

public class ImportedAnimationData : ImportedFileData
{
    public ImportedAnimationData(string path, string name, UITextButton? button) : base(path, name, button) { }
    public override void Import()
    {
        if (!Animation.LoadFromPath(Path, out Animation? animation))
            return;

        animation.Name = Name;
        GameData.AnimationSaveData animationSaveData = new GameData.AnimationSaveData(animation, Path);
        GameData.Add(animationSaveData);
    }
}