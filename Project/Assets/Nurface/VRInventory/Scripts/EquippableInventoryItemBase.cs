using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory {
    // Use this class as the base for your equippable items in order to define behaviour for them
    public abstract class EquippableInventoryItemBase : MonoBehaviour {
        // Override this method to determine what happens when the item is equipped
        public virtual void OnItemEquipped() { }

        // Override this method to determine what happens when the item is unequipped
        public virtual void OnItemUnequipped() { }

        // Override this method to determine what happens when the item is "used" while equipped
        public virtual void OnItemUsed() { }
    }
}