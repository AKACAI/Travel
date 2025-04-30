using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // 添加UI命名空间

namespace Module.UIModule
{
    public static class UIHelper
    {
        public static void SetClickEvent(GameObject obj, UnityAction action)
        {
            if (obj == null)
            {
                Debug.LogError("SetClickEvent: GameObject is null");
                return;
            }

            var button = obj.GetComponent<ComButton>();
            if (button == null)
            {
                button = obj.AddComponent<ComButton>();
            }

            button.onClick.AddListener(action);
        }

        public static GameObject NewNode(string nodeName, GameObject parent)
        {
            GameObject node = new GameObject(nodeName);
            node.transform.SetParent(parent.transform);
            node.transform.localPosition = Vector2.zero;
            node.transform.localScale = Vector2.one;
            node.transform.localRotation = Quaternion.identity;
            return node;
        }

        // 2. 设置X坐标
        public static void SetPosX(GameObject obj, float x)
        {
            if (obj == null)
            {
                Debug.LogError("SetPositionX: GameObject is null");
                return;
            }

            Vector3 position = obj.transform.position;
            position.x = x;
            obj.transform.position = position;
        }

        // 2. 设置Y坐标
        public static void SetPosY(GameObject obj, float y)
        {
            if (obj == null)
            {
                Debug.LogError("SetPositionY: GameObject is null");
                return;
            }

            Vector3 position = obj.transform.position;
            position.y = y;
            obj.transform.position = position;
        }

        // 2. 设置Z坐标
        public static void SetPositionZ(GameObject obj, float z)
        {
            if (obj == null)
            {
                Debug.LogError("SetPositionZ: GameObject is null");
                return;
            }

            Vector3 position = obj.transform.position;
            position.z = z;
            obj.transform.position = position;
        }
    }
}
