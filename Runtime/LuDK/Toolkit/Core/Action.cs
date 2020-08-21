using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class Action : MonoBehaviour
    {
        public bool playOnStart = false;
        public float delay = -1;
        public UnityEvent OnAct;
        private float timeBeforeAct = -1;

        private void Start()
        {
            if (playOnStart)
            {
                Act();
            }
        }

        public void Act()
        {          
            if (OnAct != null)
            {
                if (delay > 0)
                {
                    DelayedAct(delay);
                }
                else
                {
                    OnAct.Invoke();
                }                
            }
        }

        public void DelayedAct(float delay)
        {
            timeBeforeAct = delay;
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