using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System;
using UnityEditor;

namespace MobileVRInventory {
    public class VRInventoryFinalController : MonoBehaviour {
        [Range(0, 100)] public int fullness      = 50;
        [Range(0, 100)] public int hydration     = 50;
        [Range(0, 100)] public int fatigue       = 30;
        [Range(0, 100)] public int comfort       = 50;
        [Range(0, 100)] public int embarrasement = 15;

        [Header("References")] public VRInventory VRInventory;

        // Meter Panel
        public Image  barsPanel;
        public Slider fullnessSlider;
        public Slider hydrationSlider;
        public Slider fatigueSlider;
        public Slider comfortSlider;
        public Slider embarrasementSlider;

        [SerializeField] private GameObject _laptopPrefab;
        [SerializeField] private GameObject _phonePrefab;
        [SerializeField] private GameObject _passportPrefab;

        private Transform _laptopSpawnTransform;
        private Transform _phoneSpawnTransform;

        // Coin Panel
        public Image coinsPanel;
        public Text  coinsText;

        public Image messagePanel;
        public Text  messageText;

        private int lastCoinValue = 0;

        void Start() {
            // Find the position to spawn two devices
            _laptopSpawnTransform = GameObject.Find("SpawnPosLaptop").transform;
            _phoneSpawnTransform = GameObject.Find("SpawnPosPhone").transform;

            // Listen to VR Inventory events
            VRInventory.onItemSelected.AddListener(ItemSelected);
            VRInventory.onItemPickedUp.AddListener(ItemPickedUp);

            // Update the health / mana bars
            UpdateBars(true);

            // Fade out the bars after a brief delay
            //FadeOutBars();

            // Update our coin text
            UpdateCoinText();

            // Fade out coin panel after a brief delay
            FadeOutCoins();

            // hide the message panel initially
            messagePanel.transform.localScale = Vector3.zero;
        }

        void ItemSelected(InventoryItemStack stack) {
            switch (stack.item.name) {
                case "Chocolate":
                    HandleChocolateUse(stack);
                    break;
                case "WaterBottle":
                    HandleWaterBottleUse(stack);
                    break;
                case "Laptop":
                    HandleLaptopUse(stack);
                    break;
                case "Smartphone":
                    HandleLPhoneUse(stack);
                    break;
            }
        }

        void ItemPickedUp(VRInventory.InventoryItemPickupResult result) {
            if (result.result == MobileVRInventory.VRInventory.ePickUpResult.Success) {
                switch (result.item.name) {
                    case "Coin":
                        UpdateCoinTextAnimated();
                        break;
                }
            } else {
                ShowMessage("You cannot carry anymore of those.");
            }
        }

        // ---------------------------------------------------------------------------
        // Handling Usage of all Items
        // ---------------------------------------------------------------------------

        void HandleChocolateUse(InventoryItemStack stack) {
            if (fullness < 100) {
                fullness = Math.Min(fullness + 25, 100);
                VRInventory.RemoveItem("Chocolate", 1, stack);
                UpdateBars();
            } else {
                ShowMessage("You are already full!");
            }
        }

        // TODO: when hydration > 75: automatically go to toilet, Message "Sorry, May I go to the toilet"


        void HandleWaterBottleUse(InventoryItemStack stack) {
            if (hydration < 100) {
                hydration = Math.Min(hydration + 15, 100);
                VRInventory.RemoveItem("WaterBottle", 1, stack);
                UpdateBars();
            } else {
                ShowMessage("You are already fully hydrated!");
            }
        }

        void HandleLaptopUse(InventoryItemStack stack) {
            Instantiate(
                _laptopPrefab,
                _laptopSpawnTransform.position,
                _laptopSpawnTransform.rotation);

            VRInventory.RemoveItem("Laptop", 1, stack);
        }

        void HandleLPhoneUse(InventoryItemStack stack) {
            Instantiate(
                _phonePrefab,
                _phoneSpawnTransform.position,
                _phoneSpawnTransform.rotation);

            VRInventory.RemoveItem("Smartphone", 1, stack);
        }

        // ---------------------------------------------------------------------------
        // Functions updating UI bars
        // ---------------------------------------------------------------------------

        void UpdateBars(bool instant = false) {
            var time = instant ? 0f : 0.5f;

            // Fade in the bars when there is a change to the values
            //FadeInBars();

            if (fullnessSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", fullnessSlider.value, 
                        "to",       fullness, 
                        "time",     time, 
                        "onupdate", "UpdateFullnessBar", 
                        "easetype", "easeinsine"));
            }

