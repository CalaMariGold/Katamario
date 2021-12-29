using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    [Header("Game Over")]
    [SerializeField] private TMP_Text _gameOverStatsText;
    [SerializeField] private GameObject _gameOverParentObject;

    [Header("Win")]
    [SerializeField] private TMP_Text _winStatsText;
    [SerializeField] private GameObject _winParentObject;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text _playerCurrentScoreText;

    [Space]

    private PlayerBallController playerBallController;

    // For debugging and testing, adjust in inspector
    [SerializeField] private float timeScale = 1;

    private void Awake()
    {
        playerBallController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBallController>();
    }
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Time.timeScale = timeScale;

        // Displays the score of the player, which is their size minus the starting size * 100
        _playerCurrentScoreText.text = "Score: " + (Mathf.Round((playerBallController.playerSize - 1) * 100));
    }

    private void OnEnable()
    {
        AIBallController.GameOver += LoseScreen;
        PlayerBallController.WinGame += WinScreen;
    }
    private void OnDisable()
    {
        AIBallController.GameOver -= LoseScreen;
        PlayerBallController.WinGame += WinScreen;
    }

    private void LoseScreen()
    {
        _playerCurrentScoreText.GetComponentInParent<Transform>().gameObject.SetActive(false);

        _gameOverParentObject.SetActive(true);
        _gameOverStatsText.text = "Score: " + (Mathf.Round((playerBallController.playerSize - 1) * 100));
    }

    private void WinScreen()
    {
        _playerCurrentScoreText.GetComponentInParent<Transform>().gameObject.SetActive(false);

        _winParentObject.SetActive(true);
        _winStatsText.text = "Score: " + (Mathf.Round((playerBallController.playerSize - 1) * 100));
    }
}
