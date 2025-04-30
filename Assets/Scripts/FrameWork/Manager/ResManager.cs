using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.CompilerServices;

namespace FrameWork.Manager
{
    public class ResManager : Singleton<ResManager>
    {
        private Dictionary<string, AssetBundle> _bundleCache = new Dictionary<string, AssetBundle>();
        private Dictionary<string, Object> _assetCache = new Dictionary<string, Object>();
        private Dictionary<string, int> _bundleRefCount = new Dictionary<string, int>();
        private Dictionary<string, int> _assetRefCount = new Dictionary<string, int>();

        public void LoadBundle(string bundleName, System.Action callback = null)
        {
#if UNITY_EDITOR
            callback?.Invoke();
#else
            if (_bundleCache.TryGetValue(bundleName, out AssetBundle cachedBundle))
            {
                IncreaseBundleRefCount(bundleName);
                callback?.Invoke(cachedBundle);
                return;
            }

            string bundlePath = Path.Combine(Application.streamingAssetsPath, bundleName);
            AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(bundlePath);
            request.completed += (operation) =>
            {
                AssetBundle bundle = request.assetBundle;

                if (bundle == null)
                {
                    LogManager.LogError($"Failed to load bundle: {bundleName}");
                    callback?.Invoke(null);
                    return;
                }

                _bundleCache[bundleName] = bundle;
                IncreaseBundleRefCount(bundleName);
                callback?.Invoke);
            };
#endif
        }

        public void LoadAssetFromBundle<T>(string bundleName, string assetName, System.Action<T> callback = null) where T : Object
        {
#if UNITY_EDITOR
            var result = LoadAssetInEditor<T>(assetName);
            callback?.Invoke(result);
#else
            var bundle = _bundleCache[bundleName];
            if (bundle == null)
            {
                LogManager.LogError($"Failed to load bundle: {bundleName}");
                callback?.Invoke(null);
                return;
            }
            var result = bundle.LoadAssetAsync<T>(assetName);
            result.completed += (operation) =>
            {
                T asset = result.asset as T;
                if (asset != null)
                {
                    string cacheKey = $"{bundleName}_{assetName}_{typeof(T).Name}";
                    _assetCache[cacheKey] = asset;
                    IncreaseAssetRefCount(cacheKey);
                }
                callback?.Invoke(asset as T);
            };
#endif
        }

        public void LoadAsset<T>(string bundleName, string assetName, System.Action<T> callback = null) where T : Object
        {
#if UNITY_EDITOR
            var result = LoadAssetInEditor<T>(assetName);
            callback?.Invoke(result);
#else
            string cacheKey = $"{bundleName}_{assetName}_{typeof(T).Name}";

            if (_assetCache.TryGetValue(cacheKey, out Object cachedAsset))
            {
                IncreaseAssetRefCount(cacheKey);
                callback?.Invoke(cachedAsset as T);
                return;
            }

            if (!_bundleCache.TryGetValue(bundleName, out AssetBundle bundle))
            {
                callback?.Invoke(null);
                return;
            }

            AssetBundleRequest request = bundle.LoadAssetAsync<T>(assetName);
            request.completed += (operation) =>
            {
                T asset = request.asset as T;
                if (asset != null)
                {
                    _assetCache[cacheKey] = asset;
                    IncreaseAssetRefCount(cacheKey);
                }
                callback?.Invoke(asset);
            };
#endif
        }

        private void IncreaseBundleRefCount(string bundleName)
        {
            if (_bundleRefCount.ContainsKey(bundleName))
            {
                _bundleRefCount[bundleName]++;
            }
            else
            {
                _bundleRefCount[bundleName] = 1;
            }
            LogManager.Log($"Bundle {bundleName} 引用计数: {_bundleRefCount[bundleName]}");
        }

        private void IncreaseAssetRefCount(string assetKey)
        {
            if (_assetRefCount.ContainsKey(assetKey))
            {
                _assetRefCount[assetKey]++;
            }
            else
            {
                _assetRefCount[assetKey] = 1;
            }
            LogManager.Log($"Asset {assetKey} 引用计数: {_assetRefCount[assetKey]}");
        }

        private bool DecreaseBundleRefCount(string bundleName)
        {
            if (!_bundleRefCount.ContainsKey(bundleName))
            {
                LogManager.LogWarning($"尝试减少不存在的Bundle引用计数: {bundleName}");
                return false;
            }

            _bundleRefCount[bundleName]--;
            LogManager.Log($"Bundle {bundleName} 引用计数: {_bundleRefCount[bundleName]}");

            if (_bundleRefCount[bundleName] <= 0)
            {
                _bundleRefCount.Remove(bundleName);
                return true;
            }
            return false;
        }

