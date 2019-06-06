using UnityEngine;
using System.Collections;

namespace MobileVRInventory {
    public class Sword : EquippableInventoryItemBase {
        // used to show messages to the player
        private VRInventoryExampleSceneController exampleController;

        // used to detect what we're looking at
        private GazeInputModuleInventory gazeInputModuleInventory;

        // How far can we swing this sword?
        public float SwingDistance = 2.5f;

        // An array of sounds to play when swinging the sword
        public AudioClip[] SwingSound = new AudioClip[] { };

        // variable used to ensure that we can't swing multiple times simultaneously
        private bool swingInProgress = false;

        void Awake() {
            exampleController        = GameObject.FindObjectOfType<VRInventoryExampleSceneController>();
            gazeInputModuleInventory = GameObject.FindObjectOfType<GazeInputModuleInventory>();
        }

        // Called by the VR Inventory system when the item is equipped
        public override void OnItemEquipped() {
            exampleController.ShowMessage("Try hitting the target dummy.");
        }

        // Called by the VR Inventory system when InputFire1 is triggered while the item is equipped
        public override void OnItemUsed() {
            if (swingInProgress) return;

            var itemGazedAt = gazeInputModuleInventory.GetCurrentGameObject();

            if (itemGazedAt != null) {
                // Don't swing at inventory items, just pick them up
                var inventoryItem = itemGazedAt.GetComponent<InventoryItem>();
                if (inventoryItem != null) return;
            }

            SwingBegin();

            if (itemGazedAt != null) {
                var enemy = itemGazedAt.GetComponent<Enemy>();

                if (enemy != null) {
                    var distance = Vector3.Distance(itemGazedAt.transform.position, Camera.main.transform.position);
                    if (distance <= SwingDistance) {
                        // TakeDamage returns true if the target has been killed
                        if (enemy.TakeDamage(35)) {
                            // notify the controller that we have won
                            exampleController.Victory();
                        }
                    }
                }
            }
        }

        private void SwingBegin() {
            swingInProgress = true;
            iTween.Stop(this.gameObject);
            iTween.RotateTo(this.gameObject,
                iTween.Hash("isLocal", true, 
                    "rotation",   new Vector3(120f, 0f, 90f), 
                    "time",       0.125f, 
                    "oncomplete","SwingReturn", 
                    "easetype",   iTween.EaseType.easeInQuad));

            // Play swing sound
            if (SwingSound != null && SwingSound.Length > 0) {
                var _swingSound = SwingSound[Random.Range(0, SwingSound.Length)];

                if (_swingSound != null) {
                    AudioSource.PlayClipAtPoint(_swingSound, this.transform.position);
                }
            }
        }

        private void SwingReturn() {
            iTween.Stop(this.gameObject);
            iTween.RotateTo(this.gameObject,
                iTween.Hash("isLocal", true, "rotation", new Vector3(90f, 0f, 90f), "time", 0.125f, "oncomplete",
                    "SwingComplete", "easetype", iTween.EaseType.easeInQuad));
        }

        private void SwingComplete() {
            swingInProgress = false;
        }
    }
}