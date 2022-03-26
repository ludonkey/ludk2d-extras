using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace LuDK.Toolkit.L2D
{
    public class TriggerController2D : MonoBehaviour
    {
        [Flags]
        public enum CheckType
        {
            Player = 1,
            CarriedObject = 2,
            DirectObject = 4,
            Particle = 8,
            Skin = 16
        }

        public CheckType checkType = CheckType.Player;

        public bool consumeObject = false;

        public string objectName;

        public UnityEvent onEnter;

        public UnityEvent onExit;

        private float nbInteractions = 0;

        private float disableTime = 0.0f;

        public void DisableTemporarily(float duration)
        {
            disableTime = duration;
        }

        private void Update()
        {
            if (disableTime > 0f)
                disableTime -= Time.deltaTime;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject goInContact = collision.gameObject;
            OnObjectEnter2D(goInContact);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject goInContact = collision.gameObject;
            OnObjectEnter2D(goInContact);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            GameObject goInContact = collision.gameObject;
            OnObjectExit2D(goInContact);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            GameObject goInContact = collision.gameObject;
            OnObjectExit2D(goInContact);
        }

        private void OnParticleCollision(GameObject goInContact)
        {
            Process(goInContact, true);
        }
            
        private void OnObjectEnter2D(GameObject goInContact)
        {
            if (Process(goInContact, true))
                nbInteractions++;
        }

        private void OnObjectExit2D(GameObject goInContact)
        {
            if (Process(goInContact, false))
                nbInteractions--;
        }

        private bool Process(GameObject goInContact, bool entered)
        {
            if (!enabled)
            {
                return false;
            }

            if ((checkType & CheckType.Particle) == CheckType.Particle)
            {
                bool objectIsOk = string.IsNullOrEmpty(objectName) || objectName == goInContact.name;
                if (objectIsOk)
                {
                    onEnter.Invoke();
                    return true;
                }
            }

            if ((checkType & CheckType.CarriedObject) == CheckType.CarriedObject)
            {
                CarryController2D carrier = goInContact.GetComponent<CarryController2D>();
                if (carrier != null)
                {
                    GameObject carriedObject = carrier.GetObject();
                    if (carriedObject != null)
                    {
                        bool objectIsOk = string.IsNullOrEmpty(objectName) || objectName == carriedObject.name;
                        if (objectIsOk)
                        {
                            if (entered)
                            {
                                if (nbInteractions == 0 && disableTime <= 0)
                                    onEnter.Invoke();
                            }
                            else
                            {
                                if (nbInteractions == 1 && disableTime <= 0)
                                    onExit.Invoke();
                            }
                            if (consumeObject && disableTime <= 0)
                            {
                                carrier.Consume();
                            }
                            return true;
                        }
                    }
                }
            }

            if ((checkType & CheckType.DirectObject) == CheckType.DirectObject)
            {
                PlayerController2D player = goInContact.GetComponent<PlayerController2D>();
                if (player == null)
                {
                    bool objectIsOk = string.IsNullOrEmpty(objectName) || objectName == goInContact.name;
                    if (objectIsOk)
                    {
                        if (entered)
                        {
                            if (nbInteractions == 0 && disableTime <= 0)
                                onEnter.Invoke();
                        }
                        else
                        {
                            if (nbInteractions == 1 && disableTime <= 0)
                                onExit.Invoke();
                        }
                        if (consumeObject && disableTime <= 0)
                        {
                            Destroy(goInContact);
                        }
                        return true;
                    }
                }
            }

            if ((checkType & CheckType.Player) == CheckType.Player)
            {
                PlayerController2D player = goInContact.GetComponent<PlayerController2D>();
                if (player != null)
                {
                    if (entered)
                    {
                        if (nbInteractions == 0 && disableTime <= 0)
                            onEnter.Invoke();
                    }
                    else
                    {
                        if (nbInteractions == 1 && disableTime <= 0)
                            onExit.Invoke();
                    }
                    return true;
                }
            }

            if ((checkType & CheckType.Skin) == CheckType.Skin)
            {
                PlayerController2D player = goInContact.GetComponent<PlayerController2D>();
                if (player != null && player.skin != null)
                {
                    bool skinIsOk = string.IsNullOrEmpty(objectName) || objectName == player.skin.Name();
                    if (skinIsOk)
                    {
                        if (entered)
                        {
                            if (nbInteractions == 0 && disableTime <= 0)
                                onEnter.Invoke();
                        }
                        else
                        {
                            if (nbInteractions == 1 && disableTime <= 0)
                                onExit.Invoke();
                        }
                        return true;
                    }
                }
            }

            return false;
        }

        void Reset()
        {
            onEnter = new UnityEvent();
            onExit = new UnityEvent();
#if UNITY_EDITOR
            UnityAction destroyCall = new UnityAction(this.AutoDestroy);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(onEnter, destroyCall);
#endif
        }

        public void AutoDestroy()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Trigger")]
        public static GameObject CreateObject()
        {
            Sprite triggerSprite = (Sprite)UnityEditor.Selection.activeObject;
            GameObject newObj = NewTrigger(triggerSprite);
            UnityEditor.Selection.activeObject = newObj;
            return newObj;
        }

        private static GameObject NewTrigger(Sprite triggerSprite)
        {
            GameObject newObj = new GameObject();
            newObj.name = "Trigger";
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            var sr = newObj.AddComponent<SpriteRenderer>();
            sr.sprite = triggerSprite;
            sr.sortingLayerName = PlayerController2D.LAYER_MIDDLEGROUND;
            newObj.AddComponent<PolygonCollider2D>();
            newObj.AddComponent<TriggerController2D>();          
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Trigger", true)]
        private static bool Validation()
        {
            return UnityEditor.Selection.activeObject is Sprite && UnityEditor.Selection.objects.Length == 1;
        }

        [UnityEditor.MenuItem("GameObject/LuDK/2D/CreateEmptyTrigger", false, 0)]
        public static void CreateEmptyTrigger()
        {
            GameObject parent = (GameObject)UnityEditor.Selection.activeObject;
            GameObject newTrigger = NewTrigger(null);
            newTrigger.GetComponent<TriggerController2D>().onEnter = new UnityEvent();
            newTrigger.GetComponent<TriggerController2D>().onExit = new UnityEvent();
            newTrigger.name = "EmptyTrigger";
            newTrigger.transform.parent = parent.transform;
            newTrigger.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            DestroyImmediate(newTrigger.GetComponent<PolygonCollider2D>());
            var collider = newTrigger.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1, 1);
            UnityEditor.Selection.activeObject = newTrigger;
        }

        [UnityEditor.MenuItem("GameObject/LuDK/2D/CreateEmptyTrigger", true)]
        private static bool ValidationEmptyTrigger()
        {
            return UnityEditor.Selection.activeObject is GameObject && UnityEditor.Selection.objects.Length == 1;
        }

#endif
    }
}