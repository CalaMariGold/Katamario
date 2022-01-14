using UnityEngine;

// Allows us to specify some types and functions that all objects that derive from this interface have to implement
public interface IPooledObject
{
    void OnObjectSpawn();
}
