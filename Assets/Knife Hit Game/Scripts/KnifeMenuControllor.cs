using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KnifeMenuControllor : MonoBehaviour
{
    private const string SelectedKnifeKey = "SelectedKnifeId";

    [SerializeField] private GameObject knifeMenu;
    [SerializeField] private Button exit;
    [SerializeField] private GameObject popupPraent;
    [SerializeField] private GameObject failPopup;
    [SerializeField] private GameObject purchasedPopup;
    [SerializeField] private List<PurchaseableKnifeData> purchaseableKnivesList;

    public static KnifeMenuControllor Instance { get; private set; }

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
        if (exit != null)
        {
            exit.onClick.AddListener(CloseKnifeMenu);
        }

        InitialiseAllKnives();
        LoadSelectedKnife();
    }

    public void OpenKnifeMenu()
    {
        RefreshAllKnifeUI();

        if (knifeMenu != null)
        {
            knifeMenu.SetActive(true);
        }
    }

    public void CloseKnifeMenu()
    {
        SoundManager.Instance.PlaySFX("button");
        if (knifeMenu != null)
        {
            knifeMenu.SetActive(false);
        }
        GameController.instance.Menu.SetActive(true);
    }

    public void BuyKnife(PurchaseableKnifeData knifeData)
    {
        if (knifeData == null || knifeData.IsOwned())
        {
            return;
        }

        if (GameController.instance == null)
        {
            Debug.LogError("GameController is missing. Cannot buy knife.");
            return;
        }

        if (!GameController.instance.TrySpendApples(knifeData.Price))
        {
            SoundManager.Instance.PlaySFX("notpurchased");
            StartCoroutine(ShowPopup(1.5f, false));
            Debug.Log("Not enough apples to buy: " + knifeData.KnifeId);
            return;
        }

        knifeData.SetOwned();
        StartCoroutine(ShowPopup(1.5f, true));
        SoundManager.Instance.PlaySFX("purchased");

        // After purchase:
        // Buy false, amount false, selected image false, Select button true.
        RefreshAllKnifeUI();
    }

    public void SelectKnife(PurchaseableKnifeData knifeData)
    {
        if (knifeData == null || !knifeData.IsOwned())
        {
            return;
        }

        if (knifeData.KnifePrefab == null)
        {
            Debug.LogError("No knife prefab assigned for: " + knifeData.KnifeId);
            return;
        }

        // This overwrites the old selected knife.
        // Therefore only one knife can ever be selected.
        SoundManager.Instance.PlaySFX("button");
        PlayerPrefs.SetString(SelectedKnifeKey, knifeData.KnifeId);
        PlayerPrefs.Save();

        if (SpawnController.instance != null)
        {
            SpawnController.instance.SetKnifePrefab(knifeData.KnifePrefab);
        }

        // Every item checks whether it is the one saved as selected.
        // Old selected image becomes false automatically.
        RefreshAllKnifeUI();
    }

    public bool IsKnifeSelected(PurchaseableKnifeData knifeData)
    {
        if (knifeData == null)
        {
            return false;
        }

        return PlayerPrefs.GetString(SelectedKnifeKey, string.Empty) == knifeData.KnifeId;
    }

    private void InitialiseAllKnives()
    {
        foreach (PurchaseableKnifeData knifeData in purchaseableKnivesList)
        {
            if (knifeData != null)
            {
                knifeData.Initialise(this);
            }
        }
    }

    private void LoadSelectedKnife()
    {
        string savedKnifeId = PlayerPrefs.GetString(SelectedKnifeKey, string.Empty);
        PurchaseableKnifeData selectedKnife = FindKnifeById(savedKnifeId);

        // First launch: select the first knife you marked Owned in Inspector.
        if (selectedKnife == null || !selectedKnife.IsOwned())
        {
            selectedKnife = FindFirstOwnedKnife();
        }

        if (selectedKnife == null)
        {
            Debug.LogWarning("No starter knife is owned. Enable Owned on one knife.");
            RefreshAllKnifeUI();
            return;
        }

        PlayerPrefs.SetString(SelectedKnifeKey, selectedKnife.KnifeId);
        PlayerPrefs.Save();

        if (SpawnController.instance != null)
        {
            SpawnController.instance.SetKnifePrefab(selectedKnife.KnifePrefab);
        }

        RefreshAllKnifeUI();
    }

    private void RefreshAllKnifeUI()
    {
        foreach (PurchaseableKnifeData knifeData in purchaseableKnivesList)
        {
            if (knifeData != null)
            {
                knifeData.RefreshUI(IsKnifeSelected(knifeData));
            }
        }
    }

    private PurchaseableKnifeData FindKnifeById(string knifeId)
    {
        foreach (PurchaseableKnifeData knifeData in purchaseableKnivesList)
        {
            if (knifeData != null && knifeData.KnifeId == knifeId)
            {
                return knifeData;
            }
        }

        return null;
    }

    private PurchaseableKnifeData FindFirstOwnedKnife()
    {
        foreach (PurchaseableKnifeData knifeData in purchaseableKnivesList)
        {
            if (knifeData != null && knifeData.IsOwned())
            {
                return knifeData;
            }
        }

        return null;
    }

    public IEnumerator ShowPopup(float duration, bool success)
    {
        if (popupPraent == null)
        {
            yield break;
        }
        popupPraent.SetActive(true);
        if(success)
        {
            purchasedPopup.SetActive(true);
        }
        else
        {
            failPopup.SetActive(true);
        }
        yield return new WaitForSeconds(duration);
        popupPraent.SetActive(false);
        if(failPopup.activeSelf) failPopup.SetActive(false);
        if(purchasedPopup.activeSelf) purchasedPopup.SetActive(false);
    }
}