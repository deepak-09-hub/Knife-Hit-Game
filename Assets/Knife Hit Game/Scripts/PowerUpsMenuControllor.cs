using UnityEngine;
using UnityEngine.UI;

public class PowerUpsMenuController : MonoBehaviour
{
    [SerializeField] private GameObject powerUpsMenu;

    [SerializeField] private GameObject halfHitslock;
    [SerializeField] private GameObject multiplierlock;
    [SerializeField] private GameObject allAppleslock;
    [SerializeField] private GameObject secondChancelock;
    [SerializeField] private GameObject controlTrunklock;
    [SerializeField] private GameObject variableKniveslock;
    [SerializeField] private GameObject superStrikelock;

    [SerializeField] private Button exit;

    [HideInInspector]public bool halfHitsUnlocked = false;
    [HideInInspector]public bool multiplierUnlocked = false;
    [HideInInspector]public bool allApplesUnlocked = false;
    [HideInInspector]public bool secondChanceUnlocked = false;
    [HideInInspector]public bool controlTrunkUnlocked = false;
    [HideInInspector]public bool variableKnivesUnlocked = false;
    [HideInInspector]public bool superStrikeUnlocked = false;

    public static PowerUpsMenuController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        exit.onClick.AddListener(ClosePowerUpsMenu);

        LoadPowerUps();
    }

    public void OpenPowerUpsMenu()
    {
        ShowPowerup();
        powerUpsMenu.SetActive(true);
    }

    public void ClosePowerUpsMenu()
    {
        SoundManager.Instance.PlaySFX("button");
        GameController.instance.Menu.SetActive(true);
        powerUpsMenu.SetActive(false);
    }

    private void ShowPowerup()
    {
        halfHitslock.SetActive(!halfHitsUnlocked);
        multiplierlock.SetActive(!multiplierUnlocked);
        allAppleslock.SetActive(!allApplesUnlocked);
        secondChancelock.SetActive(!secondChanceUnlocked);
        controlTrunklock.SetActive(!controlTrunkUnlocked);
        variableKniveslock.SetActive(!variableKnivesUnlocked);
        superStrikelock.SetActive(!superStrikeUnlocked);
    }

    private void LoadPowerUps()
    {
        halfHitsUnlocked = PlayerPrefs.GetInt("HalfHitsUnlocked", 0) == 1;
        multiplierUnlocked = PlayerPrefs.GetInt("MultiplierUnlocked", 0) == 1;
        allApplesUnlocked = PlayerPrefs.GetInt("AllApplesUnlocked", 0) == 1;
        secondChanceUnlocked = PlayerPrefs.GetInt("SecondChanceUnlocked", 0) == 1;
        controlTrunkUnlocked = PlayerPrefs.GetInt("ControlTrunkUnlocked", 0) == 1;
        variableKnivesUnlocked = PlayerPrefs.GetInt("VariableKnivesUnlocked", 0) == 1;
        superStrikeUnlocked = PlayerPrefs.GetInt("SuperStrikeUnlocked", 0) == 1;
    }

    private void SavePowerUps()
    {
        PlayerPrefs.SetInt("HalfHitsUnlocked", halfHitsUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("MultiplierUnlocked", multiplierUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("AllApplesUnlocked", allApplesUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("SecondChanceUnlocked", secondChanceUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("ControlTrunkUnlocked", controlTrunkUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("VariableKnivesUnlocked", variableKnivesUnlocked ? 1 : 0);
        PlayerPrefs.SetInt("SuperStrikeUnlocked", superStrikeUnlocked ? 1 : 0);

        PlayerPrefs.Save();
    }

    private void UnlockPowerUp(string powerUpName)
    {
        switch (powerUpName)
        {
            case "HalfHits":
                halfHitsUnlocked = true;
                break;
            case "Multiplier":
                multiplierUnlocked = true;
                break;
            case "AllApples":
                allApplesUnlocked = true;
                break;
            case "SecondChance":
                secondChanceUnlocked = true;
                break;
            case "ControlTrunk":
                controlTrunkUnlocked = true;
                break;
            case "VariableKnives":
                variableKnivesUnlocked = true;
                break;
            case "SuperStrike":
                superStrikeUnlocked = true;
                break;
            default:
                Debug.LogWarning($"Unknown power-up name: {powerUpName}");
                return;
        }
        SavePowerUps();
        ShowPowerup();
    }

    public void AwardRandomPowerUp()
    {
        string[] powerUps = new string[]
        {
            "HalfHits",
            "Multiplier",
            "AllApples",
            "SecondChance",
            "ControlTrunk",
            "VariableKnives",
            "SuperStrike"
        };
        int randomIndex = Random.Range(0, powerUps.Length);
        UnlockPowerUp(powerUps[randomIndex]);
    }
}
