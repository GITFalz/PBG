using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class ThreadPool
{
    private static readonly int MAX_THREAD_COUNT = Environment.ProcessorCount;
    public static int ThreadCount { get; private set; } = 4;

    private static PriorityQueue _actions = new PriorityQueue();
    private static readonly ConcurrentBag<Task> _runningTasks = new ConcurrentBag<Task>();

    public static void SetThreadCount(int count)
    {
        ThreadCount = Math.Clamp(count, 1, MAX_THREAD_COUNT);
    }

    public static void QueueAction(Func<Task> action, TaskPriority priority = TaskPriority.Normal)
    {
        _actions.Enqueue(action, priority);
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
                _runningTasks.TryTake(out task);
            }
        }
    }

    private class PriorityQueue 
    {
        private readonly ConcurrentQueue<Func<Task>> _highPriorityActions = new ConcurrentQueue<Func<Task>>();
        private readonly ConcurrentQueue<Func<Task>> _normalPriorityActions = new ConcurrentQueue<Func<Task>>();
        private readonly ConcurrentQueue<Func<Task>> _lowPriorityActions = new ConcurrentQueue<Func<Task>>();

        public void Enqueue(Func<Task> action, TaskPriority priority = TaskPriority.Normal)
        {
            switch (priority)
            {
                case TaskPriority.Low:
                    _lowPriorityActions.Enqueue(action);
                    break;
                case TaskPriority.Normal:
                    _normalPriorityActions.Enqueue(action);
                    break;
                case TaskPriority.High:
                    _highPriorityActions.Enqueue(action);
                    break;
            }
        }

        public bool TryDequeue(out Func<Task> action)
        {
            action = () => { return Task.CompletedTask; };
            if (_highPriorityActions.TryDequeue(out var a))
            {
                action = a;
                return true;
            }
            if (_normalPriorityActions.TryDequeue(out a))
            {
                action = a;
                return true;
            }
            if (_lowPriorityActions.TryDequeue(out a))
            {
                action = a;
                return true;
            }
            return false;
        }
    }
}

public enum TaskPriority
{
    Low,
    Normal,
    High
}