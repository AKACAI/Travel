using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameWork.Manager
{
    public class EventManager : Singleton<EventManager>
    {
        // 使用字典存储事件名称和对应的委托
        private Dictionary<string, Delegate> eventDictionary = new Dictionary<string, Delegate>();

        // 添加无参数事件监听
        public void AddListener(string eventName, Action handler)
        {
            if (handler == null)
            {
                Debug.LogError("添加事件监听失败：处理函数不能为空");
                return;
            }

            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary.Add(eventName, handler);
            }
            else
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], handler);
            }
        }

        // 添加带一个参数的事件监听
        public void AddListener<T>(string eventName, Action<T> handler)
        {
            if (handler == null)
            {
                Debug.LogError("添加事件监听失败：处理函数不能为空");
                return;
            }

            if (!eventDictionary.ContainsKey(eventName))
            {
                eventDictionary.Add(eventName, handler);
            }
            else
            {
                eventDictionary[eventName] = Delegate.Combine(eventDictionary[eventName], handler);
            }
        }

        // 移除无参数事件监听
        public void RemoveListener(string eventName, Action handler)
        {
            if (handler == null || !eventDictionary.ContainsKey(eventName))
            {
                return;
            }

            eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], handler);

            // 如果没有监听者了，移除该事件
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }

        // 移除带一个参数的事件监听
        public void RemoveListener<T>(string eventName, Action<T> handler)
        {
            if (handler == null || !eventDictionary.ContainsKey(eventName))
            {
                return;
            }

            eventDictionary[eventName] = Delegate.Remove(eventDictionary[eventName], handler);

            // 如果没有监听者了，移除该事件
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }

        // 派发无参数事件
        public void Dispatch(string eventName)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                return;
            }

            Delegate d = eventDictionary[eventName];
            if (d is Action action)
            {
                action();
            }
            else
            {
                Debug.LogError($"事件 {eventName} 的类型不匹配：期望 Action，实际为 {d.GetType()}");
            }
        }

        // 派发带一个参数的事件
        public void Dispatch<T>(string eventName, T arg)
        {
            if (!eventDictionary.ContainsKey(eventName))
            {
                return;
            }

            Delegate d = eventDictionary[eventName];
            if (d is Action<T> action)
            {
                action(arg);
            }
            else
            {
                Debug.LogError($"事件 {eventName} 的类型不匹配：期望 Action<{typeof(T)}>, 实际为 {d.GetType()}");
            }
        }

        // 清除所有事件
        public void ClearAllEvents()
        {
            eventDictionary.Clear();
        }
    }
}
