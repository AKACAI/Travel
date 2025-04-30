using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using FrameWork.Manager;

namespace FrameWork
{
    public class Main : MonoSingleton<Main>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            Init();
        }

        private void Init()
        {
            GameObject node_root = this.gameObject;
            GameApp.Instance.Init(node_root);
        }

        void Update()
        {
            TimerManager.Instance.Update();
        }
    }
}