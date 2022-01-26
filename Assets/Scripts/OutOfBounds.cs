using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    public static event Action GameOver;

    private void OnCollisionEnter(Collision collision)
    {
        // Invoke the GameOver function in GameManager
        GameOver?.Invoke();

        Destroy(collision.gameObject);
    }
}
