using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceStash : MonoBehaviour
{
    public static DiceStash Instance { get; private set; }
    public List<GameObject> dice = new List<GameObject>();
    public GameObject uiImagePrefab;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void BuildUI(Transform parent)
    {
        foreach (Transform child in parent)
            Destroy(child.gameObject);

        foreach (var d in dice)
        {
            var db = d.GetComponent<DiceBase>();
            if (db == null || db.uiPromptSprite == null) continue;

            GameObject newIcon = Instantiate(uiImagePrefab, parent);
            Image img = newIcon.GetComponent<Image>();
            if (img != null) img.sprite = db.uiPromptSprite;
        }
    }
}
