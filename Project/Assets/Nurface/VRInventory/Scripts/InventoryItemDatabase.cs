using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace MobileVRInventory {
    [CreateAssetMenu(menuName = "MobileVRInventory/New InventoryItemDatabase", fileName = "MyInventoryItemDatabase")]
    public class InventoryItemDatabase : ScriptableObject {
        public List<InventoryItemData>  items                = new List<InventoryItemData>();
        public ItemOutlineConfiguration outlineConfiguration = new ItemOutlineConfiguration();

        public InventoryItemData GetItem(string itemName) {
            return items.FirstOrDefault(i => i.name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }
    }

    [Serializable]
    public class ItemOutlineConfiguration {
        [SerializeField] public bool useOutline = true;

        [SerializeField] public Color outlineColor = Color.white;

        [SerializeField, Range(0.0f, 0.03f)] public float outlineWidth = 0.005f;

        [SerializeField] public float outlineAdjustSpeed = 3f;
    }
}