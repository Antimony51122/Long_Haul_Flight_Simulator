using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MobileVRInventory
{
    [CustomEditor(typeof(VRInventory))]
    public class VRInventoryEditor : Editor
    {
        private SerializedObject SO_target;
        private SerializedProperty itemDatabase;
        private SerializedProperty inventoryTriggerMode;
        private SerializedProperty hideWhenItemSelected;
        private SerializedProperty showInventoryItemsInEditMode;
        private SerializedProperty inventoryUIPrefab;
        private SerializedProperty itemTemplate;
        private SerializedProperty inventoryPositionTransform;
        private SerializedProperty handPosition;
        private SerializedProperty onItemSelected;
        private SerializedProperty onItemPickedUp;

        private SerializedProperty autoSave;
        private SerializedProperty saveSlotName;

        private bool showReferences = false;
        private VRInventory vrInventoryTarget;
        private int availableItemIndex = 0;
        private int newItemQuantity = 1;
        
        void OnEnable() {
            SO_target = new SerializedObject(target);
            itemDatabase = SO_target.FindProperty("itemDatabase");
            inventoryTriggerMode = SO_target.FindProperty("inventoryTriggerMode");
            hideWhenItemSelected = SO_target.FindProperty("hideWhenItemSelected");
            showInventoryItemsInEditMode = SO_target.FindProperty("showInventoryItemsInEditMode");
            inventoryUIPrefab = SO_target.FindProperty("inventoryUIPrefab");
            itemTemplate = SO_target.FindProperty("itemTemplate");
            inventoryPositionTransform = SO_target.FindProperty("inventoryPositionTransform");
            handPosition = SO_target.FindProperty("handPosition");
            onItemSelected = SO_target.FindProperty("onItemSelected");
            onItemPickedUp = SO_target.FindProperty("onItemPickedUp");

            autoSave = SO_target.FindProperty("autoSave");
            saveSlotName = SO_target.FindProperty("saveSlotName");

            vrInventoryTarget = target as VRInventory;            
        }        

        public override void OnInspectorGUI() {
            SO_target.Update();
            bool forceItemReRender = false;
            EditorGUI.BeginChangeCheck();
            itemDatabase.objectReferenceValue = EditorGUILayout.ObjectField("Item Database", itemDatabase.objectReferenceValue, typeof(InventoryItemDatabase), false);

            if (itemDatabase.objectReferenceValue == null) {
                EditorGUILayout.HelpBox("Please select an Inventory Item Database", MessageType.Warning);
                vrInventoryTarget.items.Clear();
                return;
            }            

            EditorGUILayout.PropertyField(inventoryTriggerMode);
            if (inventoryTriggerMode.enumValueIndex == (int)VRInventory.eInventoryTriggerMode.InputFire1) EditorGUILayout.PropertyField(hideWhenItemSelected);

            EditorGUI.BeginChangeCheck(); {
                EditorGUILayout.PropertyField(showInventoryItemsInEditMode);
            }
            if (EditorGUI.EndChangeCheck())  {
                forceItemReRender = true;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Save / Load", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(autoSave);
            if (autoSave.boolValue)
            {
                EditorGUILayout.PropertyField(saveSlotName);
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PrefixLabel(" ");
                    if (GUILayout.Button("Clear Save"))
                    {
                        PlayerPrefs.SetString("vrInventory_" + saveSlotName.stringValue, "");
                        PlayerPrefs.Save();

                        EditorUtility.DisplayDialog("Success", "Save slot '" + saveSlotName.stringValue + "' cleared.", "Okay");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            var changedItems = RenderItemListEditor();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(onItemSelected);
            EditorGUILayout.PropertyField(onItemPickedUp);
            EditorGUILayout.Space();

            showReferences = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showReferences, "UI References", true);
            if (showReferences)            {
                EditorGUILayout.PropertyField(inventoryUIPrefab);
                EditorGUILayout.PropertyField(itemTemplate);
                EditorGUILayout.PropertyField(inventoryPositionTransform);
                EditorGUILayout.PropertyField(handPosition);
            }

            if (EditorGUI.EndChangeCheck())
            {
                SO_target.ApplyModifiedProperties();
                if(!Application.isPlaying) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(vrInventoryTarget.gameObject.scene);
            }

            if (forceItemReRender)
            {                
                vrInventoryTarget.ItemsUpdated();
            }

            foreach (var addedItemName in changedItems["added"])
            {                
                vrInventoryTarget.AddItem(addedItemName, newItemQuantity);
            }

            foreach (var removedItemName in changedItems["removed"])
            {                
                vrInventoryTarget.RemoveItem(removedItemName, -1);
            }
        }

        Dictionary<string, List<string>> RenderItemListEditor()
        {
            Dictionary<string, List<string>> changedItems = new Dictionary<string, List<string>>()
            {
                {"added", new List<string>()},
                {"removed", new List<string>()}
            };

            var itemStyle = new GUIStyle(EditorStyles.toolbar);
            itemStyle.fixedHeight = 0;

            var textStyle = new GUIStyle(EditorStyles.largeLabel);
            textStyle.alignment = TextAnchor.MiddleLeft;
            textStyle.fixedHeight = 0;

            var buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
            buttonStyle.fixedHeight = 0;

            List<string> itemsToRemove = new List<string>();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                EditorGUILayout.LabelField("Items", EditorStyles.boldLabel);

                foreach (var stack in vrInventoryTarget.items)
                {
                    var item = stack.item;

                    EditorGUILayout.BeginHorizontal(itemStyle, GUILayout.Height(24));

                    GUILayout.BeginVertical(GUILayout.Width(20));
                    {
                        GUILayout.Space(2);

                        var imageRect = EditorGUILayout.GetControlRect(GUILayout.Width(20), GUILayout.Height(20));
                        if (item.image != null)
                        {
                            EditorGUI.DrawPreviewTexture(imageRect, item.image.texture);
                        }
                    }
                    GUILayout.EndVertical();

                    EditorGUILayout.LabelField(item.name, textStyle);
                    
                    EditorGUILayout.LabelField("x", GUILayout.Width(10));
                    if (item.stackSize == 1)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                    }

                    var newQuantity = EditorGUILayout.IntField(stack.quantity, GUILayout.Width(50));
                    if (item.stackSize < 0 || newQuantity <= item.stackSize)
                    {
                        stack.quantity = newQuantity;
                    }
                    else
                    {
                        stack.quantity = System.Math.Max(1, item.stackSize);
                        Debug.Log(stack.item.name);
                        Debug.Log("The quantity you have specified exceeds the maximum stack size for this item.");
                        //EditorUtility.DisplayDialog("Stack size exceeded", "The quantity you have specified exceeds the maximum stack size for this item.", "Okay");                        
                    }

                    if (item.stackSize == 1)
                    {
                        EditorGUI.EndDisabledGroup();
                    }

                    EditorGUILayout.Separator();

                    GUILayout.BeginVertical(GUILayout.Width(20));
                    {
                        GUILayout.Space(2);
                        if (GUILayout.Button("X", buttonStyle, GUILayout.Width(20), GUILayout.Height(20)))
                        {
                            itemsToRemove.Add(item.name);
                        }
                        GUILayout.Space(2);
                    }
                    GUILayout.EndVertical();

                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                {
                    if (vrInventoryTarget.itemDatabase != null)
                    {
                        //var itemsInInventory = vrInventoryTarget.items.Select(i => i.item.name);                        

                        var availableItems = vrInventoryTarget.itemDatabase.items.Select(i => i.name).ToArray();

                        availableItemIndex = EditorGUILayout.Popup(availableItemIndex, availableItems);

                        EditorGUILayout.LabelField("x", GUILayout.Width(10));
                        newItemQuantity = EditorGUILayout.IntField(newItemQuantity, GUILayout.Width(50));

                        if (GUILayout.Button("Add item") && availableItems.Count() > availableItemIndex)
                        {                            
                            changedItems["added"].Add(availableItems[availableItemIndex]);                            
                        }
                    }

                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            if (itemsToRemove.Any())
            {
                foreach (var name in itemsToRemove)
                {                    
                    changedItems["removed"].Add(name);                    
                }                
            }

            return changedItems;
        }
    }
}
