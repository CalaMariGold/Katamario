using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerBallController : MonoBehaviour
{
    private const string _AItag = "AI";
    private const string _propTag = "Prop";
    private const string _boostTag = "BoostPowerUp";

    private GameObject _player;
    private Rigidbody _playerRigidbody;
    private GameObject[] _props;
    private GameObject[] _AIobjects;

    [Header("Camera")]
    [SerializeField] private Camera _camera;
    [SerializeField] private CinemachineFreeLook _cineCamera;

    [Header("Scripts")]
    [SerializeField] private PowerUpManager _powerUpManager;

    [Header("Audio")]
    [SerializeField] private AudioSource pickUpAudioSource;
    [SerializeField] private AudioClip propPickUpAudioClip;
    [SerializeField] private AudioClip playerPickUpAudioClip;
    [SerializeField] private AudioClip boostPickUpAudioClip;


    // Public Vars
    [Header("Player Settings")]
    public float playerSize = 1;
    public float rollSpeed;
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
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        Vector3 movement = (input.z * _camera.transform.forward) + (input.x * _camera.transform.right);
        _playerRigidbody.AddForce(rollSpeed * Time.deltaTime * movement);

        // If the AI has any children, absorb them if they aren't a prop
        foreach (Transform child in this.transform)
        {
            if (child.tag == "Collected" && child.GetComponent<Prop>() == null)
            {
                StartCoroutine(AbsorbEntityOverTime(child, this.transform));
            }
        }
    }

    public void ChangeRollSpeed(float speed)
    {
        rollSpeed += speed;
    }

    // Loops through all collectable in the scene
    // If any collectable is able to be picked up, change its material color
    public void UpdateCanCollectMesh()
    {
        // Check this every time just in case one of the AI's got collected
        _AIobjects = GameObject.FindGameObjectsWithTag(_AItag);
        _props = GameObject.FindGameObjectsWithTag(_propTag);

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
                if (_AIobjects[i].GetComponent<AIBallController>().aiSize < playerSize)
                    _AIobjects[i].GetComponent<MeshRenderer>().material.color = Color.red;
                else _AIobjects[i].GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }

    // Power Up Pickups
    private void OnTriggerEnter(Collider other)
    {
        // If the player picks up a boost powerup
        if (other.gameObject.CompareTag(_boostTag))
        {
            _powerUpManager.PickUpBoost(this.gameObject);
            pickUpAudioSource.PlayOneShot(boostPickUpAudioClip, 0.7f);
            Destroy(other.gameObject);
        }
    }

    public IEnumerator AbsorbEntityOverTime(Transform child, Transform absorber)
    {
        // Seconds to wait before absorption starts
        yield return new WaitForSeconds(3);
        Vector3 destinationScale = new Vector3(0, 0, 0);

        if (child != null)
        {
            // Move the child's transform towards the center of the absorber
            // Decrease the child's scale to 0
            // 0.05 is an arbituary number to slow down the process
            child.transform.position = Vector3.MoveTowards(child.transform.position, absorber.position, Time.deltaTime * 0.05f);
            child.transform.localScale = Vector3.Lerp(child.transform.localScale, destinationScale, Time.deltaTime * 0.05f);
        }

        // Release prop and exit when child object reaches the center
        if (child.transform.position == absorber.transform.position)
        {
            Destroy(child.gameObject);
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        #region Collect Prop
        // If player collides with and the prop's scale is <= the player's size
        if (collision.gameObject.CompareTag(_propTag) && collision.transform.localScale.magnitude <= playerSize)
        {
            // Store the size of the collected prop
            float collectedPropSize = collision.transform.localScale.magnitude;

            // Update meshes, stick the prop to the player, increase player's size number
            UpdateCanCollectMesh();
            collision.transform.parent = transform;
            playerSize += collectedPropSize;
            ChangeRollSpeed(-collectedPropSize * 3);
            pickUpAudioSource.PlayOneShot(propPickUpAudioClip, 0.4f);

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
            collision.gameObject.GetComponent<MeshCollider>().enabled = false;
            collision.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
            collision.gameObject.tag = "Collected";
        }
        #endregion

        #region Collect AI
        // If player collides with and collects an AI
        if (collision.gameObject.CompareTag(_AItag) && collision.gameObject.GetComponent<AIBallController>().aiSize < playerSize)
        {
            // Store the size of the collected AI
            float collectedAISize = collision.gameObject.GetComponent<AIBallController>().aiSize;

            // Update meshes, stick the prop to the player, increase player's size number
            UpdateCanCollectMesh();
            collision.transform.parent = transform;
            playerSize += collectedAISize;
            ChangeRollSpeed(-collectedAISize * 3);
            pickUpAudioSource.PlayOneShot(playerPickUpAudioClip, 1f);

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
            collision.gameObject.tag = "Collected";

            // If there are no other AI, invoke the WinGame function in the GameManager
            if (GameObject.FindGameObjectWithTag(_AItag) == null)
            {
                WinGame?.Invoke();
            }
        }
        #endregion

    }


}
