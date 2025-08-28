using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SacrificeSelectable : MonoBehaviour
{
    public Color normalColor = Color.white;
    public Color selectedColor = new Color(1f, 0.3f, 0.3f, 1f);

    Image img;
    Button btn;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        if (!img) img = GetComponentInChildren<Image>(true); // finds first child Image if needed

        // Optional: prevent Unity's hover/pressed tint from overriding your color
        // If you don't want this, delete the next line.
        if (btn) btn.transition = Selectable.Transition.None;
    }

    void OnEnable()
    {
        RefreshVisual();
    }

    public void OnClick()
    {
        var mgr = SacrificeManager2.Instance;
        if (!mgr) return;

        mgr.OnItemClicked(gameObject); // toggles lists
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        var mgr = SacrificeManager2.Instance;
        if (!mgr) return;

        if (!img)
        {
            img = GetComponent<Image>();
            if (!img) img = GetComponentInChildren<Image>(true);
            if (!img) return;
        }

        bool isSelected = mgr.SelectedSac.Contains(gameObject);
        img.color = isSelected ? selectedColor : normalColor;
    }
}
