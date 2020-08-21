using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class AnimatedSprite : MonoBehaviour
    {
        public enum LoopType
        {
            None,
            Restart,
            Yoyo
        }

        SpriteRenderer sr;
        public bool playOnAwake = true;
        public LoopType loop = LoopType.Restart;
        public List<Sprite> sprites;
        public float animationDelay = 0.1f;
        float animationEllapsedTime;
        int currentSpriteIndex = 0;
        int deltaSpriteIndex = 1;
        bool playing;

        public UnityEvent OnPlay;
        public UnityEvent OnEnd;

        void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (playOnAwake)
            {
                Play();
            }
        }

        void Update()
        {
            // animated sprite
            if (playing && sprites != null && sprites.Count > 0)
            {
                animationEllapsedTime += Time.deltaTime;
                if (animationEllapsedTime >= animationDelay)
                {
                    animationEllapsedTime = 0;
                    currentSpriteIndex += deltaSpriteIndex;
                    if (currentSpriteIndex == sprites.Count || currentSpriteIndex == -1)
                    {
                        if (loop == LoopType.Restart)
                        {
                            currentSpriteIndex = 0;
                        }
                        else if (loop == LoopType.Yoyo)
                        {
                            deltaSpriteIndex *= -1;
                            if (sprites.Count > 1)
                            {
                                currentSpriteIndex += deltaSpriteIndex * 2;
                            }
                            else
                            {
                                currentSpriteIndex = 0;
                            }
                        }
                        else
                        {
                            currentSpriteIndex -= deltaSpriteIndex;
                            OnEnd.Invoke();
                            PauseIntenral();
                        }
                    }
                    sr.sprite = sprites[currentSpriteIndex];
                }
            }
        }

        /// <summary>
        /// To play the animation.
        /// </summary>
        public void Play()
        {
            OnPlay.Invoke();
            Reset();
            ResumeInternal();
        }

        /// <summary>
        /// To stop the animation.
        /// </summary>
        public void Stop()
        {
            Reset();
            PauseIntenral();
        }

        /// <summary>
        /// To resume the animation.
        /// </summary>
        public void Resume()
        {
            ResumeInternal();
        }

        /// <summary>
        /// To pause the animation.
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
            currentSpriteIndex = 0;
            if (sprites != null && sprites.Count > 0)
            {
                sr.sprite = sprites[currentSpriteIndex];
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Sprite")]
        public static GameObject CreateSprite()
        {
            Object[] sprites = UnityEditor.Selection.objects;
            GameObject newObj = new GameObject();
            newObj.name = "AnimatedSprite";
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            UnityEditor.Selection.activeObject = newObj;
            return AddAnimatedSprite(newObj, sprites);
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Destination")]
        public static GameObject CreateDestination()
        {
            Object[] sprites = UnityEditor.Selection.objects;
            GameObject newObj = Destination2D.CreateObject();
            newObj.name = "AnimatedDestination";
            return AddAnimatedSprite(newObj, sprites);
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Trigger")]
        public static GameObject CreateTrigger()
        {
            Object[] sprites = UnityEditor.Selection.objects;
            GameObject newObj = TriggerController2D.CreateObject();
            newObj.name = "AnimatedTrigger";
            return AddAnimatedSprite(newObj, sprites);
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Carriable")]
        public static GameObject CreateCarriable()
        {
            Object[] sprites = UnityEditor.Selection.objects;
            GameObject newObj = CarryController2D.CreateObject();
            newObj.name = "AnimatedCarriable";
            return AddAnimatedSprite(newObj, sprites);
        }

        private static GameObject AddAnimatedSprite(GameObject newObj, Object[] sprites)
        {
            SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = newObj.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = PlayerController2D.LAYER_BACKGROUND;
                sr.sortingOrder = 1;
            }
            sr.sprite = (Sprite)sprites[0];
            AnimatedSprite animatedSprite = newObj.AddComponent<AnimatedSprite>();
            animatedSprite.sprites = new List<Sprite>();
            foreach (var oneObj in sprites)
            {
                animatedSprite.sprites.Add((Sprite)oneObj);
            }
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Sprite", true)]
        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Destination", true)]
        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Trigger", true)]
        [UnityEditor.MenuItem("Assets/LuDK/2D/Animated/Carriable", true)]
        private static bool Validation()
        {
            if (null == UnityEditor.Selection.objects || UnityEditor.Selection.objects.Length <= 1)
                return false;
            foreach (var oneObj in UnityEditor.Selection.objects)
            {
                if (!(oneObj is Sprite))
                {
                    return false;
                }
            }
            return true;
        }
#endif
    }
}