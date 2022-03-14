using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    public class CarriedObject2D : MonoBehaviour, CarryController2D. Carryable
    {
        public bool alwaysOnTop = false;
        public float deltaXFront = 0.3f;
        public float deltaYFront = 0.0f;
        public float deltaXBack = -0.3f;
        public float deltaYBack = 0.0f;
        public float holdingRotation = 0.0f;
        [Header("On Carrying Events")]
        public UnityEvent OnTake;
        public UnityEvent OnDrop;
        public UnityEvent OnConsume;

        [Header("On Action Events")]
        public UnityEvent onActionStart;
        public UnityEvent onActionRun;
        public UnityEvent onActionEnd;

        public void ActionStart()
        {
            onActionStart?.Invoke();
        }

        public void ActionRun()
        {
            onActionRun?.Invoke();
        }

        public void ActionEnd()
        {
            onActionEnd?.Invoke();
        }

        public bool AlwaysOnTop()
        {
            return alwaysOnTop;
        }

        public float GetDeltaX(bool front)
        {
            return front ? deltaXFront : deltaXBack;
        }

        public float GetDeltaY(bool front)
        {
            return front ? deltaYFront : deltaYBack;
        }

        public float GetHoldingRotation()
        {
            return holdingRotation;
        }

        void CarryController2D.Carryable.OnTake()
        {
            OnTake?.Invoke();
        }

        void CarryController2D.Carryable.OnDrop()
        {
            OnDrop?.Invoke();
        }

        void CarryController2D.Carryable.OnConsume()
        {
            OnConsume?.Invoke();
        }
    }
}