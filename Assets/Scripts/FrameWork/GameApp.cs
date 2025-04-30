using System.Collections;
using System.Collections.Generic;
using FrameWork.Manager;
using UnityEngine;

namespace FrameWork
{
    public class GameApp : Singleton<GameApp>
    {
        private GameObject node_root;
        private GameObject node_map;
        private GameObject node_canvas;

        public void Init(GameObject node_root)
        {
            this.node_root = node_root;
            InitUI();
            InitFinish();
        }

        private void InitUI()
        {
            node_map = new GameObject("node_map");
            node_map.transform.SetParent(node_root.transform);

            node_canvas = this.node_root.transform.Find("node_canvas").gameObject;
        }

        private void InitFinish()
        {
            WindowManager.Instance.Open(WindowName.GameLoadView);
        }

        public GameObject GetUIRootNode()
        {
            return node_canvas;
        }
    }
}
