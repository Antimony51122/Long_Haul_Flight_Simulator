using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using UnityEngine.Events;
using UnityEditor.EventSystems;

namespace MobileVRInventory {
    public static class VRInventoryMenuItems {
        [MenuItem("GameObject/UI/MobileVRInventory/New VR Inventory")]
        public static void NewVRInventory() {
            var playerPrefabGuid = AssetDatabase.FindAssets("t:Prefab PlayerPrefab").FirstOrDefault();
            if (string.IsNullOrEmpty(playerPrefabGuid)) {
                Debug.LogWarning("[MobileVRInventory] Player Prefab not found.");
                return;
            }

            var playerPrefab = (GameObject) AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(playerPrefabGuid),
                typeof(GameObject));

            var playerInstance = GameObject.Instantiate(playerPrefab);
            playerInstance.name = "Player";

            var inventoryPrefabGuid = AssetDatabase.FindAssets("t:Prefab VRInventory").FirstOrDefault();
            if (string.IsNullOrEmpty(inventoryPrefabGuid)) {
                Debug.LogWarning("[MobileVRInventory] VRInventory Prefab not found.");
                return;
            }

            var inventoryPrefab = (GameObject) AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GUIDToAssetPath(inventoryPrefabGuid),
                typeof(GameObject));

            var inventoryInstance = GameObject.Instantiate(inventoryPrefab);
            inventoryInstance.name = "VRInventory";

            // Find UI References
            VRInventory vrInventoryScript = inventoryInstance.GetComponent<VRInventory>();
            vrInventoryScript.inventoryPositionTransform = GameObject.Find("InventoryPosition").transform;
            vrInventoryScript.handPosition               = GameObject.Find("HandPosition").transform;

            // Add an event system if necessary
            var        eventSystem   = GameObject.FindObjectOfType<EventSystem>();
            GameObject eventSystemGO = null;

            if (eventSystem == null) {
                eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
            } else {
                eventSystemGO = eventSystem.gameObject;
            }

            // Add the Gaze Input Module if necessary
            var gazeInputModule = eventSystemGO.GetComponent<GazeInputModuleInventory>();
            if (gazeInputModule == null) {
                eventSystemGO.AddComponent<GazeInputModuleInventory>();
            }

            // link the trigger to the VRInventory
            var trigger = playerInstance.GetComponentInChildren<VRInventoryTrigger>();
            trigger.vrInventory = vrInventoryScript;
        }
    }
}