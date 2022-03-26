using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class Action : MonoBehaviour
    {
        public bool playOnStart = false;
        public float delay = 0;
        public UnityEvent OnAct;
        private float timeBeforeAct = -1;

        private void Start()
        {
            if (playOnStart)
            {
                DelayedAct(delay);
            }
        }

        public void Act()
        {
            DelayedAct(0);           
        }

        public void DelayedAct(float delay)
        {
            if (OnAct != null)
            {
                if (delay > 0)
                {
                    timeBeforeAct = delay;
                }
                else
                {
                    OnAct.Invoke();
                }
            }
        }

        private void Update()
        {
            if (timeBeforeAct > 0)
            {
                timeBeforeAct -= Time.deltaTime;
                if (timeBeforeAct <= 0)
                {
                    timeBeforeAct = -1;
                    if (OnAct != null)
                    {
                        OnAct.Invoke();
                    }
                }
            }
        }
    }
}