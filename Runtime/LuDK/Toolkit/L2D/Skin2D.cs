using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    public class Skin2D : MonoBehaviour, ISkin2D
    {
        private PlayerController2D player;
        public bool applyOnStart = false;

        [Header("Properties")]
        public float _moveSpeed = 3.0f;
        public float moveSpeed { get { return _moveSpeed; } set { _moveSpeed = value; } }
        public bool _flipXAnimation = false;
        public bool flipXAnimation { get { return _flipXAnimation; } set { _flipXAnimation = value; } }
        public bool _flipYAnimation = false;
        public bool flipYAnimation { get { return _flipYAnimation; } set { _flipYAnimation = value; } }
        public List<Sprite> sprites;
        public float animationTimeInBetween = 0.1f;

        [Header("Only for Platformer")]
        public bool _jumpEnabled = true;
        public bool jumpEnabled { get { return _jumpEnabled; } set { _jumpEnabled = value; } }
        public float _gravityScale = 1f;
        public float gravityScale { 
            get { 
                return _gravityScale; 
            } 
            set { _gravityScale = value;
                player?.UpdateGravityScale();
            } 
        }
        public float _jumpFactor = 1.4f;
        public float jumpFactor { get { return _jumpFactor; } set { _jumpFactor = value; } }
        public List<Sprite> inTheAirSprites;
        public float inTheAirAnimationTimeInBetween = 0.1f;

        [Header("Events")]
        public UnityEvent OnApply;
        public UnityEvent OnUnapply;

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
                OnApply?.Invoke();
                player.UpdateGravityScale();
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
                OnUnapply?.Invoke();
                player.UpdateGravityScale();
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

        public bool JumpEnabled()
        {
            return jumpEnabled;
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
        bool JumpEnabled();
        float GravityScale();
        float JumpFactor();
        List<Sprite> InTheAirSprites();
        float InTheAirAnimationTimeInBetween();
    }
}