using UnityEngine;

namespace ZhouSoftware.Inventory
{
    public enum ItemKind { KeyItem, Equipment, Consumable }

    [CreateAssetMenu(menuName = "ZhouSoftware/Item/Item Definition", fileName = "NewItem")]
    public class ItemDefinition : ScriptableObject
    {
        public string id;                    // unique string (e.g., "flashlight", "key_lobby")
        public string displayName;
        public ItemKind kind = ItemKind.KeyItem;
        public bool stackable = false;
        public int maxStack = 1;
        public Sprite icon;                  // optional (UI)
        public GameObject prefab;            // optional (in-game item, e.g., flashlight, key)
        public string description;            // optional (UI)

#if UNITY_EDITOR
        void OValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = name.ToLowerInvariant().Replace(" ", "_");
            }
        }
#endif
    }
}
