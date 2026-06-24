using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public enum PowerUpType
    {
        None,
        SecondChance,
        SuperStrike,
        RewardAllApples,
        HalfHits,
        ControlTrunkMovement,
        Multiplier,
        VariableKnives
    }

    private const string TotalApplesKey = "TotalApples";

    [Header("Game UI")]
    [SerializeField] private Button start;
    [SerializeField] private Button restart;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] public int level = 1;
    //[SerializeField] private TMP_Text appleCountText;
    //[SerializeField] private TMP_Text powerUpText;

    [Header("Control Trunk Movement")]
    [SerializeField] private float controlTrunkDuration = 6f;
    [SerializeField] private float manualRotationDegreesPerButtonPress = 15f;

    private int score;
    private int totalApples;
    private int applesCollectedThisRun;
    private int appleMultiplier = 1;
    private bool runIsActive;
    private bool runRewardBanked;
    private Coroutine controlTrunkCoroutine;

    public static GameController instance;

    public GameObject SpawnPoint => spawnPoint;
    public PowerUpType ActivePowerUp { get; private set; } = PowerUpType.None;
    public int TotalApples => totalApples;
    public int ApplesCollectedThisRun => applesCollectedThisRun;
    public int AppleMultiplier => appleMultiplier;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            totalApples = PlayerPrefs.GetInt(TotalApplesKey, 0);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        start.onClick.AddListener(Play);
        restart.onClick.AddListener(Restart);

        UpdateScoreText();
        UpdateAppleText();
    }

    public void SetScore(int value)
    {
        score += value;
        UpdateScoreText();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }

    public int GetScore()
    {
        return score;
    }

    public void Play()
    {
        startScreen.SetActive(false);
        start.gameObject.SetActive(false);
        StartNewRun();
    }

    private void Restart()
    {
        startScreen.SetActive(false);
        gameOver.SetActive(false);
        restart.gameObject.SetActive(false);
        StartNewRun();
    }

    private void StartNewRun()
    {
        StopManualTrunkControl();
        ActivePowerUp = PowerUpType.None;
        applesCollectedThisRun = 0;
        appleMultiplier = 1;
        level = 1;
        runRewardBanked = false;
        runIsActive = true;

        ResetScore();
        UpdateAppleText();
        UpdateLevelText();

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

    public void updateHealthText(int health)
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + health;
        }
    }

    public void CollectApple(int amount = 1)
    {
        if (!runIsActive)
        {
            return;
        }

        applesCollectedThisRun += Mathf.Max(1, amount);
        UpdateAppleText();
    }

    public void CollectAllRemainingApples()
    {
        Apple[] apples = FindObjectsOfType<Apple>();

        foreach (Apple apple in apples)
        {
            apple.Collect();
        }
    }

    // Call this once when the player loses. It does not reset the saved wallet.
    public void FinalizeRun()
    {
        if (!runIsActive || runRewardBanked)
        {
            return;
        }

        int finalAppleReward = applesCollectedThisRun * appleMultiplier;
        totalApples += finalAppleReward;
        PlayerPrefs.SetInt(TotalApplesKey, totalApples);
        PlayerPrefs.Save();

        runRewardBanked = true;
        runIsActive = false;
        UpdateAppleText();
    }

    public bool TryActivatePowerUp(PowerUpType powerUpType, int multiplierValue = 2)
    {
        if (powerUpType == PowerUpType.None || ActivePowerUp != PowerUpType.None)
        {
            return false;
        }

        if (powerUpType == PowerUpType.VariableKnives)
        {
            //will use it later. 
            return false;
        }

        ActivePowerUp = powerUpType;

        switch (powerUpType)
        {
            case PowerUpType.RewardAllApples:
                CollectAllRemainingApples();
                ClearActivePowerUp();
                break;

            case PowerUpType.HalfHits:
                if (!ApplyHalfHits())
                {
                    ClearActivePowerUp();
                    return false;
                }

                ClearActivePowerUp();
                break;

            case PowerUpType.ControlTrunkMovement:
                if (!StartManualTrunkControl())
                {
                    ClearActivePowerUp();
                    return false;
                }
                break;

            case PowerUpType.Multiplier:
                appleMultiplier *= Mathf.Max(1, multiplierValue);
                ClearActivePowerUp();
                break;
        }

        return true;
    }

    // Super Strike is consumed by the next successful trunk hit.
    public int GetDamageForTrunkHit()
    {
        if (ActivePowerUp != PowerUpType.SuperStrike || TrunkController.instance.currentTrunk == null)
        {
            return 1;
        }

        int damage = Mathf.Max(1, TrunkController.instance.currentTrunk.currentHealth);
        ClearActivePowerUp();
        return damage;
    }

    // Second Chance is consumed only when the knife collides with another knife.
    public bool TryUseSecondChance()
    {
        if (ActivePowerUp != PowerUpType.SecondChance)
        {
            return false;
        }

        ClearActivePowerUp();
        return true;
    }

    // Left and Right Button
    public void RotateTrunkLeft()
    {
        RotateTrunkManually(-manualRotationDegreesPerButtonPress);
    }

    public void RotateTrunkRight()
    {
        RotateTrunkManually(manualRotationDegreesPerButtonPress);
    }

    private void RotateTrunkManually(float degrees)
    {
        if (ActivePowerUp != PowerUpType.ControlTrunkMovement || TrunkController.instance.currentTrunk == null)
        {
            return;
        }

        TrunkController.instance.currentTrunk.RotateManually(degrees);
    }

    private bool ApplyHalfHits()
    {
        Trunk trunk = TrunkController.instance.currentTrunk;

        if (trunk == null || trunk.currentHealth <= 0)
        {
            return false;
        }

        trunk.HalveRemainingHealth();
        TrunkController.instance.SyncHealthFromCurrentTrunk();
        return true;
    }

    private bool StartManualTrunkControl()
    {
        Trunk trunk = TrunkController.instance.currentTrunk;

        if (trunk == null)
        {
            return false;
        }

        trunk.SetManualControl(true);
        controlTrunkCoroutine = StartCoroutine(ManualTrunkControlTimer());
        return true;
    }

    private IEnumerator ManualTrunkControlTimer()
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, controlTrunkDuration));
        ClearActivePowerUp();
    }

    private void StopManualTrunkControl()
    {
        if (controlTrunkCoroutine != null)
        {
            StopCoroutine(controlTrunkCoroutine);
            controlTrunkCoroutine = null;
        }

        if (TrunkController.instance != null && TrunkController.instance.currentTrunk != null)
        {
            TrunkController.instance.currentTrunk.SetManualControl(false);
        }
    }

    private void ClearActivePowerUp()
    {
        if (ActivePowerUp == PowerUpType.ControlTrunkMovement)
        {
            StopManualTrunkControl();
        }

        ActivePowerUp = PowerUpType.None;
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score;
        }
    }

    private void UpdateAppleText()
    {
        //if (appleCountText != null)
        //{
        //    int visibleAppleCount = totalApples + applesCollectedThisRun;
        //    appleCountText.text = "Apples: " + visibleAppleCount;
        //}
    }

    public void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + level;
        }
    }
}