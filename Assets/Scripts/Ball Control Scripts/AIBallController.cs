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

    public static event Action GameOver;

    // Player Variables
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerBallController playerBallController;


    // AI Variables
    [SerializeField] private Rigidbody AIrigidbody;
    [SerializeField] private GameObject AIgameObject;
    [SerializeField] private float rollSpeed;
    public float AIsize = 1;
    private Vector3 AImovement;

    // AITotalSearchRadius is the (AIsize/2) + AISearchRadius
    [SerializeField] private float AISearchRadius;
    private float AITotalSearchRadius;

    public bool chasingPlayer = false;
    private bool otherAIChasingPlayer = false;


    private void Awake()
    {
        // Check if the player exists, then assign it to player var
        if (GameObject.FindGameObjectWithTag(_playerTag) != null)
            player = GameObject.FindGameObjectWithTag(_playerTag);
        else
            player = null;

        playerBallController = player.GetComponent<PlayerBallController>();
    }

    private void Start()
    {

    }

    void FixedUpdate()
    {
        DetermineAIState();
    }

    private void DetermineAIState()
    {
        // Increase AISearchRadius based on their size
        AITotalSearchRadius = (AIsize/2) + AISearchRadius;

        // First, we check if AI size > player size, also make sure neither are null
        if (player != null && AIgameObject != null &&
            AIsize > playerBallController.playerSize)
        {
            // Then check if the player is within the AISearchRadius
            if (Vector3.Distance(AIgameObject.transform.position, player.transform.position) <= AITotalSearchRadius)
            {
                // Requirements met, chase the player and set their color
                player.GetComponent<MeshRenderer>().material.color = Color.red;

                chasingPlayer = true;

                // Move AI towards player
                AImovement = (player.transform.position - this.transform.position).normalized;
                AIrigidbody.AddForce(AImovement * rollSpeed * Time.fixedDeltaTime);
            }
            // If the player is outside the AISearchRadius
            else
            {
                ChaseAI();
                ChaseProps();
                chasingPlayer = false;
                if (player != null)
                {
                    // Check if any other AI in the game is chasing the player
                    GameObject[] gos;
                    gos = GameObject.FindGameObjectsWithTag(_AItag);
                    foreach (GameObject go in gos)
                    {
                        // If it finds any other AI is chasing the player, cache it and do nothing
                        if (go.GetComponent<AIBallController>().chasingPlayer == true)
                        {
                            otherAIChasingPlayer = true;
                        }
                    }

                    // Don't set the player's color back to white if another AI is chasing them
                    if (!otherAIChasingPlayer)
                        player.GetComponent<MeshRenderer>().material.color = Color.white;
                }
            }
        }
        // If the player or AI doesn't exist, or if the AI is smaller than the player
        else
        {
            chasingPlayer = false;
            ChaseAI();
            ChaseProps();
            if (player != null)
            {
                // Check if any other AI in the game is chasing the player
                GameObject[] gos;
                gos = GameObject.FindGameObjectsWithTag(_AItag);
                foreach (GameObject go in gos)
                {
                    // If it finds any other AI is chasing the player, remember it and do nothing
                    if (go.GetComponent<AIBallController>().chasingPlayer == true)
                    {
                        otherAIChasingPlayer = true;
                    }
                    else otherAIChasingPlayer = false;
                }
                // Don't set the player's color back to white if another AI is chasing them
                if (!otherAIChasingPlayer)
                    player.GetComponent<MeshRenderer>().material.color = Color.white;
            }
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

        // Checking this again to make sure otherAIChasingPlayer gets set to false
        GameObject[] AIgos;
        AIgos = GameObject.FindGameObjectsWithTag(_AItag);
        foreach (GameObject go in AIgos)
        {
            // If it finds any other AI is chasing the player, cache it and do nothing
            if (go.GetComponent<AIBallController>().chasingPlayer == true)
            {
                otherAIChasingPlayer = true;
            }
            else otherAIChasingPlayer = false;
        }

        for (int i = 0; i < gos.Length; i++)
        {
            // Trying to implement the AI looking for a different prop if the prop its trying to collect is too big
            // but it no work :(
            //if (gos[i].transform.localScale.magnitude > AIsize) { return; }
            //else
            //{
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
                AImovement = (closest.transform.position - this.transform.position).normalized;
            //}
        }
        AIrigidbody.AddForce(AImovement * rollSpeed * Time.fixedDeltaTime);

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
            if (go != null && AIgameObject != null && AIsize > go.GetComponent<AIBallController>().AIsize)
            { 
                // Check if the AI is close enough to the enemy
                if (Vector3.Distance(AIgameObject.transform.position, go.transform.position) <= AITotalSearchRadius)
                {
                    chasingAI = true;
                    AImovement = (closest.transform.position - this.transform.position).normalized;
                }
                else chasingAI = false;
            }
            else chasingAI = false;
            if (chasingAI) 
                AIrigidbody.AddForce(AImovement * rollSpeed * Time.fixedDeltaTime);
        }
        

    }

    void OnTriggerEnter(Collider collision)
    {

        #region Collect Prop
        // If AI collides with and collects a prop
        if (AIgameObject != null && collision.gameObject.CompareTag(_propTag) && collision.transform.localScale.magnitude * 5 <= AIsize)
        {

            // Store the size of the collected prop
            float collectedPropSize = collision.transform.localScale.magnitude;

            // Update meshes, stick the prop to the AI, increase AI's size number
            playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            AIsize += collectedPropSize;

            // Increase the scale of the AI depending on the scale of the prop
            // No scale overtime, but possibly should add later
            AIgameObject.transform.localScale += collision.transform.localScale / 10;

            // Ensure child props' scale don't increase
            foreach (Transform child in AIgameObject.transform)
            {
                child.transform.localScale -= collision.transform.localScale / 1000;
            }

            // Disable the prop's collider and change it's material
            collision.transform.GetComponent<BoxCollider>().enabled = false;
            collision.transform.GetComponent<MeshRenderer>().material.color = Color.green;
        }
        #endregion

        #region Collect Player
        // If AI collides with and collects a player
        if (collision.gameObject.CompareTag(_playerTag) && playerBallController.playerSize < AIsize)
        {
            // Store the size of the collected player
            float collectedPlayerSize = collision.gameObject.GetComponent<PlayerBallController>().playerSize;

            // Update meshes, stick the prop to the AI, increase AI's size number
            playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            AIsize += playerBallController.playerSize;

            // Increase the scale of the AI depending on the scale of the prop
            // No scale overtime, but possibly should add later
            AIgameObject.transform.localScale += collision.transform.localScale / 10;

            // Ensure child props' scale don't increase
            foreach (Transform child in AIgameObject.transform)
            {
                child.transform.localScale -= collision.transform.localScale / 700;
            }

            // Disable the players's components and change its tag
            player = null;
            foreach (var spherecollider in collision.transform.GetComponents<SphereCollider>())
            {
                spherecollider.enabled = false;
            }
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.transform.tag = "Collected";

            // Invoke the GameOver function in GameManager
            GameOver?.Invoke();
        }
        #endregion

        #region Collect Other AI
        // If AI collides with and collects another AI
        if (collision.gameObject.CompareTag(_AItag) && collision.gameObject.GetComponent<AIBallController>().AIsize < AIsize)
        {
            // Store the size of the collected AI
            float collectedAISize = collision.gameObject.GetComponent<AIBallController>().AIsize;

            // Update meshes, stick the prop to the AI, increase AI's size number
            playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            AIsize += collectedAISize;

            // Increase the scale of the AI depending on the scale of the prop
            // No scale overtime, but possibly should add later
            AIgameObject.transform.localScale += collision.transform.localScale / 10;

            // Ensure child props' scale don't increase
            foreach (Transform child in AIgameObject.transform)
            {
                child.transform.localScale -= collision.transform.localScale / 700;
            }

            // Disable the AI's components and change its tag
            collision.gameObject.GetComponent<AIBallController>().AIgameObject = null;
            foreach (var spherecollider in collision.transform.GetComponents<SphereCollider>())
            {
                spherecollider.enabled = false;
            }
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.transform.tag = "Collected";
        }
        #endregion

    }
}
