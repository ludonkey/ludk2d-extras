using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    public class CarriedObject2D : MonoBehaviour, CarryController2D.Carryable
    {
        public bool alwaysOnTop = false;
        public float deltaXFront = 0.3f;
        public float deltaYFront = 0.0f;
        public float deltaXBack = -0.3f;
        public float deltaYBack = 0.0f;
        public float holdingRotation = 0.0f;

        public UnityEvent onActionStart;
        public UnityEvent onActionRun;
        public UnityEvent onActionEnd;

        public void ActionStart()
        {
            if (onActionStart != null)
            {
                onActionStart.Invoke();
            }
        }

        public void ActionRun()
        {
            if (onActionRun != null)
            {
                onActionRun.Invoke();
            }
        }

        public void ActionEnd()
        {
            if (onActionEnd != null)
            {
                onActionEnd.Invoke();
            }
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
    }
}