            if (hydrationSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", hydrationSlider.value, 
                        "to",       hydration, 
                        "time",     time, 
                        "onupdate", "UpdateHydrationBar", 
                        "easetype", "easeinsine"));
            }

            if (fatigueSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", fatigueSlider.value,
                        "to",       fatigue,
                        "time",     time,
                        "onupdate", "UpdateFatigueBar",
                        "easetype", "easeinsine"));
            }

            if (comfortSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", comfortSlider.value,
                        "to",       comfort,
                        "time",     time,
                        "onupdate", "UpdateComfortBar",
                        "easetype", "easeinsine"));
            }

            if (embarrasementSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", embarrasementSlider.value,
                        "to",       embarrasement,
                        "time",     time,
                        "onupdate", "UpdateEmbarrassmentBar",
                        "easetype", "easeinsine"));
            }

            // Fade out the bars after a brief delay
            //FadeOutBars();
        }

        // Called by iTween to animate the fullness Bar
        void UpdateFullnessBar(float newValue) {
            fullnessSlider.value = newValue;
        }

        // Called by iTween to animate the hydration Bar
        void UpdateHydrationBar(float newValue) {
            hydrationSlider.value = newValue;
        }

        // Called by iTween to animate the fatigue Bar
        void UpdateFatigueBar(float newValue) {
            fatigueSlider.value = newValue;
        }

        // Called by iTween to animate the comfort Bar
        void UpdateComfortBar(float newValue) {
            comfortSlider.value = newValue;
        }

        // Called by iTween to animate the embarrassment  Bar
        void UpdateEmbarrassmentBar(float newValue) {
            embarrasementSlider.value = newValue;
        }

        void FadeOutBars() {
            iTween.StopByName(barsPanel.gameObject, "fadeOutBars");
            iTween.ScaleTo(barsPanel.gameObject,
                iTween.Hash("scale", Vector3.zero, 
                    "time",  1f, 
                    "delay", 4f, 
                    "name",  "fadeOutBars"));
        }

        void FadeInBars() {
            iTween.StopByName(barsPanel.gameObject, "fadeInBars");
            iTween.ScaleTo(barsPanel.gameObject, 
                iTween.Hash("scale", Vector3.one, 
                    "time", 1f, 
                    "name", "fadeInBars"));
        }

        void FadeOutCoins() {
            iTween.StopByName(coinsPanel.gameObject, "fadeOutCoins");
            iTween.ScaleTo(coinsPanel.gameObject,
                iTween.Hash("scale", Vector3.zero, 
                    "time",  1f, 
                    "delay", 4f, 
                    "name",  "fadeOutCoins"));
        }

        void FadeInCoins() {
            iTween.StopByName(coinsPanel.gameObject, "fadeInCoins");
            iTween.ScaleTo(coinsPanel.gameObject, 
                iTween.Hash("scale", Vector3.one, "time", 1f, "name", "fadeInCoins"));
        }

        void UpdateCoinTextAnimated() {
            FadeInCoins();

            var valueBefore = Int32.Parse(coinsText.text.Replace("x", ""));
            var valueAfter  = VRInventory.GetItemQuantity("Coin");

            // Take longer depending on how many coins were added (but don't exceed 2 seconds)
            var time = Mathf.Min(0.1f * Math.Abs(valueAfter - valueBefore), 2f);

            iTween.ValueTo(this.gameObject,
                iTween.Hash(
                    "from", valueBefore, 
                    "to",         valueAfter, 
                    "time",       time, 
                    "onupdate",   "UpdateCoinText",
                    "easetype",   "easeinsine", 
                    "oncomplete", "FadeOutCoins"));
        }


        void UpdateCoinText(int newValue = -1) {
            if (lastCoinValue == newValue) return;

            if (newValue < 0) {
                // if no value was provided, get it from the inventory
                newValue = VRInventory.GetItemQuantity("Coin");
            }

            coinsText.text = String.Format("x{0}", newValue);
            lastCoinValue  = newValue;
        }

        public void ShowMessage(string message) {
            messageText.text = message;
            FadeInMessage();
            FadeOutMessage();
        }

        private void FadeInMessage() {
            iTween.StopByName(messagePanel.gameObject, "fadeInMessage");
            iTween.ScaleTo(messagePanel.gameObject,
                iTween.Hash(
                    "scale", Vector3.one, 
                    "time", 1f, 
                    "name", "fadeInMessage"));
        }

        private void FadeOutMessage() {
            iTween.StopByName(messagePanel.gameObject, "fadeOutMessage");
            iTween.ScaleTo(messagePanel.gameObject,
                iTween.Hash(
                    "scale", Vector3.zero, 
                    "time",  1f, 
                    "delay", 4f, 
                    "name",  "fadeOutMessage"));
        }

        public void Victory() {
            ShowMessage("Victory! You can select the sword in your inventory again to put it away.");
        }
    }
}