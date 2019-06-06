using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MobileVRInventory {
    [ExecuteInEditMode]
    public class VRInventory : MonoBehaviour {
        #region Items

        [SerializeField] public InventoryItemDatabase    itemDatabase;
        [SerializeField] public List<InventoryItemStack> items = new List<InventoryItemStack>();

        [SerializeField] public bool   autoSave     = false;
        [SerializeField] public string saveSlotName = "Slot1";

        public int itemStackCount {
            get { return items.Count; }
        }

        private int itemStackColumnCount {
            get { return Mathf.CeilToInt(itemStackCount / 3f); }
        }

        #endregion

        #region References

        public GameObject inventoryUIPrefab;
        public GameObject itemTemplate;
        public Transform  inventoryPositionTransform;
        public Transform  handPosition;

        #endregion

        #region Events

        public enum eInventoryTriggerMode {
            GazeTrigger,
            InputFire1
        };

        public eInventoryTriggerMode inventoryTriggerMode = eInventoryTriggerMode.GazeTrigger;
        public bool                  hideWhenItemSelected = false;

        [System.Serializable]
        public class VRInventoryItemSelectEvent : UnityEvent<InventoryItemStack> { }

        public VRInventoryItemSelectEvent onItemSelected = new VRInventoryItemSelectEvent();

        public enum ePickUpResult {
            Success,
            Failed_CannotCarryMode
        };

        [System.Serializable]
        public class InventoryItemPickupResult {
            public InventoryItemData item;
            public int               quantity = 1;
            public ePickUpResult     result   = ePickUpResult.Success;
        }

        [System.Serializable]
        public class VRInventoryItemPickupResultEvent : UnityEvent<InventoryItemPickupResult> { };

        public VRInventoryItemPickupResultEvent onItemPickedUp = new VRInventoryItemPickupResultEvent();

        #endregion

        #region Edit Mode

        public bool showInventoryItemsInEditMode = false;

        #endregion

        #region Internal

        private float                       columnWidth             = 50f;
        private int                         column                  = 0;
        private Vector3                     desiredScale            = Vector3.one;
        private bool                        hideAnimationInProgress = false;
        private Transform                   mainCamera;
        private GameObject                  currentUI;
        private GameObject                  scrollLeftButton;
        private GameObject                  scrollRightButton;
        private ScrollRect                  itemContainerScrollRect;
        private Button                      _scrollLeftButton;
        private Button                      _scrollRightButton;
        private bool                        pointerOverTrigger, pointerOverCanvas;
        private InventoryItemData           equippedItem          = null;
        private EquippableInventoryItemBase equippedItemInstance  = null;
        private Transform                   equippedItemTransform = null;
        private bool                        itemEquippedThisFrame = false;

        private GazeInputModuleInventory _gazeInputModule = null;

        private GazeInputModuleInventory gazeInputModule {
            get {
                if (_gazeInputModule == null) {
                    _gazeInputModule = GameObject.FindObjectOfType<GazeInputModuleInventory>();
                }

                return _gazeInputModule;
            }
        }

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

        #endregion

        void Awake() {
            if (!itemDatabase){
                Debug.LogWarning(
                    "[MobileVRInventory] Warning: VRInventory requires a reference to an InventoryItemDatabase.");}
            if (inventoryPositionTransform == null) {
                inventoryPositionTransform = Camera.main.transform.Find("InventoryPosition");
            }

            mainCamera = Camera.main.transform;

            if (Application.isPlaying && autoSave) {
                Load();
            }
        }

        void OnEnable() {
            ValidateItems();
        }

        void ValidateItems() {
            if (itemDatabase == null) return;

            // remove all items that aren't in the database, just in case items have been removed (from the database)
            items.RemoveAll(i => itemDatabase.GetItem(i.item.name) == null);

            // update item images/etc. if need be
            for (var x = 0; x < items.Count; x++) {
                items[x].item = itemDatabase.GetItem(items[x].item.name);
            }

            // lastly, sort items, pinned items first
            items = items.OrderBy(i => !i.item.pinItemInInventory).ToList();
        }

        void ScrollLeftButtonClicked() {
            if (column <= 0) return;
            column -= 1;
            AnimateToShowCurrentColumn();
            UpdateNextAndPreviousButtons();
        }

        void ScrollRightButtonClicked() {
            if (column >= itemStackColumnCount - 3) return;
            column += 1;
            AnimateToShowCurrentColumn();
            UpdateNextAndPreviousButtons();
        }

        void AnimateToShowCurrentColumn() {
            iTween.StopByName(itemContainerScrollRect.content.gameObject, "vrInventoryScrollAnimation");
            iTween.ValueTo(itemContainerScrollRect.content.gameObject,
                iTween.Hash("name", "vrInventoryScrollAnimation",
                    "from",           itemContainerScrollRect.content.anchoredPosition3D.x,
                    "to",             columnWidth * -column,
                    "time",           0.25f,
                    "onupdatetarget", this.gameObject,
                    "onupdate",       "SetScrollRectContentXPosition"));
        }

        void UpdateNextAndPreviousButtons() {
            _scrollLeftButton.interactable = column > 0;
            _scrollRightButton.interactable = column < itemStackColumnCount - 3;
        }

        void SetScrollRectContentXPosition(float xPosition) {            
            itemContainerScrollRect.content.anchoredPosition3D = new Vector3(xPosition, 0.5f, 0f);
        }

        void RenderItems() {
            // hide the item template in play mode
            if (Application.isPlaying && itemTemplate.gameObject.activeInHierarchy) itemTemplate.gameObject.SetActive(false);

            // clear existing items
            var existingItems = itemContainerScrollRect.content.GetComponentsInChildren<VRInventoryItem>();
            foreach (var item in existingItems) {                
                if (item != itemTemplate) {
                    if (Application.isPlaying) Destroy(item.gameObject);
                    else DestroyImmediate(item.gameObject);                    
                }
            }            

#if UNITY_EDITOR
            // if we're not in play mode, and showInventoryItemsInEditMode is false, stop here
            if (!Application.isPlaying && !showInventoryItemsInEditMode) {
                return;
            }
#endif            
            int index = 0;
            // create the new items
            foreach (var itemStack in items) {
                var newItem = Instantiate(itemTemplate);
                
                newItem.gameObject.SetActive(true);
                newItem.transform.SetParent(itemContainerScrollRect.content);

                var rectTransform = newItem.transform as RectTransform;

                rectTransform.localScale = Vector3.one;
                rectTransform.anchoredPosition3D = Vector3.zero;
                rectTransform.localRotation = new Quaternion();
                
                newItem.GetComponent<VRInventoryItem>().SetData(itemStack.item, itemStack.quantity);
                newItem.name = itemStack.item.name;

                // we need a copy, otherwise a reference will be used instead
                itemStack.index = index; // we're using this index to uniquely reference each stack
                var _stack = new InventoryItemStack { item = itemStack.item, quantity = itemStack.quantity, index = index };

                CreateEvent(newItem.transform.GetChild(0).gameObject, EventTriggerType.PointerDown, () => SelectItem(_stack));
                
                index++;
            }
        }

        void SelectItem(string name) {            
            var itemStack = items.FirstOrDefault(i => i.item.name == name);
            SelectItem(itemStack);
        }

        void SelectItem(InventoryItemStack itemStack) {            
            // retrieve the actual stack from the collection
            itemStack = items.FirstOrDefault(i => i.Equals(itemStack));

            if (itemStack != null) {
                if (itemStack.item.useSound != null) {
                    _audioSource.clip = itemStack.item.useSound;
                    _audioSource.Play();
                }

                onItemSelected.Invoke(itemStack);

                if (hideWhenItemSelected && inventoryTriggerMode == eInventoryTriggerMode.InputFire1) {
                    Hide();
                }

                if (itemStack.item.equippable) {
                    if (equippedItem != null && equippedItem == itemStack.item) {
                        UnEquipItem();
                    } else {
                        EquipItem(itemStack.item.name);
                    }
                }
            }
        }

        public void EquipItem(string name) {
            var item = items.FirstOrDefault(i => i.item.name == name);

            if (item == null) {
                Debug.Log("[MobileVRInventory]:: Attempted to equip an item that wasn't in the inventory! ('" + name + "')");
                return;
            }            

            if(equippedItem != null) {
                var stopHere = false;

                // if we're 'deselecting' the currently equipped item, then don't re-equip it
                if (equippedItem == item.item) {
                    stopHere = true;
                }

                UnEquipItem();

                if (stopHere) return;
            }

            equippedItem = item.item;
            var prefab = item.item.itemPrefab;

            if (prefab != null) {
                var handItem = GameObject.Instantiate(prefab);
                handItem.SetParent(handPosition);


                handItem.localPosition = prefab.localPosition;
                handItem.localRotation = prefab.localRotation;
                handItem.localScale = prefab.localScale;

                equippedItemInstance = handItem.GetComponent<EquippableInventoryItemBase>();
                equippedItemTransform = handItem;

                // fire the equip event
                if (equippedItemInstance != null) equippedItemInstance.OnItemEquipped();
            }

            itemEquippedThisFrame = true;
        }

        public void UnEquipItem()
        {
            if (equippedItem == null) return;

            equippedItem = null;

            // fire the Unequip event
            if (equippedItemInstance != null) equippedItemInstance.OnItemUnequipped();

            // Remove the object
            if(equippedItemTransform != null) GameObject.Destroy(equippedItemTransform.gameObject);
        }

        /// <summary>
        /// Add the specified item to the inventory
        /// (an item with this name must exist in the inventory item database)
        /// </summary>
        /// <param name="name"></param>
        public ePickUpResult AddItem(string name, int quantity = 1) {            
            var item = itemDatabase.GetItem(name);

            var pickupResult = new InventoryItemPickupResult() { item = item, quantity = quantity };

            if (item != null) {
                while (quantity > 0){
                    var stack = items.FirstOrDefault(i => i.item == item && (item.stackSize < 0 || i.quantity < item.stackSize));                    

                    if (stack == null) {
                        var stackCount = items.Count(i => i.item == item);

                        if (item.maxNumberOfStacks == -1 || stackCount < item.maxNumberOfStacks) {
                            stack = new InventoryItemStack() { item = item, quantity = 0 };
                            items.Add(stack);
                        }
                    }

                    if (stack != null) {
                        stack.quantity += 1;
                        quantity -= 1;
                    } else {
                        pickupResult.result = ePickUpResult.Failed_CannotCarryMode;
                        break;
                    }
                     
                }

                onItemPickedUp.Invoke(pickupResult);
            }

            ItemsUpdated();

            return pickupResult.result;
        }


        /// Remove the specified item from the inventory
        /// Specify a quantity of -1 in order to remove all items
        /// <param name="name"></param>
        public void RemoveItem(string name, int quantity = 1, InventoryItemStack stackToRemoveFrom = null) {
            var stack = stackToRemoveFrom ?? items.FirstOrDefault(i => i.item.name == name);

            // if there's nothing to remove, return
            if (stack == null) return;

            if (quantity < 0) {
                // if a negative quantity has been specified, remove all
                stack.quantity = 0;
            } else {
                // remove the specified quantity
                stack.quantity -= quantity;
            }

            // if there are none left, remove the stack
            if (stack.quantity <= 0) {
                items.Remove(stack);
            }   

            ItemsUpdated();
        }

        // Notify that the item collection has been updated (and should be re-rendered)
        public void ItemsUpdated() {            
            ValidateItems();
            if (autoSave) Save();

            if (currentUI == null) return;
            RenderItems();
            UpdateNextAndPreviousButtons();            
        }

        // -------------------------------------------------------------------------------------------
        // Spawn the Inventory UI
        // -------------------------------------------------------------------------------------------
        public void Spawn() {
            currentUI = (GameObject)Instantiate(inventoryUIPrefab, inventoryPositionTransform.position, mainCamera.rotation);
            currentUI.transform.eulerAngles = new Vector3(currentUI.transform.eulerAngles.x, currentUI.transform.eulerAngles.y, 0f);
            GameObject canvas = currentUI.transform.GetChild(0).GetChild(0).gameObject;
            CreateEvent(canvas, EventTriggerType.PointerEnter, PointerEnterCanvas);
            CreateEvent(canvas, EventTriggerType.PointerExit, PointerExitCanvas);
            scrollLeftButton = currentUI.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).gameObject;
            CreateEvent(scrollLeftButton, EventTriggerType.PointerDown, ScrollLeftButtonClicked);
            scrollRightButton = currentUI.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(2).gameObject;
            CreateEvent(scrollRightButton, EventTriggerType.PointerDown, ScrollRightButtonClicked);
            itemContainerScrollRect = currentUI.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1).GetChild(0).gameObject.GetComponent<ScrollRect>();

            _scrollLeftButton = scrollLeftButton.GetComponent<Button>();
            _scrollRightButton = scrollRightButton.GetComponent<Button>();

            currentUI.transform.localScale = Vector3.zero;
            iTween.ScaleTo(currentUI, desiredScale, 1f);

            column = 0;
            columnWidth = itemContainerScrollRect.content.GetComponent<GridLayoutGroup>().cellSize.x;

            ItemsUpdated();
            UpdateNextAndPreviousButtons();            
        }

        public void PointerEnterTrigger() {
            pointerOverTrigger = true;
            if (currentUI != null) {
                return;
            }

            if (inventoryTriggerMode == eInventoryTriggerMode.GazeTrigger) {
                Spawn();
            }
        }

        public void PointerExitTrigger() {
            pointerOverTrigger = false;
            StartCoroutine(Hide());
        }

        public void PointerEnterCanvas() {
            pointerOverCanvas = true;
        }

        public void PointerExitCanvas() {
            pointerOverCanvas = false;
            StartCoroutine(Hide());
        }

        // Hide the inventory
        public IEnumerator Hide() {
            yield return new WaitForEndOfFrame();
            if (pointerOverCanvas == false && pointerOverTrigger == false) {
                if (hideAnimationInProgress || currentUI == null || this == null || this.gameObject == null) yield break;
                hideAnimationInProgress = true;
                iTween.ScaleTo(currentUI, 
                    iTween.Hash("scale", Vector3.zero,
                        "time",             0.75f, 
                        "oncomplete",       "HideComplete", 
                        "oncompletetarget", this.gameObject));
            }
        }

        void HideComplete() {
            hideAnimationInProgress = false;
            if (currentUI != null) {
                Destroy(currentUI);
            }
        }
        
        void Update() {
            // Contextual item usage
            if (!itemEquippedThisFrame && Input.GetButtonDown("Fire1") && equippedItemInstance != null) {
                equippedItemInstance.OnItemUsed();
            }

            itemEquippedThisFrame = false;

            if (inventoryTriggerMode == eInventoryTriggerMode.InputFire1) {
                if (Input.GetButtonDown("Fire1")) {
                    if(currentUI == null) {
                        Spawn();
                    }
                    else {
                        if (hideAnimationInProgress == true) {
                            HideComplete();
                            Spawn();
                        }
                        else {
                            Hide();
                        }
                    }
                }
            }            
        }

        // Create an Event on an Event Trigger
        private void CreateEvent(GameObject o, EventTriggerType type, UnityAction action) {            
            EventTrigger trigger = o.GetComponent<EventTrigger>();
            if (trigger == null) trigger = o.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = type;
            entry.callback.AddListener((eventData) => { action.Invoke(); });
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Are we carrying the specified item?
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasItem(string name) {
            return items.Any(i => i.item.name == name && i.quantity > 0);
        }

        /// <summary>
        /// Check how many items we are carrying of a particular type
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetItemQuantity(string name) {
            var itemStacks = items.Where(i => i.item.name == name).ToList();
            var count = 0;

            foreach (var itemStack in itemStacks) {
                count += itemStack.quantity;
            }

            return count;
        }

        public InventoryItemData GetEquippedItem() {
            return equippedItem;
        }

        public string GetEquippedItemName() {
            if (equippedItem != null) return equippedItem.name;

            return null;
        }

        public void Save(string saveSlotName = null) {
            if (saveSlotName == null && !autoSave) {
                Debug.LogWarning("[VRInventory][Warning] When saving manually, you must provide a save slot name.");
                return;
            } else {
                saveSlotName = this.saveSlotName;
            }
            
            var fullSaveSlotName = "vrInventory_" + saveSlotName;
            var serializedString = string.Join(",", items.Select(s => string.Format("\"{0}\"", s.ToString())).ToArray());

            PlayerPrefs.SetString(fullSaveSlotName, serializedString);
            PlayerPrefs.Save();
        }

        public void Load(string saveSlotName = null) {
            if (saveSlotName == null && !autoSave) {
                Debug.LogWarning("[VRInventory][Warning] When loading from a manual save, you must provide a save slot name.");
                return;
            } else {
                saveSlotName = this.saveSlotName;
            }

            var fullSaveSlotName = "vrInventory_" + saveSlotName;
            var serializedString = PlayerPrefs.GetString(fullSaveSlotName);

            if (string.IsNullOrEmpty(serializedString)) return;

            var reg = new Regex("\".*?\"");
            var matches = reg.Matches(serializedString);
            foreach (var match in matches) {
                var stackDetails = match.ToString().Replace("\"", "").Split(new string[] { ":x:" }, System.StringSplitOptions.RemoveEmptyEntries);

                var itemName = stackDetails[0];
                var itemQuantity = stackDetails[1];

                var item = itemDatabase.GetItem(itemName);                

                // something went wrong
                if (item == null) continue;

                items.Add(new InventoryItemStack { item = item, quantity = int.Parse(itemQuantity) });                
            }

            ItemsUpdated();
        }
    }
}