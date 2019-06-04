using System.Collections;
using System.Collections.Generic;
using MobileVRInventory;
using UnityEngine;

public class TestScript : MonoBehaviour {
    void Start() { }

    void Update() { }

    public void OnItemSelected(InventoryItemStack stack) {
        Debug.Log(stack.item.name);
    }

    public void OnItemPickedUp(VRInventory.InventoryItemPickupResult result) {
        Debug.Log(result.item.name);
    }
}