using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class ThreadPool
{
    private static readonly int MAX_THREAD_COUNT = Environment.ProcessorCount;
    public static int ThreadCount { get; private set; } = 4;

    private static readonly ConcurrentQueue<Func<Task>> _actions = new ConcurrentQueue<Func<Task>>();
    private static readonly List<Task> _runningTasks = new List<Task>();

    public static void SetThreadCount(int count)
    {
        ThreadCount = Math.Clamp(count, 1, MAX_THREAD_COUNT);
    }

    public static void QueueAction(Func<Task> action)
    {
        _actions.Enqueue(action);
    }

    public static void Update()
    {
        ProcessTasks();
    }

    private static async void ProcessTasks()
    {
        for (int i = _runningTasks.Count; i < ThreadCount - 1; i++)
        {
            if (_actions.TryDequeue(out var action))
            {
                var task = action();
                _runningTasks.Add(task);
                await task;
                _runningTasks.Remove(task);
            }
        }
    }
}