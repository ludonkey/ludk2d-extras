using UnityEngine;
using UnityEngine.U2D;

namespace LuDK.Toolkit.L2D
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(PixelPerfectCamera))]
    public class CameraController2D : MonoBehaviour
    {
        private PlayerController2D player;

        public float trackSpeed = 5;

        public float offsetX = 0;

        public float offsetY = 0;

        public bool followX = true;

        public bool followY = true;

        public bool offsetXFlipWithPlayer = true;

        public bool offsetYFlipWithPlayer = true;

        [Header("Shake")]
        private Vector3 posBeforeShaking;
        private float shakeDuration = 0f;
        private float shakeEllapsedTime = 0f;
        public float shakeAmount = 0.1f;

        [Header("Only for game type Platformer")]
        public float durationPlayerGroundedBeforeFollowingY = 0.05f;

        private float waitingDelay { get; set; }
        public float timeToWaitAfterLanding { get; private set; }

        private void Awake()
        {
            waitingDelay = 0;
            player = GameObject.FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                CenterToPlayer();
            }
        }

        void Update()
        {
            if (shakeEllapsedTime < shakeDuration)
            {
                float factor = 1f - shakeEllapsedTime / shakeDuration;
                float x = Random.Range(-1f, 1f) * shakeAmount * factor;
                float y = Random.Range(-1f, 1f) * shakeAmount * factor;
                transform.localPosition = posBeforeShaking + new Vector3(x, y, 0);
                shakeEllapsedTime += Time.deltaTime;
                if (shakeEllapsedTime >= shakeDuration)
                {
                    shakeDuration = 0f;
                    shakeEllapsedTime = 0f;
                    transform.localPosition = posBeforeShaking;
                }
                return;
            }
            if (player)
            {
                if (player.gameType == GameType2D.Platformer)
                {
                    if (!player.consideredAsInTheAir && timeToWaitAfterLanding > 0f)
                    {
                        timeToWaitAfterLanding -= Time.deltaTime;
                    }
                    else if (player.consideredAsInTheAir)
                    {
                        timeToWaitAfterLanding = durationPlayerGroundedBeforeFollowingY;
                    }
                }
                if (waitingDelay == 0.0f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, GetTargetPosition(), trackSpeed * Time.deltaTime);
                } else
                {
                    waitingDelay -= Time.deltaTime;
                    if (waitingDelay <= 0)
                    {
                        waitingDelay = 0;
                        CenterToPlayer();
                    }
                }
            }             
        }

        public void Shake(float duration)
        {
            posBeforeShaking = transform.localPosition;
            shakeDuration = duration;
            shakeEllapsedTime = 0;
        }

        /// <summary>
        /// It will teleport the camera right on the player.
        /// Useful when the just teleported in another location.
        /// </summary>
        /// <param name="waitingDelay">Delay to wait before teleporting or 0 for immediate.</param>
        public void Teleport(float waitingDelay)
        {
            if (player)
            {
                if (waitingDelay == 0)
                {
                    CenterToPlayer();
                }
                else if (waitingDelay > 0)
                {
                    this.waitingDelay = waitingDelay;
                }
            }
        }      

        public void CenterToPlayer()
        {
            transform.position = GetTargetPosition();
        }

        private Vector3 GetTargetPosition()
        {
            Vector3 targetPos = player.transform.position;
            targetPos.z = transform.position.z;
            Vector3 offset = new Vector3(offsetX, offsetY, 0);
            if (offsetXFlipWithPlayer && !player.IsLookingToRight())
            {
                offset = new Vector3(-offset.x, offset.y, 0);
            }
            if (offsetYFlipWithPlayer && player.IsUpsideDown())
            {
                offset = new Vector3(offset.x, -offset.y, 0);
            }
            targetPos += offset;

            if (player.gameType == GameType2D.Platformer && timeToWaitAfterLanding > 0)
            {
                targetPos.y = transform.position.y;
            }

            if (!followX)
            {
                targetPos.x = transform.position.x;
            }

            if (!followY)
            {
                targetPos.y = transform.position.y;
            }

            return targetPos;
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/LuDK/2D/PixelPerfectCam", false, 0)]
        public static void UpdateCamera()
        {
            GameObject go = (GameObject)UnityEditor.Selection.activeObject;
            Camera cam = go.GetComponent<Camera>();
            cam.orthographic = true;
            PixelPerfectCamera ppc = go.GetComponent<PixelPerfectCamera>();
            if (ppc == null)
            {
                ppc = go.AddComponent<PixelPerfectCamera>();
            }
            ppc.cropFrameX = true;
            ppc.cropFrameY = true;
            ppc.stretchFill = true;
            ppc.assetsPPU = 16;
            CameraController2D cc = go.GetComponent<CameraController2D>();
            if (cc == null)
            {
                go.AddComponent<CameraController2D>();
            }
        }

        [UnityEditor.MenuItem("GameObject/LuDK/2D/PixelPerfectCam", true)]
        private static bool Validation()
        {
            if (!(UnityEditor.Selection.activeObject is GameObject))
            {
                return false;
            }
            GameObject go = (GameObject)UnityEditor.Selection.activeObject;
            Camera cam = go.GetComponent<Camera>();
            return cam != null;
        }
#endif
    }
}