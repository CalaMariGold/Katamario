using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour
{

    private GameObject player;

    private const string _playerTag = "Player";
    private const string _boostTag = "BoostPowerUp";
    private const string _propTag = "Prop";

    static List<GameObject> boostUsers = new List<GameObject>();
    static List<GameObject> magnetUsers = new List<GameObject>();

    bool cooldown = false;
    bool alreadyBoosting = false;

    [SerializeField] public GameObject pickupEffect;
    [SerializeField] private GameObject boostImage;
    [SerializeField] private GameObject BoostCooldownImage;
    [SerializeField] private GameObject speedParticles;

    private GameObject[] _props;

    private void Awake()
    {
        // Check if the player exists, then assign it to player var
        if (GameObject.FindGameObjectWithTag(_playerTag) != null)
            player = GameObject.FindGameObjectWithTag(_playerTag);
        else
            player = null;

        _props = GameObject.FindGameObjectsWithTag(_propTag);


    }

    private void Start()
    {
        

    }

    private void Update()
    {
        #region Boost
        foreach (GameObject user in boostUsers)
        {
            if (cooldown == false)
            {
                boostImage.SetActive(true);
                BoostCooldownImage.SetActive(false);
            }
            else
            {
                boostImage.SetActive(false);
                BoostCooldownImage.SetActive(true);
            }

            if (!alreadyBoosting && Input.GetKeyDown(KeyCode.Space))
            {
                if (cooldown == false) {
                    StartCoroutine(BoostCoolDown(3, 5));
                    user.GetComponent<PlayerBallController>().changeRollSpeed(400);
                }

            }
            else if (cooldown == true)
                user.GetComponent<PlayerBallController>().rollSpeed = 400;
        }
        #endregion

        #region Magnet
        foreach (GameObject user in magnetUsers)
        {
            for (int i = 0; i < _props.Length; i++)
            {
                if (_props[i] != null && Vector3.Distance(_props[i].transform.position, user.transform.position) <= 2)
                {

                    if (_props[i] == null ||
                        _props[i].GetComponentInParent<AIBallController>() != null ||
                        _props[i].GetComponentInParent<PlayerBallController>() != null ||
                        _props[i].transform.position == user.transform.position ||
                        _props[i].transform.localScale.magnitude * 5 > user.GetComponent<PlayerBallController>().playerSize)
                    {
                        i++;
                    }
                    else
                    {
                        _props[i].GetComponent<BoxCollider>().isTrigger = true;
                        _props[i].transform.position = Vector3.MoveTowards(_props[i].transform.position, user.transform.position, Time.deltaTime * 3.5f);
                    }



                }
            }
        }
        #endregion
    }

    private IEnumerator BoostCoolDown(int boostDuration, int boostCooldown)
    {
        // Player is currently boosting
        alreadyBoosting = true;
        boostImage.GetComponent<Image>().color = new Color32(160, 160, 160, 255);
        speedParticles.SetActive(true);
        yield return new WaitForSeconds(boostDuration);

        // Player is no longer boosting, starting cooldown
        boostImage.GetComponent<Image>().color = new Color(255, 255, 255, 100);
        speedParticles.SetActive(false);
        cooldown = true;
        alreadyBoosting = false;
        yield return new WaitForSeconds(boostCooldown);

        // Boost is available again
        cooldown = false;

        yield return null;
    }

    public void PickUpBoost(GameObject player)
    {
        // Spawn particle effect at player location when picking up boost
        Instantiate(pickupEffect, player.transform.position, player.transform.rotation);

        // Add the player to one of the player's who have the boost
        // I dont think this implementation is gonna work if we decide to do splitscreen co-op or whatever else
        boostUsers.Add(player);
    }
    public void PickUpMagnet(GameObject player)
    {
        Instantiate(pickupEffect, player.transform.position, player.transform.rotation);

        magnetUsers.Add(player);
    }


}
