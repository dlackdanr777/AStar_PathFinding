
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class PathfindingQueue : MonoBehaviour
{

    public static PathfindingQueue Instance => _instance;
    private static PathfindingQueue _instance;

    private ConcurrentQueue<Action> _pathfindingQueue = new ConcurrentQueue<Action>();
    private CancellationTokenSource _cancellationTokenSource;
    private Task _processingTask;


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateObj()
    {
        if (_instance != null)
            return;

        GameObject obj = new GameObject("PathfindingQueue");
        _instance = obj.AddComponent<PathfindingQueue>();
        DontDestroyOnLoad(obj);
    }

    private void Awake()
    {
        StartProcessingQueue();
    }


    private void OnDestroy()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void StartProcessingQueue()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        var token = _cancellationTokenSource.Token;

        _processingTask = Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                if (_pathfindingQueue.TryDequeue(out Action action))
                {
                    action.Invoke();
                }
                else
                {
                    await Task.Delay(10, token); // 큐가 비어 있을 때 잠시 대기
                }
            }
        }, token);
    }

    public void Enqueue(Action action)
    {
        _pathfindingQueue.Enqueue(action);
    }
}
