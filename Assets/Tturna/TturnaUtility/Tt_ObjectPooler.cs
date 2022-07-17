using System.Collections.Generic;
using UnityEngine;

namespace Tturna.Utility
{
    public class Tt_ObjectPooler : MonoBehaviour
    {
        [System.Serializable]
        struct PoolObjectBase
        {
            public string name;
            public GameObject poolObject;
            public int poolSize;

            [Tooltip("Should the pooler create a temporary object that stays active for 1 second before being destroyed? " +
                    "This may help with lag spikes when first using an object (e.g. Decal Projector).")]
            public bool createDummyObject;
        }

        [Header("Settings")]
        [SerializeField, Tooltip("Struct array storing a list of objects to be pooled and their respective pool sizes.")] PoolObjectBase[] poolObjects;

        List<GameObject[]> pools = new List<GameObject[]>();

        public static Tt_ObjectPooler Instance;

        private void Awake() => Instance = this;

        void Start()
        {
            GameObject poolParent = new GameObject("Object Pool");
            poolParent.transform.SetParent(transform);

            for (int i = 0; i < poolObjects.Length; i++)
            {
                // Set up pool parent object and declare new array for pooled objects
                GameObject subPoolParent = new GameObject($"{poolObjects[i].name} Pool");
                subPoolParent.transform.SetParent(poolParent.transform);
                pools.Add(new GameObject[poolObjects[i].poolSize]);

                // Populate pool with new objects
                for (int n = 0; n < poolObjects[i].poolSize; n++)
                {
                    pools[i][n] = Instantiate(poolObjects[i].poolObject, subPoolParent.transform);
                    pools[i][n].SetActive(false);
                }

                if (!poolObjects[i].createDummyObject) continue;
                StartCoroutine(Tt_Helpers.DelayDestroy(Instantiate(poolObjects[i].poolObject), 1));
            }

        }

        public GameObject GetPoolObjectByPoolName(string name)
        {
            // TODO: more efficient way of finding index with name
            int pidx = -1;
            for (int i = 0; i < pools.Count; i++)
            {
                if (poolObjects[i].name == name)
                {
                    pidx = i;
                    break;
                }
            }

            if (pidx == -1)
            {
                Debug.LogError($"Couldn't find name '{name}' from object pools.");
                return null;
            }

            for (int i = 0; i < pools[pidx].Length; i++)
            {
                if (!pools[pidx][i].activeInHierarchy)
                {
                    pools[pidx][i].SetActive(true);
                    return pools[pidx][i];
                }
            }

            // Would be cool if the system could somehow return the object that was used first out of the ones that already exist.
            // For now, just return the first object in the pool when the whole pool is already used.
            pools[pidx][0].SetActive(true);
            return pools[pidx][0];
        }
    }
}