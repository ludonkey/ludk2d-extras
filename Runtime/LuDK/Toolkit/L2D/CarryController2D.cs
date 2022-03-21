using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(PlayerController2D))]
    public class CarryController2D : MonoBehaviour
    {
        public bool carryOnFront = false;
        public bool alwaysOnTop = true;
        public CarryMethod carryMethod;
        public KeyCode carryKey;
        public KeyCode actionKey;
        public UnityEvent OnTake;
        public UnityEvent OnDrop;
        public UnityEvent OnConsume;

        private GameObject thingToCarry;
        private Carryable thingToCarryAdvanced;

        private PlayerController2D player;

        private int playerSortingOrder { get;  set; }

        private float ignoreNextCollisionDelay = 0f;
        private Vector3 originalLocalScale;
        private int originalSortingOrder;
        private Vector3 originalLocalRotation;

        void Reset()
        {
            if (carryKey == KeyCode.None)
            {
                carryKey = KeyCode.LeftAlt;
            }
            if (actionKey == KeyCode.None)
            {
                actionKey = KeyCode.LeftControl;
            }
        }

        private void Awake()
        {
            player = GetComponent<PlayerController2D>();
            playerSortingOrder = player.GetComponent<SpriteRenderer>().sortingOrder;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (thingToCarry == null)
            {
                var carryable = collision.gameObject.GetComponents<Component>().OfType<Carryable>().FirstOrDefault<Carryable>();
                if (carryable != null)
                {
                    if (ignoreNextCollisionDelay > 0f)
                    {
                        return;
                    }
                    if (carryMethod == CarryMethod.HoldKey && !Input.GetKey(carryKey))
                    {
                        return;
                    }
                    thingToCarry = collision.gameObject;
                    originalSortingOrder = thingToCarry.GetComponent<SpriteRenderer>().sortingOrder;
                    thingToCarry.GetComponent<Collider2D>().enabled = false;                   
                    originalLocalScale = thingToCarry.transform.localScale;
                    originalLocalRotation = thingToCarry.transform.localEulerAngles;
                    if (carryable != null)
                    {
                        thingToCarryAdvanced = carryable;
                        thingToCarryAdvanced.OnTake();
                    }
                    OnTake.Invoke();
                }
            }
        }

        /// <summary>
        /// To consume the carried object.
        /// </summary>
        public void Consume()
        {
            if (null != thingToCarryAdvanced)
            {
                thingToCarryAdvanced.OnConsume();
            }
            if (null != thingToCarry)
            {            
                OnConsume.Invoke();
                thingToCarry.SetActive(false);
            }
            Destroy(thingToCarry);
            thingToCarry = null;
            thingToCarryAdvanced = null;
        }

        private void LateUpdate()
        {
            if (ignoreNextCollisionDelay > 0.0)
            {
                ignoreNextCollisionDelay -= Time.deltaTime;
            }
            if (thingToCarry != null)
            {               
                bool needToDropCarriedObject = (Input.GetKey(carryKey) && carryMethod == CarryMethod.Stick) 
                    || (!Input.GetKey(carryKey) && carryMethod == CarryMethod.HoldKey);

                if (player.IsFreeToMove() && !needToDropCarriedObject && thingToCarryAdvanced != null)
                {
                    if (Input.GetKeyDown(actionKey))
                    {
                        thingToCarryAdvanced.ActionStart();
                    } else if (Input.GetKeyUp(actionKey))
                    {
                        thingToCarryAdvanced.ActionEnd();
                    } else if (Input.GetKey(actionKey))
                    {
                        thingToCarryAdvanced.ActionRun();
                    }
                }

                if (needToDropCarriedObject && player.IsFreeToMove())
                {
                    Rigidbody2D body = thingToCarry.GetComponent<Rigidbody2D>();
                    if (null != body)
                    {
                        body.velocity = Vector2.zero;
                    }
                    ignoreNextCollisionDelay = carryMethod == CarryMethod.Stick ? 0.5f : 0.0f;
                    if (null != thingToCarryAdvanced)
                    {
                        thingToCarryAdvanced.OnDrop();
                    }
                    thingToCarryAdvanced = null;
                    thingToCarry.transform.position = gameObject.transform.position + new Vector3(player.IsLookingToRight() ? 0.5f : -0.5f, 0, 0);
                    thingToCarry.transform.localScale = originalLocalScale;
                    thingToCarry.transform.localEulerAngles = originalLocalRotation;
                    thingToCarry.GetComponent<Collider2D>().enabled = true;
                    thingToCarry.GetComponent<SpriteRenderer>().sortingOrder = originalSortingOrder;
                    thingToCarry = null;
                    OnDrop.Invoke();
                }
                else
                {
                    float newScaleX = originalLocalScale.x;
                    int newSortingOrder = 100;
                    if (!player.IsLookingToRight())
                    {
                        newScaleX = -newScaleX;
                        if (thingToCarryAdvanced != null)
                        {
                            if (!thingToCarryAdvanced.AlwaysOnTop())
                            {
                                newSortingOrder = playerSortingOrder - 1;
                            }
                        } else
                        {
                            if (!alwaysOnTop)
                            {
                                newSortingOrder = playerSortingOrder - 1;
                            }
                        }
                    }
                    thingToCarry.GetComponent<SpriteRenderer>().sortingOrder = newSortingOrder;
                    thingToCarry.transform.localScale = new Vector3(newScaleX, originalLocalScale.y, originalLocalScale.z);
                    thingToCarry.transform.position = gameObject.transform.position + DeltaPos();
                    thingToCarry.transform.localEulerAngles = Rotation();
                }
            }
        }

        private Vector3 Rotation()
        {
            Vector3 rotation = Vector3.zero;
            if (thingToCarryAdvanced != null)
            {
                float rZ = thingToCarryAdvanced.GetHoldingRotation();
                if (!player.IsLookingToRight())
                {
                    rZ *= -1f;
                }
                rotation = new Vector3(0, 0, rZ);
            }
            return rotation;
        }

        private Vector3 DeltaPos()
        {
            bool lookingToRight = player.IsLookingToRight();
            float deltaX = carryOnFront ? 0.3f : -0.3f;
            if (!lookingToRight)
            {
                deltaX *= -1f;
            }
            float deltaY = 0f;
            if (thingToCarryAdvanced != null)
            {
                deltaX = thingToCarryAdvanced.GetDeltaX(lookingToRight);
                deltaY = thingToCarryAdvanced.GetDeltaY(lookingToRight);
            }            
            return new Vector3(deltaX, deltaY, 0);
        }

        /// <summary>
        /// To get the carried object.
        /// </summary>
        /// <returns></returns>
        public GameObject GetObject()
        {
            return thingToCarry;
        }

        public enum CarryMethod
        {
            Stick,
            HoldKey
        }

        public interface Carryable
        {
            float GetDeltaX(bool front);
            float GetDeltaY(bool front);
            float GetHoldingRotation();
            bool AlwaysOnTop();
            void OnTake();
            void OnDrop();
            void OnConsume();
            void ActionStart();
            void ActionRun();
            void ActionEnd();
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/LuDK/2D/Player/CreateWithCarryController")]
        public static GameObject CreatePlayer()
        {
            GameObject newObj = PlayerController2D.CreateObject();
            var cc = newObj.AddComponent<CarryController2D>();
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Player/CreateWithCarryController", true)]
        private static bool ValidationPlayer()
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

        [UnityEditor.MenuItem("Assets/LuDK/2D/Carriable")]
        public static GameObject CreateObject()
        {
            GameObject newObj = new GameObject();
            newObj.name = "Carriable";
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            var sr = newObj.AddComponent<SpriteRenderer>();
            sr.sprite = (Sprite)UnityEditor.Selection.activeObject;
            sr.sortingLayerName = PlayerController2D.LAYER_MIDDLEGROUND;
            sr.sortingOrder = 1;
            newObj.AddComponent<PolygonCollider2D>();
            var rb = newObj.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.gravityScale = 0;
            newObj.AddComponent<CarriedObject2D>();
            UnityEditor.Selection.activeObject = newObj;
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Carriable", true)]
        private static bool Validation()
        {
            return UnityEditor.Selection.activeObject is Sprite && UnityEditor.Selection.objects.Length == 1;
        }
                       
#endif
    }
}