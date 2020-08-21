using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class SwitchSprite : MonoBehaviour
    {
        SpriteRenderer sr;
        Sprite initialSprite;

        public Sprite overrideSprite;
        public AudioSource sfxSwitch;
        public AudioSource sfxSwitchBack;

        void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            initialSprite = sr.sprite;
        }

        /// <summary>
        /// Apply the new sprite to the SpriteRenderer
        /// </summary>
        public void Switch()
        {
            sr.sprite = overrideSprite;
            if (sfxSwitch != null)
            {
                sfxSwitch.Play();
            }
        }

        /// <summary>
        /// Reapply the initial sprite to the SpriteRenderer
        /// </summary>
        public void SwitchBack()
        {
            sr.sprite = initialSprite;
            if (sfxSwitchBack != null)
            {
                sfxSwitchBack.Play();
            }
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Assets/LuDK/2D/Switch")]
        public static GameObject CreateObject()
        {
            Object[] sprites = UnityEditor.Selection.objects;
            var newObj = TriggerController2D.CreateObject();
            newObj.name = "Switch";
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            var sr = newObj.GetComponent<SpriteRenderer>();
            sr.sprite = (Sprite)sprites[0];
            sr.sortingLayerName = PlayerController2D.LAYER_BACKGROUND;
            sr.sortingOrder = 1;
            var ss = newObj.AddComponent<SwitchSprite>();
            newObj.GetComponent<Collider2D>().isTrigger = true;
            ss.overrideSprite = (Sprite)sprites[1];
            var tc = newObj.GetComponent<TriggerController2D>();
            tc.checkType = TriggerController2D.CheckType.Player | TriggerController2D.CheckType.DirectObject;
            tc.onEnter = new UnityEngine.Events.UnityEvent();
            tc.onExit = new UnityEngine.Events.UnityEvent();
            tc.onExit.RemoveAllListeners();
            var switchCall = new UnityEngine.Events.UnityAction(ss.Switch);
            var switchBackCall = new UnityEngine.Events.UnityAction(ss.SwitchBack);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(tc.onEnter, switchCall);
            UnityEditor.Events.UnityEventTools.AddPersistentListener(tc.onExit, switchBackCall);
            UnityEditor.Selection.activeObject = newObj;
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Switch", true)]
        private static bool Validation()
        {

            if (null == UnityEditor.Selection.objects || UnityEditor.Selection.objects.Length != 2)
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

#endif
    }
}