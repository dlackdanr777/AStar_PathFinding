using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Muks.PathFinding
{
    /// <summary> AStar ��ã�� ����� ��Ƽ ������ ȯ�濡�� ���������� �����Ű�� Ŭ���� </summary>
    internal class PathfindingQueue
    {
        private static ConcurrentQueue<Action> _pathfindingQueue = new ConcurrentQueue<Action>();
        private static CancellationTokenSource  _cancellationTokenSource;
        private static Task _processingTask;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Start()
        {
            _pathfindingQueue.Clear();
            StartProcessingQueue();
        }


        /// <summary> �񵿱� �۾��� ����ϴ� �Լ� </summary>
        internal static void StopProcessingQueue()
        {
            _pathfindingQueue.Clear();
            _cancellationTokenSource?.Cancel();
        }


        /// <summary> ť�� �ִ� �븮�ڸ� ���� �����Ű�� �Լ� </summary>
        internal static void StartProcessingQueue()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            _processingTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    //ť�� ����� �븮�ڰ� �������
                    if (_pathfindingQueue.TryDequeue(out Action action))
                    {
                        //�븮�ڸ� �����Ų��.
                        action.Invoke();
                    }
                    else
                    {
                        // ť�� ��� ���� �� ��� ����Ѵ�.
                        await Task.Delay(10, token); 
                    }
                }
            }, token);
        }


        /// <summary> �븮�ڸ� ť�� �����Ű�� �Լ� </summary>
        internal static void Enqueue(Action action)
        {
            _pathfindingQueue.Enqueue(action);
        }
    }
}
