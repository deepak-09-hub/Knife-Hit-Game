using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseableKnifeData : MonoBehaviour
{
    [Header("Knife Data")]
    [Tooltip("Must be unique. Example: starter_knife, gold_knife")]
    [SerializeField] private string knifeId;

    [SerializeField] private Image knifeimage;
    [SerializeField] private int price;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Knife adjacentKnifePrefab;

    [Header("UI")]
    [SerializeField] private Button selectknife;
    [SerializeField] private Button buy;
    [SerializeField] private GameObject lockedImage;
    [SerializeField] private Image selectedImage;

    [Header("Starting Ownership")]
    [Tooltip("Only enable this for your free starter knife.")]
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

        if (buy != null)
        {
            buy.onClick.RemoveListener(BuyKnife);
            buy.onClick.AddListener(BuyKnife);
        }

        if (selectknife != null)
        {
            selectknife.onClick.RemoveListener(SelectKnife);
            selectknife.onClick.AddListener(SelectKnife);
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

        // Price always gets assigned, but only shows while the knife is locked.
        if (amountText != null)
        {
            amountText.text = Price.ToString();
            amountText.gameObject.SetActive(!isOwned);
        }

        // Locked knife: show Buy.
        if (buy != null)
        {
            lockedImage.SetActive(!isOwned);
            buy.gameObject.SetActive(!isOwned);
            buy.interactable = !isOwned;
        }

        // Owned, unselected knife: show Select.
        if (selectknife != null)
        {
            bool showSelectButton = isOwned && !isSelected;

            selectknife.gameObject.SetActive(showSelectButton);
            selectknife.interactable = showSelectButton;
        }

        // Only the currently selected knife shows this.
        if (selectedImage != null)
        {
            selectedImage.gameObject.SetActive(isOwned && isSelected);
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

    private void BuyKnife()
    {
        if (menuController != null)
        {
            menuController.BuyKnife(this);
        }
    }

    private void SelectKnife()
    {
        if (menuController != null)
        {
            menuController.SelectKnife(this);
        }
    }
}