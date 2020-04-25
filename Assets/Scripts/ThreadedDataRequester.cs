using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Ocrambana.LandmassGeneration.Script.Data;

namespace Ocrambana.LandmassGeneration.Script
{
    public class ThreadedDataRequester : MonoBehaviour
    {
        private static ThreadedDataRequester instance;
        private Queue<ThreadInfo> DataQueue = new Queue<ThreadInfo>();

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        public static void RequestData(Func<object> generateData, Action<object> callback)
        {
            ThreadStart threadStart = delegate
            {
                instance.DataThread(generateData, callback);
            };

            new Thread(threadStart).Start();
        }

        private void DataThread(Func<object> generateData, Action<object> callback)
        {
            object data = generateData();

            lock (DataQueue)
            {
                DataQueue.Enqueue(new ThreadInfo(callback, data));
            }
        }

        private void Update()
        {
            while (DataQueue.Count > 0)
            {
                ThreadInfo threadinfo = DataQueue.Dequeue();
                threadinfo.callback(threadinfo.parameter);
            }
        }

        struct ThreadInfo
        {
            public readonly Action<object> callback;
            public readonly object parameter;

            public ThreadInfo(Action<object> callback, object parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }

    }
}
