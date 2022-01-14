using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropPooler : MonoBehaviour
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    #region Singleton
    public static PropPooler Instance;
    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public List<Pool> pools;
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // For each pool we want to create
        foreach (Pool pool in pools)
        {
            // Create a queue full of objects
            Queue<GameObject> objectPool = new Queue<GameObject>();

            // Add all objects to the queue
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }


            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // Take the object we want to spawn, activate it, and move it to the appropiate place in the world
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag" + tag + " doesn't exist.");
            return null;
        }

        GameObject objectToSpawn = poolDictionary[tag].Dequeue();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        IPooledObject pooledObj = objectToSpawn.GetComponent<IPooledObject>();
        if (pooledObj != null)
        {
            pooledObj.OnObjectSpawn();
        }

        return objectToSpawn;
    }

    public void AddBackToQueue(GameObject obj, string tag)
    {
        obj.transform.parent = null;
        obj.tag = "Prop";
        obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        obj.GetComponent<BoxCollider>().enabled = true;
        obj.GetComponent<MeshRenderer>().material.color = Color.red;
        poolDictionary[tag].Enqueue(obj);
        Debug.Log("Added " + obj + " with tag" + tag + " back to queue", obj);
    }

    public void RemoveFromQueue(GameObject obj, string tag)
    {
        poolDictionary["SmallProp"].Dequeue();
    }
}
