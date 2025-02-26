using System.Diagnostics;

public static class Timer
{
    private static UIController _uiTimes = new();
    private static UIVerticalCollection _timesCollection = new("Times", AnchorType.TopRight, PositionType.Absolute, (0, 0, 0), (100, 1000), (-5, 5, 5, 5), (0, 0, 0, 0), 5, 0);
    private static List<(UIText, UIText)> _timesPool = new List<(UIText, UIText)>();
    private static List<(string, double)> _times = new List<(string, double)>();
    private static TextMesh _textMesh = _uiTimes.textMesh;
    private static int _timerIndex = 0;
    private static Stopwatch _stopwatch = new();

    private static double _oldTime = 0;
    private static double _previousTime = 0;
    private static double _currentTime = 0;

    private static double _time = 0;

    public static void Start()
    {
        _stopwatch.Start();
        _uiTimes.AddElement(_timesCollection);
        _uiTimes.GenerateBuffers();
    }

    public static void Reset()
    {
        _oldTime = _stopwatch.Elapsed.TotalMilliseconds;
        _previousTime = 0;
        _currentTime = 0;
        _times.Clear();
        _timerIndex = 0;
    }

    public static void Update()
    {
        if (_time + 1 < _stopwatch.Elapsed.TotalSeconds)
        {
            for (int i = 0; i < _times.Count; i++)
            {
                string name = _times[i].Item1;
                double time = _times[i].Item2;

                GetUIText(i).Item1.SetText(name).GenerateChars();
                GetUIText(i).Item2.SetText(time.ToString()).GenerateChars();
            }

            _time = _stopwatch.Elapsed.TotalSeconds;
            _textMesh.UpdateText();
        }
        Reset();
    }

    public static void Render()
    {
        _uiTimes.Render();
    }

    public static void Stop()
    {
        _stopwatch.Stop();
    }
    
    public static void DisplayTime(string name)
    {   
        _currentTime = _stopwatch.Elapsed.TotalMilliseconds - _oldTime;
        double time = _currentTime - _previousTime;
        _previousTime = _currentTime;
        _times.Add((name, time));
        _timerIndex++;
    }

    private static (UIText, UIText) GetUIText(int index)
    {
        if (index >= _timesPool.Count)
        {
            _uiTimes.Clear();

            UIText textName = new($"Name {index}", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (0, 0, 0, 0), 0, 0 ,(0, 0), _textMesh);
            UIText textTime = new($"Time {index}", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, 20), (0, 0, 0, 0), 0, 0 ,(0, 0), _textMesh);

            textName.SetMaxCharCount(10).SetText("Name", 0.5f).GenerateChars();
            textTime.SetMaxCharCount(10).SetText("0.000000000", 0.5f).GenerateChars();

            UIHorizontalCollection text = new($"Text {index}", AnchorType.TopLeft, PositionType.Relative, (0, 0, 0), (100, textName.Scale.Y), (0, 0, 0, 0), (0, 0, 0, 0), 5, 0);

            text.AddElement(textName, textTime);
            _timesCollection.SetScale((textName.Scale.X + 5 + textTime.Scale.X, 1000));
            _timesCollection.AddElement(text);
            _timesCollection.ResetInit();

            _uiTimes.AddElement(_timesCollection);
            _uiTimes.GenerateBuffers();

            _timesPool.Insert(index, (textName, textTime));
            return (textName, textTime);
        }

        return _timesPool[index];
    }
}