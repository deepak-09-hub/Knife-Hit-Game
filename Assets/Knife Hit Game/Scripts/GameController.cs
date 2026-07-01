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
    [SerializeField] private Button home;
    [SerializeField] private Button knives;
    [SerializeField] private Button powerUps;
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject logo;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private GameObject levelUp;
    [SerializeField] private GameObject scoreObject;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject hitsLeftObject;
    [SerializeField] private TMP_Text hitsLeftText;
    [SerializeField] public int level = 1;
    [SerializeField] private GameObject levelObject;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject appleCountObject;
    [SerializeField] private TMP_Text appleCountText;

    [Header("Assigned Power Up")]
    [Tooltip("Choose the ONE power-up the player receives for this run. The player cannot choose its type.")]
    [SerializeField] private PowerUpType assignedPowerUp = PowerUpType.None;


    [Tooltip("Only used when Assigned Power Up is Multiplier.")]
    [Min(1)]
    [SerializeField] private int multiplierValue = 2;

    [Header("Control Trunk Movement")]
    [SerializeField] private float controlTrunkDuration = 6f;
    [SerializeField] private float manualRotationDegreesPerButtonPress = 15f;

    private int score;
    private int totalApples;
    private int appleMultiplier = 1;
    private bool runIsActive;
    private Coroutine controlTrunkCoroutine;

    public static GameController instance;

    // This is the power-up currently waiting for its special event.
    // Example: Super Strike waits for the next trunk hit.
    public PowerUpType ActivePowerUp { get; private set; } = PowerUpType.None;

    public GameObject SpawnPoint => spawnPoint;
    public int TotalApples => totalApples;
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
        Application.targetFrameRate = 120;
    }

    private void Start()
    {
        start.onClick.AddListener(Play);
        restart.onClick.AddListener(Restart);
        home.onClick.AddListener(Home);
        powerUps.onClick.AddListener(PowerUps);
        knives.onClick.AddListener(Knives);

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
        logo.SetActive(false);
        start.gameObject.SetActive(false);
        knives.gameObject.SetActive(false);
        powerUps.gameObject.SetActive(false);
        scoreObject.SetActive(true);
        levelObject.SetActive(true);
        hitsLeftObject.SetActive(true);
        appleCountObject.SetActive(false);
        StartNewRun();
    }

    private void Restart()
    {
        gameOver.SetActive(false);
        restart.gameObject.SetActive(false);
        home.gameObject.SetActive(false);
        scoreObject.SetActive(true);
        levelObject.SetActive(true);
        hitsLeftObject.SetActive(true);
        appleCountObject.SetActive(false);
        StartNewRun();
    }

    private void Home()
    {
        logo.SetActive(true);
        gameOver.SetActive(false);
        restart.gameObject.SetActive(false);
        start.gameObject.SetActive(true);
        home.gameObject.SetActive(false);
        knives.gameObject.SetActive(true);
        powerUps.gameObject.SetActive(true);
        scoreObject.SetActive(false);
        levelObject.SetActive(false);
        hitsLeftObject.SetActive(false);
        appleCountObject.SetActive(true);
    }

    private void PowerUps()
    {
        PowerUpsMenuController.Instance.OpenPowerUpsMenu();
    }

    private void Knives()
    {
        KnifeMenuControllor.Instance.OpenKnifeMenu();
    }

    private void StartNewRun()
    {
        StopManualTrunkControl();
        ActivePowerUp = PowerUpType.None;

        appleMultiplier = 1;
        ActivateAssignedPowerUp();

        level = 1;
        runIsActive = true;

        ResetScore();
        UpdateAppleText();
        UpdateLevelText();

        TrunkController.instance.currentTrunkIndex = 0;
        SpawnController.instance.ClearKnives();
        TrunkController.instance.spwanTrunk();
        SpawnController.instance.SpawnOnject();
    }

    public void ShowRestartScreen()
    {
        EndCurrentRun();

        gameOver.SetActive(true);
        restart.gameObject.SetActive(true);
        home.gameObject.SetActive(true);
        scoreObject.SetActive(false);
        levelObject.SetActive(false);
        hitsLeftObject.SetActive(false);
        appleCountObject.SetActive(true);
    }

    private void EndCurrentRun()
    {
        runIsActive = false;
        ClearActivePowerUp();
    }

    public void UpdateHitsLeftText(int hitsLeft)
    {
        if (hitsLeftText != null)
        {
            hitsLeftText.text = hitsLeft.ToString();
        }
    }

    // Apples are permanent currency. They are saved immediately on collection.
    public void CollectApple(int amount = 1)
    {
        int applesToAdd = Mathf.Max(1, amount) * appleMultiplier;
        totalApples += applesToAdd;

        PlayerPrefs.SetInt(TotalApplesKey, totalApples);
        PlayerPrefs.Save();

        UpdateAppleText();
    }

    public bool TrySpendApples(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (totalApples < amount)
        {
            return false;
        }

        totalApples -= amount;

        PlayerPrefs.SetInt(TotalApplesKey, totalApples);
        PlayerPrefs.Save();

        UpdateAppleText();
        return true;
    }

    public void CollectAllRemainingApples()
    {
        Trunk currentTrunk = TrunkController.instance.currentTrunk;

        if (currentTrunk == null)
        {
            return;
        }

        Apple[] apples = currentTrunk.GetComponentsInChildren<Apple>(true);

        foreach (Apple apple in apples)
        {
            apple.Collect();
        }
    }


    private void ActivateAssignedPowerUp()
    {
        ActivePowerUp = assignedPowerUp;

        switch (assignedPowerUp)
        {
            case PowerUpType.Multiplier:
                appleMultiplier = Mathf.Max(1, multiplierValue);
                break;

            case PowerUpType.RewardAllApples:
                StartCoroutine(ApplyPowerUpAfterTrunkSpawns());
                break;

            case PowerUpType.HalfHits:
                StartCoroutine(ApplyPowerUpAfterTrunkSpawns());
                break;

            case PowerUpType.ControlTrunkMovement:
                StartCoroutine(ApplyPowerUpAfterTrunkSpawns());
                break;
        }
    }

    private IEnumerator ApplyPowerUpAfterTrunkSpawns()
    {
        yield return null;

        switch (ActivePowerUp)
        {
            case PowerUpType.RewardAllApples:
                CollectAllRemainingApples();
                ActivePowerUp = PowerUpType.None;
                break;

            case PowerUpType.HalfHits:
                ApplyHalfHits();
                ActivePowerUp = PowerUpType.None;
                break;

            case PowerUpType.ControlTrunkMovement:
                StartManualTrunkControl();
                break;
        }
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

    // Second Chance is consumed only when the knife touches another knife.
    public bool TryUseSecondChance()
    {
        if (ActivePowerUp != PowerUpType.SecondChance)
        {
            return false;
        }

        ClearActivePowerUp();
        return true;
    }

    // Bind your Left UI Button here.
    public void RotateTrunkLeft()
    {
        RotateTrunkManually(-manualRotationDegreesPerButtonPress);
    }

    // Bind your Right UI Button here.
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

        ActivePowerUp = PowerUpType.ControlTrunkMovement;
        trunk.SetManualControl(true);

        if (controlTrunkCoroutine != null)
        {
            StopCoroutine(controlTrunkCoroutine);
        }

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
            scoreText.text = score.ToString();
        }
    }

    private void UpdateAppleText()
    {
        if (appleCountText != null)
        {
            appleCountText.text = totalApples.ToString();
        }
    }

    public void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text = level.ToString();
        }
    }

    public void MoveToNextLevel()
    {
        StartCoroutine(NextLevel());
    }

    private IEnumerator NextLevel()
    {
        bool bossWasDefeated = level > 0 && level % 4 == 0;

        TrunkController.instance.DestroyCurrentTrunk();

        levelUp.SetActive(true);
        yield return new WaitForSeconds(1f);
        levelUp.SetActive(false);

        ClearActivePowerUp();
        appleMultiplier = 1;

        if (bossWasDefeated)
        {
            PowerUpsMenuController.Instance.AwardRandomPowerUp();
        }

        level += 1;
        UpdateLevelText();

        SpawnController.instance.ClearKnives();
        TrunkController.instance.spwanTrunk();
        SpawnController.instance.SpawnOnject();
    }
}
