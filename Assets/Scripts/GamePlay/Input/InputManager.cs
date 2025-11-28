using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Input
{
    // 1. 定义值类型的输入事件参数（struct，无装箱）
    public struct InputEvent
    {
        public InputKey KeyType; // 自定义枚举：Jump/Attack/Move
        public InputState State; // 自定义枚举：Down/Up/Hold
        public float InputValue; // 连续输入值（如移动摇杆的-1~1）

        // 静态创建方法，避免重复new（减少GC）
        public static InputEvent Create(InputKey key, InputState state, float value = 0f)
        {
            return new InputEvent { KeyType = key, State = state, InputValue = value };
        }
    }

    // 2. 定义泛型委托（无装箱）
    public delegate void InputActionHandler(InputEvent evt);

    // 3. 输入管理器中使用泛型委托
    public class InputManager : Singleton<InputManager>
    {
        // 泛型多播委托（无装箱）
        private InputActionHandler _onInputEvent;

        // 派发事件（无装箱）
        public void DispatchInputEvent(InputEvent evt)
        {
            _onInputEvent?.Invoke(evt); // 空值检查避免空引用
        }

        // 订阅/取消订阅
        public void Subscribe(InputActionHandler handler)
        {
            _onInputEvent += handler;
        }

        public void Unsubscribe(InputActionHandler handler)
        {
            _onInputEvent -= handler;
        }
    }
}
