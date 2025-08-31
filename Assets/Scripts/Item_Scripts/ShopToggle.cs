using UnityEngine;

public class ShopToggle : MonoBehaviour
{
    public GameObject ShopButton;

    void OnEnable()
    {
        if (ShopButton != null)
            ShopButton.SetActive(true);
    }

    void OnDisable()
    {
        if (ShopButton != null)
            ShopButton.SetActive(false);
    }
}
