using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static WindowCfg;
using Unity.VisualScripting;
using System.Threading.Tasks;
using Module.UIModule;

namespace FrameWork.Manager
{
    public class WindowManager : Singleton<WindowManager>
    {
        private GameObject _uiRootNode;
        private GameObject _normalWinNode;
        private GameObject _alertWinNode;

        private List<string> _loadWinNames;
        private List<string> _openWinNames;
        private List<WindowView> _normalWinViews;
        private List<WindowView> _alertWinViews;

        private Dictionary<string, Coroutine> _disposeCoroutines = new Dictionary<string, Coroutine>();

        private readonly string _disposeCoroutineName = "WindowManager.DelayDispose";

        public WindowManager()
        {
            _uiRootNode = GameApp.Instance.GetUIRootNode();
            _normalWinViews = new List<WindowView>();
            _alertWinViews = new List<WindowView>();
            _loadWinNames = new List<string>();
            _openWinNames = new List<string>();

            // 创建普通窗口的全屏面板
            _normalWinNode = UIHelper.NewNode("node_normal_win", _uiRootNode);

            // 添加RectTransform组件并设置为全屏
            RectTransform normalRectTransform = _normalWinNode.AddComponent<RectTransform>();
            normalRectTransform.anchorMin = Vector2.zero;
            normalRectTransform.anchorMax = Vector2.one;
            normalRectTransform.offsetMin = Vector2.zero;
            normalRectTransform.offsetMax = Vector2.zero;

            // 创建弹窗窗口的全屏面板
            _alertWinNode = UIHelper.NewNode("node_alert_win", _uiRootNode);

            // 添加RectTransform组件并设置为全屏
            RectTransform alertRectTransform = _alertWinNode.AddComponent<RectTransform>();
            alertRectTransform.anchorMin = Vector2.zero;
            alertRectTransform.anchorMax = Vector2.one;
            alertRectTransform.offsetMin = Vector2.zero;
            alertRectTransform.offsetMax = Vector2.zero;
        }

        public void Open(string windowName, GameObject parent = null)
        {
            LogManager.Log($"尝试打开窗口: {windowName}");

            if (_disposeCoroutines.ContainsKey(windowName))
            {
                TimerManager.Instance.RemoveTimer(_disposeCoroutineName);
            }

            WindowView windowView = _normalWinViews.Find(view => view.WinName == windowName);
            if (windowView == null)
            {
                windowView = _alertWinViews.Find(view => view.WinName == windowName);
            }

            if (windowView != null)
            {
                windowView.Open();
                if (windowView.WindowType == WindowType.Normal)
                {
                    if (_normalWinViews.Count > 0)
                    {
                        WindowView topView = _normalWinViews[_normalWinViews.Count - 1];
                        topView.Close();
                    }
                    _normalWinViews.Remove(windowView);
                    _normalWinViews.Add(windowView);
                    windowView.transform.SetSiblingIndex(_normalWinNode.transform.childCount - 1);
                }
                else
                {
                    _alertWinViews.Remove(windowView);
                    _alertWinViews.Add(windowView);
                    windowView.transform.SetSiblingIndex(_alertWinNode.transform.childCount - 1);
                }
                return;
            }

            if (_loadWinNames.Contains(windowName))
            {
                return;
            }

            _loadWinNames.Add(windowName);

            string winPath = WindowCfg.windowPathDic[windowName];
            if (string.IsNullOrEmpty(winPath))
            {
                LogManager.LogError($"窗口路径为空: {windowName}");
                _loadWinNames.Remove(windowName);
                return;
            }

            string bundleName = WindowCfg.rootPath + winPath.Split('/')[0];
            string assetName = winPath.Split('/')[1];
            ResManager.Instance.LoadBundle(bundleName, () =>
            {
                ResManager.Instance.LoadAssetFromBundle<GameObject>(bundleName, assetName, (prefab) =>
                {
                    if (prefab == null)
                    {
                        LogManager.LogError($"窗口预制体为空: {windowName}");
                        this.Close(windowName);
                        return;
                    }
                    GameObject winNode = GameObject.Instantiate(prefab);
                    WindowView windowView = winNode.GetComponent<WindowView>();
                    if (windowView == null)
                    {
                        LogManager.LogError($"窗口没有WindowView组件: {windowName}");
                        this.Close(windowName);
                        return;
                    }
                    if (!_loadWinNames.Contains(windowName))
                    {
                        this.Close(windowName);
                        return;
                    }
                    LogManager.Log($"真正打开窗口: {windowName}");
                    GameObject winRootNode = UIHelper.NewNode(windowName, parent ?? _normalWinNode);
                    winNode.transform.SetParent(winRootNode.transform);
                    winNode.transform.localPosition = Vector2.zero;
                    winNode.transform.localScale = Vector2.one;
                    winNode.transform.localRotation = Quaternion.identity;

                    _loadWinNames.Add(windowName);
                    if (windowView.WindowType == WindowType.Normal)
                    {
                        if (_normalWinViews.Count > 0)
                        {
                            WindowView topView = _normalWinViews[_normalWinViews.Count - 1];
                            topView.Close();
                        }
                        _normalWinViews.Add(windowView);
                    }
                    else
                    {
                        _alertWinViews.Add(windowView);
                    }
                    _openWinNames.Add(windowName);
                    if (_loadWinNames.Contains(windowName))
                    {
                        _loadWinNames.Remove(windowName);
                    }
                    windowView.PreLoad((bool isLoad) =>
                    {
                        if (isLoad)
                        {
                            windowView.Init();
                            windowView.Open();
                        }
                        else
                        {
                            LogManager.LogError($"窗口预加载失败: {windowName}");
                            this.Close(windowName);
                        }
                    });
                });
            });
        }

        public void Close(string windowName)
        {
            LogManager.Log($"关闭窗口: {windowName}");
            if (_loadWinNames.Contains(windowName))
            {
                _loadWinNames.Remove(windowName);
            }
            if (_openWinNames.Contains(windowName))
            {
                _openWinNames.Remove(windowName);
            }

            WindowView windowView = _normalWinViews.Find(view => view.WinName == windowName);
            if (windowView == null)
            {
                windowView = _alertWinViews.Find(view => view.WinName == windowName);
            }

            if (windowView != null)
            {
                windowView.Close();

                if (_disposeCoroutines.ContainsKey(windowName))
                {
                    TimerManager.Instance.RemoveTimer(_disposeCoroutineName);
                }

                DelayDispose(windowView, windowName, 5f);
            }
        }

        private void DelayDispose(WindowView windowView, string windowName, float delay)
        {
            TimerManager.Instance.AddTimer(delay, () =>
            {
                if (windowView != null)
                {
                    windowView.Dispose();

                    if (windowView.WindowType == WindowType.Normal)
                    {
                        _normalWinViews.Remove(windowView);
                    }
                    else
                    {
                        _alertWinViews.Remove(windowView);
                    }

                    GameObject.Destroy(windowView.transform.parent.gameObject);
                }

                _disposeCoroutines.Remove(windowName);
            }, false, _disposeCoroutineName);
        }

        public void CloseAll()
        {
            foreach (var view in _normalWinViews.ToArray())
            {
                Close(view.WinName);
            }

            foreach (var view in _alertWinViews.ToArray())
            {
                Close(view.WinName);
            }

            _loadWinNames.Clear();
            _openWinNames.Clear();
        }
    }
}
