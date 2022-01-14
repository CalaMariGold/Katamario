using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIBallController : MonoBehaviour
{
    private const string _playerTag = "Player";
    private const string _AItag = "AI";
    private const string _propTag = "Prop";

    [Header("Scripts")]
    [SerializeField] private GameManager _gameManager;

    [Header("Player")]
    [SerializeField] private GameObject _player;
    [SerializeField] private PlayerBallController _playerBallController;

    [Header("AI")]
    [SerializeField] private Rigidbody _aiRigidbody;
    [SerializeField] private GameObject _aiGameObject;
    [SerializeField] private float _rollSpeed;
    [Tooltip("This is (aiSize/2) + _aiSearchRadius")]
    [SerializeField] private float _aiSearchRadius;
    private Vector3 _aiMovement;
    private float _aiTotalSearchRadius;

    // Public Vars
    public static event Action GameOver;
    public bool chasingPlayer = false;
    public float aiSize = 1;


    private void Awake()
    {
        // Check if the player exists, then assign it to player var
        if (GameObject.FindGameObjectWithTag(_playerTag) != null)
            _player = GameObject.FindGameObjectWithTag(_playerTag);
        else
            _player = null;

        _playerBallController = _player.GetComponent<PlayerBallController>();
    }

    private void FixedUpdate()
    {
        DetermineAIState();

        // If the AI has any children, absorb them if they are a prop
        foreach (Transform child in this.transform)
        {
            if(child.tag == "Collected" && child.GetComponent<Prop>() == null)
            {
                StartCoroutine(AbsorbEntityOverTime(child, this.transform));
            }
        }

    }

    public void ChangeRollSpeed(float speed)
    {
        _rollSpeed += speed;
    }

    private void DetermineAIState()
    {
        // Increase _aiSearchRadius based on their size
        _aiTotalSearchRadius = (aiSize / 2) + _aiSearchRadius;

        // First, we check if AI size > player size, also make sure neither are null
        if (_player != null && _aiGameObject != null &&
            aiSize > _playerBallController.playerSize)
        {
            // Then check if the player is within the _aiSearchRadius
            if (Vector3.Distance(_aiGameObject.transform.position, _player.transform.position) <= _aiTotalSearchRadius)
            {
                // Requirements met, chase the player and set their color
                _player.GetComponent<MeshRenderer>().material.color = Color.red;

                chasingPlayer = true;

                // Move AI towards player
                _aiMovement = (_player.transform.position - this.transform.position).normalized;
                _aiRigidbody.AddForce(_aiMovement * _rollSpeed * Time.fixedDeltaTime);
            }
            // If the player is outside the _aiSearchRadius
            else
            {
                ChaseAI();
                ChaseProps();
                chasingPlayer = false;
            }
        }
        // If the player or AI doesn't exist, or if the AI is smaller than the player
        else
        {
            chasingPlayer = false;
            ChaseAI();
            ChaseProps();
        }
    }

    private void ChaseProps()
    {
        #region ChaseProp Variables
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(_propTag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        #endregion

        chasingPlayer = false;

        for (int i = 0; i < gos.Length; i++)
        {
            Vector3 diff = gos[i].transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                if (gos[i].GetComponent<BoxCollider>().enabled == false)
                {
                    gos[i].tag = "Collected";
                    return;
                }

                closest = gos[i];
                distance = curDistance;
            }
            _aiMovement = (closest.transform.position - this.transform.position).normalized;
        }
        _aiRigidbody.AddForce(_aiMovement * _rollSpeed * Time.fixedDeltaTime);

    }

    private void ChaseAI()
    {
        #region ChaseAI Variables
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag(_AItag);
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        bool chasingAI = false;
        #endregion

        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                if (go.GetComponent<SphereCollider>().enabled == false)
                {
                    go.tag = "Collected";
                    return;
                }

                closest = go;
                distance = curDistance;
            }
            // Null check, as well as check if AI > enemy AI
            if (go != null && _aiGameObject != null && aiSize > go.GetComponent<AIBallController>().aiSize)
            { 
                // Check if the AI is close enough to the enemy
                if (Vector3.Distance(_aiGameObject.transform.position, go.transform.position) <= _aiTotalSearchRadius)
                {
                    chasingAI = true;
                    _aiMovement = (closest.transform.position - this.transform.position).normalized;
                }
                else chasingAI = false;
            }
            else chasingAI = false;
            if (chasingAI) 
                _aiRigidbody.AddForce(_aiMovement * _rollSpeed * Time.fixedDeltaTime);
        }
    }

    public IEnumerator AbsorbEntityOverTime(Transform child, Transform absorber)
    {
        // Seconds to wait before absorption starts
        yield return new WaitForSeconds(1);
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

    void OnCollisionEnter(Collision collision)
    {
        
        #region Collect Prop
        // If AI collides with and collects a prop
        if (_aiGameObject != null && collision.gameObject.CompareTag(_propTag) && collision.transform.localScale.magnitude * 5 <= aiSize)
        {
            // Store the size of the collected prop
            float collectedPropSize = collision.transform.localScale.magnitude;

            // Update meshes, stick the prop to the AI, increase AI's size number, decrease speed
            _playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            aiSize += collectedPropSize;
            ChangeRollSpeed(-collectedPropSize * 4);
            

            // Increase the scale of the AI depending on the scale of the prop
            // No scale overtime, but possibly should add later
            _aiGameObject.transform.localScale += collision.transform.localScale / 10;

            // Ensure child props' scale don't increase
            foreach (Transform child in _aiGameObject.transform)
            {
                child.transform.localScale -= collision.transform.localScale / 1000;
            }

            // Disable the prop's collider and change it's material
            collision.transform.GetComponent<BoxCollider>().enabled = false;
            collision.transform.GetComponent<MeshRenderer>().material.color = Color.green;
            collision.gameObject.tag = "Collected";

            // Check if player mesh color needs to change
            _gameManager.UpdatePlayerMeshColor();
        }
        #endregion

        #region Collect Player
        // If AI collides with and collects a player
        if (collision.gameObject.CompareTag(_playerTag) && _playerBallController.playerSize < aiSize)
        {
            // Store the size of the collected player
            float collectedPlayerSize = collision.gameObject.GetComponent<PlayerBallController>().playerSize;

            // Update meshes, stick the prop to the AI, increase AI's size number
            _playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            aiSize += _playerBallController.playerSize;
            ChangeRollSpeed(-_playerBallController.playerSize * 4);

            // Increase the scale of the AI depending on the scale of the prop
            // No scale overtime, but possibly should add later
            _aiGameObject.transform.localScale += collision.transform.localScale / 10;

            // Ensure child props' scale don't increase
            foreach (Transform child in _aiGameObject.transform)
            {
                child.transform.localScale -= collision.transform.localScale / 700;
            }

            // Disable the players's components and change its tag
            _player = null;
            collision.transform.GetComponent<SphereCollider>().enabled = false;
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.gameObject.tag = "Collected";

            // Check if player mesh color needs to change
            _gameManager.UpdatePlayerMeshColor();

            // Invoke the GameOver function in GameManager
            GameOver?.Invoke();
        }
        #endregion

        #region Collect Other AI
        // If AI collides with and collects another AI
        if (collision.gameObject.CompareTag(_AItag) && collision.gameObject.GetComponent<AIBallController>().aiSize < aiSize)
        {
            // Store the size of the collected AI
            float collectedaiSize = collision.gameObject.GetComponent<AIBallController>().aiSize;

            // Update meshes, stick the prop to the AI, increase AI's size number
            _playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            aiSize += collectedaiSize;
            ChangeRollSpeed(-collectedaiSize*4);


            // Increase the scale of the AI depending on the scale of the prop
            // No scale overtime, but possibly should add later
            _aiGameObject.transform.localScale += collision.transform.localScale / 10;

            // Ensure child props' scale don't increase
            foreach (Transform child in _aiGameObject.transform)
            {
                child.transform.localScale -= collision.transform.localScale / 700;
            }

            // Disable the AI's components and change its tag
            collision.gameObject.GetComponent<AIBallController>()._aiGameObject = null;
            collision.transform.GetComponent<SphereCollider>().enabled = false;
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.gameObject.tag = "Collected";

            // Check if player mesh color needs to change
            _gameManager.UpdatePlayerMeshColor();
        }
        #endregion

    }
}
