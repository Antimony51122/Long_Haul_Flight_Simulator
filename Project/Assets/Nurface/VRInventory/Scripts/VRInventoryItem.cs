using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory
{    
    /// <summary>
    /// This behaviour represents an item in the inventory window
    /// </summary>
    public class VRInventoryItem : MonoBehaviour
    {
        public InventoryItemData itemData;
        public int quantity = 1;
        
        public Image image;
        public Text nameText;
        public Text quantityText;

        public void SetData(InventoryItemData data, int quantity = 1) {
            itemData = data;

            if(image != null) image.sprite = data.image;
            if(nameText != null) nameText.text = data.name;
            
            if (quantityText != null) {                
                quantityText.text = String.Format("x{0}", quantity);
                // only show the quantity text if we have more than one of the item
                quantityText.gameObject.SetActive(quantity > 1);
            }
        }
    }
}
