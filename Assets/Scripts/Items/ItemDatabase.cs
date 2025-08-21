using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZhouSoftware.Inventory
{
    [CreateAssetMenu(menuName = "ZhouSoftware/Item/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemDefinition> items = new();

        public ItemDefinition FindById(string itemId)
            => items.FirstOrDefault(i => i && i.id == itemId);
    }
}
