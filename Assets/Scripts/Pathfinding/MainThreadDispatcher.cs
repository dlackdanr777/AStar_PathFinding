using System;
using System.Collections.Generic;
using UnityEngine;

namespace Muks.PathFinding
{
    /// <summary> 메인스레드에서 큐에 적재된 명령을 순차적으로 실행하는 클래스 </summary>
    internal class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        internal static MainThreadDispatcher Instance => _instance;
        private static MainThreadDispatcher _instance;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateObj()
        {
            if (_instance != null)
                return;

            GameObject obj = new GameObject("MainThreadDispatcher");
            _instance = obj.AddComponent<MainThreadDispatcher>();
            DontDestroyOnLoad(obj);
        }


        private void Update()
        {
            lock (_executionQueue)
            {
                while (0 < _executionQueue.Count)
                {
                    _executionQueue.Dequeue().Invoke();
                }
            }
        }

        internal void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }
    }
}

