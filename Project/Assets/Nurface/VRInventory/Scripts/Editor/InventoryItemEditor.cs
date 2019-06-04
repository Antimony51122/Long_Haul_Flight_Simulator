using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MobileVRInventory
{
    [CustomEditor(typeof(InventoryItem))]
    [CanEditMultipleObjects()]
    public class InventoryItemEditor : Editor
    {        
        private SerializedProperty itemDatabase;        
        private SerializedProperty quantity;

        private InventoryItem itemTarget;
        
        void OnEnable() {            
            itemDatabase = serializedObject.FindProperty("itemDatabase");            
            quantity = serializedObject.FindProperty("quantity");

            itemTarget = target as InventoryItem;
        }        

        public override void OnInspectorGUI() {
            serializedObject.Update();

            InventoryItemData itemData = itemTarget.itemData;

            EditorGUI.BeginChangeCheck();

            itemDatabase.objectReferenceValue = EditorGUILayout.ObjectField("Item Database", itemDatabase.objectReferenceValue, typeof(InventoryItemDatabase), false);

            if (itemDatabase.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("Please select an Inventory Item Database", MessageType.Warning);
            } else if(itemTarget.itemDatabase != null) {

                var _itemDatabase = itemTarget.itemDatabase;
                var availableItems = _itemDatabase.items.Select(i => i.name).ToArray();
                var selectedIndex = itemTarget.itemData != null ? availableItems.ToList().IndexOf(itemTarget.itemData.name) : 0;

                selectedIndex = EditorGUILayout.Popup("Item", selectedIndex, availableItems);

                if (selectedIndex >= 0) {
                    itemData = _itemDatabase.GetItem(availableItems[selectedIndex]);
                }

                quantity.intValue = EditorGUILayout.IntField("Quantity", quantity.intValue);
            }
            
            if(EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();

                foreach (var t in targets)
                {
                    (t as InventoryItem).itemData = itemData;
                }
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(itemTarget.gameObject.scene);
            }
        }       
    }
}
