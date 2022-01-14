using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script handles what happens to prop objects in any player or UI after they've been collected
public class CollectionController : MonoBehaviour
{
    private const string _playerTag = "Player";
    private const string _AItag = "AI";

    private GameObject _playerObject;
    private GameObject[] _AIobjects;

    public PropPooler propPooler;

    private Vector3 originalChildScale;

    private void Start()
    {
        // Check if the player exists, then assign it to player var
        if (GameObject.FindGameObjectWithTag(_playerTag) != null)
            _playerObject = GameObject.FindGameObjectWithTag(_playerTag);
        else
            _playerObject = null;


        if (GameObject.FindGameObjectsWithTag(_AItag) != null)
            _AIobjects = GameObject.FindGameObjectsWithTag(_AItag);
        else
            _AIobjects = null;

        propPooler = PropPooler.Instance;
    }

    private void Update()
    {
        if (_playerObject != null)
        {
            foreach (Transform child in _playerObject.transform)
            {
                StartCoroutine(AbsorbPropOverTime(child, _playerObject.transform));
            }
        }

        for (int i = 0; i < _AIobjects.Length; i++)
        {
            if (_AIobjects[i] != null)
            {
                foreach (Transform child in _AIobjects[i].transform)
                {
                    StartCoroutine(AbsorbPropOverTime(child, _AIobjects[i].transform));
                }
            }
        }
    }

    IEnumerator AbsorbPropOverTime(Transform child, Transform absorber)
    {
        // Seconds to wait before absorption starts
        yield return new WaitForSeconds(3);
        Vector3 destinationScale = new(0, 0, 0);

        if (child != null)
        {
            // Move the child's transform towards the center of the absorber
            // Decrease the child's scale to 0
            // 0.05 is an arbituary number to slow down the process
            child.transform.position = Vector3.MoveTowards(child.transform.position, absorber.position, Time.deltaTime * 0.05f);
            child.transform.localScale = Vector3.Lerp(child.transform.localScale, destinationScale, Time.deltaTime * 0.05f);
        }

        // Destroy and exit when child object reaches the center
        if (child != null && child.transform.position == absorber.transform.position)
        {
            if (child.GetComponent<Prop>() != null)
            {
                propPooler.AddBackToQueue(child.gameObject, "SmallProp");
            }
            else Object.Destroy(child.gameObject);
            yield return null;
        }
    }



}
