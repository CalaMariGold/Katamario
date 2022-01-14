using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour, IPooledObject
{
    public void OnObjectSpawn()
    {
        Debug.Log("Prop spawned");
    }

    void Update()
    {
        
    }
}
