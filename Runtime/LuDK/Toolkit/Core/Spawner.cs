using UnityEngine;
using UnityEngine.Events;

namespace LuDK.Toolkit.Core
{
    public class Spawner : MonoBehaviour
    {
        public GameObject originalGameObject;

        public UnityEvent OnSpawn;

        public void Spawn()
        {
            var newGO = Instantiate(originalGameObject);
            newGO.transform.position = transform.position;
            newGO.transform.rotation = transform.rotation;
            newGO.transform.localScale = transform.localScale;
            newGO.transform.parent = transform;
            newGO.SetActive(true);
            OnSpawn?.Invoke();
        }
    }
}