using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LuDK.Toolkit.Core
{
    public class SceneController : MonoBehaviour
    {
        public UnityEvent OnStart;

        private float delayTime = 0;
        private bool delayedQuit = false;
        private bool delayedLoadScene = false;
        private string sceneToLoad;
        private float volumeFactor = 1;
        private float lastVolume = 0;

        private void Start()
        {
            if (OnStart != null)
            {
                OnStart.Invoke();
            }
        }

        private void Update()
        {
            if (delayedQuit)
            {
                if (delayTime >= 0)
                {
                    delayTime -= Time.deltaTime;
                    if (delayTime <= 0.0f)
                    {
                        InternalQuit();
                    }
                }
            } else if (delayedLoadScene)
            {
                if (delayTime >= 0)
                {
                    delayTime -= Time.deltaTime;
                    if (delayTime <= 0.0f)
                    {
                        LoadScene(sceneToLoad);
                    }
                }
            }
            KeepFocusOnLastButton();
        }

        public void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            keepFocusOnLastButton = true;
        }

        public void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            keepFocusOnLastButton = false;
        }

        private bool keepFocusOnLastButton = false;
        private GameObject lastSelectedButton;

        private void KeepFocusOnLastButton()
        {
            if (EventSystem.current != null)
            {
                if (EventSystem.current.currentSelectedGameObject == null && lastSelectedButton != null)
                {
                    if (lastSelectedButton.gameObject.activeSelf &&
                        lastSelectedButton.GetComponent<Button>() != null &&
                        lastSelectedButton.GetComponent<Button>().interactable)
                    {
                        EventSystem.current.SetSelectedGameObject(lastSelectedButton);
                    }
                }
                else
                {
                    lastSelectedButton = EventSystem.current.currentSelectedGameObject;
                }
            }
        }

        public void SetVolumeFactor(float factor)
        {
            volumeFactor = factor;
            SetVolume(lastVolume);
        }

        public void SetVolume(float volume)
        {
            lastVolume = volume;
            AudioListener.volume = volume * volumeFactor;
        }

        public void LoadScene(string sceneName)
        {
            if (DoesSceneExist(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            } else
            {
                SceneManager.LoadScene(GetSceneFromEndName(sceneName));
            }
        }

        public static bool DoesSceneExist(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var lastSlash = scenePath.LastIndexOf("/");
                var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

                if (string.Compare(name, sceneName, true) == 0)
                    return true;
            }

            return false;
        }

        public static string GetSceneFromEndName(string end)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                var lastSlash = scenePath.LastIndexOf("/");
                var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);

                if (sceneName.EndsWith(end))
                    return sceneName;
            }
            return string.Empty;
        }

        public void DelayedLoadScene(string sceneNameWithDelay)
        {
            int lastIndex = sceneNameWithDelay.LastIndexOf('_');
            if (lastIndex == -1)
            {
                LoadScene(sceneNameWithDelay);
                return;
            }            
            string delayStr = sceneNameWithDelay.Substring(lastIndex + 1);
            if (!float.TryParse(delayStr, NumberStyles.Float, CultureInfo.InvariantCulture, out delayTime))
            {
                LoadScene(sceneNameWithDelay);
                return;
            }
            delayedLoadScene = true;
            sceneToLoad = sceneNameWithDelay.Substring(0, lastIndex);
        }

        public void Quit(float delay)
        {
            if (delayedQuit)
                return;
            if (delay > 0.0f)
            {
                delayTime = delay;
                delayedQuit = true;
                return;
            }
            InternalQuit();
        }

        private static void InternalQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
        }
    }
}