using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class RandomizedAction : MonoBehaviour
    {
        public bool playOnStart = false;
        public float delay = 0;
        public List<UnityEvent> OnAct;
        private float timeBeforeAct = -1;

        private void Awake()
        {
            if (null == OnAct)
                OnAct = new List<UnityEvent>();
        }

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
            if (OnAct != null && OnAct.Count > 0)
            {
                if (delay > 0)
                {
                    timeBeforeAct = delay;
                }
                else
                {
                    UnityEvent oneRandomizedAction = OnAct[Random.Range(0, OnAct.Count)];
                    oneRandomizedAction?.Invoke();
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
                    if (OnAct != null && OnAct.Count > 0)
                    {
                        UnityEvent oneRandomizedAction = OnAct[Random.Range(0, OnAct.Count)];
                        oneRandomizedAction?.Invoke();
                    }
                }
            }
        }
    }
}