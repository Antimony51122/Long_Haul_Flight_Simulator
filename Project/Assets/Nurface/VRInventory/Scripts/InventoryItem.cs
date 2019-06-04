using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace MobileVRInventory {
    // This represents an item in the game-world
    public class InventoryItem : MonoBehaviour {
        public InventoryItemDatabase itemDatabase;
        public InventoryItemData     itemData;
        public int                   quantity = 1;

        private EventTrigger _eventTrigger;
        private VRInventory  _vrInventory;
        private bool         _pickedUp = false;

        private bool         _highlight                   = false;
        private bool         _highlightWaitForFixedUpdate = false;
        private Material     _outlineMaterial;
        private float        _currentOutlineWidth = 0.0f;
        private GameObject   _outlineObject;
        private MeshRenderer _outlineRenderer;

        private AudioSource _audioSource {
            get {
                var source = this.GetComponent<AudioSource>();
                if (source == null) {
                    source        = this.gameObject.AddComponent<AudioSource>();
                    source.volume = 0.25f;
                }

                return source;
            }
        }

        void Awake() {
            _outlineMaterial = Resources.Load<Material>("Materials/vrInventoryMatOutlineOnly");
        }

        void OnEnable() {
            if (itemDatabase != null && itemData != null) {
                // refresh the item's data from the item database
                itemData = itemDatabase.GetItem(itemData.name);
            }

            if (_vrInventory == null) {
                _vrInventory = GameObject.FindObjectOfType<VRInventory>();
                if (_vrInventory == null) {
                    Debug.LogWarning(
                        "Warning: Inventory item could not locate a valid VRInventory object to attach to.");
                    return;
                }
            }

            if (_eventTrigger == null) {
                _eventTrigger = this.GetComponent<EventTrigger>();
                if (_eventTrigger == null) _eventTrigger = this.gameObject.AddComponent<EventTrigger>();
            }

            // clear any existing events, just in case this is called more than once
            _eventTrigger.triggers.Clear();

            CreateEvent(this.gameObject, EventTriggerType.PointerDown,  PickItemUp);
            CreateEvent(this.gameObject, EventTriggerType.PointerEnter, OnPointerEnter);
            CreateEvent(this.gameObject, EventTriggerType.PointerExit,  OnPointerExit);
        }

        // Create an Event on an Event Trigger
        private void CreateEvent(GameObject o, EventTriggerType type, UnityAction action) {
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((eventData) => { action.Invoke(); });
            _eventTrigger.triggers.Add(entry);
        }

        private void PickItemUp() {
            if (_pickedUp) return;

            // Check how far away the player is from the item
            var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);

            // if we're too far away, don't allow the user to pick the item up
            if (distance > itemData.maxPickupDistance) {
                return;
            }

            var result = _vrInventory.AddItem(itemData.name, quantity);

            if (result != VRInventory.ePickUpResult.Success) {
                // if we weren't able to pick up the item, don't continue
                return;
            }

            // prevent this event from being fired again
            _pickedUp = true;

            // Disable item collision (it can get in the way of the camera)
            var collider                           = this.GetComponent<Collider>();
            if (collider != null) collider.enabled = false;

            // Move the item to the player as a 'pick up' animation
            var position = Camera.main.transform.position + new Vector3(0, -0.5f, 0);
            iTween.MoveTo(this.gameObject,
                iTween.Hash("position", position,
                    "time", 0.35f,
                    "oncomplete", "ItemPickupAnimationComplete",
                    "oncompletetarget", this.gameObject));

            var sound = itemData.pickupSound;
            if (quantity > 1 && itemData.pickupMultipleSound != null) {
                sound = itemData.pickupMultipleSound;
            }

            if (sound != null) {
                _audioSource.clip = sound;
                _audioSource.Play();
            }
        }

        private void ItemPickupAnimationComplete() {
            Destroy(this.gameObject);
        }

        private void OnPointerEnter() {
            _highlight                   = true;
            _highlightWaitForFixedUpdate = true;
        }

        private void OnPointerExit() {
            _highlight = false;
        }

        private void FixedUpdate() {
            if (_highlight) {
                // check to see if we are close enough to the item to pick it up before highlighting it
                // this check is performed here as, ideally, we don't want to perform it every frame
                var distance = Vector3.Distance(this.transform.position, Camera.main.transform.position);

                if (distance > itemData.maxPickupDistance) {
                    _highlight = false;
                } else {
                    // distance is okay; go ahead and highlight
                    _highlightWaitForFixedUpdate = false;
                }
            }
        }

        private void Update() {
            if (!itemDatabase.outlineConfiguration.useOutline) return;

            if (_highlight && !_highlightWaitForFixedUpdate) {
                if (_outlineObject == null) SpawnOutlineObject();

                // grow the outline
                if (_currentOutlineWidth < itemDatabase.outlineConfiguration.outlineWidth) {
                    _currentOutlineWidth = Mathf.Lerp(_currentOutlineWidth,
                        itemDatabase.outlineConfiguration.outlineWidth,
                        Time.deltaTime * itemDatabase.outlineConfiguration.outlineAdjustSpeed);
                    _outlineRenderer.material.SetFloat("_Outline", _currentOutlineWidth);
                }
            } else {
                // shrink the outline
                if (_currentOutlineWidth > 0.0f) {
                    _currentOutlineWidth = Mathf.Lerp(_currentOutlineWidth, 0.0f,
                        Time.deltaTime * itemDatabase.outlineConfiguration.outlineAdjustSpeed);

                    // round off, otherwise lerping will continue indefinitely
                    if (_currentOutlineWidth <= 0.001f) _currentOutlineWidth = 0f;

                    _outlineRenderer.material.SetFloat("_Outline", _currentOutlineWidth);
                }

                // if the outline is no longer needed, remove it
                if (_currentOutlineWidth <= 0.0f) DestroyOutlineObject();
            }
        }

        private void SpawnOutlineObject() {
            if (_outlineObject != null) return;

            _outlineObject = new GameObject(this.name + "_Outline");
            _outlineObject.transform.SetParent(this.transform);
            _outlineObject.transform.localPosition = Vector3.zero;
            _outlineObject.transform.localRotation = Quaternion.identity;
            _outlineObject.transform.localScale    = Vector3.one;

            _outlineRenderer = _outlineObject.AddComponent<MeshRenderer>();

            MeshFilter outlineFilter = _outlineObject.AddComponent<MeshFilter>();
            outlineFilter.mesh = (Mesh) this.gameObject.GetComponent<MeshFilter>().mesh;

            _outlineRenderer.material = _outlineMaterial;
            _outlineRenderer.material.SetFloat("_Outline",      0f);
            _outlineRenderer.material.SetFloat("_zDepthOffset", 0f);
            _outlineRenderer.material.SetColor("_OutlineColor", itemDatabase.outlineConfiguration.outlineColor);
        }

        private void DestroyOutlineObject() {
            if (_outlineObject != null) {
                Destroy(_outlineObject);
            }
        }
    }
}