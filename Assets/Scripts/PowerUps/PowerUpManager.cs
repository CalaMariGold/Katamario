using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerUpManager : MonoBehaviour
{
    static List<GameObject> boostUsers = new List<GameObject>();
    private GameObject player;
    private bool _cooldown = false;
    private bool _alreadyBoosting = false;
    private bool _changedRollSpeed;

    [Header("Power Ups")]
    [SerializeField] private GameObject boostPrefab;

    [Header("UI")]
    [SerializeField] private GameObject boostImage;
    [SerializeField] private GameObject boostIcon;
    [SerializeField] private GameObject BoostCooldownImage;
    [Range(0, 60)]
    [SerializeField] private float _defaultCD = 5;
    private float _currentCD;
    public TMP_Text _boostCDtext;

    [Header("Particles")]
    [SerializeField] private GameObject speedParticles;
    [SerializeField] private GameObject speedParticlesGround;
    [SerializeField] public GameObject pickupEffect;

    [Header("Audio")]
    [SerializeField] private AudioSource abilityAudioSource;
    [SerializeField] private AudioClip abilityReadyAudioClip;
    [SerializeField] private AudioClip boostingAudioClip;

    [Header("Other")]
    [SerializeField] private GameObject[] powerUpSpawnLocations;
    [SerializeField] public Transform powerUpSpawningParent;
    private const string _playerTag = "Player";
    private const string _boostTag = "BoostPowerUp";


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
        SpawnPowerUp();
    }

    private void Update()
    {
        foreach (GameObject user in boostUsers)
        {
            if (_cooldown == false)
            {
                boostImage.SetActive(true);
                BoostCooldownImage.SetActive(false);
                _boostCDtext.gameObject.SetActive(false);
                _currentCD = _defaultCD;
            }
            else
            {
                boostImage.SetActive(false);
                BoostCooldownImage.SetActive(true);
                _boostCDtext.gameObject.SetActive(true);
                _currentCD -= Time.deltaTime;
                _boostCDtext.text = ((int)_currentCD).ToString();
            }


            if (!_alreadyBoosting && Input.GetKeyDown(KeyCode.Space))
            {
                if (_cooldown == false)
                {
                    _changedRollSpeed = false;
                    StartCoroutine(BoostCoolDown(3, (int)_defaultCD));
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

    private void SpawnPowerUp()
    {
        int rand = Random.Range(0, 6);
        var newBoost = Instantiate(boostPrefab, powerUpSpawnLocations[rand].transform.position, Quaternion.identity);
        newBoost.transform.parent = powerUpSpawningParent;
    }

    private IEnumerator BoostCoolDown(int boostDuration, int boostCooldown)
    {
        // Player is currently boosting
        _alreadyBoosting = true;
        boostIcon.GetComponent<Image>().color = new Color32(255, 150, 150, 255);
        speedParticles.SetActive(true);
        speedParticlesGround.SetActive(true);
        speedParticlesGround.GetComponent<ParticleSystem>().Play();
        abilityAudioSource.PlayOneShot(boostingAudioClip, 0.8f);
        yield return new WaitForSeconds(boostDuration);

        // Player is no longer boosting, starting cooldown
        boostIcon.GetComponent<Image>().color = new Color(255, 0, 0, 100);
        speedParticlesGround.GetComponent<ParticleSystem>().Stop();
        speedParticles.SetActive(false);
        _cooldown = true;
        _alreadyBoosting = false;
        yield return new WaitForSeconds(boostCooldown);

        // Boost is available again
        _cooldown = false;
        abilityAudioSource.PlayOneShot(abilityReadyAudioClip, 1f);

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
