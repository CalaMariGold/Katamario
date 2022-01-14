using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerUp : MonoBehaviour
{

    private GameObject player;

    private const string _playerTag = "Player";
    private const string _boostTag = "BoostPowerUp";

    static List<GameObject> boostUsers = new List<GameObject>();

    private bool _cooldown = false;
    private bool _alreadyBoosting = false;

    private bool _changedRollSpeed;

    [SerializeField] public GameObject pickupEffect;

    [SerializeField] private GameObject boostImage;
    [SerializeField] private GameObject BoostCooldownImage;

    [SerializeField] private GameObject speedParticles;

    private void Awake()
    {
        // Check if the player exists, then assign it to player var
        if (GameObject.FindGameObjectWithTag(_playerTag) != null)
            player = GameObject.FindGameObjectWithTag(_playerTag);
        else
            player = null;

    }

    private void Start()
    {
        

    }

    private void Update()
    {

        foreach (GameObject user in boostUsers)
        {
            if (_cooldown == false)
            {
                boostImage.SetActive(true);
                BoostCooldownImage.SetActive(false);
            }
            else
            {
                boostImage.SetActive(false);
                BoostCooldownImage.SetActive(true);
            }


            if (!_alreadyBoosting && Input.GetKeyDown(KeyCode.Space))
            {
                if (_cooldown == false)
                {
                    _changedRollSpeed = false;
                    StartCoroutine(BoostCoolDown(3, 5));
                    user.GetComponent<PlayerBallController>().ChangeRollSpeed(500);
                }

            }
            else if (_cooldown == true && _changedRollSpeed == false)
            {
                user.GetComponent<PlayerBallController>().ChangeRollSpeed(-500);
                _changedRollSpeed = true;
            }
        }
    }

    private IEnumerator BoostCoolDown(int boostDuration, int boostCooldown)
    {
        // Player is currently boosting
        _alreadyBoosting = true;
        boostImage.GetComponent<Image>().color = new Color32(160, 160, 160, 255);
        speedParticles.SetActive(true);
        yield return new WaitForSeconds(boostDuration);

        // Player is no longer boosting, starting cooldown
        boostImage.GetComponent<Image>().color = new Color(255, 255, 255, 100);
        speedParticles.SetActive(false);
        _cooldown = true;
        _alreadyBoosting = false;
        yield return new WaitForSeconds(boostCooldown);

        // Boost is available again
        _cooldown = false;

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


}
