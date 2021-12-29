using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerBallController : MonoBehaviour
{
    private GameObject _player;
    private Rigidbody _playerRigidbody;
    private const string _AItag = "AI";
    private const string _propTag = "Prop";
    private const string _boostTag = "BoostPowerUp";

    [SerializeField] public float rollSpeed;

    [SerializeField] private Camera _camera;

    [SerializeField] private CinemachineFreeLook _cineCamera;
    [SerializeField] private PowerUp _powerUp;


    public float playerSize = 1;
    private GameObject[] _props;
    private GameObject[] _AIobjects;

    public static event Action WinGame;

    private void Awake()
    {
        // Cache all our variables
        _player = this.gameObject;
        _playerRigidbody = _player.GetComponent<Rigidbody>();
        _props = GameObject.FindGameObjectsWithTag(_propTag);
        _AIobjects = GameObject.FindGameObjectsWithTag(_AItag);

        // If there is no specific camera assigned, use camera.main
        _camera ??= Camera.main;
    }

    private void Start()
    {
        UpdateCanCollectMesh();
    }

    private void Update()
    {
        // Player movement
        Vector3 input = new(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 movement = (input.z * _camera.transform.forward) + (input.x * _camera.transform.right);
        _playerRigidbody.AddForce(rollSpeed * Time.deltaTime * movement);
    }

    public void changeRollSpeed(float speed)
    {
        rollSpeed += speed;
    }

    // Loops through all collectable in the scene
    // If any collectable is able to be picked up, change its material color
    public void UpdateCanCollectMesh()
    {
        // Check this every time just in case one of the AI's got collected
        _AIobjects = GameObject.FindGameObjectsWithTag(_AItag);

        // PROP
        for (int i = 0; i < _props.Length; i++)
        {
            if (_props[i] != null)
            {
                if (_props[i].transform.localScale.magnitude * 5 <= playerSize)
                    _props[i].GetComponent<MeshRenderer>().material.color = Color.green;
            }
        }

        // AI
        for (int i = 0; i < _AIobjects.Length; i++)
        {
            if (_AIobjects[i] != null)
            {
                if (_AIobjects[i].GetComponent<AIBallController>().AIsize < playerSize)
                    _AIobjects[i].GetComponent<MeshRenderer>().material.color = Color.red;
                else _AIobjects[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the player picks up a boost powerup
        if (other.gameObject.CompareTag(_boostTag))
        {
            _powerUp.PickUpBoost(this.gameObject);
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        #region Collect Prop
        // If player collides with and the prop's scale * 5 is <= the player's size
        if (collision.gameObject.CompareTag(_propTag) && collision.transform.localScale.magnitude * 5 <= playerSize)
        {
            // Store the size of the collected prop
            float collectedPropSize = collision.transform.localScale.magnitude;

            // Update meshes, stick the prop to the player, increase player's size number
            UpdateCanCollectMesh();
            collision.transform.parent = transform;
            playerSize += collectedPropSize;

            // Here, we increase the player's scale and FOV overtime depending on the scale of the prop it collected
            StartCoroutine(ScalePlayerOverTime(0.1f));
            IEnumerator ScalePlayerOverTime(float time)
            {
                Vector3 originalScale = _player.transform.localScale;
                // We divide by 10 so that the player growth isn't 1 to 1
                Vector3 destinationScale = _player.transform.localScale + (collision.transform.localScale / 10);
                float currentTime = 0.0f;

                do
                {
                    _player.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
                    // 4 is an arbiturary number to adjust game feel
                    _cineCamera.m_Lens.FieldOfView += (collectedPropSize * 4) * Time.deltaTime;
                    currentTime += Time.deltaTime;
                    yield return null;
                } while (currentTime <= time);
            }

            // Disable the prop's collider and change it's material
            collision.transform.GetComponent<BoxCollider>().enabled = false;
            collision.transform.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        #endregion

        #region Collect AI
        // If player collides with and collects an AI
        if (collision.gameObject.CompareTag(_AItag) && collision.gameObject.GetComponent<AIBallController>().AIsize < playerSize)
        {
            // Store the size of the collected AI
            float collectedAISize = collision.gameObject.GetComponent<AIBallController>().AIsize;

            // Update meshes, stick the prop to the player, increase player's size number
            UpdateCanCollectMesh();
            collision.transform.parent = transform;
            playerSize += collectedAISize;
                
            // Here, we increase the player's scale and FOV overtime depending on the **scale** of the AI it collected
            StartCoroutine(ScalePlayerOverTime(0.1f));
            IEnumerator ScalePlayerOverTime(float time)
            {
                Vector3 originalScale = _player.transform.localScale;
                // We divide by 10 so that the player growth isn't 1 to 1
                Vector3 destinationScale = _player.transform.localScale + (collision.transform.localScale / 10);
                float currentTime = 0.0f;

                do
                {
                    _player.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / time);
                    // 4 is an arbiturary number to adjust game feel
                    _cineCamera.m_Lens.FieldOfView += (collision.transform.localScale.magnitude * 4) * Time.deltaTime;
                    currentTime += Time.deltaTime;
                    yield return null;
                } while (currentTime <= time);
            }

            // Disable the AI's components and change its tag
            collision.transform.GetComponent<SphereCollider>().enabled = false;
            collision.transform.GetComponent<AIBallController>().enabled = false;
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.transform.tag = "Collected";

            // If there are no other AI, invoke the WinGame function in the GameManager
            if (GameObject.FindGameObjectWithTag(_AItag) == null)
            {
                WinGame?.Invoke();
            }
        }
        #endregion

    }


}
