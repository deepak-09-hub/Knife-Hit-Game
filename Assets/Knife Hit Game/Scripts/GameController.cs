using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour 
{
    private int score = 0;
    public Button start;
    public Button restart;
    public GameObject spawnPoint;
    public GameObject startScreen;
    public GameObject gameOver;
    public TMP_Text scoreText;
    public TMP_Text healthText;
    public int lives = 3; // can be used for power-ups or health system
    //public TMP_Text appleCount;

    public static GameController instance;

    // Use this for initialization
    void Start () 
    {
        start.onClick.AddListener(Play);
        restart.onClick.AddListener(Restart);
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void SetScore(int newScore)
    {
        score += newScore;
        updateScore(score);
    }

    public void ResetScore()
    {
        score = 0;
        updateScore(score);
    }

    public int GetScore()
    {
        return score;
    }

    public void Play()
    {
        startScreen.SetActive(false);
        start.gameObject.SetActive(false);
        SpawnController.instance.ClearKnives();
        TrunkController.instance.spwanTrunk();
        SpawnController.instance.SpawnOnject();
    }

    private void Restart()
    {
        startScreen.SetActive(false);
        gameOver.SetActive(false);
        restart.gameObject.SetActive(false);
        SpawnController.instance.ClearKnives();
        TrunkController.instance.spwanTrunk();
        SpawnController.instance.SpawnOnject();
    }

    public void ShowRestartScreen()
    {
        startScreen.SetActive(true);
        gameOver.SetActive(true);
        restart.gameObject.SetActive(true);
    }

    private void updateScore(int s)
    {
        scoreText.text = "Score: " + s;
    }

    public void updateHealthText(int health)
    {
        healthText.text = "Health: " + health.ToString();
    }
}
