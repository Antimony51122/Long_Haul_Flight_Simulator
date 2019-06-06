using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System;

namespace MobileVRInventory {
    public class VRInventoryExampleSceneController : MonoBehaviour {


        [Range(0, 100)] public int health = 50;
        [Range(0, 100)] public int mana = 15;

        [Header("References")] public VRInventory VRInventory;

        // Meter Panel
        public Image  barsPanel;
        public Slider healthSlider;
        public Slider manaSlider;

        // Coin Panel
        public Image  coinsPanel;
        public Text   coinsText;

        public Image  messagePanel;
        public Text   messageText;

        private int lastCoinValue = 0;

        void Start() {
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
                case "Health Potion":
                    HandleHealthPotionUse(stack);
                    break;
                case "Mana Potion":
                    HandleManaPotionUse(stack);
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
        
        void HandleHealthPotionUse(InventoryItemStack stack) {
            if (health < 100) {
                health = Math.Min(health + 25, 100);
                VRInventory.RemoveItem("Health Potion", 1, stack);
                UpdateBars();
            } else {
                ShowMessage("You are already at full health!");
            }
        }

        void HandleManaPotionUse(InventoryItemStack stack) {
            if (mana < 100) {
                mana = Math.Min(mana + 15, 100);
                VRInventory.RemoveItem("Mana Potion", 1, stack);
                UpdateBars();
            } else {
                ShowMessage("Mana full!");
            }
        }

        void UpdateBars(bool instant = false) {
            var time = instant ? 0f : 0.5f;

            // Fade in the bars when there is a change to the values
            //FadeInBars();

            if (healthSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", healthSlider.value, 
                        "to",       health, 
                        "time",     time, 
                        "onupdate", "UpdateHealthBarAnimation", 
                        "easetype", "easeinsine"));
            }

            if (manaSlider != null) {
                iTween.ValueTo(gameObject,
                    iTween.Hash("from", manaSlider.value, 
                        "to",       mana, 
                        "time",     time, 
                        "onupdate", "UpdateManaBarAnimation", 
                        "easetype", "easeinsine"));
            }

            // Fade out the bars after a brief delay
            //FadeOutBars();
        }

        // Called by iTween to animate the Health Bar
        void UpdateHealthBarAnimation(float newValue) {
            healthSlider.value = newValue;
        }

        // Called by iTween to animate the Mana Bar
        void UpdateManaBarAnimation(float newValue) {
            manaSlider.value = newValue;
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
                iTween.Hash("scale", Vector3.one, "time", 1f, "name", "fadeInMessage"));
        }

        private void FadeOutMessage() {
            iTween.StopByName(messagePanel.gameObject, "fadeOutMessage");
            iTween.ScaleTo(messagePanel.gameObject,
                iTween.Hash("scale", Vector3.zero, "time", 1f, "delay", 4f, "name", "fadeOutMessage"));
        }

        public void Victory() {
            ShowMessage("Victory! You can select the sword in your inventory again to put it away.");
        }
    }
}