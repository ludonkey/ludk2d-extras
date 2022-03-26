using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class Spawner : MonoBehaviour
    {
        public List<GameObject> originalGameObjects;

        public UnityEvent OnSpawn;

        private void Awake()
        {
            if (null == originalGameObjects)
            {
                originalGameObjects = new List<GameObject>();
            }
        }

        public void Spawn()
        {
            if (null == originalGameObjects || originalGameObjects.Count == 0)
                return;
            var newGO = Instantiate(originalGameObjects[Random.Range(0, originalGameObjects.Count)]);
            newGO.transform.position = transform.position;
            newGO.transform.rotation = transform.rotation;
            newGO.transform.localScale = transform.localScale;
            newGO.transform.parent = transform;
            newGO.SetActive(true);
            OnSpawn?.Invoke();
        }
    }
}