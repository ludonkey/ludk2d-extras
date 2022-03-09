using LuDK.Toolkit.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController2D : MonoBehaviour
    {
        public static string GROUND_DEFAULT_LAYER = "Ground";

        public static string LAYER_BACKGROUND = "BackGround";
        public static string LAYER_MIDDLEGROUND = "MiddleGround";
        public static string LAYER_FOREGROUND = "ForeGround";

        private Rigidbody2D body;
        private SpriteRenderer sr;

        private float horizontalMove;
        private float verticalMove;
        private float moveLimiter = 0.7f;
      
        public GameType2D gameType = GameType2D.TopDown;
        public float runSpeed = 3.0f;
        public bool flipAnimation = false;
        public List<Sprite> sprites;
        public float animationDelay = 0.1f;

        [Header("Only for SideScroller")]
        public LayerMask groundLayer;
        public Vector2 gravity2D = new Vector2(0, -40);
        public KeyCode jumpKey;
        public float jumpFactor = 1.4f;
        public Sprite jumpingSprite;
        public float durationConsideredAsGrounded = 0.1f;
        public UnityEvent OnJump;

        private float animationEllapsedTime;
        private int currentSpriteIndex = 0;

        private Vector2 forcedAnimationRelativeDestination;        
        private Vector2 forcedAnimationFinalPosition;
        private float forcedAnimationDistanceToEllapsed;
        private float forcedAnimationSpeedFactor;
        private float forcedAnimationDelay;

        private Vector3 lastPosition { get; set; }
        private ContactFilter2D contactFilter;
        public bool consideredAsGrounded { get; private set; }
        private float timeWhereNoGrounded { get; set; }

        void Reset()
        {
            if (jumpKey == KeyCode.None)
            {
                jumpKey = KeyCode.Space;
            }
        }

        private void Awake()
        {
            contactFilter = new ContactFilter2D();
            contactFilter.useLayerMask = true;
            contactFilter.SetLayerMask(groundLayer);
            Physics2D.gravity = gravity2D;
            body = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            SetGameType(gameType);
            ResetForcedAnimation();
        }

        void Update()
        {
            if (gameType == GameType2D.SideScroller)
            {
                bool nowGrounded = isNowGrounded;
                consideredAsGrounded = nowGrounded ? true : timeWhereNoGrounded <= durationConsideredAsGrounded;
                if (nowGrounded)
                {
                    timeWhereNoGrounded = 0f;
                    if (canLand)
                    {
                        isJumping = false;
                    }
                }
                else if (consideredAsGrounded) // but not nowGrounded
                {
                    timeWhereNoGrounded += Time.deltaTime;
                    canLand = true;
                }
            }
            if (forcedAnimationDistanceToEllapsed <= 0f)
            {
                horizontalMove = Input.GetAxisRaw("Horizontal"); // -1 is left
                verticalMove = Input.GetAxisRaw("Vertical"); // -1 is down
                if (gameType == GameType2D.SideScroller && Input.GetKeyDown(jumpKey) && consideredAsGrounded)
                {
                    body.velocity = new Vector2(body.velocity.x, Math.Max(0, body.velocity.y));
                    body.AddForce(new Vector2(0, 500f * jumpFactor));
                    isJumping = true;
                    canLand = false;
                    OnJump.Invoke();
                }
            } else
            {
                horizontalMove = forcedAnimationRelativeDestination.normalized.x;
                verticalMove = forcedAnimationRelativeDestination.normalized.y;
            }
        }

        private void OnDisable()
        {
            body.velocity = Vector3.zero;
        }

        void FixedUpdate()
        {            
            if (forcedAnimationDelay > 0)
            {
                forcedAnimationDelay -= Time.fixedDeltaTime;
                return;
            }
            if (forcedAnimationDistanceToEllapsed > 0)
            {
                float ellapsedDistance = Vector3.Distance(transform.position, lastPosition);
                forcedAnimationDistanceToEllapsed -= ellapsedDistance;
                if (forcedAnimationDistanceToEllapsed <= 0)
                {
                    transform.localPosition = forcedAnimationFinalPosition;
                    ResetForcedAnimation();
                    lastPosition = transform.position;
                    return;
                }
            }
            switch (gameType)
            {
                default:
                case GameType2D.TopDown:
                    if (horizontalMove != 0 && verticalMove != 0) // Check for diagonal movement
                    {
                        // limit movement speed diagonally, so you move at 70% speed
                        horizontalMove *= moveLimiter;
                        verticalMove *= moveLimiter;
                    }
                    body.velocity = new Vector2(
                         horizontalMove * runSpeed * forcedAnimationSpeedFactor,
                         verticalMove * runSpeed * forcedAnimationSpeedFactor);
                    break;
                case GameType2D.SideScroller:
                    body.velocity = new Vector2(
                         horizontalMove * runSpeed * forcedAnimationSpeedFactor,
                         body.velocity.y);                
                    break;
            }

            // flip X
            if (horizontalMove < 0)
            {
                sr.flipX = !flipAnimation;
            }
            else if (horizontalMove > 0)
            {
                sr.flipX = flipAnimation;
            }
            // animated sprite
            if (sprites != null && sprites.Count > 0 && !isJumping)
            {
                if (body.velocity.magnitude > 0.1f)
                {
                    animationEllapsedTime += Time.deltaTime;
                    if (animationEllapsedTime >= animationDelay)
                    {
                        animationEllapsedTime = 0;
                        currentSpriteIndex++;
                        if (currentSpriteIndex >= sprites.Count)
                            currentSpriteIndex = 0;
                    }
                }
                sr.sprite = sprites[currentSpriteIndex];
            } else if (isJumping && jumpingSprite != null)
            {
                sr.sprite = jumpingSprite;
            }
            lastPosition = transform.position;
        }

        public void SetSideScroller(bool isSideScroller)
        {
            SetGameType(isSideScroller ? GameType2D.SideScroller : GameType2D.TopDown);
        }

        public void SetGameType(GameType2D newGameType)
        {

            gameType = newGameType;
            isJumping = false;
            canLand = true;
            UpdateGravityScale();
        }

        private void UpdateGravityScale()
        {
            switch (gameType)
            {
                default:
                case GameType2D.TopDown:
                    body.gravityScale = 0;
                    break;
                case GameType2D.SideScroller:
                    body.gravityScale = 1;
                    break;
            }
        }

        private bool isNowGrounded
        {
            get
            {
                if (gameType == GameType2D.TopDown)
                {
                    return true;
                }
                if (contactFilter.layerMask.value == 0)
                {
                    return false;
                }
                ContactPoint2D[] contactPoints = new ContactPoint2D[10];
                int count = body.GetContacts(contactFilter, contactPoints);
                for (int i = 0; i < count; ++i)
                {

                    if (Vector2.Dot(Vector2.up, contactPoints[i].normal) > .3f)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool isJumping { get; private set; }
        public bool canLand { get; private set; }

        public bool IsFreeToMove()
        {
            return forcedAnimationDistanceToEllapsed <= 0;
        }

        public bool IsLookingToRight()
        {
            return !sr.flipX && !flipAnimation;
        }

        private void ResetForcedAnimation()
        {
            GetComponent<Collider2D>().enabled = true;
            forcedAnimationRelativeDestination = Vector2.zero;
            forcedAnimationDistanceToEllapsed = 0;
            forcedAnimationSpeedFactor = 1.0f;
            forcedAnimationDelay = 0;
            body.velocity = Vector3.zero;
        }

        /// <summary>
        /// Force the player to move until he reachs his destination.
        /// See the complete usage with ForcedMove(x, y, offsetX, offsetY, speedFactor, delay)
        /// </summary>
        /// <param name="movement">X_Y_OffSetX_OffSetY_SpeedFactor_Delay</param>
        public void ForcedMove(string movement)
        {            
            float x = 0;
            float y = 0;
            float offsetX = 0;
            float offsetY = 0;
            float speedFactor = 0;
            float delay = 0;

            bool thereIsX = false;
            bool thereIsY = false;
            string[] parameters = movement.Split('_');
            if (parameters.Length >= 1)
            {
                string oneParam = parameters[0];
                thereIsX = float.TryParse(oneParam, NumberStyles.Float, CultureInfo.InvariantCulture, out x);
            }
            if (parameters.Length >= 2)
            {
                string oneParam = parameters[1];
                thereIsY = float.TryParse(oneParam, NumberStyles.Float, CultureInfo.InvariantCulture, out y);
            }
            if (parameters.Length >= 3)
            {
                string oneParam = parameters[2];
                float.TryParse(oneParam, NumberStyles.Float, CultureInfo.InvariantCulture, out offsetX);
            }
            if (parameters.Length >= 4)
            {
                string oneParam = parameters[3];
                float.TryParse(oneParam, NumberStyles.Float, CultureInfo.InvariantCulture, out offsetY);
            }
            if (parameters.Length >= 5)
            {
                string oneParam = parameters[4];
                float.TryParse(oneParam, NumberStyles.Float, CultureInfo.InvariantCulture, out speedFactor);
            }
            if (parameters.Length >= 6)
            {
                string oneParam = parameters[5];
                float.TryParse(oneParam, NumberStyles.Float, CultureInfo.InvariantCulture, out delay);
            }
            if (thereIsX && thereIsY)
            {
                ForcedMove(x, y, offsetX, offsetY, speedFactor, delay);
            }
        }

        /// <summary>
        /// Force the player to move until he reachs his destination.
        /// </summary>
        /// <param name="x">x amount to move</param>
        /// <param name="y">y amount to move</param>
        /// <param name="offsetX">x offset applied before to move</param>
        /// <param name="offsetY">y offser applied before to move</param>
        /// <param name="speedFactor">speed factor (1 = normal)</param>
        /// <param name="delay">delay to wait before starting the forced move</param>
        public void ForcedMove(float x, float y, float offsetX = 0, float offsetY = 0, float speedFactor = 1, float delay = 0)
        {
            body.velocity = Vector3.zero;
            transform.position += new Vector3(offsetX, offsetY, 0);
            forcedAnimationSpeedFactor = speedFactor;
            forcedAnimationDelay = delay;
            lastPosition = transform.position;
            GetComponent<Collider2D>().enabled = false;
            forcedAnimationRelativeDestination = new Vector2(x, y) - new Vector2(offsetX, offsetY);
            forcedAnimationFinalPosition = transform.localPosition + (Vector3)forcedAnimationRelativeDestination;
            forcedAnimationDistanceToEllapsed = forcedAnimationRelativeDestination.magnitude;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/LuDK/2D/Player/CreateSimple")]
        public static GameObject CreateObject()
        {
            GameObject newObj = new GameObject();
            newObj.name = "Player";
            newObj.transform.position = GetMiddleOfScreenWorldPos();
            var sr = newObj.AddComponent<SpriteRenderer>();
            sr.sprite = (Sprite)UnityEditor.Selection.objects[0];
            sr.sortingLayerName = PlayerController2D.LAYER_MIDDLEGROUND;
            newObj.AddComponent<CircleCollider2D>();
            Rigidbody2D body = newObj.AddComponent<Rigidbody2D>();
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            PlayerController2D pc = newObj.GetComponent<PlayerController2D>();
            if (pc == null)
            {
                pc = newObj.AddComponent<PlayerController2D>();
            }
            pc.groundLayer = 1 << LayerMask.NameToLayer(GROUND_DEFAULT_LAYER);
            pc.sprites = new List<Sprite>();
            foreach (var oneObj in UnityEditor.Selection.objects)
            {
                pc.sprites.Add((Sprite)oneObj);
            }
            UnityEditor.Selection.activeObject = newObj;
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Player/CreateSimple", true)]
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
            return true;
        }

        /// <summary>
        /// Get the 3D coordinates (with Z = 0) for the middle of the scene view.
        /// </summary>
        /// <returns>The 3D coordinates (with Z = 0) for the middle of the scene view.</returns>
        public static Vector3 GetMiddleOfScreenWorldPos()
        {
            Vector3 pos = Vector3.zero;
            if (UnityEditor.SceneView.lastActiveSceneView != null)
            {
                pos = UnityEditor.SceneView.lastActiveSceneView.camera.ViewportToWorldPoint(new Vector2(0.5f, 0.5f));
            }
            Vector3 middleOfScreen = new Vector3((int)Math.Round(pos.x) + 0.5f, (int)Math.Round(pos.y) + 0.5f, 0);
            return middleOfScreen;
        }
#endif
    }
}

[Serializable]
public enum GameType2D
{
    TopDown,
    SideScroller
}