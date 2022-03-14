using System.Collections.Generic;
using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    public class Skin2D : MonoBehaviour, ISkin2D
    {
        public PlayerController2D player;
        public bool applyOnStart = false;

        [Header("Properties")]
        public float moveSpeed = 3.0f;
        public bool flipXAnimation = false;
        public bool flipYAnimation = false;
        public List<Sprite> sprites;
        public float animationTimeInBetween = 0.1f;

        [Header("Only for Platformer")]
        public float gravityScale = 1f;
        public float jumpFactor = 1.4f;
        public List<Sprite> inTheAirSprites;
        public float inTheAirAnimationTimeInBetween = 0.1f;

        private void Awake()
        {
            if (sprites == null)
                sprites = new List<Sprite>();
            if (inTheAirSprites == null)
                inTheAirSprites = new List<Sprite>();
        }

        private void Start()
        {
            if (applyOnStart)
                Apply();
        }

        public void Apply()
        {
            if (null == player)
            {
                player = GetComponentInParent<PlayerController2D>();
                if (null == player)
                {
                    player = GameObject.FindObjectOfType<PlayerController2D>();
                }
            }
            if (player != null)
            {
                player.skin = this;
            }
            else
            {
                Debug.LogError("[Skin2D] Missing player");
            }
        }

        public void Unapply()
        {
            if (null != player)
            {
                player.skin = null;
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/LuDK/2D/Player/Skin")]
        public static GameObject CreateObject()
        {
            var player = GameObject.FindObjectOfType<PlayerController2D>();
            GameObject newObj = new GameObject();
            newObj.transform.parent = player.transform;
            newObj.transform.localPosition = Vector3.zero;
            newObj.name = "NewSkin";
            Skin2D skin = newObj.AddComponent<Skin2D>();
            skin.sprites = new List<Sprite>();
            foreach (var oneObj in UnityEditor.Selection.objects)
            {
                skin.sprites.Add((Sprite)oneObj);
            }
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Player/Skin", true)]
        private static bool Validation()
        {
            if (null == UnityEditor.Selection.objects || UnityEditor.Selection.objects.Length < 1)
                return false;
            foreach (var oneObj in UnityEditor.Selection.objects)
            {
                if (!(oneObj is Sprite))
                {
                    return false;
                }
            }
            var player = GameObject.FindObjectOfType<PlayerController2D>();
            return player != null;
        }

        public float MoveSpeed()
        {
            return moveSpeed;
        }

        public bool FlipXAnimation()
        {
            return flipXAnimation;
        }

        public bool FlipYAnimation()
        {
            return flipYAnimation;
        }

        public List<Sprite> Sprites()
        {
            return sprites;
        }

        public float AnimationTimeInBetween()
        {
            return animationTimeInBetween;
        }

        public float GravityScale()
        {
            return gravityScale;
        }

        public float JumpFactor()
        {
            return jumpFactor;
        }

        public List<Sprite> InTheAirSprites()
        {
            return inTheAirSprites;
        }

        public float InTheAirAnimationTimeInBetween()
        {
            return inTheAirAnimationTimeInBetween;
        }
#endif
    }

    public interface ISkin2D {
        float MoveSpeed();
        bool FlipXAnimation();
        bool FlipYAnimation();
        List<Sprite> Sprites();
        float AnimationTimeInBetween();
        //Only for Platformer
        float GravityScale();
        float JumpFactor();
        List<Sprite> InTheAirSprites();
        float InTheAirAnimationTimeInBetween();
    }
}