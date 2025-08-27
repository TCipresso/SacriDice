using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [Min(1)] public int maxHP = 10;
    public int currentHP;

    [Header("Game Over UI (enable on death)")]
    [SerializeField] private GameObject gameOverScreen; // assign in Inspector

    void Awake()
    {
        currentHP = Mathf.Clamp(currentHP <= 0 ? maxHP : currentHP, 0, maxHP);
        if (gameOverScreen != null) gameOverScreen.SetActive(false);
    }

    // --- Public API ---
    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead()) return;
        currentHP = Mathf.Max(0, currentHP - amount);
        Debug.Log($"Player took {amount} dmg. HP: {currentHP}/{maxHP}");

        if (currentHP == 0) OnZeroHP();
    }

    /*

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead()) return;
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        Debug.Log($"Player healed {amount}. HP: {currentHP}/{maxHP}");
    }

    */

    public bool IsDead() => currentHP <= 0;

    // --- Internal ---
    private void OnZeroHP()
    {
        Debug.Log("Game Over triggered (HP reached 0).");
        // Enable the simple Game Over screen as requested
        if (gameOverScreen != null) gameOverScreen.SetActive(true);

        // Optional: also pause & show Game Over menu via MenuManager if present
        if (MenuManager.Instance != null)
            MenuManager.Instance.ShowGameOverMenu();
    }
}
