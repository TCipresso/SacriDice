using UnityEngine;
using TMPro;

public class WalletManager : MonoBehaviour
{
    public static WalletManager Instance { get; private set; }

    [Header("Starting Settings")]
    [Min(0)] public int startingCash = 100;
    [Min(1)] public int targetCash = 100; // cash required to win

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI cashText;
    public GameObject WinScreen;
    public GameObject TextBox8;  // show this if they don't meet the requirement

    public int CurrentCash { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CurrentCash = startingCash;
        Refresh();
    }

    public bool CanAfford(int amount) => amount <= CurrentCash;

    public void Spend(int amount)
    {
        CurrentCash = Mathf.Max(0, CurrentCash - Mathf.Max(0, amount));
        Refresh();
    }

    public void AddCash(int amount)
    {
        CurrentCash += Mathf.Max(0, amount);
        Refresh();
    }

    public void SetCash(int amount)
    {
        CurrentCash = Mathf.Max(0, amount);
        Refresh();
    }

    void Refresh()
    {
        if (cashText != null)
            cashText.text = $"Chips: {CurrentCash}";
    }

    public void ShowWinScreen()
    {
        if (CurrentCash >= targetCash)
        {
            if (WinScreen != null) WinScreen.SetActive(true);
        }
        else
        {
            if (TextBox8 != null) TextBox8.SetActive(true);
        }
    }
}
