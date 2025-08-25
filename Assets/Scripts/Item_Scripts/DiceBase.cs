using System.Collections.Generic;
using UnityEngine;

public enum DiceType { D6, D8, D12, D20 }

public class DiceBase : MonoBehaviour
{
    public DiceType diceType;
    public int sides;
    public List<string> sideValues = new List<string>();
    public Sprite uiPromptSprite;

    void OnValidate()
    {
        sides = GetSideCount(diceType);
        EnsureSideList();
    }

    int GetSideCount(DiceType type)
    {
        return type switch
        {
            DiceType.D6 => 6,
            DiceType.D8 => 8,
            DiceType.D12 => 12,
            DiceType.D20 => 20,
            _ => 6
        };
    }

    void EnsureSideList()
    {
        if (sideValues.Count < sides)
        {
            for (int i = sideValues.Count; i < sides; i++)
                sideValues.Add((i + 1).ToString());
        }
        else if (sideValues.Count > sides)
        {
            sideValues.RemoveRange(sides, sideValues.Count - sides);
        }
    }
}
