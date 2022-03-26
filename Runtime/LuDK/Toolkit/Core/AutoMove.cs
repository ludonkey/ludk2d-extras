using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class AutoMove : MonoBehaviour
    {
        public enum LoopType
        {
            None,
            Restart,
            Yoyo,
            Incr
        }

        public bool playOnAwake = true;
        public LoopType loop = LoopType.Restart;
        public float deltaXBySecond = 0;
        public float deltaYBySecond = 0;
        public float deltaZBySecond = 0;
        public float duration = 1f;
        float currentTime = 0;
        bool forward = true;
        bool playing;
        Vector3 originalPos;

        public UnityEvent OnPlay;
        public UnityEvent OnEnd;

        void Awake()
        {
            if (playOnAwake)
            {
                Play();
            }
        }

        void Update()
        {
            if (playing)
            {
                if (forward)
                {
                    currentTime += Time.deltaTime;
                }
                else
                {
                    currentTime -= Time.deltaTime;
                }
                if (currentTime >= duration)
                {
                    currentTime = duration;
                }
                else if (currentTime <= 0)
                {
                    currentTime = 0;
                }
                transform.position = originalPos + new Vector3(deltaXBySecond, deltaYBySecond, deltaZBySecond) * currentTime;
                switch (loop)
                {
                    default:
                    case LoopType.None:
                        if (currentTime == duration)
                        {
                            Stop();
                        }
                        break;
                    case LoopType.Restart:
                        if (currentTime == duration)
                        {
                            currentTime = 0;
                        }
                        break;
                    case LoopType.Yoyo:
                        if (currentTime == duration || currentTime == 0)
                        {
                            forward = !forward;
                        }
                        break;
                    case LoopType.Incr:
                        if (currentTime == duration)
                        {
                            currentTime = 0;
                            originalPos = gameObject.transform.position;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// To play the move.
        /// </summary>
        public void Play()
        {
            originalPos = gameObject.transform.position;
            OnPlay?.Invoke();
            Reset();
            ResumeInternal();
        }

        /// <summary>
        /// To stop the move.
        /// </summary>
        public void Stop()
        {
            OnEnd?.Invoke();
            Reset();
            PauseIntenral();
        }

        /// <summary>
        /// To resume the move.
        /// </summary>
        public void Resume()
        {
            ResumeInternal();
        }

        /// <summary>
        /// To pause the move.
        /// </summary>
        public void Pause()
        {
            PauseIntenral();
        }

        private void ResumeInternal()
        {
            playing = true;
        }

        private void PauseIntenral()
        {
            playing = false;
        }

        /// <summary>
        /// To reset the animation.
        /// </summary>
        public void Reset()
        {
            currentTime = 0;
            forward = true;
        }

    }
}