using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("UI (optional)")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject shopUIRoot; // NEW: assign your shop Canvas here

    [Header("Shop Display")]
    public List<GameObject> shopItemPrefabs = new List<GameObject>();
    public Transform startAnchor;
    public Transform endAnchor;
    [Min(0.1f)] public float spacing = 1.5f;
    [Min(0.01f)] public float moveDuration = 1.2f;
    [Min(0f)] public float staggerDelay = 0.15f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Transform displayParent;
    public bool autoBuildOnEnable = true;

    private readonly List<Transform> spawned = new List<Transform>(3);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (autoBuildOnEnable) BuildDisplay();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        ClearDisplay();
    }

    void Update()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (Input.GetKeyDown(KeyCode.B)) BuildDisplay();
        if (Input.GetKeyDown(KeyCode.C)) ClearDisplay();
    }

    // Purchasing
    public bool TryBuy(Item item)
    {
        if (item == null) { Notify("No item selected."); return false; }

        if (!WalletManager.Instance.CanAfford(item.Price))
        {
            Notify("Not enough cash for " + item.ItemName + " ($" + item.Price + ").");
            return false;
        }

        WalletManager.Instance.Spend(item.Price);
        InventoryManager.Instance.Add(item.ItemName);
        item.OnPurchased();
        Notify("Bought " + item.ItemName + " for $" + item.Price + ".");

        // --- Add the attached RealDice prefab to the Purchased list ---
        if (item is Dice diceItem && diceItem.RealDice != null && DiceStash.Instance != null)
        {
            DiceStash.Instance.AddBoughtDie(diceItem.RealDice);
            // no UI rebuild here (as requested)
        }

        return true;
    }



    private void Notify(string msg)
    {
        if (messageText != null) messageText.text = msg;
        else Debug.Log(msg);
    }

    // Display building
    [ContextMenu("Build Display")]
    public void BuildDisplay()
    {
        if (startAnchor == null || endAnchor == null)
        {
            Debug.LogWarning("[ShopManager] Assign StartAnchor and EndAnchor.");
            return;
        }
        if (shopItemPrefabs == null || shopItemPrefabs.Count == 0)
        {
            Debug.LogWarning("[ShopManager] Add at least one prefab to shopItemPrefabs.");
            return;
        }

        if (displayParent == null)
        {
            var existing = transform.Find("ShopDisplayRoot");
            displayParent = existing != null ? existing : new GameObject("ShopDisplayRoot").transform;
            displayParent.SetParent(transform, false);
        }

        ClearDisplay();

        int countToShow = Mathf.Min(3, shopItemPrefabs.Count);
        List<GameObject> picks = GetUniqueRandomPrefabs(countToShow);

        Vector3 rowRight = endAnchor.right;
        Quaternion rowRot = endAnchor.rotation;
        Vector3 mid = endAnchor.position;

        List<Vector3> targets = new List<Vector3>(countToShow);
        if (countToShow == 1)
        {
            targets.Add(mid);
        }
        else if (countToShow == 2)
        {
            targets.Add(mid - rowRight * (spacing * 0.5f));
            targets.Add(mid + rowRight * (spacing * 0.5f));
        }
        else
        {
            targets.Add(mid - rowRight * spacing);
            targets.Add(mid);
            targets.Add(mid + rowRight * spacing);
        }

        StartCoroutine(SpawnAndSlideRow(picks, targets, rowRot));
    }

    [ContextMenu("Clear Display")]
    public void ClearDisplay()
    {
        foreach (var t in spawned)
            if (t) Destroy(t.gameObject);
        spawned.Clear();
    }

    private IEnumerator SpawnAndSlideRow(List<GameObject> prefabs, List<Vector3> targets, Quaternion targetRot)
    {
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            if (prefab == null) continue;

            Transform t = Instantiate(
                prefab,
                startAnchor.position,
                startAnchor.rotation,
                displayParent
            ).transform;

            spawned.Add(t);
            StartCoroutine(SlideTransform(t, targets[i], targetRot, moveDuration));

            if (staggerDelay > 0f)
                yield return new WaitForSeconds(staggerDelay);
        }
    }

    private IEnumerator SlideTransform(Transform obj, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = obj.position;
        Quaternion startRot = obj.rotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float k = ease != null ? ease.Evaluate(t) : t;

            obj.position = Vector3.LerpUnclamped(startPos, targetPos, k);
            obj.rotation = Quaternion.SlerpUnclamped(startRot, targetRot, k);
            yield return null;
        }

        obj.position = targetPos;
        obj.rotation = targetRot;
    }

    private List<GameObject> GetUniqueRandomPrefabs(int count)
    {
        List<GameObject> pool = new List<GameObject>(shopItemPrefabs);

        for (int i = 0; i < pool.Count; i++)
        {
            int j = Random.Range(i, pool.Count);
            GameObject tmp = pool[i];
            pool[i] = pool[j];
            pool[j] = tmp;
        }

        List<GameObject> picks = new List<GameObject>(count);
        for (int i = 0; i < pool.Count && picks.Count < count; i++)
        {
            if (pool[i] != null && !picks.Contains(pool[i]))
                picks.Add(pool[i]);
        }
        return picks;
    }

    // -------- NEW: Close Shop Button --------
    public void CloseShop()
    {
        ClearDisplay();

        // Hide the shop UI canvas if assigned
        if (shopUIRoot != null)
            shopUIRoot.SetActive(false);

        // Also disable this ShopManager
        gameObject.SetActive(false);

        // If you want to hide cursor when shop closes, uncomment:
        // Cursor.lockState = CursorLockMode.Locked;
        // Cursor.visible = false;
    }
}
