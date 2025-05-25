using OpenTK.Mathematics;

public static class GameDataParser
{
    private static Dictionary<string, ParseAction> _currentSaveData = new();

    public static bool Parse(string[] lines)
    {
        SaveData data = new ModelSaveData();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0)
                continue;

            if (values[0].Trim() == "Model")
            {
                _currentSaveData = _modelParseData;
            }
            else if (values[0].Trim() == "Animation")
            {
                _currentSaveData = _animationParseData;
            }
            else if (values[0].Trim() == "Rig")
            {
                _currentSaveData = _rigParseData;
            }

            while (true)
            {
                line = lines[i];
                values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 0)
                    break;

                string key = values[0].Trim();

                if (!_currentSaveData.TryGetValue(key, out ParseAction? action))
                {
                    Console.WriteLine($"Action not found for {key}");
                    return false;
                }

                bool result = action(ref i, line, ref data);
                if (!result)
                {
                    break;
                }
            }
        }

        return true;
    }

    private static readonly Dictionary<string, ParseAction> _rigParseData = new()
    {
        { "Rig", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                data = new RigSaveData();
                return true;
            }
        },
        { "{", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                return true;
            }
        },
        { "Name:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;

                data.Name = values[1];
                return true;
            }
        },
        { "Path:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;
                data.Path = values[1];
                return true;
            }
        },
        { "}", (ref int index, string line, ref SaveData data) =>
            {
                data.Import();
                return false;
            }
        }
    };

    private static readonly Dictionary<string, ParseAction> _animationParseData = new()
    {
        { "Animation", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                data = new AnimationSaveData();
                return true;
            }
        },
        { "{", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                return true;
            }
        },
        { "Name:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;

                data.Name = values[1];
                return true;
            }
        },
        { "Path:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;
                data.Path = values[1];
                return true;
            }
        },
        { "}", (ref int index, string line, ref SaveData data) =>
            {
                data.Import();
                return false;
            }
        }
    };

    private static readonly Dictionary<string, ParseAction> _modelParseData = new()
    {
        { "Model", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                data = new ModelSaveData();
                return true;
            }
        },
        { "{", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                return true;
            }
        },
        { "Name:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;

                data.Name = values[1];
                return true;
            }
        },
        { "Path:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;
                data.Path = values[1];
                return true;
            }
        },
        { "Position:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 4 || !(data is ModelSaveData modelData))
                    return false;

                float x = Float.Parse(values[1]);
                float y = Float.Parse(values[2]);
                float z = Float.Parse(values[3]);
                modelData.Position = new Vector3(x, y, z);
                return true;
            }
        },
        { "Rotation:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 4 || !(data is ModelSaveData modelData))
                    return false;

                float x = Float.Parse(values[1]);
                float y = Float.Parse(values[2]);
                float z = Float.Parse(values[3]);
                modelData.Rotation = new Vector3(x, y, z);
                return true;
            }
        },
        { "Rig:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                if (!(data is ModelSaveData modelData))
                    return false;

                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2)
                    return false;

                modelData.RigName = values[1];
                return true;
            }
        },
        { "Animation:", (ref int index, string line, ref SaveData data) =>
            {
                index++;
                if (!(data is ModelSaveData modelData))
                    return false;

                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length < 2 || values[1].ToLower().Trim() == "null")
                {
                    modelData.Animations.Add(null);
                    return true;
                }
                modelData.Animations.Add(values[1]);
                return true;
            }
        },
        { "}", (ref int index, string line, ref SaveData data) =>
            {
                data.Import();
                return false;
            }
        },
    };

    private delegate bool ParseAction(ref int index, string line, ref SaveData data);
    private abstract class SaveData
    {
        public string Name = "";
        public string Path = "";

        public abstract void Import();
    }

    private class RigSaveData : SaveData
    {
        public override void Import()
        {
            RigManager.DisplayError = false;
            new ImportedRigData(Path, Name).Import();
            RigManager.DisplayError = true;
        }
    }

    private class AnimationSaveData : SaveData
    {
        public override void Import()
        {
            AnimationManager.DisplayError = false;
            new ImportedAnimationData(Path, Name).Import();
            AnimationManager.DisplayError = true;
        }
    }

    private class ModelSaveData : SaveData
    {
        public Vector3 Position = Vector3.Zero;
        public Vector3 Rotation = Vector3.Zero;
        public string? RigName = null;
        public List<string?> Animations = [];

        public override void Import()
        {
            Model model = new Model
            {
                Name = Name,
                Position = Position,
                Rotation = Rotation
            };
            new ImportedModelData(Path, Name).Import(model);
        }
    }
}