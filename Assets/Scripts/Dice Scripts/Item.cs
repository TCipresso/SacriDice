using UnityEngine;
using TMPro;

[RequireComponent(typeof(Collider))]
public abstract class Item : MonoBehaviour, IClickable
{
    [Header("Item Data")]
    public string ItemName = "dice";
    [Min(0)] public int Price = 5;

    [Header("World-Space Price Tag (TMP)")]
    public TextMeshPro priceTag;

    [Header("Prefab Reference")]
    public GameObject RealDice;

    protected virtual void Awake()
    {
        RefreshPriceTag();
    }

    protected virtual void OnValidate()
    {
        RefreshPriceTag();
    }

    protected void RefreshPriceTag()
    {
        if (priceTag != null)
            priceTag.text = $"${Price}";
    }

    public void SetPrice(int newPrice)
    {
        Price = Mathf.Max(0, newPrice);
        RefreshPriceTag();
    }

    // Click handling (shop tries to buy this item)
    public virtual void OnClick()
    {
        ShopManager.Instance.TryBuy(this);
    }

    // What happens after it’s bought. Override if you want different behavior.
    public virtual void OnPurchased()
    {
        foreach (var r in GetComponentsInChildren<Renderer>(true)) r.enabled = false;
        if (TryGetComponent<Collider>(out var col)) col.enabled = false;
    }
}
