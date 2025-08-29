using UnityEngine;

public class LimbStats : MonoBehaviour
{
    [Header("Base values")]
    public int baseDice = 1;
    public int baseCoins = 0;
    public int baseHealth = 0;

    [Header("Runtime modifiers (optional)")]
    public int addDice = 0;
    public float mulDice = 1f;
    public int addCoins = 0;
    public float mulCoins = 1f;
    public int addHealth = 0;
    public float mulHealth = 1f;

    public int Dice() => Mathf.Max(0, Mathf.RoundToInt((baseDice + addDice) * mulDice));
    public int Coins() => Mathf.Max(0, Mathf.RoundToInt((baseCoins + addCoins) * mulCoins));
    public int Health() => Mathf.Max(0, Mathf.RoundToInt((baseHealth + addHealth) * mulHealth));
}
