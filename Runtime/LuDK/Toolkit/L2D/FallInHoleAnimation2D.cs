using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    public class FallInHoleAnimation2D : MonoBehaviour
    {
        private float animationDuration;
        private float animationEllapsedTime;

        private GameObject fallAnimationGO { get; set; }

        private void Update()
        {
            if (animationEllapsedTime < animationDuration)
            {
                animationEllapsedTime += Time.deltaTime;
                if (animationEllapsedTime >= animationDuration)
                {
                    animationDuration = 0;
                    animationEllapsedTime = 0;
                    Destroy(fallAnimationGO);
                } else
                {
                    float scale = 1f - animationEllapsedTime / animationDuration;
                    fallAnimationGO.transform.localScale = new Vector3(scale, scale, 1);
                }
            }
        }

        /// <summary>
        /// Play the falling animation of the player (just scale down to 0).
        /// </summary>
        /// <param name="duration">The duration of the animaiton.</param>
        public void Play(float duration = 1.0f)
        {
            PlayerController2D player = GameObject.FindObjectOfType<PlayerController2D>();
            if (player == null)
            {
                return;
            }
            SpriteRenderer playerSR = player.GetComponent<SpriteRenderer>();
            if (playerSR == null)
            {
                return;
            }
            animationDuration = duration;
            fallAnimationGO = new GameObject();
            fallAnimationGO.name = "fallingAnimation";
            fallAnimationGO.transform.parent = transform;
            fallAnimationGO.transform.position = transform.position;
            SpriteRenderer sr = fallAnimationGO.AddComponent<SpriteRenderer>();
            sr.sprite = playerSR.sprite;
            sr.sortingLayerID = playerSR.sortingLayerID;
            sr.sortingOrder = playerSR.sortingOrder;
            sr.flipX = playerSR.flipX;
            sr.flipY = playerSR.flipY;
            sr.color = playerSR.color;
            CarryController2D cc = player.GetComponent<CarryController2D>();
            if (cc != null && cc.GetObject() != null)
            {
                var carryObjectSR = cc.GetObject().GetComponent<SpriteRenderer>();
                if (carryObjectSR != null)
                {
                    GameObject copyCarriedObject = new GameObject();
                    copyCarriedObject.name = "fallingCarriedObject";
                    copyCarriedObject.transform.parent = fallAnimationGO.transform;
                    copyCarriedObject.transform.position = fallAnimationGO.transform.position + cc.DeltaPos();
                    copyCarriedObject.transform.localEulerAngles = cc.Rotation();
                    SpriteRenderer copyCarriedObjectSR = copyCarriedObject.AddComponent<SpriteRenderer>();
                    copyCarriedObjectSR.sprite = carryObjectSR.sprite;
                    copyCarriedObjectSR.sortingLayerID = carryObjectSR.sortingLayerID;
                    copyCarriedObjectSR.sortingOrder = carryObjectSR.sortingOrder;
                    copyCarriedObjectSR.flipX = carryObjectSR.flipX;
                    copyCarriedObjectSR.flipY = carryObjectSR.flipY;
                    copyCarriedObjectSR.color = carryObjectSR.color;
                }
            }
        }
    }
}