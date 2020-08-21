using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace LuDK.Toolkit.Core
{
    public class BubbleSpeech : MonoBehaviour
    {     
        public bool playOnAwake = true;
        public float delay = 0.0f;
        public KeyCode key;
        public float delayBetweenHit = 0.05f;
        public string content;
        public Image bgImage;
        public TMP_Text text;
        public Vector2 padding;
        public Vector2 maxSize = new Vector2(1000, float.PositiveInfinity);
        public Vector2 minSize;
        public AudioSource hitSound;
        public AudioSource endSound;
        public UnityEvent onActionPlay;
        public UnityEvent onActionClose;

        public enum Mode
        {
            None = 0,
            Horizontal = 0x1,
            Vertical = 0x2,
            Both = Horizontal | Vertical
        }
        public Mode controlAxes = Mode.Both;

        protected string lastText = null;
        protected Vector2 lastSize;
        protected bool forceRefresh = false;

        public bool ends { get; private set; }

        private bool stop;

        public bool start { get; private set; }

        protected virtual float MinX
        {
            get
            {
                if ((controlAxes & Mode.Horizontal) != 0) return minSize.x;
                return GetComponent<RectTransform>().rect.width - padding.x;
            }
        }
        protected virtual float MinY
        {
            get
            {
                if ((controlAxes & Mode.Vertical) != 0) return minSize.y;
                return GetComponent<RectTransform>().rect.height - padding.y;
            }
        }
        protected virtual float MaxX
        {
            get
            {
                if ((controlAxes & Mode.Horizontal) != 0) return maxSize.x;
                return GetComponent<RectTransform>().rect.width - padding.x;
            }
        }
        protected virtual float MaxY
        {
            get
            {
                if ((controlAxes & Mode.Vertical) != 0) return maxSize.y;
                return GetComponent<RectTransform>().rect.height - padding.y;
            }
        }

        private void Awake()
        {
            Stop();
            if (playOnAwake)
            {
                Play(delay);
            }
        }

        public void Stop()
        {            
            start = false;           
            stop = true;
            if (bgImage != null)
            {
                bgImage.enabled = false;
            }
            if (text != null)
            {
                text.text = "";
            }
            if (ends && onActionClose != null)
            {
                onActionClose.Invoke();
            }
            ends = false;
        }
         
        public void End()
        {
            ends = true;
        }

        public void Play(float delay)
        {
            PlayText(content, delay);
        }

        public void PlayText(string story)
        {
            PlayText(story, 0);
        }

        public void PlayText(string story, float delay)
        {
            StartCoroutine(InternalPlay(story, delay));
        }


        private IEnumerator InternalPlay(string story, float delay) { 
            yield return new WaitForSeconds(delay);
            start = true;
            stop = false;
            if (onActionPlay != null)
            {
                onActionPlay.Invoke();
            }
            if (bgImage != null)
            {
                bgImage.enabled = true;
            }
            foreach (char c in story)
            {
                text.text += c;
                if (hitSound != null && !hitSound.isPlaying)
                {
                    hitSound.Play();
                }
                if (ends)
                {
                    text.text = story;
                    break;
                }
                if (stop)
                {
                    break;
                }
                yield return new WaitForSeconds(delayBetweenHit);
            }
            if (endSound != null)
            {
                yield return new WaitForSeconds(delayBetweenHit);
                endSound.Play();
                ends = true;
            }
        }

        protected virtual void Update()
        {
            if (!start)
            {
                return;
            }
            if (Input.GetKeyDown(key))
            {
                if (!ends)
                {
                    End();
                } else if (!stop)
                {
                    Stop();
                }
            }
            RectTransform rt = GetComponent<RectTransform>();
            if (text != null && (text.text != lastText || lastSize != rt.rect.size || forceRefresh))
            {
                lastText = text.text;
                Vector2 preferredSize = text.GetPreferredValues(MaxX, MaxY);
                preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
                preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);
                preferredSize += padding;

                if ((controlAxes & Mode.Horizontal) != 0)
                {
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                }
                if ((controlAxes & Mode.Vertical) != 0)
                {
                    rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                }
                lastSize = rt.rect.size;
                forceRefresh = false;
            }
        }

        public virtual void Refresh()
        {
            forceRefresh = true;
        }

        void Reset()
        {
            if (key == KeyCode.None)
            {
                key = KeyCode.Space;
            }
        }
    }
}