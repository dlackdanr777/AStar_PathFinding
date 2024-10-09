using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Muks.PathFinding
{
    /// <summary> AStar 길찾기 계산을 멀티 스레드 환경에서 순차적으로 실행시키는 클래스 </summary>
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


        /// <summary> 비동기 작업을 취소하는 함수 </summary>
        internal static void StopProcessingQueue()
        {
            _pathfindingQueue.Clear();
            _cancellationTokenSource?.Cancel();
        }


        /// <summary> 큐에 있는 대리자를 순차 실행시키는 함수 </summary>
        internal static void StartProcessingQueue()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _cancellationTokenSource.Token;

            _processingTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    //큐에 적재된 대리자가 있을경우
                    if (_pathfindingQueue.TryDequeue(out Action action))
                    {
                        //대리자를 실행시킨다.
                        action.Invoke();
                    }
                    else
                    {
                        // 큐가 비어 있을 때 잠시 대기한다.
                        await Task.Delay(10, token); 
                    }
                }
            }, token);
        }


        /// <summary> 대리자를 큐에 적재시키는 함수 </summary>
        internal static void Enqueue(Action action)
        {
            _pathfindingQueue.Enqueue(action);
        }
    }
}
