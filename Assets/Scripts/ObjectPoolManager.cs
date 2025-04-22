using System.Collections.Generic;
using UnityEngine;

namespace ZhouSoftware
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance;

        private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(prefab))
                poolDictionary[prefab] = new Queue<GameObject>();

            GameObject obj;
            if (poolDictionary[prefab].Count > 0)
            {
                obj = poolDictionary[prefab].Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(prefab, position, rotation);
            }

            return obj;
        }

        public void Return(GameObject prefab, GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(null); // optional: reset parent
            if (!poolDictionary.ContainsKey(prefab))
                poolDictionary[prefab] = new Queue<GameObject>();

            poolDictionary[prefab].Enqueue(obj);
        }
    }
}
