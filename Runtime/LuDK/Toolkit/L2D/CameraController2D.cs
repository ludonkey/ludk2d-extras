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

        [Header("Shake")]
        private Vector3 posBeforeShaking;
        private float shakeDuration = 0f;
        private float shakeEllapsedTime = 0f;
        public float shakeAmount = 0.1f;

        [Header("Only for SideScroller")]
        public float landingTimeForFollowingY = 0.05f;

        private float waitingDelay { get; set; }
        public float timeToWaitAfterLanding { get; private set; }

        private void Awake()
        {
            waitingDelay = 0;
            player = GameObject.FindObjectOfType<PlayerController2D>();
            if (player != null)
            {
                Vector3 targetPos = player.transform.position;
                targetPos.z = transform.position.z;
                transform.position = targetPos;
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
                if (player.gameType == GameType2D.SideScroller)
                {
                    if (!player.isJumping && timeToWaitAfterLanding > 0f)
                    {
                        timeToWaitAfterLanding -= Time.deltaTime;
                    }
                    else if (player.isJumping)
                    {
                        timeToWaitAfterLanding = landingTimeForFollowingY;
                    }
                }
                if (waitingDelay == 0.0f)
                {
                    Vector3 targetPos = player.transform.position;
                    if (player.gameType == GameType2D.SideScroller && timeToWaitAfterLanding > 0)
                    {
                        targetPos.y = transform.position.y;
                    }
                    targetPos.z = transform.position.z;
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, trackSpeed * Time.deltaTime);
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

        private void CenterToPlayer()
        {
            Vector3 targetPos = player.transform.position;
            targetPos.z = transform.position.z;
            transform.position = targetPos;
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