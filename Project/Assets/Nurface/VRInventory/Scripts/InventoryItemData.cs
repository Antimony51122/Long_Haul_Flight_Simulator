using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory {
    // This represents an inventory item in the database.
    [Serializable]
    public class InventoryItemData {
        // The name of this inventory item. Should be unique.
        [SerializeField] public string name;

        // The image to display for this item when it is in the inventory.
        [SerializeField] public Sprite image;

        // How many of this item can be in one stack?
        // (-1 == no limit)
        [SerializeField] public int stackSize = 1;

        // How many stacks of this item should we allow? (If stackSize = 1, how many copies of this item should we allow?)
        [SerializeField] public int maxNumberOfStacks = 1;

        // If this is set to true, then this item will appear before any other non-pinned items in the inventory.
        // Use this to display things like currency/etc.
        [SerializeField] public bool pinItemInInventory = false;

        // Can this item be equipped? (Placed in the hand)
        [SerializeField] public bool equippable = false;

        // The prefab to use for this item in the player's hand.
        [SerializeField] public Transform itemPrefab;

        // What is the maximum distance away the player can be from this item in order to pick it up?
        [SerializeField] public float maxPickupDistance = 5f;

        [SerializeField] public AudioClip pickupSound = null;

        // This sound will be played when more than one of the item is picked up at once.
        // (If it is not set, then pickupSound will be used instead)
        [SerializeField] public AudioClip pickupMultipleSound = null;

        [SerializeField] public AudioClip useSound = null;

        public override bool Equals(object obj) {
            if (obj is InventoryItemData) {
                return ((InventoryItemData) obj).name == this.name;
            }

            return false;
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }
    }
}