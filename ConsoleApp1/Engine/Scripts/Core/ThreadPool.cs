using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

public static class ThreadPool
{
    private static readonly int MAX_THREAD_COUNT = Environment.ProcessorCount;
    public static int ThreadCount { get; private set; } = 4;

    private static PriorityQueue _actions = new PriorityQueue();
    private static int _currentTaskCount = 0;

    private static List<Task?> Pool = [];

    static ThreadPool()
    {
        SetThreadCount(ThreadCount);
    }

    public static void SetThreadCount(int count)
    {
        ThreadCount = Math.Clamp(count, 1, MAX_THREAD_COUNT);
        Pool.Clear();
        for (int i = 0; i < ThreadCount; i++)
        {
            Pool.Add(null);
        }
    }

    public static void QueueAction(Action action, TaskPriority priority = TaskPriority.Normal)
    {
        ActionProcess actionProcess = new ActionProcess(action);
        _actions.Enqueue(actionProcess, priority);
    }

    public static void QueueAction(ThreadProcess process, TaskPriority priority = TaskPriority.Normal)
    {
        _actions.Enqueue(process, priority);
    }

    public static void Update()
    {
        ProcessTasks();
    }

    private static void ProcessTasks()
    {
        for (int i = 0; i < ThreadCount; i++)
        {
            if (Pool[i] == null || Pool[i]!.IsCompleted)
            {
                if (_actions.TryDequeue(out var process))
                {
                    process.SetThreadIndex(i);
                    var task = process.ExecuteAsync().ContinueWith(t =>
                    {
                        Pool[i] = null;
                    });

                    Pool[i] = task;
                }
            }
        }
    }

    private class ActionProcess : ThreadProcess
    {
        private readonly Action _action;
        public ActionProcess(Action action)
        {
            _action = action;
        }
        protected override void Function()
        {
            _action.Invoke();
        }
    }

    private class PriorityQueue 
    {
        public int Count => _urgentPriorityActions.Count + _highPriorityActions.Count + _normalPriorityActions.Count + _lowPriorityActions.Count + _backgroundPriorityActions.Count;
        
        public readonly ConcurrentQueue<ThreadProcess> _urgentPriorityActions = [];
        public readonly ConcurrentQueue<ThreadProcess> _highPriorityActions = [];
        public readonly ConcurrentQueue<ThreadProcess> _normalPriorityActions = [];
        public readonly ConcurrentQueue<ThreadProcess> _lowPriorityActions = [];
        public readonly ConcurrentQueue<ThreadProcess> _backgroundPriorityActions = [];

        public void Enqueue(ThreadProcess process, TaskPriority priority = TaskPriority.Normal)
        {
            switch (priority)
            {
                case TaskPriority.Background:
                    _backgroundPriorityActions.Enqueue(process);
                    break;
                case TaskPriority.Low:
                    _lowPriorityActions.Enqueue(process);
                    break;
                case TaskPriority.Normal:
                    _normalPriorityActions.Enqueue(process);
                    break;
                case TaskPriority.High:
                    _highPriorityActions.Enqueue(process);
                    break;
                case TaskPriority.Urgent:
                    _urgentPriorityActions.Enqueue(process);
                    break;
            }
        }

        public bool TryDequeue([NotNullWhen(true)] out ThreadProcess? process)
        {
            process = null;
            if (_urgentPriorityActions.TryDequeue(out var a) ||
                _highPriorityActions.TryDequeue(out a) ||
                _normalPriorityActions.TryDequeue(out a) ||
                _lowPriorityActions.TryDequeue(out a) ||
                _backgroundPriorityActions.TryDequeue(out a))
            {
                process = a;
                return true;
            }
            return false;
        }
    }

    public static void Print()
    {
        Console.WriteLine($"ThreadPool: {ThreadCount} threads, {_actions.Count} actions queued.");
        Console.WriteLine($"Urgent: {_actions._urgentPriorityActions.Count}, High: {_actions._highPriorityActions.Count}, Normal: {_actions._normalPriorityActions.Count}, Low: {_actions._lowPriorityActions.Count}, Background: {_actions._backgroundPriorityActions.Count}");
    }
}

public class ThreadProcess
{
    public int ThreadIndex { get; private set; } = -1;

    public void SetThreadIndex(int index) => ThreadIndex = index;

    public Task ExecuteAsync()
    {
        return Task.Run(Function);
    }

    protected virtual void Function() { }
}

public enum TaskPriority
{
    Background,
    Low,
    Normal,
    High,
    Urgent,
}