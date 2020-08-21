using UnityEngine;
using UnityEngine.UI;

namespace LuDK.Toolkit.Core
{
    [RequireComponent(typeof(Image))]
    public class FadeController : MonoBehaviour
    {
        public float maxAlpha = 1.0f;
        public Material transitionMat;

        private Image screen;
        private float fadeInDuration { get; set; }
        private float fadeInEllapsedTime { get; set; }
        private float fadeOutDuration { get; set; }
        private float fadeOutEllapsedTime { get; set; }
        private bool useMaterial = false;

        void Awake()
        {
            screen = GetComponent<Image>();
            if (transitionMat != null)
            {
                SetTransitionMat(transitionMat);
                screen.material.SetFloat("_DisplayFactor", 0);
            } else
            {
                screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, 0);
            }
        }

        public void SetTransitionMat(Material newMat)
        {
            if (screen != null)
            {
                screen.material = new Material(newMat);
                useMaterial = true;
            }
        }

        void Update()
        {
            if (screen != null)
            {                
                if (fadeInDuration > fadeInEllapsedTime)
                {
                    fadeInEllapsedTime += Time.deltaTime;
                    if (fadeInEllapsedTime > fadeInDuration)
                        fadeInEllapsedTime = fadeInDuration;
                    float alpha = fadeInEllapsedTime / fadeInDuration * maxAlpha;
                    if (useMaterial)
                    {
                        screen.material.SetFloat("_DisplayFactor", alpha);
                    }
                    else
                    {
                        screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, alpha);
                    }
                } else if (fadeOutDuration > fadeOutEllapsedTime)
                {
                    fadeOutEllapsedTime += Time.deltaTime;
                    if (fadeOutEllapsedTime > fadeOutDuration)
                        fadeOutEllapsedTime = fadeOutDuration;
                    float alpha = maxAlpha - fadeOutEllapsedTime / fadeOutDuration * maxAlpha;
                    if (useMaterial)
                    {
                        screen.material.SetFloat("_DisplayFactor", alpha);
                    }
                    else
                    {
                        screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, alpha);
                    }
                }
            }
        }

        /// <summary>
        /// It will do a FadeIn (half time) and next a FadeOut (half time) .
        /// </summary>
        /// <param name="duration">FadeIn and FadeOut total time</param>
        public void FadeInOut(float duration)
        {
            FadeOut(duration / 2f);
            FadeIn(duration / 2f);                   
        }

        /// <summary>
        /// It will do a FadeOut.
        /// </summary>
        /// <param name="duration"></param>
        public void FadeOut(float duration)
        {
            fadeOutDuration = duration;
            fadeOutEllapsedTime = 0f;
            if (useMaterial)
            {
                screen.material.SetFloat("_DisplayFactor", maxAlpha);
            }
            else
            {
                screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, maxAlpha);
            }
        }

        /// <summary>
        /// It will do a FadeIn.
        /// </summary>
        /// <param name="duration"></param>
        public void FadeIn(float duration)
        {
            fadeInDuration = duration;
            fadeInEllapsedTime = 0f;
            if (useMaterial)
            {
                screen.material.SetFloat("_DisplayFactor", 0);
            }
            else
            {
                screen.color = new Color(screen.color.r, screen.color.g, screen.color.b, 0);
            }
        }
    }
}