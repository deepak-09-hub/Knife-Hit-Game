using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseableKnifeData : MonoBehaviour
{
    [Header("Knife Data")]
    [SerializeField] private string knifeId;

    [SerializeField] private Image knifeimage;
    [SerializeField] private int price;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Knife adjacentKnifePrefab;

    [Header("UI")]
    [SerializeField] private Button selectknife;
    [SerializeField] private GameObject lockedImage;
    [SerializeField] private Image selectedImage;

    [Header("Starting Ownership")]
    [SerializeField] private bool owned;

    private KnifeMenuControllor menuController;

    public string KnifeId =>
        string.IsNullOrWhiteSpace(knifeId) ? gameObject.name : knifeId;

    public int Price => Mathf.Max(0, price);
    public Knife KnifePrefab => adjacentKnifePrefab;

    private string OwnedKey => "KnifeOwned_" + KnifeId;

    public void Initialise(KnifeMenuControllor controller)
    {
        menuController = controller;

        CreateOwnershipSaveIfNeeded();

        if (selectknife != null)
        {
            selectknife.onClick.RemoveListener(UseKnifeButton);
            selectknife.onClick.AddListener(UseKnifeButton);
        }

        RefreshUI(menuController != null && menuController.IsKnifeSelected(this));
    }

    public bool IsOwned()
    {
        return PlayerPrefs.GetInt(OwnedKey, owned ? 1 : 0) == 1;
    }

    public void SetOwned()
    {
        owned = true;

        PlayerPrefs.SetInt(OwnedKey, 1);
        PlayerPrefs.Save();
    }

    public void RefreshUI(bool isSelected)
    {
        bool isOwned = IsOwned();

        if (amountText != null)
        {
            amountText.text = Price.ToString();
            amountText.gameObject.SetActive(!isOwned);
        }

        if (lockedImage != null)
        {
            lockedImage.SetActive(!isOwned);
        }

        if (selectknife != null)
        {
            bool showButton = !isSelected;

            selectknife.gameObject.SetActive(showButton);
            selectknife.interactable = showButton;
        }

        if (selectedImage != null)
        {
            selectedImage.gameObject.SetActive(isOwned && isSelected);
        }
    }

    private void UseKnifeButton()
    {
        if (menuController == null)
        {
            return;
        }

        if (IsOwned())
        {
            menuController.SelectKnife(this);
        }
        else
        {
            menuController.BuyKnife(this);
        }
    }

    private void CreateOwnershipSaveIfNeeded()
    {
        if (!PlayerPrefs.HasKey(OwnedKey))
        {
            PlayerPrefs.SetInt(OwnedKey, owned ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}