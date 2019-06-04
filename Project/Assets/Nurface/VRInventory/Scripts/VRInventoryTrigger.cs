using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory
{        
    public class VRInventoryTrigger : MonoBehaviour
    {
        // Assigned by the menu item code
        public VRInventory vrInventory;        

        void Start()
        {
            if (!Application.isPlaying) return;
            if (vrInventory == null) return;
            CreateEvent(this.gameObject, EventTriggerType.PointerEnter, vrInventory.PointerEnterTrigger);
            CreateEvent(this.gameObject, EventTriggerType.PointerExit, vrInventory.PointerExitTrigger);
        }

        private void CreateEvent(GameObject o, EventTriggerType type, UnityAction action)
        {
            EventTrigger trigger = o.GetComponent<EventTrigger>();
            if (trigger == null) trigger = o.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((eventData) => { action.Invoke(); });
            trigger.triggers.Add(entry);
        }
    }
}
