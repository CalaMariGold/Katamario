using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // Scripts
    private PlayerBallController _playerBallController;

    // Objects
    private GameObject[] _aiObjects;
    private GameObject _player;

    // Other
    private float _playerScore;

    [Header("Game Over")]
    [SerializeField] private TMP_Text _gameOverStatsText;
    [SerializeField] private GameObject _gameOverParentObject;

    [Header("Win")]
    [SerializeField] private TMP_Text _winStatsText;
    [SerializeField] private GameObject _winParentObject;

    [Header("Gameplay UI")]
    [SerializeField] private TMP_Text _playerCurrentScoreText;

    [Header("AI")]
    [SerializeField] bool aiChasingPlayer = false;

    [Header("Debugging/Testing")]
    [SerializeField] private float timeScale = 1;


    private void Awake()
    {
        _playerBallController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerBallController>();
        _aiObjects = GameObject.FindGameObjectsWithTag("AI");

        _player = GameObject.FindGameObjectWithTag("Player");
    }
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Time.timeScale = timeScale;

        // Displays the score of the player, which is their size minus the starting size * 100
        _playerCurrentScoreText.text = "Score: " + (Mathf.Round((_playerBallController.playerSize - 1) * 100));
    }

    private void OnEnable()
    {
        AIBallController.GameOver += LoseScreen;
        OutOfBounds.GameOver += LoseScreen;
        PlayerBallController.WinGame += WinScreen;
    }
    private void OnDisable()
    {
        AIBallController.GameOver -= LoseScreen;
        OutOfBounds.GameOver -= LoseScreen;
        PlayerBallController.WinGame += WinScreen;
    }

    public void UpdatePlayerMeshColor()
    {
        if (_player != null)
        {
            // Check if any other AI in the game is chasing the player
            foreach (GameObject go in _aiObjects)
            {
                // If it finds any other AI is chasing the player, set var to true
                if (go !=null && go.GetComponent<AIBallController>().chasingPlayer == true)
                {
                    aiChasingPlayer = true;
                    break;
                }
                else aiChasingPlayer = false;
            }

            // Change player's mesh depending on
            if (aiChasingPlayer)
                _player.GetComponent<MeshRenderer>().material.color = Color.red;
            else if(_player.tag.Equals("Player")) // Don't change after player died
                _player.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }

    private void LoseScreen()
    {
        _playerScore = (Mathf.Round((_playerBallController.playerSize - 1) * 100));
        _playerCurrentScoreText.GetComponentInParent<Transform>().gameObject.SetActive(false);

        _gameOverParentObject.SetActive(true);
        _gameOverStatsText.text = "Score: " + _playerScore;
    }

    private void WinScreen()
    {
        _playerScore = (Mathf.Round((_playerBallController.playerSize - 1) * 100));
        _playerCurrentScoreText.GetComponentInParent<Transform>().gameObject.SetActive(false);

        _winParentObject.SetActive(true);
        _winStatsText.text = "Score: " + _playerScore;
    }
}
