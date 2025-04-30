using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;
using FrameWork.Manager;

namespace Module.UIModule
{
    public class ComButton : Button
    {
        // 按钮状态枚举
        public enum ButtonState
        {
            Idle,       // 空闲
            Pressed,    // 按下
            Released,   // 松开
            Clicked,    // 点击
            Disabled    // 禁用
        }

        private ButtonState currentState = ButtonState.Idle;
        private Vector3 originalScale;
        private float pressedScale = 0.9f;    // 按下时缩放比例
        private float animDuration = 0.2f;     // 动画持续时间

        public event Action OnButtonClick;     // 点击事件

        protected override void Awake()
        {
            base.Awake();
            originalScale = transform.localScale;
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;

            base.OnPointerDown(eventData);
            currentState = ButtonState.Pressed;

            // 执行按下动画
            transform.DOScale(originalScale * pressedScale, animDuration);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!interactable) return;

            base.OnPointerUp(eventData);
            currentState = ButtonState.Released;

            // 恢复原始大小
            transform.DOScale(originalScale, animDuration);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (!interactable) return;

            base.OnPointerClick(eventData);
            currentState = ButtonState.Clicked;

            // 触发点击事件
            LogManager.Log($"点击按钮: {name}");
            OnButtonClick?.Invoke();

            // 点击后返回空闲状态
            currentState = ButtonState.Idle;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            currentState = ButtonState.Disabled;
            // 确保动画被清理
            DOTween.Kill(transform);
            transform.localScale = originalScale;
        }

        public ButtonState GetCurrentState()
        {
            return currentState;
        }
    }
}