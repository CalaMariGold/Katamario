using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PropSpawner : MonoBehaviour
{
    [Header("Prop")]
    [SerializeField] private Prop _smallPropPrefab;
    [Range(0, 100)]
    [SerializeField] private int _spawnAmount = 10;

    [Header("Spawning")]
    [SerializeField] private Transform _propParent;
    [SerializeField] private SpawnRegion _spawnRegion;

    private ObjectPool<Prop> _smallPropPool;
    private PlayerBallController _playerBallController;


    private void Start()
    {
        _playerBallController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBallController>();

        _smallPropPool = new ObjectPool<Prop>(() =>
        {
            return Instantiate(_smallPropPrefab); // Create
        }, prop =>
        {
            prop.transform.parent = _propParent;
            prop.gameObject.SetActive(true); // Get
            prop.gameObject.tag = "Prop";
            prop.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            prop.gameObject.GetComponent<BoxCollider>().enabled = true;
            prop.gameObject.GetComponent<MeshRenderer>().material.color = Color.red;
            prop.transform.position = _spawnRegion.SpawnPoint;
            prop.transform.rotation = Quaternion.identity;
            _playerBallController.UpdateCanCollectMesh();
            prop.SpawnParticle();
            prop.Init(ReleaseProp);
        }, prop =>
        {
            prop.gameObject.SetActive(false);  // Release
        }, prop =>
        {
            //Destroy(prop.gameObject); // Destroy
        }, false, _spawnAmount, _spawnAmount); // Collection check, capacity, max size

        InvokeRepeating(nameof(Spawn), 0.2f, 0.2f);
    }

    private void Spawn()
    {
        if (_smallPropPool.CountActive != _spawnAmount)
        {
            var smallProp = _smallPropPool.Get();
        }
    }

    public void ReleaseProp(Prop prop)
    {
        _smallPropPool.Release(prop);
    }
}
