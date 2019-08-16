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
    [CustomEditor(typeof(PoolyExtension))]
    [DisallowMultipleComponent]
    public class PoolyExtensionEditor : QEditor
    {
        private PoolyExtension poolyExtension { get { return (PoolyExtension)target; } }

        Color AccentColorBlue { get { return QUI.IsProSkin ? QColors.Blue.Color : QColors.BlueLight.Color; } }
        GUIContent tempContent;
        Vector2 tempContentSize;

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

        string renameCategory, newCategoryName, deleteCategory, newItemCategory;

        /// <summary>
        /// List of categories used by the drop down list (contains 'New Category' and 'No Category' entries, by default)
        /// </summary>
        List<string> categoriesList = new List<string> { PoolyEditor.NEW_CATEGORY, Pooly.DEFAULT_CATEGORY_NAME };
        /// <summary>
        /// Index for the selected category in the drop down list from the categoriesList
        /// </summary>
        int newItemCategoryListIndex = 1;
        /// <summary>
        /// List of categories used by the reordable list
        /// </summary>
        List<string> categories = new List<string>();

        /// <summary>
        /// Toggle for showing the 'Settings for New Items'
        /// </summary>
        AnimBool showNewItemSettings;
        /// <summary>
        /// AnimBool for rename category slide animation.
        /// </summary>
        AnimBool renameCategoryAnimBool;
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
        /// The search pattern used by Regex to filter the Items
        /// </summary>
        string searchPattern = string.Empty;
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

        GUIStyle _dropZoneBoxStyle;
        GUIStyle DropZoneBoxStyle
        {
            get
            {
                if(_dropZoneBoxStyle == null)
                {
                    _dropZoneBoxStyle = new GUIStyle
                    {
                        normal = { background = QResources.backgroundLowGray.normal2D, textColor = QUI.IsProSkin ? QColors.BlueLight.Color : QColors.BlueDark.Color },
                        hover = { background = QResources.backgroundLowBlue.normal2D, textColor = QUI.IsProSkin ? QColors.BlueLight.Color : QColors.BlueDark.Color },
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
                        normal = { textColor = QUI.IsProSkin ? QColors.BlueLight.Color : QColors.BlueDark.Color },
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


        protected override void OnEnable()
        {
            base.OnEnable();

            requiresContantRepaint = true;
            QUI.MarkSceneDirty(true);

            UpdateCategoriesAndItems();

            tempContent = new GUIContent();
            tempContentSize = Vector2.zero;
        }

        protected override void InitAnimBools()
        {
            base.InitAnimBools();

            showNewItemSettings = new AnimBool(false, Repaint);
            renameCategoryAnimBool = new AnimBool(false, Repaint);
        }

        protected override void GenerateInfoMessages()
        {
            base.GenerateInfoMessages();

            infoMessage.Add("SearchNoResults", new InfoMessage() { title = "Your search returned no results...", message = "", show = new AnimBool(false), type = InfoMessageType.Info });
            infoMessage.Add("PoolIsEmpty", new InfoMessage() { title = "The Pool Extension is empty...", show = new AnimBool(false, Repaint), type = InfoMessageType.Info });
        }

        void UpdateCategoriesAndItems()
        {
            itemsWithMissingPrefab = new List<Pooly.Item>();
            categoryItem = new Dictionary<string, List<Pooly.Item>>();
            categoryExpanded = new Dictionary<string, AnimBool>();
            itemExpanded = new Dictionary<Pooly.Item, AnimBool>();
            categoriesList = new List<string> { PoolyEditor.NEW_CATEGORY, Pooly.DEFAULT_CATEGORY_NAME };
            categories = new List<string>();
            for(int i = 0; i < poolyExtension.items.Count; i++)
            {
                if(poolyExtension.items[i].prefab == null) { itemsWithMissingPrefab.Add(poolyExtension.items[i]); }
                poolyExtension.items[i].category = string.IsNullOrEmpty(poolyExtension.items[i].category) ? Pooly.DEFAULT_CATEGORY_NAME : poolyExtension.items[i].category;
                if(!categoryItem.ContainsKey(poolyExtension.items[i].category))
                {
                    categoryItem.Add(poolyExtension.items[i].category, new List<Pooly.Item>() { poolyExtension.items[i] });
                    if(!categoriesList.Contains(poolyExtension.items[i].category)) { categoriesList.Add(poolyExtension.items[i].category); }
                    if(!categories.Contains(poolyExtension.items[i].category)) { categories.Add(poolyExtension.items[i].category); }
                }
                else
                {
                    categoryItem[poolyExtension.items[i].category].Add(poolyExtension.items[i]);
                }

                if(!categoryExpanded.ContainsKey(poolyExtension.items[i].category))
                {
                    categoryExpanded.Add(poolyExtension.items[i].category, new AnimBool(false, Repaint));
                }
                itemExpanded.Add(poolyExtension.items[i], new AnimBool(false, Repaint));
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
                onReorderCallback = (list) =>
                {
                    ReorderCategories();
                },
                showDefaultBackground = false,
                drawElementBackgroundCallback = (rect, index, active, focused) => { },
                elementHeightCallback = (index) =>
                {
                    Repaint();
                    return 28;
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    if(index == categories.Count) { return; }
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
                for(int j = 0; j < poolyExtension.items.Count; j++)
                {
                    if(poolyExtension.items[j].category.Equals(rCategories.list[i].ToString())) { tempItemsList.Add(poolyExtension.items[j]); }
                }
                newItemsList.AddRange(tempItemsList);
                tempItemsList = new List<Pooly.Item>();
            }
            poolyExtension.items = new List<Pooly.Item>();
            poolyExtension.items = newItemsList;
            SortItemsListByCategory();
        }

        void SortItemsListByCategory()
        {
            if(poolyExtension.items == null || poolyExtension.items.Count == 0) { return; }
            List<Pooly.Item> newItemsList = new List<Pooly.Item>();
            for(int i = 0; i < categories.Count; i++)
            {
                List<Pooly.Item> tempItemsListWithPrefabs = new List<Pooly.Item>();
                List<Pooly.Item> tempItemsListWithNoPrefabs = new List<Pooly.Item>();
                for(int j = 0; j < poolyExtension.items.Count; j++)
                {
                    if(poolyExtension.items[j].category.Equals(categories[i]))
                    {
                        if(poolyExtension.items[j].prefab != null)
                        {
                            tempItemsListWithPrefabs.Add(poolyExtension.items[j]);
                        }
                        else
                        {
                            tempItemsListWithNoPrefabs.Add(poolyExtension.items[j]);
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
            poolyExtension.items = new List<Pooly.Item>();
            poolyExtension.items = newItemsList;
            categoryExpandedBeforeUpdate = categoryExpanded;
            itemExpandedBeforeUpdate = itemExpanded;
            UpdateCategoriesAndItems();
            QUI.ExitGUI();
        }

        void CheckForReset()
        {
            if(poolyExtension.items.Count == 0 && categoryItem.Count > 0) //User pressed the Reset() button
            {
                UpdateCategoriesAndItems();
                newItemSettings = new Pooly.Item();
            }
        }

        public override void OnInspectorGUI()
        {
            DrawHeader(EZResources.editorHeaderPoolyExtension.texture, WIDTH_420, HEIGHT_42);
            CheckForReset();
            QUI.Space(2);
            serializedObject.Update();
            if(EditorApplication.isPlaying)
            {
                //Runtime inspector
                DrawRuntimeEditor(WIDTH_420);
            }
            else
            {
                //Editing inspector
                PoolyEditor.DragAndDropCheck(Event.current.type);
                DrawAddMissingItemsAndDropPrefab();
                QUI.Space(SPACE_4);
                DrawNewItemArea();
                QUI.Space(SPACE_4);
                DrawSearchArea();
                infoMessage["PoolIsEmpty"].show.target = poolyExtension.items.Count == 0;
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
            infoMessage["PoolIsEmpty"].show.target = poolyExtension.Categories.Count == 0;
            DrawInfoMessage("PoolIsEmpty", width);
            if(infoMessage["PoolIsEmpty"].show.target) { return; }

            QUI.Space(SPACE_2);
            foreach(var category in poolyExtension.Categories.Keys)
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
                            foreach(var pooledItem in poolyExtension.Pool.Values)
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
                                        PoolyEditor.DrawItemQuickViewInfo(pooledItem, width - SPACE_8 - tempContentSize.x, ItemQuickViewTextStyle, PoolyEditor.IconColor.Blue, true, false);
                                        QUI.FlexibleSpace();
                                    }
                                    QUI.EndHorizontal();
                                    QUI.Space(-6);
                                    QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Blue), width - SPACE_8, 18);
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

        void DrawAddMissingItemsAndDropPrefab()
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.BeginVertical(WIDTH_420);
                {
                    QUI.Label("Drop Prefabs Here", DropZoneBoxStyle, WIDTH_420 - 1, 32);
                    Rect dropZone = GUILayoutUtility.GetLastRect();
                    if(Event.current.type == EventType.DragPerform)
                    {
                        if(dropZone.Contains(Event.current.mousePosition))
                        {
                            Event.current.Use();
                            DragAndDrop.AcceptDrag();
                            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                            string targetCategory = Pooly.DEFAULT_CATEGORY_NAME;
                            if(categoriesList[newItemCategoryListIndex].Equals(PoolyEditor.NEW_CATEGORY))
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
                if(QUI.GhostButton("Create a New Item", QColors.Color.Blue, WIDTH_140))
                {
                    if(categoriesList[newItemCategoryListIndex].Equals(PoolyEditor.NEW_CATEGORY))
                    {
                        if(string.IsNullOrEmpty(newItemCategoryName))
                        {
                            QUI.DisplayDialog("Create New Item",
                                                        "Please enter a category name to continue.",
                                                        "Ok");
                        }
                        else if(newItemCategoryName.Trim().Equals(PoolyEditor.NEW_CATEGORY))
                        {
                            QUI.DisplayDialog("Create New Item",
                                                        "You cannot create a new category named '" + PoolyEditor.NEW_CATEGORY + "'.",
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
                        if(categoriesList[newItemCategoryListIndex].Equals(PoolyEditor.NEW_CATEGORY))
                        {
                            newItemCategoryName = QUI.TextField(newItemCategoryName, AccentColorBlue, 190);
                            QUI.BeginVertical(18);
                            {
                                QUI.Space(1);
                                if(QUI.ButtonCancel() ||
                                   (Event.current.isKey && Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyUp))
                                {
                                    newItemCategoryListIndex = 1;
                                    newItemCategoryName = string.Empty;
                                }
                            }
                            QUI.EndVertical();
                        }
                        else
                        {
                            QUI.BeginVertical(212);
                            {
                                QUI.Space(SPACE_2);
                                QUI.SetGUIBackgroundColor(AccentColorBlue);
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
            QUI.Space(SPACE_4);
            QUI.Button(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), WIDTH_420, 24);
            QUI.Space(-24);
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Space(2);
                PoolyEditor.DrawItemQuickViewInfo(newItemSettings, WIDTH_420, ItemQuickViewTextStyle, PoolyEditor.IconColor.Blue, true, false);
            }
            QUI.EndHorizontal();

            QUI.Space(SPACE_2);

            QUI.BeginHorizontal(WIDTH_420);
            {
                if(QUI.GhostBar("Settings for New Items", QColors.Color.Blue, showNewItemSettings, WIDTH_420 - 100 - 4, 16))
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
            if(QUI.BeginFadeGroup(showNewItemSettings.faded))
            {
                QUI.Space(SPACE_4 * showNewItemSettings.faded);
                QUI.BeginHorizontal(WIDTH_420);
                {
                    QUI.Space(6 * showNewItemSettings.faded);
                    QUI.DrawTexture(QResources.iconNumberBlue.texture, 18, 18);
                    QUI.Space(-4);
                    QUI.Label("Number of Clones", Style.Text.Normal, 106 * showNewItemSettings.faded);
                    QUI.SetGUIBackgroundColor(AccentColorBlue);
                    newItemSettings.preloadCloneCount = EditorGUILayout.IntField(newItemSettings.preloadCloneCount, GUILayout.Width(86 * showNewItemSettings.faded));
                    if(newItemSettings.preloadCloneCount < 1) { newItemSettings.preloadCloneCount = 1; }
                    QUI.ResetColors();
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
                QUI.Space(SPACE_4);
                PoolyEditor.DrawItemSettings(newItemSettings, WIDTH_420, showNewItemSettings, PoolyEditor.IconColor.Blue, AccentColorBlue);
                QUI.Space(SPACE_4);
            }
            QUI.EndFadeGroup();
            QUI.Space(SPACE_8);
            QUI.DrawLine(QColors.Color.Gray, WIDTH_420);
            QUI.Space(SPACE_4);
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
                    foreach(var item in poolyExtension.items) //close all the items and all the categories
                    {
                        itemExpanded[item].target = false;
                        categoryExpanded[item.category].target = false;
                    }
                    foreach(var item in poolyExtension.items) //open only the ones from the search
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
            QUI.Space(SPACE_8 + SPACE_2);
            QUI.DrawLine(QColors.Color.Gray, WIDTH_420);
            QUI.Space(SPACE_4);
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
                        PoolyEditor.CloseCategoryItems(category, itemExpanded); //if the category is closed, we close all of it's items
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
                PoolyEditor.DrawItemQuickViewInfo(item, width - 16, ItemQuickViewTextStyle, PoolyEditor.IconColor.Blue);
            }

            if(QUI.BeginFadeGroup(itemExpanded[item].faded))
            {
                QUI.Space(item.prefab == null ? SPACE_8 : SPACE_2);

                QUI.BeginHorizontal(width);
                {
                    QUI.Space(4);
                    if(QUI.GhostButton("", QColors.Color.Gray, 40, 40 * itemExpanded[item].faded))
                    {
                        if(item.prefab != null) { EditorGUIUtility.PingObject(item.prefab.gameObject); }
                        else { Debug.Log("[Pooly] Missing prefab. Please link a prefab to the pool object."); }
                    }
                }
                QUI.EndHorizontal();

                QUI.Space(-38);

                QUI.BeginHorizontal(width - 22);
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
                                item.prefab = EditorGUILayout.ObjectField(GUIContent.none, item.prefab, typeof(Transform), false, GUILayout.Width(180)) as Transform;
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
                                QUI.DrawTexture(QResources.iconNumberBlue.texture, 14, 14);
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

                                if(categoriesList[changeCategoryItemIndex].Equals(PoolyEditor.NEW_CATEGORY))
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
                                    if(categoriesList[changeCategoryItemIndex].Equals(PoolyEditor.NEW_CATEGORY))
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
                PoolyEditor.DrawItemSettings(item, width, itemExpanded[item], PoolyEditor.IconColor.Blue, item.prefab == null ? (QUI.IsProSkin ? QColors.Red.Color : QColors.RedLight.Color) : Color.white);
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
                            for(int i = poolyExtension.items.Count - 1; i >= 0; i--) { if(poolyExtension.items[i].category.Equals(category)) { poolyExtension.items.RemoveAt(i); } }
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
            for(int i = 0; i < poolyExtension.items.Count; i++)
            {
                input = poolyExtension.items[i].prefab != null ? poolyExtension.items[i].prefab.name : "Missing Prefab";
                try
                {
                    if(Regex.IsMatch(input, searchPattern, RegexOptions.IgnoreCase))
                    {
                        searchResults.Add(poolyExtension.items[i]);
                        DrawItem(poolyExtension.items[i], WIDTH_420);
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

            if(prefab != null && PoolyEditor.ItemExistInPool(newItem, poolyExtension.items))
            {
                QUI.DisplayDialog("Cannot add New Item to the pool!",
                                            "The '" + newItem.prefab.name + "' prefab already exists in the pool." +
                                            "\n\n" +
                                            "You cannot have different prefabs with the same name.",
                                            "Ok");
                return;
            }

            poolyExtension.items.Add(newItem);
            PoolyEditor.CloseCategoryItems(category, itemExpanded);
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
            poolyExtension.items.Remove(item);
            QUI.ExitGUI();
        }

        void RenameCategory()
        {
            if(string.IsNullOrEmpty(renameCategory)) return;
            if(string.IsNullOrEmpty(newCategoryName.Trim())) { QUI.DisplayDialog("Rename Category", "Cannot rename the category to an empty name.", "Ok"); return; }
            newCategoryName = newCategoryName.Trim();
            if(newCategoryName.Equals(PoolyEditor.NEW_CATEGORY)) { QUI.DisplayDialog("Rename Category", "Cannot rename the category to '" + PoolyEditor.NEW_CATEGORY + "'.", "Ok"); return; }
            if(newCategoryName.Equals(Pooly.DEFAULT_CATEGORY_NAME)) { QUI.DisplayDialog("Rename Category", "Cannot rename the category to '" + Pooly.DEFAULT_CATEGORY_NAME + "'.", "Ok"); return; }
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
            foreach(var item in poolyExtension.items)
            {
                if(item.category.Equals(renameCategory))
                {
                    item.category = newCategoryName;
                }
            }
            categoryExpandedBeforeUpdate = categoryExpanded;
            itemExpandedBeforeUpdate = itemExpanded;
            UpdateCategoriesAndItems();
            QUI.ExitGUI();
        }

        void ChangeItemCategory(Pooly.Item item)
        {
            if(categoriesList[changeCategoryItemIndex].Equals(PoolyEditor.NEW_CATEGORY)) //create new 'change to' category
            {
                if(!item.category.Equals(changeCategoryNewCategory))
                {
                    if(string.IsNullOrEmpty(changeCategoryNewCategory.Trim()))
                    {
                        QUI.DisplayDialog("Change Item Category",
                                                    "Cannot change the item's category to a category with no name.",
                                                    "Ok");
                    }
                    else if(changeCategoryNewCategory.Trim().Equals(PoolyEditor.NEW_CATEGORY))
                    {
                        QUI.DisplayDialog("Chage Item Category",
                                                    "Cannot create a new category named '" + PoolyEditor.NEW_CATEGORY + "'.",
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

        bool AreAllCategoriesClosed()
        {
            if(categoryExpanded != null && categoryExpanded.Count > 0)
            {
                foreach(var c in categoryExpanded.Keys)
                {
                    if(categoryExpanded[c].target) return false;
                }
            }
            return true;
        }

        void CheckItemsWithMissingPrefabs()
        {
            if(Event.current.type == EventType.Layout) return;
            if(itemsWithMissingPrefab.Count == 0) { return; }
            for(int i = itemsWithMissingPrefab.Count - 1; i >= 0; i--)
            {
                if(itemsWithMissingPrefab[i].prefab != null) //the dev added a prefab by drag and drop
                {
                    if(PoolyEditor.PrefabExistsInPool(itemsWithMissingPrefab[i].prefab, poolyExtension.items))
                    {
                        QUI.DisplayDialog("Duplicate prefab found in the pool!",
                                                    "The '" + itemsWithMissingPrefab[i].prefab.name + "' prefab already exists in the pool." +
                                                    "\n\n" +
                                                    "You cannot have different prefabs with the same name.",
                                                    "Ok");
                        itemsWithMissingPrefab[i].prefab = null;
                        GUIUtility.ExitGUI();
                    }
                    else //no duplicate prefabs found --> we sort the pool and we remove this prefab from the missing list
                    {
                        itemsWithMissingPrefab.RemoveAt(i);
                        SortItemsListByCategory();
                    }
                }
            }
        }
    }
}