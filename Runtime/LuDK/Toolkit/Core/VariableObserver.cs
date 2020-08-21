using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class VariableObserver : MonoBehaviour
    {
        public enum CompareType { equals, not_equals, lesser, lesser_or_equals, greater, greater_or_equals }
        public bool checkOnStart = true;
        public Variable target;
        [SerializeField]
        private UnityEvent OnChange;

        [Header("Compare")]
        public CompareType check;
        public float value = -1;
        public UnityEvent OnTrue;
        public UnityEvent OnFalse;    

        [Header("Advanced")]
        [SerializeField]
        private UnityEvent OnChangePassValue;

        private void OnEnable()
        {
            if (target != null)
            {
                target.AddOnChangeListener(Check);
            }
        }

        private void Start()
        {
            if (checkOnStart)
            {
                Check();
            }
        }

        private void OnDisable()
        {
            if (target != null)
            {
                target.RemoveOnChangeListener(Check);
            }
        }

        private void Check()
        {
            float targetValue = target.value;
            bool ok = false;
            switch (check)
            {
                case CompareType.equals:
                    ok = targetValue == value;
                    break;
                case CompareType.not_equals:
                    ok = targetValue != value;
                    break;
                case CompareType.lesser:
                    ok = targetValue < value;
                    break;
                case CompareType.lesser_or_equals:
                    ok = targetValue <= value;
                    break;
                case CompareType.greater:
                    ok = targetValue > value;
                    break;
                case CompareType.greater_or_equals:
                    ok = targetValue >= value;
                    break;
                default:
                    break;
            }
            if (ok)
            {
                if (OnTrue != null)
                {
                    OnTrue.Invoke();
                }
            } else
            {
                if (OnFalse != null)
                {
                    OnFalse.Invoke();
                }
            }
            if (OnChange != null)
            {
                OnChange.Invoke();
            }
            if (OnChangePassValue != null)
            {
                int nbListeners = OnChangePassValue.GetPersistentEventCount();
                for (int i = 0; i < nbListeners; i++)
                {
                    try
                    {
                        System.Object targetObject = OnChangePassValue.GetPersistentTarget(i);
                        object[] args = { targetValue };
                        MethodInfo method = targetObject.GetType().GetMethod(OnChangePassValue.GetPersistentMethodName(i));
                        method.Invoke(targetObject, args);
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogWarning("Couldn't invoke action. Error:");
                        Debug.LogWarning(exception.Message);
                    }
                }
            }
        }

    }
}