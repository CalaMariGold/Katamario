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

    // Other Script Variables
    [SerializeField] private GameManager gameManager;


    private void Awake()
    {
        // Check if the player exists, then assign it to player var
        if (GameObject.FindGameObjectWithTag(_playerTag) != null)
            player = GameObject.FindGameObjectWithTag(_playerTag);
        else
            player = null;

        playerBallController = player.GetComponent<PlayerBallController>();
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
        rollSpeed += speed;
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
            AImovement = (closest.transform.position - this.transform.position).normalized;
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
        if (AIgameObject != null && collision.gameObject.CompareTag(_propTag) && collision.transform.localScale.magnitude * 5 <= AIsize)
        {
            // Store the size of the collected prop
            float collectedPropSize = collision.transform.localScale.magnitude;

            // Update meshes, stick the prop to the AI, increase AI's size number, decrease speed
            playerBallController.UpdateCanCollectMesh();
            collision.transform.parent = transform;
            AIsize += collectedPropSize;
            ChangeRollSpeed(-collectedPropSize * 4);
            

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
            collision.gameObject.tag = "Collected";

            // Check if player mesh color needs to change
            gameManager.UpdatePlayerMeshColor();
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
            ChangeRollSpeed(-playerBallController.playerSize * 4);

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
            collision.transform.GetComponent<SphereCollider>().enabled = false;
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.gameObject.tag = "Collected";

            // Check if player mesh color needs to change
            gameManager.UpdatePlayerMeshColor();

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
            ChangeRollSpeed(-collectedAISize*4);


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
            collision.transform.GetComponent<SphereCollider>().enabled = false;
            collision.transform.GetComponent<Rigidbody>().isKinematic = true;
            collision.gameObject.tag = "Collected";

            // Check if player mesh color needs to change
            gameManager.UpdatePlayerMeshColor();
        }
        #endregion

    }
}