        private bool DecreaseAssetRefCount(string assetKey)
        {
            if (!_assetRefCount.ContainsKey(assetKey))
            {
                LogManager.LogWarning($"尝试减少不存在的Asset引用计数: {assetKey}");
                return false;
            }

            _assetRefCount[assetKey]--;
            LogManager.Log($"Asset {assetKey} 引用计数: {_assetRefCount[assetKey]}");

            if (_assetRefCount[assetKey] <= 0)
            {
                _assetRefCount.Remove(assetKey);
                return true;
            }
            return false;
        }

        public void UnloadAsset<T>(string bundleName, string assetName)
        {
            string cacheKey = $"{bundleName}_{assetName}_{typeof(T).Name}";

            if (!_assetCache.ContainsKey(cacheKey))
            {
                LogManager.LogWarning($"尝试卸载不存在的资源: {cacheKey}");
                return;
            }

            if (DecreaseAssetRefCount(cacheKey))
            {
                _assetCache.Remove(cacheKey);
                LogManager.Log($"资源已从缓存中移除: {cacheKey}");
            }
        }

        public void UnloadBundle(string bundleName, bool unloadAllLoadedObjects = false)
        {
#if UNITY_EDITOR
            return;
#else
            if (!_bundleCache.ContainsKey(bundleName))
            {
                LogManager.LogWarning($"尝试卸载不存在的Bundle: {bundleName}");
                return;
            }

            if (DecreaseBundleRefCount(bundleName))
            {
                AssetBundle bundle = _bundleCache[bundleName];
                bundle.Unload(unloadAllLoadedObjects);
                _bundleCache.Remove(bundleName);
                LogManager.Log($"Bundle已卸载: {bundleName}");

                if (unloadAllLoadedObjects)
                {
                    List<string> keysToRemove = new List<string>();
                    foreach (var key in _assetCache.Keys)
                    {
                        if (key.StartsWith($"{bundleName}_"))
                        {
                            keysToRemove.Add(key);
                        }
                    }

                    foreach (var key in keysToRemove)
                    {
                        _assetCache.Remove(key);
                        _assetRefCount.Remove(key);
                    }
                }
            }
#endif
        }

        public void UnloadAllAssets(bool unloadAllLoadedObjects = false)
        {
            foreach (var bundle in _bundleCache.Values)
            {
                bundle.Unload(unloadAllLoadedObjects);
            }
            _bundleCache.Clear();
            _assetCache.Clear();
            _bundleRefCount.Clear();
            _assetRefCount.Clear();
            LogManager.Log("所有资源已卸载");
        }

        public void PrintResourceStatus()
        {
            LogManager.Log("===== 资源管理器状态 =====");
            LogManager.Log($"已加载Bundle数量: {_bundleCache.Count}");
            LogManager.Log($"已加载Asset数量: {_assetCache.Count}");

            LogManager.Log("\n--- Bundle引用计数 ---");
            foreach (var pair in _bundleRefCount)
            {
                LogManager.Log($"{pair.Key}: {pair.Value}");
            }

            LogManager.Log("\n--- Asset引用计数 ---");
            foreach (var pair in _assetRefCount)
            {
                LogManager.Log($"{pair.Key}: {pair.Value}");
            }
            LogManager.Log("=========================");
        }

#if UNITY_EDITOR
        private T LoadAssetInEditor<T>(string assetName) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets(assetName);
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null)
                {
                    string cacheKey = $"Editor_{assetName}_{typeof(T).Name}";
                    if (!_assetCache.ContainsKey(cacheKey))
                    {
                        _assetCache[cacheKey] = asset;
                        _assetRefCount[cacheKey] = 0;
                    }
                    IncreaseAssetRefCount(cacheKey);
                    return asset;
                }
                else
                {
                    LogManager.LogError($"Asset found but not of type {typeof(T)}: {assetName}");
                }
            }
            else
            {
                LogManager.LogError($"Asset not found: {assetName}");
            }
            return null;
        }
#endif
    }

    // 将扩展方法移到这里（命名空间内，类外）
    public static class AssetBundleRequestExtensions
    {
        public static TaskAwaiter GetAwaiter(this AssetBundleRequest request)
        {
            var tcs = new TaskCompletionSource<object>();
            request.completed += operation => tcs.SetResult(null);
            return ((Task)tcs.Task).GetAwaiter();
        }

        public static Task ToTask(this AssetBundleRequest request)
        {
            var tcs = new TaskCompletionSource<object>();
            request.completed += operation => tcs.SetResult(null);
            return tcs.Task;
        }

        public static TaskAwaiter GetAwaiter(this AssetBundleCreateRequest request)
        {
            var tcs = new TaskCompletionSource<object>();
            request.completed += operation => tcs.SetResult(null);
            return ((Task)tcs.Task).GetAwaiter();
        }

        public static Task ToTask(this AssetBundleCreateRequest request)
        {
            var tcs = new TaskCompletionSource<object>();
            request.completed += operation => tcs.SetResult(null);
            return tcs.Task;
        }
    }
}