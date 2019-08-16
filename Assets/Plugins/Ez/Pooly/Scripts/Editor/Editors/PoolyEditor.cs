// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using QuickEditor;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;

namespace Ez.Pooly
{
    [CustomEditor(typeof(Pooly))]
    [DisallowMultipleComponent]
    public class PoolyEditor : QEditor
    {
        private Pooly pooly { get { return (Pooly)target; } }

        public const string NEW_CATEGORY = "New Category";

        Color accentColorGreen { get { return QUI.IsProSkin ? QColors.Green.Color : QColors.GreenLight.Color; } }
        GUIContent tempContent;
        Vector2 tempContentSize;

        AnimBool showHelp, renameCategoryAnimBool;

        SerializedProperty debug, autoAddMissingItems;

        /// <summary>
        /// ReorderableList that helps reorder the categories list
        /// </summary>
        ReorderableList rCategories;

        /// <summary>
        /// All the items sorted by category
        /// </summary>
        Dictionary<string, List<Pooly.Item>> categoryItem = new Dictionary<string, List<Pooly.Item>>();
        /// <summary>
        /// Each category expanded state
        /// </summary>
        Dictionary<string, AnimBool> categoryExpanded = new Dictionary<string, AnimBool>();
        /// <summary>
        /// Each item expanded state
        /// </summary>
        Dictionary<Pooly.Item, AnimBool> itemExpanded = new Dictionary<Pooly.Item, AnimBool>();

        /// <summary>
        /// Saves all the categories expanded state before we update the items list
        /// </summary>
        Dictionary<string, AnimBool> categoryExpandedBeforeUpdate = new Dictionary<string, AnimBool>();
        /// <summary>
        /// Saves all the items expanded state before we update the items list
        /// </summary>
        Dictionary<Pooly.Item, AnimBool> itemExpandedBeforeUpdate = new Dictionary<Pooly.Item, AnimBool>();

        GUIStyle _dropZoneBoxStyle;
        GUIStyle DropZoneBoxStyle
        {
            get
            {
                if(_dropZoneBoxStyle == null)
                {
                    _dropZoneBoxStyle = new GUIStyle
                    {
                        normal = { background = QResources.backgroundLowGray.normal2D, textColor = QUI.IsProSkin ? QColors.GreenLight.Color : QColors.GreenDark.Color },
                        hover = { background = QResources.backgroundLowGreen.normal2D, textColor = QUI.IsProSkin ? QColors.GreenLight.Color : QColors.GreenDark.Color },
                        font = QResources.GetFont(FontName.Ubuntu.Light),
                        fontSize = 12,
                        alignment = TextAnchor.MiddleCenter,
                        border = new RectOffset(8, 8, 8, 8)
                    };
                }
                return _dropZoneBoxStyle;
            }
        }

        GUIStyle _itemQuickViewTextStyle;
        GUIStyle ItemQuickViewTextStyle
        {
            get
            {
                if(_itemQuickViewTextStyle == null)
                {
                    _itemQuickViewTextStyle = new GUIStyle
                    {
                        normal = { textColor = QUI.IsProSkin ? QColors.GreenLight.Color : QColors.GreenDark.Color },
                        font = QUI.IsProSkin ? QResources.GetFont(FontName.Ubuntu.Regular) : QResources.GetFont(FontName.Ubuntu.Bold),
                        fontStyle = FontStyle.Normal,
                        fontSize = 13,
                        alignment = TextAnchor.MiddleLeft,
                        padding = new RectOffset(0, 0, 0, 2)
                    };
                }
                return _itemQuickViewTextStyle;
            }
        }

        /// <summary>
        /// List of categories used by the drop down list (contains 'New Category' and 'No Category' entries, by default)
        /// </summary>
        List<string> categoriesList = new List<string> { NEW_CATEGORY, Pooly.DEFAULT_CATEGORY_NAME };
        /// <summary>
        /// Index for the selected category in the drop down list from the categoriesList
        /// </summary>
        int newItemCategoryListIndex = 1;
        /// <summary>
        /// List of categories used by the reordable list
        /// </summary>
        List<string> categories = new List<string>();

        /// <summary>
        /// Toggle for whoing the 'Settings for New Items'
        /// </summary>
        AnimBool showNewItemSettings;
        /// <summary>
        /// Name for a new category created by the New Item menu (at the top)
        /// </summary>
        string newItemCategoryName = string.Empty;
        /// <summary>
        /// Deafult settings when creating/adding a new item
        /// </summary>
        Pooly.Item newItemSettings = new Pooly.Item();

        /// <summary>
        /// The item for which we are changing the category
        /// </summary>
        Pooly.Item changeCategoryItem = null;
        /// <summary>
        /// The category index for the change drop down list
        /// </summary>
        int changeCategoryItemIndex = 1;
        /// <summary>
        /// Name for a new category created by the Change Category menu (found inside every expanded Item)
        /// </summary>
        string changeCategoryNewCategory = string.Empty;

        /// <summary>
        /// Variable used to determine if the searchPattern used generated any search results
        /// </summary>
        bool searchReturnedResuls = false;
        /// <summary>
        /// Contains all the Items that contain the searchPattern
        /// </summary>
        List<Pooly.Item> searchResults = new List<Pooly.Item>();

        /// <summary>
        /// Keeps track of all the Items with null prefabs (missing prefabs) and it is used to know when the developer drags and drops a new reference. Using this, we are able to check if the new reference is valid or not.
        /// </summary>
        List<Pooly.Item> itemsWithMissingPrefab = new List<Pooly.Item>();

        string renameCategory, newCategoryName, deleteCategory, newItemCategory = string.Empty, searchPattern;

        protected override void OnEnable()
        {
            base.OnEnable();

            requiresContantRepaint = true;
            QUI.MarkSceneDirty(true);

            UpdateCategoriesAndItems();

            tempContent = new GUIContent();
            tempContentSize = Vector2.zero;
        }

        protected override void SerializedObjectFindProperties()
        {
            base.SerializedObjectFindProperties();

            debug = serializedObject.FindProperty("debug");
            autoAddMissingItems = serializedObject.FindProperty("autoAddMissingItems");
        }

        protected override void InitAnimBools()
        {
            base.InitAnimBools();

            showHelp = new AnimBool(false, Repaint);
            showNewItemSettings = new AnimBool(false, Repaint);
            renameCategoryAnimBool = new AnimBool(false, Repaint);
        }

