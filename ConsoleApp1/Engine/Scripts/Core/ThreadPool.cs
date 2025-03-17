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
    private static int _currentTaskCount = 0;

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
        while (_currentTaskCount < ThreadCount - 1)
        {
            if (_actions.TryDequeue(out var action))
            {
                Interlocked.Increment(ref _currentTaskCount);
                try
                {
                    await action();
                }
                finally
                {
                    Interlocked.Decrement(ref _currentTaskCount);
                }
            }
            else
            {
                break;
            }
        }
    }

    private class PriorityQueue 
    {
        private readonly ConcurrentQueue<Func<Task>> _urgentPriorityActions = [];
        private readonly ConcurrentQueue<Func<Task>> _highPriorityActions = [];
        private readonly ConcurrentQueue<Func<Task>> _normalPriorityActions = [];
        private readonly ConcurrentQueue<Func<Task>> _lowPriorityActions = [];
        private readonly ConcurrentQueue<Func<Task>> _backgroundPriorityActions = [];

        public void Enqueue(Func<Task> action, TaskPriority priority = TaskPriority.Normal)
        {
            switch (priority)
            {
                case TaskPriority.Background:
                    _backgroundPriorityActions.Enqueue(action);
                    break;
                case TaskPriority.Low:
                    _lowPriorityActions.Enqueue(action);
                    break;
                case TaskPriority.Normal:
                    _normalPriorityActions.Enqueue(action);
                    break;
                case TaskPriority.High:
                    _highPriorityActions.Enqueue(action);
                    break;
                case TaskPriority.Urgent:
                    _urgentPriorityActions.Enqueue(action);
                    break;
            }
        }

        public bool TryDequeue(out Func<Task> action)
        {
            action = () => { return Task.CompletedTask; };
            if (_urgentPriorityActions.TryDequeue(out var a) ||
                _highPriorityActions.TryDequeue(out a) ||
                _normalPriorityActions.TryDequeue(out a) ||
                _lowPriorityActions.TryDequeue(out a) ||
                _backgroundPriorityActions.TryDequeue(out a))
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
    Background,
    Low,
    Normal,
    High,
    Urgent,
}