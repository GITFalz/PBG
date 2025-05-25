using System.Diagnostics.CodeAnalysis;
using OpenTK.Mathematics;

public static class AnimationParser
{
    public static Animation currentAnimation = null!;
    public static BoneAnimation? currentBoneAnimation;

    public static bool Parse(string name, string[] lines, [NotNullWhen(true)] out Animation? animation)
    {
        animation = null;
        currentAnimation = new Animation(name);
        currentBoneAnimation = null;

        AnimationData data = new AnimationData();

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
            if (values.Length == 0)
                continue;

            while (true)
            {
                line = lines[i];
                values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length == 0)
                    break;

                string key = values[0].Trim();

                if (!_currentParseAction.TryGetValue(key, out ParseAction? action))
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

        animation = currentAnimation;
        return true;
    }
    
    private delegate bool ParseAction(ref int index, string line, ref AnimationData data);
    private static readonly Dictionary<string, ParseAction> _boneData = new()
    {
        { "Bone:", (ref int index, string line, ref AnimationData data) =>
            {
                index++;
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 2)
                    return false;

                string name = values[1];
                currentBoneAnimation = new BoneAnimation(name);
                return true;
            }
        },
        { "{", (ref int index, string line, ref AnimationData data) =>
            {
                index++;
                return true;
            }
        },
        { "Keyframe:", (ref int index, string line, ref AnimationData data) =>
            {
                index++;
                _currentParseAction = _keyframeData;
                data.Clear();
                return true;
            }
        },
        { "}", (ref int index, string line, ref AnimationData data) =>
            {
                if (currentBoneAnimation != null)
                    currentAnimation.AddBoneAnimation(currentBoneAnimation);
                return false;
            }
        }
    };

    private static readonly Dictionary<string, ParseAction> _keyframeData = new()
    {
        { "Keyframe:", (ref int index, string line, ref AnimationData data) =>
            {
                index++;
                return true;
            }
        },
        { "{", (ref int index, string line, ref AnimationData data) =>
            {
                index++;
                return true;
            }
        },
        { "Position:", (ref int index, string line, ref AnimationData data) =>
            {
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 4)
                    return false;

                data.Position = new Vector3(Float.Parse(values[1]), Float.Parse(values[2]), Float.Parse(values[3]));
                index++;
                return true;
            }
        },
        { "Rotation:", (ref int index, string line, ref AnimationData data) =>
            {
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 4)
                    return false;

                data.Rotation = Quaternion.FromEulerAngles(Float.Parse(values[1]), Float.Parse(values[2]), Float.Parse(values[3]));
                index++;
                return true;
            }
        },
        { "Scale:", (ref int index, string line, ref AnimationData data) =>
            {
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 2)
                    return false;

                data.Scale = Float.Parse(values[1]);
                index++;
                return true;
            }
        },
        { "Index:", (ref int index, string line, ref AnimationData data) =>
            {
                var values = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (values.Length != 2)
                    return false;

                data.Index = Int.Parse(values[1]);
                index++;
                return true;
            }
        },
        { "}", (ref int index, string line, ref AnimationData data) =>
            {
                index++;
                if (currentBoneAnimation != null)
                    currentBoneAnimation.AddOrUpdateKeyframe(new AnimationKeyframe(data.Index, data.Position, data.Rotation, data.Scale));

                _currentParseAction = _boneData;
                return true;
            }
        }
    };

    private static Dictionary<string, ParseAction> _currentParseAction = _boneData;

    private struct AnimationData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public float Scale;
        public int Index;

        public AnimationData()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = 1.0f;
            Index = 0;
        }

        public Matrix4 GetLocalMatrix()
        {
            return Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rotation) * Matrix4.CreateTranslation(Position);
        }

        public void Clear()
        {
            Position = Vector3.Zero;
            Rotation = Quaternion.Identity;
            Scale = 1.0f;
            Index = 0;
        }
    }
}