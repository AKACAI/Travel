using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace FrameWork.Manager
{
    public class WindowView : MonoBehaviour
    {
        public virtual WindowCfg.WindowType WindowType { get; set; }
        public virtual string WinName { get; set; }
        protected ResHandler loadHandler = new ResHandler();
        protected bool isOpen = false;
        protected bool isInitialized = false;

        // 窗体预加载
        public virtual void PreLoad(System.Action<bool> finish)
        {
            finish.Invoke(true);
        }

        public virtual void Init()
        {
            isInitialized = true;
        }

        // 释放资源
        public virtual void Dispose()
        {
            if (loadHandler != null)
            {
                loadHandler.ReleaseAllAssets();
                loadHandler = null;
            }
            isInitialized = false;
        }

        public virtual void Open()
        {
            if (isOpen) return;
            isOpen = true;
            OnOpen();
        }

        public virtual void Close()
        {
            if (!isOpen) return;
            isOpen = false;
            OnClose();
        }

        protected virtual void OnOpen()
        {

        }

        protected virtual void OnClose()
        {

        }

        protected void AddLoadRes()
        {

        }

        public bool IsInitialized()
        {
            return isInitialized;
        }

        protected void CloseBySelf()
        {
            WindowManager.Instance.Close(this.WinName);
        }
    }
}