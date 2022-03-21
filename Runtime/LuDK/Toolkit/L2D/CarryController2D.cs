using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(PlayerController2D))]
    public class CarryController2D : MonoBehaviour
    {
        public CarryMethod carryMethod;
        public KeyCode carryKey;
        public KeyCode actionKey;

        private Carryable thingToCarry;
        private GameObject thingToCarryGO;

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
                    thingToCarry = carryable;
                    thingToCarryGO = collision.gameObject;
                    originalSortingOrder = thingToCarryGO.GetComponent<SpriteRenderer>().sortingOrder;
                    thingToCarryGO.GetComponent<Collider2D>().enabled = false;                   
                    originalLocalScale = thingToCarryGO.transform.localScale;
                    originalLocalRotation = thingToCarryGO.transform.localEulerAngles;
                    thingToCarry.OnTake();
                }
            }
        }

        /// <summary>
        /// To consume the carried object.
        /// </summary>
        public void Consume()
        {
            if (null != thingToCarry)
            {
                thingToCarry.OnConsume();
            }
            if (null != thingToCarryGO)
            {            
                thingToCarryGO.SetActive(false);
                Destroy(thingToCarryGO);
            }
            thingToCarry = null;
            thingToCarryGO = null;
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

                if (player.IsFreeToMove() && !needToDropCarriedObject && thingToCarry != null)
                {
                    if (Input.GetKeyDown(actionKey))
                    {
                        thingToCarry.ActionStart();
                    } else if (Input.GetKeyUp(actionKey))
                    {
                        thingToCarry.ActionEnd();
                    } else if (Input.GetKey(actionKey))
                    {
                        thingToCarry.ActionRun();
                    }
                }

                if (needToDropCarriedObject && player.IsFreeToMove() && thingToCarryGO != null)
                {
                    Rigidbody2D body = thingToCarryGO.GetComponent<Rigidbody2D>();
                    if (null != body)
                    {
                        body.velocity = Vector2.zero;
                    }
                    ignoreNextCollisionDelay = carryMethod == CarryMethod.Stick ? 0.5f : 0.0f;
                    thingToCarry.OnDrop();
                    thingToCarry = null;
                    thingToCarryGO.transform.position = gameObject.transform.position + new Vector3(player.IsLookingToRight() ? 0.5f : -0.5f, 0, 0);
                    thingToCarryGO.transform.localScale = originalLocalScale;
                    thingToCarryGO.transform.localEulerAngles = originalLocalRotation;
                    thingToCarryGO.GetComponent<Collider2D>().enabled = true;
                    thingToCarryGO.GetComponent<SpriteRenderer>().sortingOrder = originalSortingOrder;
                    thingToCarryGO = null;
                }
                else
                {
                    float newScaleX = originalLocalScale.x;
                    int newSortingOrder = 100;
                    if (!player.IsLookingToRight())
                    {
                        newScaleX = -newScaleX;
                        if (thingToCarry != null && !thingToCarry.AlwaysOnTop())
                        {
                            newSortingOrder = playerSortingOrder - 1;
                        }
                    }
                    thingToCarryGO.GetComponent<SpriteRenderer>().sortingOrder = newSortingOrder;
                    thingToCarryGO.transform.localScale = new Vector3(newScaleX, originalLocalScale.y, originalLocalScale.z);
                    thingToCarryGO.transform.position = gameObject.transform.position + DeltaPos();
                    thingToCarryGO.transform.localEulerAngles = Rotation();
                }
            }
        }

        public Vector3 Rotation()
        {
            Vector3 rotation = Vector3.zero;
            if (thingToCarry != null)
            {
                float rZ = thingToCarry.GetHoldingRotation();
                if (!player.IsLookingToRight())
                {
                    rZ *= -1f;
                }
                rotation = new Vector3(0, 0, rZ);
            }
            return rotation;
        }

        public Vector3 DeltaPos()
        {            
            float deltaX = 0f;
            float deltaY = 0f;
            if (thingToCarry != null)
            {
                bool lookingToRight = player.IsLookingToRight();
                deltaX = thingToCarry.GetDeltaX(lookingToRight);
                deltaY = thingToCarry.GetDeltaY(lookingToRight);
            }            
            return new Vector3(deltaX, deltaY, 0);
        }

        /// <summary>
        /// To get the carried object.
        /// </summary>
        /// <returns></returns>
        public GameObject GetObject()
        {
            return thingToCarryGO;
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