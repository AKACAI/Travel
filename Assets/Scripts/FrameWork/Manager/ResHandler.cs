using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FrameWork.Manager
{
    public class ResHandler
    {
        private List<string> _loadResList;
        // 缓存资源路径信息load
        private Dictionary<string, (string bundleName, string assetName)> _resourcePaths;
        // 已加载的资源引用
        private List<UnityEngine.Object> _loadedAssets;
        // 待加载的资源数量
        private int _pendingLoadCount;
        // 完成所有加载后的回调
        private Action _onAllLoaded;
        // 加载任务队列
        private List<Action> _loadTasks;
        // 是否已经开始加载
        private bool _isLoading;
        // 记录已加载的资源信息，用于卸载
        private Dictionary<string, (string bundleName, string assetName, System.Type type)> _loadedAssetInfo;

        public ResHandler()
        {
            _resourcePaths = new Dictionary<string, (string bundleName, string assetName)>();
            _loadResList = new List<string>();
            _loadedAssets = new List<UnityEngine.Object>();
            _loadTasks = new List<Action>();
            _loadedAssetInfo = new Dictionary<string, (string bundleName, string assetName, System.Type type)>();
            _pendingLoadCount = 0;
            _isLoading = false;
        }

        public void AddLoadRes(string path, string bundleName)
        {
            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);
            _resourcePaths[path] = (bundleName, assetName);
            _loadResList.Add(path);
        }

        // 添加预制体加载任务
        public void AddPrefabLoad(string path, string bundleName, Action<GameObject> callback = null)
        {
            AddLoadRes(path, bundleName);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 将加载任务添加到队列中，而不是立即执行
            _loadTasks.Add(() =>
            {
                _pendingLoadCount++;
                ResManager.Instance.LoadAssetFromBundle<GameObject>(bundleName, assetName, (obj) =>
                {
                    if (obj != null)
                    {
                        _loadedAssets.Add(obj);
                        // 记录资源信息用于卸载
                        _loadedAssetInfo[obj.GetInstanceID().ToString()] = (bundleName, assetName, typeof(GameObject));
                        callback?.Invoke(obj);
                    }
                    _pendingLoadCount--;
                    CheckAllLoaded();
                });
            });
        }

        // 添加纹理加载任务
        public void AddTextureLoad(string path, string bundleName, Action<Texture2D> callback = null)
        {
            AddLoadRes(path, bundleName);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 将加载任务添加到队列中
            _loadTasks.Add(() =>
            {
                _pendingLoadCount++;
                ResManager.Instance.LoadAssetFromBundle<Texture2D>(bundleName, assetName, (texture) =>
                {
                    if (texture != null)
                    {
                        _loadedAssets.Add(texture);
                        // 记录资源信息用于卸载
                        _loadedAssetInfo[texture.GetInstanceID().ToString()] = (bundleName, assetName, typeof(Texture2D));
                        callback?.Invoke(texture);
                    }
                    _pendingLoadCount--;
                    CheckAllLoaded();
                });
            });
        }

        // 添加精灵加载任务
        public void AddSpriteLoad(string path, string bundleName, Action<Sprite> callback = null)
        {
            AddLoadRes(path, bundleName);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 将加载任务添加到队列中
            _loadTasks.Add(() =>
            {
                _pendingLoadCount++;
                ResManager.Instance.LoadAssetFromBundle<Sprite>(bundleName, assetName, (sprite) =>
                {
                    if (sprite != null)
                    {
                        _loadedAssets.Add(sprite);
                        // 记录资源信息用于卸载
                        _loadedAssetInfo[sprite.GetInstanceID().ToString()] = (bundleName, assetName, typeof(Sprite));
                        callback?.Invoke(sprite);
                    }
                    _pendingLoadCount--;
                    CheckAllLoaded();
                });
            });
        }

        // 添加音频加载任务
        public void AddAudioLoad(string path, string bundleName, Action<AudioClip> callback = null)
        {
            AddLoadRes(path, bundleName);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 将加载任务添加到队列中
            _loadTasks.Add(() =>
            {
                _pendingLoadCount++;
                ResManager.Instance.LoadAssetFromBundle<AudioClip>(bundleName, assetName, (clip) =>
                {
                    if (clip != null)
                    {
                        _loadedAssets.Add(clip);
                        // 记录资源信息用于卸载
                        _loadedAssetInfo[clip.GetInstanceID().ToString()] = (bundleName, assetName, typeof(AudioClip));
                        callback?.Invoke(clip);
                    }
                    _pendingLoadCount--;
                    CheckAllLoaded();
                });
            });
        }

        // 泛型方法，添加任意类型资源的加载任务
        public void AddAssetLoad<T>(string path, string bundleName, Action<T> callback = null) where T : UnityEngine.Object
        {
            AddLoadRes(path, bundleName);
            string assetName = System.IO.Path.GetFileNameWithoutExtension(path);

            // 将加载任务添加到队列中
            _loadTasks.Add(() =>
            {
                _pendingLoadCount++;
                ResManager.Instance.LoadAssetFromBundle<T>(bundleName, assetName, (asset) =>
                {
                    if (asset != null)
                    {
                        _loadedAssets.Add(asset);
                        // 记录资源信息用于卸载
                        _loadedAssetInfo[asset.GetInstanceID().ToString()] = (bundleName, assetName, typeof(T));
                        callback?.Invoke(asset);
                    }
                    _pendingLoadCount--;
                    CheckAllLoaded();
                });
            });
        }

        // 开始加载所有资源，并设置完成回调
        public void StartLoad(Action onAllLoaded = null)
        {
            if (_isLoading)
            {
                Debug.LogWarning("ResHandler: 已经开始加载，忽略重复调用");
                return;
            }

            _isLoading = true;
            _onAllLoaded = onAllLoaded;

            // 执行所有加载任务
            foreach (var task in _loadTasks)
            {
                task.Invoke();
            }

            // 如果没有待加载的资源，直接调用回调
            if (_pendingLoadCount == 0)
            {
                _onAllLoaded?.Invoke();
            }
        }

        // 检查是否所有资源都已加载完成
        private void CheckAllLoaded()
        {
            if (_pendingLoadCount <= 0 && _onAllLoaded != null)
            {
                Action callback = _onAllLoaded;
                _onAllLoaded = null;
                callback.Invoke();
            }
        }

        // 释放所有已加载的资源
        public void ReleaseAllAssets()
        {
            // 使用ResManager卸载资源
            foreach (var assetInfo in _loadedAssetInfo)
            {
                var (bundleName, assetName, type) = assetInfo.Value;

                // 使用反射调用泛型方法
                var method = typeof(ResManager).GetMethod("UnloadAsset");
                var genericMethod = method.MakeGenericMethod(type);
                genericMethod.Invoke(ResManager.Instance, new object[] { bundleName, assetName });
            }

            // 清理本地缓存
            _loadedAssets.Clear();
            _loadResList.Clear();
            _loadTasks.Clear();
            _loadedAssetInfo.Clear();
            _pendingLoadCount = 0;
            _onAllLoaded = null;
            _isLoading = false;
        }

        // 析构函数，确保资源被释放
        ~ResHandler()
        {
            ReleaseAllAssets();
        }
    }
}