// File: InventoryManager.cs
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public List<string> ownedItems = new List<string>();
    void Awake() { if (Instance != null && Instance != this) { Destroy(gameObject); return; } Instance = this; }
    public void Add(string name) { if (!string.IsNullOrEmpty(name)) ownedItems.Add(name); }
}
