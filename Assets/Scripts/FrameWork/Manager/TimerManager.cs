using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameWork.Manager
{
    public class TimerManager : Singleton<TimerManager>
    {
        private PriorityQueue<TimerTask> timerHeap = new PriorityQueue<TimerTask>();
        private List<TimerTask> removeList = new List<TimerTask>();
        private List<TimerTask> addList = new List<TimerTask>();
        private Dictionary<string, TimerTask> tokenToTask = new Dictionary<string, TimerTask>();

        public void Update()
        {
            float currentTime = Time.time;

            // 处理需要添加的定时器
            if (addList.Count > 0)
            {
                foreach (var task in addList)
                {
                    timerHeap.Enqueue(task);
                }
                addList.Clear();
            }

            // 检查并执行到期的定时器
            while (timerHeap.Count > 0)
            {
                TimerTask task = timerHeap.Peek();
                if (task.NextTriggerTime > currentTime)
                    break;

                timerHeap.Dequeue();

                if (!task.IsCanceled)
                {
                    task.Action?.Invoke();

                    if (task.IsLoop)
                    {
                        task.NextTriggerTime = currentTime + task.Interval;
                        addList.Add(task);
                    }
                }
            }

            // 清理已取消的定时器
            removeList.Clear();
        }

        /// <summary>
        /// 添加定时器（支持token）
        /// </summary>
        /// <param name="interval">间隔时间（秒）</param>
        /// <param name="callback">回调方法</param>
        /// <param name="token">定时器标识（可选）</param>
        /// <param name="isLoop">是否循环</param>
        /// <returns>定时器任务对象</returns>
        public TimerTask AddTimer(float interval, Action callback, bool isLoop = true, string token = null)
        {
            // 如果token存在，先移除旧的定时器
            if (!string.IsNullOrEmpty(token) && tokenToTask.ContainsKey(token))
            {
                RemoveTimer(token);
            }

            TimerTask task = new TimerTask(interval, callback, isLoop);
            addList.Add(task);

            // 如果有token，添加到字典中
            if (!string.IsNullOrEmpty(token))
            {
                tokenToTask[token] = task;
                task.Token = token;
            }

            return task;
        }

        /// <summary>
        /// 通过token移除定时器
        /// </summary>
        /// <param name="token">定时器标识</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveTimer(string token)
        {
            if (string.IsNullOrEmpty(token) || !tokenToTask.ContainsKey(token))
                return false;

            TimerTask task = tokenToTask[token];
            task.Cancel();
            tokenToTask.Remove(token);
            return true;
        }

        /// <summary>
        /// 检查定时器是否存在
        /// </summary>
        /// <param name="token">定时器标识</param>
        /// <returns>是否存在</returns>
        public bool HasTimer(string token)
        {
            return !string.IsNullOrEmpty(token) &&
                   tokenToTask.ContainsKey(token) &&
                   !tokenToTask[token].IsCanceled;
        }
    }

    public class TimerTask : IComparable<TimerTask>
    {
        public float NextTriggerTime { get; set; }
        public float Interval { get; private set; }
        public Action Action { get; private set; }
        public bool IsLoop { get; private set; }
        public bool IsCanceled { get; private set; }
        public string Token { get; internal set; }

        public TimerTask(float interval, Action action, bool isLoop)
        {
            Interval = interval;
            Action = action;
            IsLoop = isLoop;
            NextTriggerTime = Time.time + interval;
            IsCanceled = false;
            Token = null;
        }

        public void Cancel()
        {
            IsCanceled = true;
            Action = null;
        }

        public int CompareTo(TimerTask other)
        {
            return NextTriggerTime.CompareTo(other.NextTriggerTime);
        }
    }

    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> heap = new List<T>();

        public int Count => heap.Count;

        public void Enqueue(T item)
        {
            heap.Add(item);
            SiftUp(heap.Count - 1);
        }

        public T Dequeue()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            T result = heap[0];
            int lastIndex = heap.Count - 1;
            heap[0] = heap[lastIndex];
            heap.RemoveAt(lastIndex);

            if (heap.Count > 0)
                SiftDown(0);

            return result;
        }

        public T Peek()
        {
            if (heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            return heap[0];
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (heap[index].CompareTo(heap[parentIndex]) >= 0)
                    break;

                T temp = heap[index];
                heap[index] = heap[parentIndex];
                heap[parentIndex] = temp;
                index = parentIndex;
            }
        }

        private void SiftDown(int index)
        {
            while (true)
            {
                int smallest = index;
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;

                if (leftChild < heap.Count && heap[leftChild].CompareTo(heap[smallest]) < 0)
                    smallest = leftChild;

                if (rightChild < heap.Count && heap[rightChild].CompareTo(heap[smallest]) < 0)
                    smallest = rightChild;

                if (smallest == index)
                    break;

                T temp = heap[index];
                heap[index] = heap[smallest];
                heap[smallest] = temp;
                index = smallest;
            }
        }
    }
}