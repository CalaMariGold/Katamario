using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropSpawner : MonoBehaviour
{
    PropPooler propPooler;

    GameObject[] regions;

    public SpawnRegion spawnRegion;

    void Start()
    {
        propPooler = PropPooler.Instance;

        regions = GameObject.FindGameObjectsWithTag("PropSpawnRegion");
    }

    void Update()
    {
        propPooler.SpawnFromPool("SmallProp", spawnRegion.SpawnPoint, Quaternion.identity);
    }
}
