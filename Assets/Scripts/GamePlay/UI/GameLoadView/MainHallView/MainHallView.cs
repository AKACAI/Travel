using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FrameWork.Manager;
using Module.UIModule;
namespace UI
{
    public class MainHallView : WindowView
    {
        public override WindowCfg.WindowType WindowType { get; set; } = WindowCfg.WindowType.Normal;
        public override string WinName { get; set; } = "MainHallView";

        private GameObject btn_start;

        // 窗体预加载
        public override void PreLoad(System.Action<bool> finish)
        {
            finish.Invoke(true);
        }

        public override void Init()
        {
            this.btn_start = transform.Find("btn_start").gameObject;
            
        }

        // 释放资源
        public override void Dispose()
        {
            base.Dispose();
        }

        public override void Open()
        {
            base.Open();
        }

        public override void Close()
        {
            base.Close();
        }

        protected override void OnOpen()
        {
            base.OnOpen();
        }

        protected override void OnClose()
        {
            base.OnClose();
        }
    }
}