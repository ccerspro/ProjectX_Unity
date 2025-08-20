using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZhouSoftware.Inventory;

namespace ZhouSoftware
{
    //8/15/2025: This script is intended to define items that are required for certain actions or interactions in the game.
    public class RequireItem : MonoBehaviour
    {
        public ItemDefinition requireItem;
        public bool consumeOnUse = false;
        public int requiredAmount = 1;
        public int consumeAmount = 1;
        
        public bool CheckAndConsume(PlayerInventory inventory)
        {
            if (inventory.Has(requireItem) && inventory.Count(requireItem) >= requiredAmount)
            {
                if (consumeOnUse)
                {
                    inventory.Remove(requireItem, consumeAmount);
                }
                return true;
            }
            return false;
        }
    }
}