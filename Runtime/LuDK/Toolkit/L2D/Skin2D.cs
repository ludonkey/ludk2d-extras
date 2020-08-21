using System.Collections.Generic;
using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    public class Skin2D : MonoBehaviour
    {
        public float runSpeed = 3.0f;
        public bool flipAnimation = false;
        public List<Sprite> sprites;
        public float animationDelay = 0.1f;

        private PlayerController2D player { get; set; }

        void Start()
        {
            player = GameObject.FindObjectOfType<PlayerController2D>();
        }

        /// <summary>
        /// Call this method to apply this skin to the player.
        /// </summary>
        public void Apply()
        {
            if (player != null && sprites != null && sprites.Count > 0)
            {
                player.runSpeed = runSpeed;
                player.flipAnimation = flipAnimation;
                player.sprites = sprites;
                player.animationDelay = animationDelay;
                player.GetComponent<SpriteRenderer>().sprite = player.sprites[0];
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
#endif
    }
}