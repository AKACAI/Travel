using UnityEngine;

namespace FrameWork
{
    /// <summary>
    /// 泛型单例基类
    /// </summary>
    /// <typeparam name="T">需要实现单例的类型</typeparam>
    public abstract class Singleton<T> where T : class
    {
        private static readonly object _lock = new object();
        private static T _instance;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = CreateInstance();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 创建实例的虚方法，可以在子类中重写以自定义实例创建方式
        /// </summary>
        protected static T CreateInstance()
        {
            return System.Activator.CreateInstance(typeof(T)) as T;
        }
    }

    /// <summary>
    /// MonoBehaviour的单例基类
    /// </summary>
    /// <typeparam name="T">需要实现单例的MonoBehaviour类型</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static readonly object _lock = new object();
        private static T _instance;

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // 先尝试在场景中查找实例
                            _instance = FindObjectOfType<T>();

                            // 如果场景中没有，则创建新的实例
                            if (_instance == null)
                            {
                                GameObject go = new GameObject(typeof(T).Name);
                                _instance = go.AddComponent<T>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                // 如果场景中已经存在实例，则销毁新创建的实例
                Destroy(gameObject);
            }
        }
    }
}