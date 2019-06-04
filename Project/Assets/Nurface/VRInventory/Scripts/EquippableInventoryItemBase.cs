using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory
{    
    /// <summary>
    /// Use this class as the base for your equippable items in order to define behaviour for them
    /// </summary>    
    public abstract class EquippableInventoryItemBase : MonoBehaviour
    {
        /// <summary>
        /// Override this method to determine what happens when the item is equipped
        /// </summary>
        public virtual void OnItemEquipped()
        {
        }

        /// <summary>
        /// Override this method to determine what happens when the item is unequipped
        /// </summary>
        public virtual void OnItemUnequipped()
        {
        }

        /// <summary>
        /// Override this method to determine what happens when the item is "used" while equipped
        /// </summary>
        public virtual void OnItemUsed()
        {
        }        
    }
}