        protected override void GenerateInfoMessages()
        {
            base.GenerateInfoMessages();

            infoMessage.Add("SearchNoResults", new InfoMessage() { title = "Your search returned no results...", message = "", show = new AnimBool(false), type = InfoMessageType.Info });
            infoMessage.Add("AutoAddMissingItems", new InfoMessage() { title = "Auto Add Missing Items", message = "Adds new items, at runtime, to the pool if you try to spawn/despawn prefabs that are not in the pool.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("DropPrefabsHere", new InfoMessage() { title = "Drop Prefabs Here", message = "Adds new items to the pool in bulk. The items are added to the selected category with the current settings.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("CreateNewItem", new InfoMessage() { title = "Create New Item", message = "Adds a new empty item to the selected category, with the current settings for new items.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("SettingsForNewItems", new InfoMessage() { title = "Settings For New Items", message = "Every new item added to the pool will have these settings.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("Search", new InfoMessage() { title = "Search", message = "Regex search that allows filtering the prefabs by name.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("CollapseAll", new InfoMessage() { title = "Collapse All", message = "Closes all opened items and categories.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("ClearSearch", new InfoMessage() { title = "Clear Search", message = "Clears the search and opens the filtered prefabs.", show = showHelp, type = InfoMessageType.Help });
            infoMessage.Add("PoolIsEmpty", new InfoMessage() { title = "The Pool is empty...", show = new AnimBool(false, Repaint), type = InfoMessageType.Info });
        }

        void UpdateCategoriesAndItems()
        {
            itemsWithMissingPrefab = new List<Pooly.Item>();
            categoryItem = new Dictionary<string, List<Pooly.Item>>();
            categoryExpanded = new Dictionary<string, AnimBool>();
            itemExpanded = new Dictionary<Pooly.Item, AnimBool>();
            categoriesList = new List<string> { NEW_CATEGORY, Pooly.DEFAULT_CATEGORY_NAME };
            categories = new List<string>();
            for(int i = 0; i < pooly.items.Count; i++)
            {
                if(pooly.items[i].prefab == null) { itemsWithMissingPrefab.Add(pooly.items[i]); }
                pooly.items[i].category = string.IsNullOrEmpty(pooly.items[i].category) ? Pooly.DEFAULT_CATEGORY_NAME : pooly.items[i].category;
                if(!categoryItem.ContainsKey(pooly.items[i].category))
                {
                    categoryItem.Add(pooly.items[i].category, new List<Pooly.Item>() { pooly.items[i] });
                    if(!categoriesList.Contains(pooly.items[i].category)) { categoriesList.Add(pooly.items[i].category); }
                    if(!categories.Contains(pooly.items[i].category)) { categories.Add(pooly.items[i].category); }
                }
                else
                {
                    categoryItem[pooly.items[i].category].Add(pooly.items[i]);
                }

                if(!categoryExpanded.ContainsKey(pooly.items[i].category))
                {
                    categoryExpanded.Add(pooly.items[i].category, new AnimBool(false, Repaint));
                }
                itemExpanded.Add(pooly.items[i], new AnimBool(false, Repaint));
            }

            if(!string.IsNullOrEmpty(renameCategory) && !string.IsNullOrEmpty(newCategoryName)) //if we just renamed a category we leave that category open
            {
                categoryExpanded[newCategoryName].value = true;
                renameCategory = string.Empty;
                newCategoryName = string.Empty;
            }

            if(!string.IsNullOrEmpty(newItemCategory)) //if we just added a new item to a category, we leave the category open
            {
                categoryExpanded[newItemCategory].value = true;
                newItemCategory = string.Empty;
            }

            rCategories = new ReorderableList(categories, typeof(string), true, false, false, false)
            {
                onReorderCallback = (list) => { ReorderCategories(); },
                showDefaultBackground = false,
                drawElementBackgroundCallback = (rect, index, active, focused) => { },
                elementHeightCallback = (index) =>
                {
                    Repaint();
                    return 28;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    if(index == categories.Count) return;
                    if(QUI.GhostBar(new Rect(rect.x + 6, rect.y, WIDTH_420 - 26, 24), categories[index], QColors.Color.Gray, categoryExpanded[categories[index]]))
                    {
                        categoryExpanded[categories[index]].target = true;
                    }
                }
            };

            if(itemExpandedBeforeUpdate.Count > 0) { itemExpanded = itemExpandedBeforeUpdate; itemExpandedBeforeUpdate = new Dictionary<Pooly.Item, AnimBool>(); }
            if(categoryExpandedBeforeUpdate.Count > 0) { categoryExpanded = categoryExpandedBeforeUpdate; categoryExpandedBeforeUpdate = new Dictionary<string, AnimBool>(); }
        }

        void ReorderCategories()
        {
            if(categories == null || categories.Count == 0) { return; }
            List<Pooly.Item> newItemsList = new List<Pooly.Item>();
            for(int i = 0; i < rCategories.list.Count; i++)
            {
                categoriesList.Add(rCategories.list[i].ToString());
                List<Pooly.Item> tempItemsList = new List<Pooly.Item>();
                for(int j = 0; j < pooly.items.Count; j++)
                {
                    if(pooly.items[j].category.Equals(rCategories.list[i].ToString())) { tempItemsList.Add(pooly.items[j]); }
                }
                newItemsList.AddRange(tempItemsList);
                tempItemsList = new List<Pooly.Item>();
            }
            pooly.items = new List<Pooly.Item>();
            pooly.items = newItemsList;
            SortItemsListByCategory();
        }

        void SortItemsListByCategory()
        {
            if(pooly.items == null || pooly.items.Count == 0) { return; }
            List<Pooly.Item> newItemsList = new List<Pooly.Item>();
            for(int i = 0; i < categories.Count; i++)
            {
                List<Pooly.Item> tempItemsListWithPrefabs = new List<Pooly.Item>();
                List<Pooly.Item> tempItemsListWithNoPrefabs = new List<Pooly.Item>();
                for(int j = 0; j < pooly.items.Count; j++)
                {
                    if(pooly.items[j].category.Equals(categories[i]))
                    {
                        if(pooly.items[j].prefab != null)
                        {
                            tempItemsListWithPrefabs.Add(pooly.items[j]);
                        }
                        else
                        {
                            tempItemsListWithNoPrefabs.Add(pooly.items[j]);
                        }
                    }
                }
                if(tempItemsListWithPrefabs.Count > 0)
                {
                    tempItemsListWithPrefabs.Sort((a, b) => a.prefab.name.CompareTo(b.prefab.name));
                    newItemsList.AddRange(tempItemsListWithPrefabs);
                }
                if(tempItemsListWithNoPrefabs.Count > 0)
                {
                    newItemsList.AddRange(tempItemsListWithNoPrefabs);
                }
            }
            pooly.items = new List<Pooly.Item>();
            pooly.items = newItemsList;
            categoryExpandedBeforeUpdate = categoryExpanded;
            itemExpandedBeforeUpdate = itemExpanded;
            UpdateCategoriesAndItems();
            QUI.ExitGUI();
        }

        void CheckForReset()
        {
            if(pooly.items.Count == 0 && categoryItem.Count > 0) //User pressed the Reset() button
            {
                UpdateCategoriesAndItems();
                newItemSettings = new Pooly.Item();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawHeader(EZResources.editorHeaderPooly.texture, WIDTH_420, HEIGHT_42);
            CheckForReset();
            serializedObject.Update();
            if(EditorApplication.isPlaying)
            {
                // Runtime inspector
                DrawRuntimeEditor(WIDTH_420);
            }
            else
            {
                // Editing inspector
                DrawShowHelpAndDebug();
                DragAndDropCheck(Event.current.type);
                DrawAddMissingItemsAndDropPrefab();
                DrawInfoMessage("AutoAddMissingItems", WIDTH_420);
                DrawInfoMessage("DropPrefabsHere", WIDTH_420);
                QUI.Space(SPACE_4);
                DrawNewItemArea();
                QUI.Space(SPACE_4);
                DrawSearchArea();
                infoMessage["PoolIsEmpty"].show.target = pooly.items.Count == 0;
                DrawInfoMessage("PoolIsEmpty", WIDTH_420);
                if(infoMessage["PoolIsEmpty"].show.target) { QUI.Space(SPACE_4); return; }
                if(!GenerateSearchResults())
                {
                    if(AreAllCategoriesClosed())
                    {
                        QUI.BeginVertical();
                        {
                            QUI.Space(-16);
                            rCategories.DoLayoutList();
                        }
                        QUI.EndVertical();
                    }
                    else
                    {
                        QUI.Space(SPACE_4);
                        CheckItemsWithMissingPrefabs();
                        DrawItems();
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            QUI.Space(SPACE_4);
        }

        void DrawRuntimeEditor(float width)
        {
            infoMessage["PoolIsEmpty"].show.target = Pooly.Categories.Count == 0;
            DrawInfoMessage("PoolIsEmpty", width);
            if(infoMessage["PoolIsEmpty"].show.target) { return; }

            QUI.Label("Auto add missing items is " + (pooly.autoAddMissingItems ? "ENABLED" : "DISABLED"), Style.Text.Help);
            QUI.Space(SPACE_2);
            foreach(var category in GetPooledItemsCategories())
            {
                if(!categoryExpanded.ContainsKey(category)) { categoryExpanded.Add(category, new AnimBool(true, Repaint)); }

                if(QUI.GhostBar(category, QColors.Color.Gray, categoryExpanded[category], width, 24)) { categoryExpanded[category].target = !categoryExpanded[category].target; }

                QUI.BeginHorizontal(width);
                {
                    QUI.Space(SPACE_8 * categoryExpanded[category].faded);
                    if(QUI.BeginFadeGroup(categoryExpanded[category].faded))
                    {
                        QUI.BeginVertical(width - SPACE_8);
                        {
                            foreach(var pooledItem in Pooly.Pool.Values)
                            {
                                if(pooledItem.prefab != null && pooledItem.category.Equals(category))
                                {
                                    QUI.Space(SPACE_2);
                                    QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), width - SPACE_8, 36);
                                    QUI.Space(-38);
                                    QUI.BeginHorizontal(width - SPACE_8);
                                    {
                                        tempContent.text = pooledItem.prefab.name;
                                        tempContentSize = QStyles.CalcSize(tempContent, Style.Text.Subtitle);
                                        QUI.Label(tempContent.text, Style.Text.Subtitle, tempContentSize.x, 24);
                                        DrawItemQuickViewInfo(pooledItem, width - SPACE_8 - tempContentSize.x, ItemQuickViewTextStyle, IconColor.Green, true, false);
                                        QUI.FlexibleSpace();
                                    }
                                    QUI.EndHorizontal();
                                    QUI.Space(-6);
                                    QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Green), width - SPACE_8, 18);
                                    QUI.Space(-18);
                                    QUI.Label(pooledItem.ActiveClones.Count + " Spawned Clones | " + pooledItem.DisabledClones.Count + " Available Clones", Style.Text.Small);
                                }
                            }
                            QUI.Space(SPACE_8 * categoryExpanded[category].faded);
                        }
                        QUI.EndVertical();
                    }
                    QUI.EndFadeGroup();
                }
                QUI.EndHorizontal();
                QUI.Space(SPACE_8);
            }
            Repaint();
        }

        void DrawShowHelpAndDebug()
        {
            QUI.SetGUIBackgroundColor(QColors.GreenLight.Color);
            QUI.BeginHorizontal(WIDTH_420);
            {
                showHelp.target = QUI.Toggle(showHelp.target);
                QUI.Label("Show Help", Style.Text.Normal, 64);
                QUI.Toggle(debug);
                QUI.Label("Debug", Style.Text.Normal, 84);
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
            QUI.ResetColors();
        }

        public static void DragAndDropCheck(EventType eventType)
        {
            switch(eventType)
            {
                //case EventType.MouseDown: DragAndDrop.PrepareStartDrag(); break; //Debug.Log("MouseDown"); //reset the DragAndDrop Data
                case EventType.DragUpdated: DragAndDrop.visualMode = DragAndDropVisualMode.Copy; break; //Debug.Log("DragUpdated " + Event.current.mousePosition);
                case EventType.DragPerform: DragAndDrop.AcceptDrag(); break; //Debug.Log("Drag accepted");
                //case EventType.MouseDrag: DragAndDrop.StartDrag("Dragging"); Event.current.Use(); break; //Debug.Log("MouseDrag: " + Event.current.mousePosition);
                case EventType.MouseUp: DragAndDrop.PrepareStartDrag(); break; //Debug.Log("MouseUp had " + DragAndDrop.GetGenericData("GameObject"));  //Clean up, in case MouseDrag never occurred
                case EventType.DragExited: break; //Debug.Log("DragExited");
            }
        }

        void DrawAddMissingItemsAndDropPrefab()
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.SetGUIBackgroundColor(QColors.GreenLight.Color);
                QUI.Toggle(autoAddMissingItems);
                QUI.ResetColors();
                QUI.Label("Auto add missing items", Style.Text.Normal, 130);
                QUI.BeginVertical(WIDTH_420 - 130);
                {
                    QUI.Space(-17);
                    QUI.Label("Drop Prefabs Here", DropZoneBoxStyle, WIDTH_420 - 150, 32);
                    Rect dropZone = GUILayoutUtility.GetLastRect();
                    if(Event.current.type == EventType.DragPerform)
                    {
                        if(dropZone.Contains(Event.current.mousePosition))
                        {
                            Event.current.Use();
                            DragAndDrop.AcceptDrag();
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            string targetCategory = Pooly.DEFAULT_CATEGORY_NAME;
                            if(categoriesList[newItemCategoryListIndex].Equals(NEW_CATEGORY))
                            {
                                if(!string.IsNullOrEmpty(newItemCategoryName))
                                {
                                    targetCategory = newItemCategoryName;
                                }
                            }
                            else
                            {
                                targetCategory = categoriesList[newItemCategoryListIndex];
                            }
                            foreach(var obj in DragAndDrop.objectReferences)
                            {
                                if(obj.GetType() != typeof(GameObject))
                                {
                                    QUI.DisplayDialog("Cannot add New Item to the pool!",
                                                                "The '" + obj.name + "' object you are trying to add to the pool is not a gameObject.",
                                                                "Ok");
                                }
                                else if(PrefabUtility.GetPrefabObject(obj) == null) // If TRUE then this is not a prefab
                                {
                                    QUI.DisplayDialog("Cannot add New Item to the pool!",
                                                                "The '" + obj.name + "' gameObject you are trying to add to the pool is not a prefab." +
                                                                "\n\n" +
                                                                "You cannot and should not try to add scene objects.",
                                                                "Ok");
                                }
                                else
                                {
                                    GameObject go = (GameObject)obj;
                                    AddItemToCategory(targetCategory, go.transform);
                                    newItemCategoryListIndex = categoriesList.FindIndex(x => x.Equals(targetCategory));
                                    newItemCategoryName = string.Empty;
                                }
                            }
                        }
                    }
                }
                QUI.EndVertical();
            }
            QUI.EndHorizontal();
        }

        void DrawNewItemArea()
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                if(newItemCategoryListIndex >= categoriesList.Count) { newItemCategoryListIndex = 1; } //fix for a strange bug when the index gets out of bounds - hard reset to the 'No Category' selection
                if(QUI.GhostButton("Create a New Item", QColors.Color.Green, WIDTH_140))
                {
                    if(categoriesList[newItemCategoryListIndex].Equals(NEW_CATEGORY))
                    {
                        if(string.IsNullOrEmpty(newItemCategoryName))
                        {
                            QUI.DisplayDialog("Create New Item",
                                                        "Please enter a category name to continue.",
                                                        "Ok");
                        }
                        else if(newItemCategoryName.Trim().Equals(NEW_CATEGORY))
                        {
                            QUI.DisplayDialog("Create New Item",
                                                        "You cannot create a new category named '" + NEW_CATEGORY + "'.",
                                                        "Ok");
                        }
                        else
                        {
                            AddItemToCategory(newItemCategoryName, null);
                            newItemCategoryListIndex = categoriesList.FindIndex(x => x.Equals(newItemCategoryName));
                            newItemCategoryName = string.Empty;
                        }
                    }
                    else
                    {
                        AddItemToCategory(categoriesList[newItemCategoryListIndex], null);
                    }
                }
                QUI.BeginVertical(60);
                {
                    QUI.Space(-1);
                    QUI.Label("in Category", Style.Text.Small, 60);
                }
                QUI.EndVertical();
                QUI.BeginVertical(70);
                {
                    QUI.Space(-SPACE_2);
                    QUI.BeginHorizontal();
                    {
                        if(categoriesList[newItemCategoryListIndex].Equals(NEW_CATEGORY))
                        {
                            newItemCategoryName = QUI.TextField(newItemCategoryName, accentColorGreen, 190);
                            if(QUI.ButtonCancel() ||
                               (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp))
                            {
                                newItemCategoryListIndex = 1;
                                newItemCategoryName = string.Empty;
                            }
                        }
                        else
                        {
                            QUI.BeginVertical(212);
                            {
                                QUI.Space(SPACE_2);
                                QUI.SetGUIBackgroundColor(accentColorGreen);
                                newItemCategoryListIndex = EditorGUILayout.Popup(newItemCategoryListIndex, categoriesList.ToArray(), GUILayout.Width(212), GUILayout.Height(18));
                                QUI.ResetColors();
                                QUI.Space(-1);
                            }
                            QUI.EndVertical();
                        }
                    }
                    QUI.EndHorizontal();
                }
                QUI.EndVertical();
            }
            QUI.EndHorizontal();
            DrawInfoMessage("CreateNewItem", WIDTH_420);
            QUI.Space(SPACE_4);
            QUI.Button(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), WIDTH_420, 24);
            QUI.Space(-24);
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Space(2);
                DrawItemQuickViewInfo(newItemSettings, WIDTH_420, ItemQuickViewTextStyle, IconColor.Green, true, false);
            }
            QUI.EndHorizontal();

            QUI.Space(SPACE_2);

            QUI.BeginHorizontal(WIDTH_420);
            {
                if(QUI.GhostBar("Settings for New Items", QColors.Color.Green, showNewItemSettings, WIDTH_420 - 100 - 4, 16))
                {
                    showNewItemSettings.target = !showNewItemSettings.target;
                }
                QUI.Space(SPACE_2);
                if(QUI.GhostButton("reset to default", QColors.Color.Gray, 100))
                {
                    newItemSettings = new Pooly.Item();
                    showNewItemSettings.target = false;
                }
            }
            QUI.EndHorizontal();
            DrawInfoMessage("SettingsForNewItems", WIDTH_420);
            if(QUI.BeginFadeGroup(showNewItemSettings.faded))
            {
                QUI.Space(SPACE_4 * showNewItemSettings.faded);
                QUI.BeginHorizontal(WIDTH_420);
                {
                    QUI.Space(6 * showNewItemSettings.faded);
                    QUI.DrawTexture(QResources.iconNumberGreen.texture, 18, 18);
                    QUI.Space(-4);
                    tempContent.text = "Number of Clones";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x * showNewItemSettings.faded);
                    QUI.SetGUIBackgroundColor(accentColorGreen);
                    newItemSettings.preloadCloneCount = EditorGUILayout.IntField(newItemSettings.preloadCloneCount, GUILayout.Width(86 * showNewItemSettings.faded));
                    QUI.ResetColors();
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
                QUI.Space(SPACE_4);
                DrawItemSettings(newItemSettings, WIDTH_420, showNewItemSettings, IconColor.Green, accentColorGreen);
                QUI.Space(SPACE_4);
            }
            QUI.EndFadeGroup();
            QUI.Space(SPACE_8);
            QUI.DrawLine(QColors.Color.Gray, WIDTH_420);
            QUI.Space(SPACE_4);
        }

        public enum IconColor
        {
            Green,
            Blue
        }

        public static void DrawItemSettings(Pooly.Item item, float width, AnimBool aBool, IconColor iconColor, Color accentColor)
        {
            GUIContent content = new GUIContent();
            if(!item.allowRecycleClones)
            {
                QUI.BeginHorizontal(width);
                {
                    QUI.Space(6 * aBool.faded);
                    QUI.DrawTexture(item.allowInstantiateMore
                                    ? (iconColor == IconColor.Green ? QResources.iconArrowUpGreen.texture : QResources.iconArrowUpBlue.texture)
                                    : QResources.iconArrowUpGray.texture, 18, 18);
                    QUI.SetGUIBackgroundColor(accentColor);
                    item.allowInstantiateMore = QUI.Toggle(item.allowInstantiateMore);
                    QUI.ResetColors();
                    content.text = "Instantiate more clones, if needed";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
            }
            else if(item.allowInstantiateMore) { item.allowInstantiateMore = false; }
            QUI.Space(SPACE_2);
            if(item.allowInstantiateMore)
            {
                QUI.BeginHorizontal(width - 22);
                {
                    QUI.Space(23 * aBool.faded);
                    QUI.BeginVertical(29);
                    {
                        QUI.Space(1);
                        QUI.DrawTexture(item.limitCloneCount
                                        ? (iconColor == IconColor.Green ? QResources.iconMaxGreen.texture : QResources.iconMaxBlue.texture)
                                        : QResources.iconMaxGray.texture, 29, 18);
                    }
                    QUI.EndVertical();
                    QUI.SetGUIBackgroundColor(accentColor);
                    item.limitCloneCount = QUI.Toggle(item.limitCloneCount);
                    QUI.ResetColors();
                    content.text = "Limit the max number of clones" + (item.limitCloneCount ? " to" : "");
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                    if(item.limitCloneCount)
                    {
                        QUI.SetGUIBackgroundColor(accentColor);
                        item.cloneCountLimit = EditorGUILayout.IntField(item.cloneCountLimit, GUILayout.Width(62 * aBool.faded));
                        QUI.ResetColors();
                        content.text = item.cloneCountLimit == 1 ? "clone" : "clones";
                        QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                        if(item.cloneCountLimit < item.preloadCloneCount) { item.cloneCountLimit = item.preloadCloneCount; }
                    }
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
                QUI.Space(SPACE_2);
            }
            if(!item.allowInstantiateMore && item.limitCloneCount) { item.limitCloneCount = false; }
            if(!item.allowInstantiateMore)
            {
                QUI.BeginHorizontal(width);
                {
                    QUI.Space(6 * aBool.faded);
                    QUI.DrawTexture(item.allowRecycleClones
                                    ? (iconColor == IconColor.Green ? QResources.iconRecycleGreen.texture : QResources.iconRecycleBlue.texture)
                                    : QResources.iconRecycleGray.texture, 18, 18);
                    QUI.SetGUIBackgroundColor(accentColor);
                    item.allowRecycleClones = QUI.Toggle(item.allowRecycleClones);
                    QUI.ResetColors();
                    content.text = "Recycle the oldest spawned clone, if needed";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
            }
            else if(item.allowRecycleClones) { item.allowRecycleClones = false; }

            QUI.Space(2);
            QUI.BeginHorizontal(width - 22);
            {
                QUI.Space(6 * aBool.faded);
                QUI.DrawTexture(item.limitCloneCreationPerFrame
                                ? (iconColor == IconColor.Green ? QResources.iconTimeGreen.texture : QResources.iconTimeBlue.texture)
                                : QResources.iconTimeGray.texture, 18, 18);
                QUI.SetGUIBackgroundColor(accentColor);
                item.limitCloneCreationPerFrame = QUI.Toggle(item.limitCloneCreationPerFrame);
                QUI.ResetColors();
                content.text = "Limit the number of clones created each frame";
                QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();

            if(item.limitCloneCreationPerFrame)
            {
                QUI.BeginHorizontal(width - 22);
                {
                    QUI.Space(40 * aBool.faded);
                    content.text = "Create";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                    QUI.SetGUIBackgroundColor(accentColor);
                    item.clonesOnFirstFrame = EditorGUILayout.IntField(item.clonesOnFirstFrame, GUILayout.Width(62 * aBool.faded));
                    QUI.ResetColors();
                    if(item.clonesOnFirstFrame < 0) { item.clonesOnFirstFrame = 0; }
                    content.text = (item.clonesOnFirstFrame == 1 ? "clone" : "clones") + " on the first frame";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                }
                QUI.EndHorizontal();
                QUI.BeginHorizontal(width - 22);
                {
                    QUI.Space(40 * aBool.faded);
                    content.text = "Then wait for";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                    QUI.SetGUIBackgroundColor(accentColor);
                    item.delayCreatingClonesForFrames = EditorGUILayout.IntField(item.delayCreatingClonesForFrames, GUILayout.Width(62 * aBool.faded));
                    QUI.ResetColors();
                    if(item.delayCreatingClonesForFrames < 0) { item.delayCreatingClonesForFrames = 0; }
                    content.text = (item.delayCreatingClonesForFrames == 1 ? "frame" : "frames");
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                }
                QUI.EndHorizontal();
                QUI.BeginHorizontal(width - 22);
                {
                    QUI.Space(40 * aBool.faded);
                    content.text = "Then create";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                    QUI.SetGUIBackgroundColor(accentColor);
                    item.clonesPerFrame = EditorGUILayout.IntField(item.clonesPerFrame, GUILayout.Width(62 * aBool.faded));
                    QUI.ResetColors();
                    if(item.clonesPerFrame < 1) { item.clonesPerFrame = 1; }
                    content.text = (item.clonesPerFrame == 1 ? "clone" : "clones") + " each frame";
                    QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                }
                QUI.EndHorizontal();
                QUI.Space(SPACE_2);
            }

            QUI.Space(2);
            QUI.BeginHorizontal(width - 22);
            {
                QUI.Space(6 * aBool.faded);
                QUI.DrawTexture(item.debug
                                ? (iconColor == IconColor.Green ? QResources.iconDebugGreen.texture : QResources.iconDebugBlue.texture)
                                : QResources.iconDebugGray.texture, 18, 18);
                QUI.SetGUIBackgroundColor(accentColor);
                item.debug = QUI.Toggle(item.debug);
                QUI.ResetColors();
                content.text = "Log debug messages";
                QUI.Label(content.text, Style.Text.Normal, QStyles.CalcSize(content, Style.Text.Normal).x * aBool.faded);
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
        }

        void DrawSearchArea()
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Label("Search", Style.Text.Normal, 44);
                searchPattern = QUI.TextField(searchPattern, Color.white, WIDTH_420 - 44 - 10 - 120);
                QUI.Space(1);
                if(QUI.GhostButton(string.IsNullOrEmpty(searchPattern) ? "collapse all" : "clear search", QColors.Color.Gray, 120, 16))
                {
                    foreach(var item in pooly.items) //close all the items and all the categories
                    {
                        itemExpanded[item].target = false;
                        categoryExpanded[item.category].target = false;
                    }
                    foreach(var item in pooly.items) //open only the ones from the search
                    {
                        if(searchResults.Contains(item))
                        {
                            itemExpanded[item].target = true;
                            categoryExpanded[item.category].target = true;
                        }
                    }
                    searchResults = new List<Pooly.Item>();
                    searchPattern = string.Empty;
                    renameCategory = string.Empty; //just in case we have a rename option active
                    newCategoryName = string.Empty; //just in case we have a new category option active
                }
            }
            QUI.EndHorizontal();
            DrawInfoMessage("Search", WIDTH_420);
            DrawInfoMessage(string.IsNullOrEmpty(searchPattern) ? "CollapseAll" : "ClearSearch", WIDTH_420);
            QUI.Space(SPACE_8 + SPACE_2);
            QUI.DrawLine(QColors.Color.Gray, WIDTH_420);
            QUI.Space(SPACE_4);
        }

        public static void DrawItemQuickViewInfo(Pooly.Item item, float width, GUIStyle textStyle, IconColor iconColor, bool alignLeft = false, bool alignCenter = false)
        {
            GUIContent label;
            QUI.BeginHorizontal(width);
            {
                if(!alignLeft) { QUI.FlexibleSpace(); }
                QUI.BeginVertical(14, 20);
                {
                    QUI.Space(2);
                    QUI.DrawTexture(iconColor == IconColor.Green ? QResources.iconNumberGreen.texture : QResources.iconNumberBlue.texture, 20, 20);
                    QUI.Space(5);
                }
                QUI.EndVertical();
                label = new GUIContent("[" + item.preloadCloneCount.ToString() + "]");
                QUI.Label(label, textStyle, textStyle.CalcSize(label).x, 20);
                if(item.allowInstantiateMore)
                {
                    QUI.BeginVertical(14, 20);
                    {
                        QUI.Space(2);
                        QUI.DrawTexture(iconColor == IconColor.Green ? QResources.iconArrowUpGreen.texture : QResources.iconArrowUpBlue.texture, 20, 20);
                        QUI.Space(5);
                    }
                    QUI.EndVertical();
                }
                if(item.limitCloneCount)
                {
                    QUI.Space(4);
                    QUI.BeginVertical(27, 20);
                    {
                        QUI.Space(2);
                        QUI.DrawTexture(iconColor == IconColor.Green ? QResources.iconMaxGreen.texture : QResources.iconMaxBlue.texture, 32, 20);
                        QUI.Space(5);
                    }
                    QUI.EndVertical();
                    label = new GUIContent("[" + item.cloneCountLimit.ToString() + "]");
                    QUI.Label(label, textStyle, textStyle.CalcSize(label).x, 20);
                }
                if(item.allowRecycleClones)
                {
                    QUI.Space(2);
                    QUI.BeginVertical(14, 20);
                    {
                        QUI.Space(2);
                        QUI.DrawTexture(iconColor == IconColor.Green ? QResources.iconRecycleGreen.texture : QResources.iconRecycleBlue.texture, 20, 20);
                        QUI.Space(5);
                    }
                    QUI.EndVertical();
                    QUI.Space(2);
                }
                if(item.limitCloneCreationPerFrame)
                {
                    QUI.Space(4);
                    QUI.BeginVertical(16, 20);
                    {
                        QUI.Space(2);
                        QUI.DrawTexture(iconColor == IconColor.Green ? QResources.iconTimeGreen.texture : QResources.iconTimeBlue.texture, 20, 20);
                        QUI.Space(5);
                    }
                    QUI.EndVertical();
                    label = new GUIContent("(" + item.clonesOnFirstFrame + "/" + item.delayCreatingClonesForFrames + "/" + item.clonesPerFrame + ")");
                    QUI.Label(label, textStyle, textStyle.CalcSize(label).x, 20);
                }
                if(item.debug)
                {
                    QUI.Space(4);
                    QUI.BeginVertical(14, 20);
                    {
                        QUI.Space(2);
                        QUI.DrawTexture(iconColor == IconColor.Green ? QResources.iconDebugGreen.texture : QResources.iconDebugBlue.texture, 20, 20);
                        QUI.Space(5);
                    }
                    QUI.EndVertical();
                }
                if(alignCenter) { QUI.FlexibleSpace(); }
                else { QUI.Space(4); }
            }
            QUI.EndHorizontal();
        }

        void DrawItems()
        {
            float fullWidth = WIDTH_420;
            float indentWidth = WIDTH_420 - INDENT_24;
            foreach(var category in categoryItem.Keys)
            {
                if(QUI.GhostBar(category, QColors.Color.Gray, categoryExpanded[category], WIDTH_420, 24))
                {
                    categoryExpanded[category].target = !categoryExpanded[category].target;
                    if(categoryExpanded[category].target == false)
                    {
                        CloseCategoryItems(category, itemExpanded); //if the category is closed, we close all of it's items
                        if(!string.IsNullOrEmpty(renameCategory) && category.Equals(renameCategory)) //if the dev closed this category and it was in rename mode, we get it out of that mode
                        {
                            renameCategory = string.Empty;
                            newCategoryName = string.Empty;
                        }
                    }
                }
                if(QUI.BeginFadeGroup(categoryExpanded[category].faded))
                {
                    QUI.Space(SPACE_2);
                    DrawCategoryOptionsButtons(category);
                    QUI.Space(SPACE_8);
                    QUI.BeginHorizontal(fullWidth);
                    {
                        QUI.Space(SPACE_16);
                        QUI.BeginVertical();
                        {
                            foreach(var item in categoryItem[category])
                            {
                                DrawItem(item, indentWidth);
                                QUI.Space(SPACE_4);
                            }
                        }
                        QUI.EndVertical();
                    }
                    QUI.EndHorizontal();
                    QUI.Space(SPACE_16);
                }
                QUI.EndFadeGroup();
                QUI.Space(SPACE_4);
            }
        }

        void DrawItem(Pooly.Item item, float width)
        {
            if(QUI.GhostBar((item.prefab == null) ? "Missing Prefab" : item.prefab.name,
                            (item.prefab == null) ? QColors.Color.Red : QColors.Color.Gray,
                            itemExpanded[item],
                            width - SPACE_8 - SPACE_4,
                            22))
            {
                itemExpanded[item].target = !itemExpanded[item].target;
            }

            QUI.Space(-20);

            QUI.BeginHorizontal(width);
            {
                QUI.Space(width - SPACE_8);
                if(QUI.ButtonCancel())
                {
                    DeleteItem(item);
                }
            }
            QUI.EndHorizontal();

            if(item.prefab != null)
            {
                QUI.Space(-21);
                DrawItemQuickViewInfo(item, width - 16, ItemQuickViewTextStyle, IconColor.Green);
            }

            if(QUI.BeginFadeGroup(itemExpanded[item].faded))
            {
                QUI.Space(item.prefab == null ? SPACE_8 : SPACE_2);

                QUI.BeginHorizontal(width);
                {
                    QUI.Space(SPACE_4);
                    if(QUI.GhostButton("", QColors.Color.Gray, 40, 40 * itemExpanded[item].faded))
                    {
                        if(item.prefab != null) { EditorGUIUtility.PingObject(item.prefab.gameObject); }
                        else { Debug.Log("[Pooly] Missing prefab. Please link a prefab to the pool object."); }
                    }
                }
                QUI.EndHorizontal();

                QUI.Space(-38);

                QUI.BeginHorizontal(width);
                {
                    QUI.Space(6);
                    if(item.prefab != null)
                    {
                        Texture2D assetPreview = AssetPreview.GetAssetPreview(item.prefab.gameObject) as Texture2D;
                        if(assetPreview == null)
                        {
                            QUI.Box(new GUIContent("Find"), QStyles.GetStyle(Style.SlicedButton.BlueSelected), 36, 36 * itemExpanded[item].faded);
                        }
                        else
                        {
                            QUI.Box(new GUIContent(assetPreview), QStyles.GetStyle(Style.GhostButton.Gray), 36, 36 * itemExpanded[item].faded);
                        }
                    }
                    else
                    {
                        QUI.Box(new GUIContent("---"), QStyles.GetStyle(Style.SlicedButton.RedSelected), 36, 36 * itemExpanded[item].faded);
                    }
                }
                QUI.EndHorizontal();

                QUI.Space(-43);

                QUI.BeginHorizontal(width);
                {
                    QUI.Space(47);
                    QUI.BeginVertical(width);
                    {
                        QUI.BeginHorizontal(width);
                        {
                            QUI.Label("Number of Clones", Style.Text.Normal, 108);
                            QUI.Space(18);
                            if(item.prefab == null) { QUI.SetGUIBackgroundColor(QUI.IsProSkin ? QColors.Red.Color : QColors.RedLight.Color); }
                            if(item.prefab != null || (item.prefab == null && itemExpanded[item].faded > 0.1f))
                            {
                                item.prefab = EditorGUILayout.ObjectField(GUIContent.none, item.prefab, typeof(Transform), false, GUILayout.Width(width - SPACE_16 - 108 - 72)) as Transform;
                            }
                            if(item.prefab == null) { QUI.ResetColors(); }
                            QUI.FlexibleSpace();
                        }
                        QUI.EndHorizontal();
                        QUI.BeginHorizontal(width);
                        {
                            QUI.Space(SPACE_4);
                            QUI.BeginVertical(14, QUI.SingleLineHeight);
                            {
                                QUI.FlexibleSpace();
                                QUI.DrawTexture(QResources.iconNumberGreen.texture, 14, 14);
                            }
                            QUI.EndVertical();
                            item.preloadCloneCount = EditorGUILayout.IntField(item.preloadCloneCount, GUILayout.Width(86));
                            if(item.preloadCloneCount < 1) { item.preloadCloneCount = 1; }
                            QUI.Space(22);
                            if(changeCategoryItem != item)
                            {
                                if(QUI.GhostButton("Change Category", QColors.Color.Gray, 202, 16 * itemExpanded[item].faded))
                                {
                                    changeCategoryItem = item;
                                    changeCategoryItemIndex = categoriesList.FindIndex(x => x.Equals(item.category));
                                    newCategoryName = string.Empty;
                                }
                            }
                            else
                            {

                                if(categoriesList[changeCategoryItemIndex].Equals(NEW_CATEGORY))
                                {
                                    GUI.SetNextControlName("changeCategoryNewCategory");
                                    changeCategoryNewCategory = QUI.TextField(changeCategoryNewCategory, Color.white, 164);
                                }
                                else
                                {
                                    GUI.SetNextControlName("changeCategoryItemIndex");
                                    changeCategoryItemIndex = EditorGUILayout.Popup(changeCategoryItemIndex, categoriesList.ToArray(), GUILayout.Width(164));
                                }

                                if(QUI.ButtonCancel() ||
                                   (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp))
                                {
                                    if(categoriesList[changeCategoryItemIndex].Equals(NEW_CATEGORY))
                                    {
                                        changeCategoryItemIndex = categoriesList.FindIndex(x => x.Equals(item.category));
                                    }
                                    else
                                    {
                                        changeCategoryItem = null;
                                    }
                                }
                                QUI.Space(SPACE_2);
                                if(QUI.ButtonOk() ||
                                   (Event.current.isKey && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && (GUI.GetNameOfFocusedControl() == "changeCategoryNewCategory" || GUI.GetNameOfFocusedControl() == "changeCategoryItemIndex")))
                                {
                                    ChangeItemCategory(item);
                                }
                            }
                        }
                        QUI.EndHorizontal();
                    }
                    QUI.EndVertical();
                }
                QUI.EndHorizontal();

                QUI.Space(12);
                DrawItemSettings(item, width, itemExpanded[item], IconColor.Green, item.prefab == null ? (QUI.IsProSkin ? QColors.Red.Color : QColors.RedLight.Color) : Color.white);
                QUI.Space(SPACE_4);
            }
            QUI.EndFadeGroup();

            if(item.prefab == null && itemExpanded[item].faded < 0.4f)
            {
                QUI.SetGUIBackgroundColor(QUI.IsProSkin ? QColors.Red.Color : QColors.RedLight.Color);
                QUI.Space(-QUI.SingleLineHeight - 3 - 16 * itemExpanded[item].faded);
                QUI.BeginHorizontal(width - SPACE_2);
                {
                    QUI.FlexibleSpace();
                    item.prefab = EditorGUILayout.ObjectField(GUIContent.none, item.prefab, typeof(Transform), false, GUILayout.Width(202)) as Transform;
                    QUI.Space(12 * (1 - itemExpanded[item].faded));
                    QUI.Space(8 * itemExpanded[item].faded);
                }
                QUI.EndHorizontal();
                QUI.ResetColors();
                QUI.Space(SPACE_4);
            }
            QUI.ResetColors();
        }

        void DrawCategoryOptionsButtons(string category)
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Space(SPACE_8);
                if(string.IsNullOrEmpty(renameCategory) || !category.Equals(renameCategory)) //if the renameCategory is empty or if this is not the category we are renaming, we show the category option buttons
                {
                    if(QUI.GhostButton("Add Item", QColors.Color.Gray, (WIDTH_420 - SPACE_8 - SPACE_2 - SPACE_2) / 3, 20 * categoryExpanded[category].faded))
                    {
                        AddItemToCategory(category, null);
                    }
                    QUI.Space(SPACE_2);
                    if(QUI.GhostButton("Rename Category", QColors.Color.Gray, (WIDTH_420 - SPACE_8 - SPACE_2 - SPACE_2) / 3, 20 * categoryExpanded[category].faded))
                    {
                        renameCategory = category;
                        newCategoryName = category;
                        renameCategoryAnimBool.target = true;
                    }
                    QUI.Space(SPACE_2);
                    if(QUI.GhostButton("Delete Category", QColors.Color.Gray, (WIDTH_420 - SPACE_8 - SPACE_2 - SPACE_2) / 3, 20 * categoryExpanded[category].faded))
                    {
                        if(QUI.DisplayDialog("Delete Category",
                                                       "Are you sure you want to delete the '" + category + "' Category? This will delete all the items inside of it as well." +
                                                       "\n\n" +
                                                       "Operation cannot be undone!",
                                                       "Ok",
                                                       "Cancel"))
                        {
                            for(int i = pooly.items.Count - 1; i >= 0; i--) { if(pooly.items[i].category.Equals(category)) { pooly.items.RemoveAt(i); } }
                            if(!category.Equals(Pooly.DEFAULT_CATEGORY_NAME)) { categoriesList.Remove(category); }
                            categories.Remove(category);
                            newItemCategoryListIndex = categoriesList.FindIndex(x => x.Equals(Pooly.DEFAULT_CATEGORY_NAME));
                            categoryItem.Remove(category);
                            categoryExpanded.Remove(category);
                            categoryExpandedBeforeUpdate = categoryExpanded;
                            itemExpandedBeforeUpdate = itemExpanded;
                            UpdateCategoriesAndItems();
                            QUI.ExitGUI();
                        }
                    }
                }
                else //this is the cateogry we are currently renaming so we draw the rename category zone
                {
                    DrawRenameCategoryZone();
                }
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
            if(renameCategoryAnimBool.target && category.Equals(renameCategory))
            {
                QUI.Space(SPACE_4);
            }
        }

        void DrawRenameCategoryZone()
        {
            GUI.SetNextControlName("newCategoryName");
            newCategoryName = QUI.TextField(newCategoryName, QUI.IsProSkin ? QColors.Orange.Color : QColors.OrangeLight.Color, (WIDTH_420 - SPACE_8 - SPACE_8 - SPACE_16 * 2) * renameCategoryAnimBool.faded);
            if(QUI.ButtonCancel() ||
               (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp))
            {
                renameCategory = string.Empty;
                newCategoryName = string.Empty;
                renameCategoryAnimBool.target = false;
                QUI.ExitGUI();
            }
            QUI.Space(SPACE_2);
            if(QUI.ButtonOk() ||
               (Event.current.isKey && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && GUI.GetNameOfFocusedControl() == "newCategoryName"))
            {
                RenameCategory();
            }
        }

        bool GenerateSearchResults()
        {
            if(string.IsNullOrEmpty(searchPattern)) { return false; };
            searchReturnedResuls = false;
            searchResults = new List<Pooly.Item>();
            string input = string.Empty;
            for(int i = 0; i < pooly.items.Count; i++)
            {
                input = pooly.items[i].prefab != null ? pooly.items[i].prefab.name : "Missing Prefab";
                try
                {
                    if(Regex.IsMatch(input, searchPattern, RegexOptions.IgnoreCase))
                    {
                        searchResults.Add(pooly.items[i]);
                        DrawItem(pooly.items[i], WIDTH_420);
                        QUI.Space(SPACE_4);
                        searchReturnedResuls = true;
                    }
                }
                catch(Exception)
                {
                    Debug.Log("[Pooly] Invalid search pattern.");
                }
            }

            infoMessage["SearchNoResults"].show.target = !searchReturnedResuls;
            DrawInfoMessage("SearchNoResults", WIDTH_420);
            return true;
        }

        void AddItemToCategory(string category, Transform prefab)
        {
            Pooly.Item newItem = new Pooly.Item
            {
                category = category,
                prefab = prefab,
                preloadCloneCount = newItemSettings.preloadCloneCount,
                limitCloneCount = newItemSettings.limitCloneCount,
                cloneCountLimit = newItemSettings.cloneCountLimit,
                limitCloneCreationPerFrame = newItemSettings.limitCloneCreationPerFrame,
                clonesOnFirstFrame = newItemSettings.clonesOnFirstFrame,
                clonesPerFrame = newItemSettings.clonesPerFrame,
                delayCreatingClonesForFrames = newItemSettings.delayCreatingClonesForFrames,
                allowInstantiateMore = newItemSettings.allowInstantiateMore,
                allowRecycleClones = newItemSettings.allowRecycleClones,
                debug = newItemSettings.debug
            };

            if(prefab != null && ItemExistInPool(newItem, pooly.items))
            {
                QUI.DisplayDialog("Cannot add New Item to the pool!",
                                            "The '" + newItem.prefab.name + "' prefab already exists in the pool." +
                                            "\n\n" +
                                            "You cannot have different prefabs with the same name.",
                                            "Ok");
                return;
            }

            pooly.items.Add(newItem);
            CloseCategoryItems(category, itemExpanded);
            if(!categoryItem.ContainsKey(category)) { categoryItem.Add(category, new List<Pooly.Item>()); categoriesList.Add(category); categories.Add(category); }
            categoryItem[category].Add(newItem);
            if(!categoryExpanded.ContainsKey(category)) { categoryExpanded.Add(category, new AnimBool(false, Repaint)); }
            categoryExpanded[category].target = true;
            itemExpanded.Add(newItem, new AnimBool(true, Repaint));
            if(prefab == null)
            {
                itemsWithMissingPrefab.Add(newItem);
            }
        }

        void DeleteItem(Pooly.Item item)
        {
            if(categoryItem[item.category].Count == 1)
            {
                if(!QUI.DisplayDialog("Delete Item",
                                                "This was the last item of the '" + item.category + "' category. The category will get deteled as well.",
                                                "Ok",
                                                "Cancel"))
                {
                    return;
                }
            }
            categoryItem[item.category].Remove(item);
            if(categoryItem[item.category].Count == 0)
            {
                categoryItem.Remove(item.category);
                categoryExpanded.Remove(item.category);
                categories.Remove(item.category);
                if(!item.category.Equals(Pooly.DEFAULT_CATEGORY_NAME))
                {
                    categoriesList.Remove(item.category);
                }
                newItemCategoryListIndex = 1;
            }
            itemExpanded.Remove(item);
            pooly.items.Remove(item);
            QUI.ExitGUI();
        }

        void RenameCategory()
        {
            if(string.IsNullOrEmpty(renameCategory)) return;
            if(string.IsNullOrEmpty(newCategoryName.Trim()))
            {
                QUI.DisplayDialog("Rename Category",
                                            "Cannot rename the category to an empty name.",
                                            "Ok");
                return;
            }
            newCategoryName = newCategoryName.Trim();
            if(newCategoryName.Equals(NEW_CATEGORY))
            {
                QUI.DisplayDialog("Rename Category",
                                            "Cannot rename the category to '" + NEW_CATEGORY + "'.",
                                            "Ok");
                return;
            }
            if(newCategoryName.Equals(Pooly.DEFAULT_CATEGORY_NAME))
            {
                QUI.DisplayDialog("Rename Category",
                                            "Cannot rename the category to '" + Pooly.DEFAULT_CATEGORY_NAME + "'.",
                                            "Ok");
                return;
            }
            if(renameCategory.Equals(newCategoryName)) { renameCategory = string.Empty; newCategoryName = string.Empty; return; }
            if(categoriesList.Contains(newCategoryName)) //this is a merge
            {
                newItemCategoryListIndex = 1;
                if(!newCategoryName.Equals(Pooly.DEFAULT_CATEGORY_NAME))
                {
                    categoriesList.Remove(newCategoryName);
                    categories.Remove(newCategoryName);
                }
            }
            else
            {
                categoryExpanded.Add(newCategoryName, categoryExpanded[renameCategory]);
                categoryExpanded.Remove(renameCategory);
            }
            foreach(var item in pooly.items)
            {
                if(item.category.Equals(renameCategory))
                {
                    item.category = newCategoryName;
                }
            }
            categoryExpandedBeforeUpdate = categoryExpanded;
            itemExpandedBeforeUpdate = itemExpanded;
            UpdateCategoriesAndItems();
            renameCategoryAnimBool.target = false;
            QUI.ExitGUI();
        }

        void ChangeItemCategory(Pooly.Item item)
        {
            if(categoriesList[changeCategoryItemIndex].Equals(NEW_CATEGORY)) //create new 'change to' category
            {
                if(!item.category.Equals(changeCategoryNewCategory))
                {
                    if(string.IsNullOrEmpty(changeCategoryNewCategory.Trim()))
                    {
                        QUI.DisplayDialog("Change Item Category",
                                                    "Cannot change the item's category to a category with no name.",
                                                    "Ok");
                    }
                    else if(changeCategoryNewCategory.Trim().Equals(NEW_CATEGORY))
                    {
                        QUI.DisplayDialog("Chage Item Category",
                                                    "Cannot create a new category named '" + NEW_CATEGORY + "'.",
                                                    "Ok");
                    }
                    else if(changeCategoryNewCategory.Trim().Equals(Pooly.DEFAULT_CATEGORY_NAME))
                    {
                        QUI.DisplayDialog("Change Item Category",
                                                    "Cannot create a new category named '" + Pooly.DEFAULT_CATEGORY_NAME + "'.",
                                                    "Ok");
                    }
                    else if(!item.category.Equals(changeCategoryNewCategory)) //check that we do not move to the same category (case where we do nothing)
                    {
                        if(!categoriesList.Contains(changeCategoryNewCategory)) //this is a new category name
                        {
                            categoriesList.Add(changeCategoryNewCategory);
                            categories.Add(changeCategoryNewCategory);
                            categoryItem.Add(changeCategoryNewCategory, new List<Pooly.Item> { item });
                            categoryExpanded.Add(changeCategoryNewCategory, new AnimBool(false, Repaint));
                        }
                        else
                        {
                            categoryItem[changeCategoryNewCategory].Add(item);
                        }
                        categoryExpanded[changeCategoryNewCategory].target = true;
                        categoryItem[item.category].Remove(item);
                        if(categoryItem[item.category].Count == 0) //was this the last item in the category
                        {
                            categoryItem.Remove(item.category);
                            categoryExpanded.Remove(item.category);
                            categories.Remove(item.category);
                        }
                        item.category = changeCategoryNewCategory;
                    }
                }
            }
            else if(!item.category.Equals(categoriesList[changeCategoryItemIndex])) //check that we do not move to the same category (case where we do nothing)
            {
                if(!categoryItem.ContainsKey(categoriesList[changeCategoryItemIndex]))
                {
                    categoryItem.Add(categoriesList[changeCategoryItemIndex], new List<Pooly.Item> { item });
                    categoryExpanded.Add(categoriesList[changeCategoryItemIndex], new AnimBool(false, Repaint));
                }
                else
                {
                    categoryItem[categoriesList[changeCategoryItemIndex]].Add(item);
                }
                categoryExpanded[categoriesList[changeCategoryItemIndex]].target = true;
                categoryItem[item.category].Remove(item);
                if(categoryItem[item.category].Count == 0) //was this the last item in the category
                {
                    categoryItem.Remove(item.category);
                    categoryExpanded.Remove(item.category);
                }
                item.category = categoriesList[changeCategoryItemIndex];
            }
            changeCategoryItem = null;
            categoryExpandedBeforeUpdate = categoryExpanded;
            itemExpandedBeforeUpdate = itemExpanded;
            UpdateCategoriesAndItems();
            QUI.ExitGUI();
        }

        /// <summary>
        /// Closes all the items of a given category
        /// </summary>
        public static void CloseCategoryItems(string category, Dictionary<Pooly.Item, AnimBool> itemExpanded)
        {
            if(itemExpanded == null || itemExpanded.Count == 0) { return; }
            foreach(var item in itemExpanded) { if(item.Key.category.Equals(category)) { item.Value.target = false; } }
        }

        public static List<Pooly.Item> RemoveDuplicateItems(List<Pooly.Item> pool)
        {
            if(pool != null && pool.Count > 0)
            {
                List<Pooly.Item> tempList = new List<Pooly.Item>();
                bool addItemToTempList = false;
                for(int i = 0; i < pool.Count; i++)
                {
                    if(pool[i].prefab != null && tempList.Count > 0)
                    {
                        addItemToTempList = true;
                        for(int j = 0; j < tempList.Count; j++)
                        {
                            if(tempList[j].prefab != null && tempList[j].prefab.name.Equals(pool[i].prefab.name))
                            {
                                addItemToTempList = false;
                            }
                        }
                        if(addItemToTempList)
                        {
                            tempList.Add(pool[i]);
                        }
                    }
                    else
                    {
                        tempList.Add(pool[i]);
                    }
                }
                return tempList;
            }
            return new List<Pooly.Item>();
        }

        public static bool ItemExistInPool(Pooly.Item item, List<Pooly.Item> pool)
        {
            if(item == null || item.prefab == null) return false;
            if(pool == null) return false;
            if(pool.Count == 0) return false;
            for(int i = 0; i < pool.Count; i++)
            {
                if(pool[i].prefab != null && pool[i].prefab.name.Equals(item.prefab.name))
                {
                    return true;
                }
            }
            return false;
        }

        public static string[] GetPooledItemsCategories()
        {
            List<string> cat = new List<string>();
            if(Pooly.Pool != null && Pooly.Pool.Count > 0)
            {
                foreach(var pItem in Pooly.Pool.Values) { if(!cat.Contains(pItem.category)) { cat.Add(pItem.category); } }
                return cat.ToArray();
            }
            return null;
        }

        void CheckItemsWithMissingPrefabs()
        {
            if(Event.current.type == EventType.Layout) return;
            if(itemsWithMissingPrefab.Count == 0) { return; }
            for(int i = itemsWithMissingPrefab.Count - 1; i >= 0; i--)
            {
                if(itemsWithMissingPrefab[i].prefab != null) //the dev added a prefab by drag and drop
                {
                    if(PrefabExistsInPool(itemsWithMissingPrefab[i].prefab, pooly.items))
                    {
                        QUI.DisplayDialog("Duplicate prefab found in the pool!",
                                                    "The '" + itemsWithMissingPrefab[i].prefab.name + "' prefab already exists in the pool." +
                                                    "\n\n" +
                                                    "You cannot have different prefabs with the same name.",
                                                    "Ok");
                        itemsWithMissingPrefab[i].prefab = null;
                        QUI.ExitGUI();
                    }
                    else //no duplicate prefabs found --> we sort the pool and we remove this prefab from the missing list
                    {
                        itemsWithMissingPrefab.RemoveAt(i);
                        SortItemsListByCategory();
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if two items have the same prefab reference.
        /// </summary>
        public static bool PrefabExistsInPool(Transform transform, List<Pooly.Item> pool)
        {
            if(transform == null) return false;
            if(pool == null) return false;
            if(pool.Count == 0) return false;
            int timesFoundInThePool = 0;
            for(int i = 0; i < pool.Count; i++)
            {
                if(pool[i].prefab != null && pool[i].prefab.name.Equals(transform.name))
                {
                    timesFoundInThePool++;
                }
            }
            return timesFoundInThePool > 1;
        }

        bool AreAllCategoriesClosed()
        {
            if(categoryExpanded != null && categoryExpanded.Count > 0)
            {
                foreach(var c in categoryExpanded.Keys)
                {
                    if(categoryExpanded[c].value) return false;
                }
            }
            return true;
        }
    }
}
