using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory {
    // This is used to store information about an item / stack in the inventory
    [Serializable]
    public class InventoryItemStack {
        public InventoryItemData item;

        public int quantity = 1;

        public int index = 0;

        public override string ToString() {
            // Using :x: as a separator as an attempt to ensure that any unusual characters can still be used in item names without causing problems with this method
            return String.Format("{0}:x:{1}", item.name, quantity);
        }

        public override bool Equals(object obj) {
            if (!(obj is InventoryItemStack)) return false;

            var o = (InventoryItemStack) obj;
            if (o.item == this.item && o.index == this.index) return true;

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}