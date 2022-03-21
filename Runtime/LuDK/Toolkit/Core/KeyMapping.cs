using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class KeyMapping : MonoBehaviour
    {
        public KeyCode key;
        public UnityEvent OnDown;
        public UnityEvent OnAction;
        public UnityEvent OnUp;

        private void Update()
        {
            if (Input.GetKeyDown(key) && OnDown != null)
            {
                OnDown?.Invoke();
            }
            if (Input.GetKey(key) && OnAction != null)
            {
                OnAction?.Invoke();
            }
            if (Input.GetKeyUp(key) && OnUp != null)
            {
                OnUp?.Invoke();
            }
        }
    }
}