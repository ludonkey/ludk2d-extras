using UnityEngine;

namespace LuDK.Toolkit.L2D
{
    public class Destination2D : MonoBehaviour
    {
        private PlayerController2D player { get; set; }

        void Start()
        {
            player = GameObject.FindObjectOfType<PlayerController2D>();
        }

        /// <summary>
        /// To teleport the player (and the camera) and the carried object
        /// to the location of this destination. Give a negative delay (e.g. -1)
        /// if you don't want to teleport the camera.
        /// </summary>
        /// <param name="delayBeforeTeleportingCamera"></param>
        public void Teleport(float delayBeforeTeleportingCamera)
        {
            if (player != null)
            {
                player.transform.position = transform.position;
                CarryController2D cc = player.GetComponent<CarryController2D>();
                if (cc != null && cc.GetObject() != null)
                {
                    cc.GetObject().transform.position = transform.position;
                }
                if (delayBeforeTeleportingCamera >= 0.0f)
                {
                    CameraController2D cam = FindObjectOfType<CameraController2D>();
                    if (cam != null)
                    {
                        cam.Teleport(delayBeforeTeleportingCamera);
                    }
                }
            }
        }

#if UNITY_EDITOR

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(1,1,1));
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Destination")]
        public static GameObject CreateObject()
        {
            Sprite destinationSprite = (Sprite)UnityEditor.Selection.activeObject;
            GameObject newObj = NewDestination(destinationSprite);
            UnityEditor.Selection.activeObject = newObj;
            return newObj;            
        }

        private static GameObject NewDestination(Sprite destinationSprite)
        {
            
            GameObject newObj = new GameObject();
            newObj.transform.position = PlayerController2D.GetMiddleOfScreenWorldPos();
            newObj.name = "Destination";
            var sr = newObj.AddComponent<SpriteRenderer>();
            sr.sprite = destinationSprite;
            sr.sortingLayerName = PlayerController2D.LAYER_BACKGROUND;
            sr.sortingOrder = 1;
            newObj.AddComponent<Destination2D>();
            return newObj;
        }

        [UnityEditor.MenuItem("Assets/LuDK/2D/Destination", true)]
        private static bool Validation()
        {
            return UnityEditor.Selection.activeObject is Sprite && UnityEditor.Selection.objects.Length == 1;
        }

        [UnityEditor.MenuItem("GameObject/LuDK/2D/CreateEmptyDesination", false, 0)]
        public static void CreateEmptyDesination()
        {
            GameObject parent = (GameObject)UnityEditor.Selection.activeObject;
            GameObject newTrigger = NewDestination(null);
            newTrigger.name = "EmptyDestination";
            newTrigger.transform.parent = parent.transform;
            UnityEditor.Selection.activeGameObject = newTrigger;
        }

        [UnityEditor.MenuItem("GameObject/LuDK/2D/CreateEmptyDesination", true)]
        private static bool ValidationEmptyTrigger()
        {
            return UnityEditor.Selection.activeObject is GameObject && UnityEditor.Selection.objects.Length == 1;
        }
#endif
    }
}