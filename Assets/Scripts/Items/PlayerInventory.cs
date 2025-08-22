using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZhouSoftware.Inventory
{
    // This script is intended to manage the player's inventory, including adding, removing, and using items.
    // It can be extended to include more complex inventory management features.
    public class PlayerInventory : MonoBehaviour
    {
        public static PlayerInventory I { get; private set; }
        [Serializable]
        struct Entry
        {
            public ItemDefinition item;
            public int count;
        }

        [Header("Debug View(readonly)")]
        [SerializeField] private List<Entry> entries = new List<Entry>();

        readonly Dictionary<ItemDefinition, int> _counts = new();

        public event Action<ItemDefinition, int> OnItemAdded;
        public event Action<ItemDefinition, int> OnItemRemoved;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
        }


        public bool Has(ItemDefinition item)
        {
            return _counts.TryGetValue(item, out int count) && count > 0;
        }

        public int Count(ItemDefinition item)
        {
            _counts.TryGetValue(item, out var c);
            return c;
        }

        public bool Add(ItemDefinition item, int amount = 1)
        {
            if (!item || amount <= 0) return false;

            _counts.TryGetValue(item, out var c);
            int newCount = item.stackable ? Mathf.Min(c + amount, item.maxStack) : 1;
            bool changed = newCount != c;
            if (!changed) return false;

            _counts[item] = newCount;
            SyncDebug();
            OnItemAdded?.Invoke(item, newCount);
            return true;
        }

        public bool Remove(ItemDefinition item, int amount = 1)
        {
            if (!item || amount <= 0 || !_counts.TryGetValue(item, out var c)) return false;

            int newCount = Mathf.Max(0, c - amount);
            if (newCount == 0) _counts.Remove(item);
            else _counts[item] = newCount;

            SyncDebug();
            OnItemRemoved?.Invoke(item, newCount);
            return true;
        }

        void SyncDebug()
        {
            entries.Clear();
            foreach (var kv in _counts)
                entries.Add(new Entry { item = kv.Key, count = kv.Value });
        }


    }
}