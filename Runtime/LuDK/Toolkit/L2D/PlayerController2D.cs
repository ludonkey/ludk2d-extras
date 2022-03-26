using System;
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
        public float moveSpeed = 3.0f;
        public float MoveSpeed { get { return skin != null ? skin.MoveSpeed() : moveSpeed; } }
        public bool flipXAnimation = false;
        public bool FlipXAnimation { get { return skin != null ? skin.FlipXAnimation() : flipXAnimation; } }
        public bool flipYAnimation = false;
        public bool FlipYAnimation { get { return skin != null ? skin.FlipYAnimation() : flipYAnimation; } }
        public List<Sprite> sprites;
        public List<Sprite> Sprites { get { return skin != null ? skin.Sprites() : sprites; } }
        public float animationTimeInBetween = 0.1f;
        public float AnimationTimeInBetween { get { return skin != null ? skin.AnimationTimeInBetween() : animationTimeInBetween; } }

        public List<Sprite> idleSprites;
        public List<Sprite> IdleSprites { get { return skin != null ? skin.IdleSprites() : idleSprites; } }
        public float animationIdleTimeInBetween = 0.1f;
        public float AnimationIdleTimeInBetween { get { return skin != null ? skin.AnimationIdleTimeInBetween() : animationIdleTimeInBetween; } }
   
        [Header("Only for Platformer")]
        public bool jumpEnabled = true;
        public bool JumpEnabled { get { return skin != null ? skin.JumpEnabled() : jumpEnabled; } }
        public Vector2 worldGravity2D = new Vector2(0, -40);
        public float gravityScale = 1f;
        public float GravityScale { get { return skin != null ? skin.GravityScale() : gravityScale; } }
        public float durationInTheAirConsideredAsGrounded = 0.2f;
        private bool gracePeriodForJumpEnable;
        public LayerMask groundLayer;
        public KeyCode jumpKey;
        public float jumpFactor = 1.4f;
        public float JumpFactor { get { return skin != null ? skin.JumpFactor() : jumpFactor; } }
        public List<Sprite> inTheAirSprites;
        public List<Sprite> InTheAirSprites { get { return skin != null ? skin.InTheAirSprites() : inTheAirSprites; } }
        public float inTheAirAnimationTimeInBetween = 0.1f;
        public float InTheAirAnimationTimeInBetween { get { return skin != null ? skin.InTheAirAnimationTimeInBetween() : inTheAirAnimationTimeInBetween; } }
        public UnityEvent OnJump;
        public UnityEvent OnLand;

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
        private float timeInTheAir { get; set; }

        private ISkin2D internalSkin;

        public ISkin2D skin
        {
            get
            {
                return internalSkin;
            }
            set
            {
                internalSkin = value;
                animationEllapsedTime = 0;
                currentSpriteIndex = 0;
                UpdateGravityScale();
                UpdateCurrentSprite();
            }
        }

        void Reset()
        {
            if (jumpKey == KeyCode.None)
            {
                jumpKey = KeyCode.Space;
            }
        }

        private void Awake()
        {
            gracePeriodForJumpEnable = true;
            if (sprites == null)
                sprites = new List<Sprite>();
            if (idleSprites == null)
                idleSprites = new List<Sprite>();
            if (inTheAirSprites == null)
                inTheAirSprites = new List<Sprite>();
            contactFilter = new ContactFilter2D();
            contactFilter.useLayerMask = true;
            contactFilter.SetLayerMask(groundLayer);
            Physics2D.gravity = worldGravity2D;
            body = GetComponent<Rigidbody2D>();
            sr = GetComponent<SpriteRenderer>();
            SetGameType(gameType);
            ResetForcedAnimation();
        }

        void Update()
        {
            if (gameType == GameType2D.Platformer)
            {
                bool physicallyGrounded = isNowGrounded;
                consideredAsGrounded = physicallyGrounded || (gracePeriodForJumpEnable && (timeInTheAir <= durationInTheAirConsideredAsGrounded));

                bool previousConsideredAsInTheAir = consideredAsInTheAir;
                consideredAsInTheAir = !consideredAsGrounded;
                if (previousConsideredAsInTheAir != consideredAsInTheAir)
                {
                    animationEllapsedTime = 0;
                    currentSpriteIndex = 0;
                    if (!consideredAsInTheAir)
                    {
                        gracePeriodForJumpEnable = true;
                        OnLand.Invoke();
                        //Debug.Log("OnLand");
                    }
                }
                if (physicallyGrounded)
                {
                    timeInTheAir = 0f;
                }
                else
                {
                    timeInTheAir += Time.deltaTime;
                }
            }
            if (forcedAnimationDistanceToEllapsed <= 0f)
            {
                horizontalMove = Input.GetAxisRaw("Horizontal"); // -1 is left
                verticalMove = Input.GetAxisRaw("Vertical"); // -1 is down
                if (gameType == GameType2D.Platformer && Input.GetKeyDown(jumpKey) && consideredAsGrounded && JumpEnabled)
                {
                    body.velocity = new Vector2(body.velocity.x, 0);
                    body.AddForce(new Vector2(0, 500f * JumpFactor * Mathf.Sign(GravityScale)));
                    animationEllapsedTime = 0;
                    currentSpriteIndex = 0;
                    gracePeriodForJumpEnable = false;
                    OnJump.Invoke();
                    //Debug.Log("OnJump");
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
                         horizontalMove * MoveSpeed * forcedAnimationSpeedFactor,
                         verticalMove * MoveSpeed * forcedAnimationSpeedFactor);
                    break;
                case GameType2D.Platformer:
                    body.velocity = new Vector2(
                         horizontalMove * MoveSpeed * forcedAnimationSpeedFactor,
                         body.velocity.y);
                    break;
            }
            UpdateCurrentSprite();
            lastPosition = transform.position;
        }

        private void UpdateCurrentSprite()
        {
            sr.flipY = FlipYAnimation;
            // flip X
            if (horizontalMove < 0)
            {
                sr.flipX = !FlipXAnimation;
            }
            else if (horizontalMove > 0)
            {
                sr.flipX = FlipXAnimation;
            }
            // animated sprite
            var spritesToUse = consideredAsInTheAir ? InTheAirSprites : Sprites;
            if (body.velocity.magnitude > 0.1f)
            {
                animationEllapsedTime += Time.deltaTime;
                if (animationEllapsedTime >= (consideredAsInTheAir ? InTheAirAnimationTimeInBetween : AnimationTimeInBetween))
                {
                    animationEllapsedTime = 0;
                    currentSpriteIndex++;
                    if (currentSpriteIndex >= spritesToUse.Count)
                        currentSpriteIndex = 0;
                }
            } else if (IdleSprites.Count > 0)
            {
                spritesToUse = IdleSprites;
                animationEllapsedTime += Time.deltaTime;
                if (animationEllapsedTime >=  AnimationIdleTimeInBetween)
                {
                    animationEllapsedTime = 0;
                    currentSpriteIndex++;
                    if (currentSpriteIndex >= spritesToUse.Count)
                        currentSpriteIndex = 0;
                }
            }
            if (spritesToUse.Count > currentSpriteIndex)
            {
                sr.sprite = spritesToUse[currentSpriteIndex];
            }
        }

        public void SetSideScroller(bool isSideScroller)
        {
            SetGameType(isSideScroller ? GameType2D.Platformer : GameType2D.TopDown);
        }

        public void SetGameType(GameType2D newGameType)
        {

            gameType = newGameType;
            if (gameType != GameType2D.Platformer)
            {
                consideredAsInTheAir = false;
            }
            UpdateGravityScale();
        }

        public void UpdateGravityScale()
        {
            switch (gameType)
            {
                default:
                case GameType2D.TopDown:
                    body.gravityScale = 0;
                    break;
                case GameType2D.Platformer:
                    body.gravityScale = GravityScale;
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

                    if (Vector2.Dot(Mathf.Sign(GravityScale) * Vector2.up, contactPoints[i].normal) > .3f)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool consideredAsInTheAir { get; private set; }

        public bool IsFreeToMove()
        {
            return forcedAnimationDistanceToEllapsed <= 0;
        }

        public bool IsLookingToRight()
        {
            return !sr.flipX && !FlipXAnimation;
        }

        public bool IsUpsideDown()
        {
            return sr.flipY;
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

        public void ChangeSortingLayerName(string newSortingLayerName)
        {
            var sr = GetComponent<SpriteRenderer>();
            if (null != sr)
            {
                sr.sortingLayerName = newSortingLayerName;
            }
            var cc = GetComponent<CarryController2D>();
            if (null != cc)
            {
                var carriedObjSR = cc.GetObject()?.GetComponent<SpriteRenderer>();
                if (null != carriedObjSR)
                {
                    carriedObjSR.sortingLayerName = newSortingLayerName;
                }
            }
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
    Platformer
}