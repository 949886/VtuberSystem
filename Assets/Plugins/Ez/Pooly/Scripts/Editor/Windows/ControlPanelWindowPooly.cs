// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using Ez.Pooly;
using Ez.Pooly.Statistics;
using QuickEditor;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using QuickEngine.Core;

namespace Ez
{
    public partial class ControlPanelWindow : QWindow
    {
#if EZ_POOLY
        private static PoolySettings _poolySettings;
        public static PoolySettings PoolySettings
        {
            get
            {
                if(_poolySettings == null)
                {
                    _poolySettings = Q.GetResource<PoolySettings>(EZT.RESOURCES_PATH_POOLY_SETTINGS, "PoolySettings");
                    if(_poolySettings == null)
                    {
                        _poolySettings = Q.CreateAsset<PoolySettings>(EZT.RELATIVE_PATH_POOLY_SETTINGS, "PoolySettings");
                    }
                }
                return _poolySettings;
            }
        }

        private static PoolyStatistics _poolyStatistics;
        public static PoolyStatistics PS
        {
            get
            {
                if(_poolyStatistics == null)
                {
                    _poolyStatistics = Q.GetResource<PoolyStatistics>(EZT.RESOURCES_PATH_POOLY_STATISTICS, "PoolyStatistics");
                    if(_poolyStatistics == null)
                    {
                        _poolyStatistics = Q.CreateAsset<PoolyStatistics>(EZT.RELATIVE_PATH_POOLY_STATISTICS, "PoolyStatistics");
                    }
                }
                return _poolyStatistics;
            }
        }

        private static Pooly.Pooly _pooly;
        public static Pooly.Pooly Pooly
        {
            get
            {
                if(_pooly == null)
                {
                    _pooly = FindObjectOfType<Pooly.Pooly>();
                }
                return _pooly;
            }
        }

        private static PoolyExtension _poolyExtension;
        public static PoolyExtension PoolyExtension
        {
            get
            {
                if(_poolyExtension == null)
                {
                    _poolyExtension = FindObjectOfType<PoolyExtension>();
                }
                return _poolyExtension;
            }
        }

        AnimBool editStats;

        GUIStyle graphNumberStyle, itemDetailsStyle, miniTitleStyle, miniSubtitleStyle;

        QLabel poolyQLabel;
        GUIContent poolyTempContent;
        Vector2 poolyTempContentSize;

        void GeneratePoolyInfoMessages()
        {
            if(infoMessage == null) { infoMessage = new Dictionary<string, InfoMessage>(); }
            if(!infoMessage.ContainsKey("PoolyNoStatistics")) { infoMessage.Add("PoolyNoStatistics", new InfoMessage() { title = "No statistics have been registered yet...", message = "Please enter runtime at least once and spawn something...", show = new AnimBool(true), type = InfoMessageType.Info }); }
            if(!infoMessage.ContainsKey("PoolyEmptyPool")) { infoMessage.Add("PoolyEmptyPool", new InfoMessage() { title = "Empty pool...", message = "", show = new AnimBool(true), type = InfoMessageType.Info }); }
            if(!infoMessage.ContainsKey("PoolyEmptyCategory")) { infoMessage.Add("PoolyEmptyCategory", new InfoMessage() { title = "Empty category...", message = "", show = new AnimBool(true), type = InfoMessageType.Info }); }
            if(!infoMessage.ContainsKey("PoolyEmptyItem")) { infoMessage.Add("PoolyEmptyItem", new InfoMessage() { title = "No statistics have been registered for this prefab...", message = "", show = new AnimBool(true), type = InfoMessageType.Info }); }
        }

        void CreateCustomStylesForPooly()
        {
            if(Event.current == null || Event.current.type == EventType.Layout)
            {
                return;
            }
            //if(EditorApplication.isPlayingOrWillChangePlaymode)
            //{
            //    return;
            //}
            graphNumberStyle = new GUIStyle(QStyles.GetStyle(Style.Text.Small))
            {
                normal = { textColor = QUI.IsProSkin ? QColors.UnityLight.Color : QColors.UnityDark.Color },
                alignment = TextAnchor.MiddleCenter
            };

            itemDetailsStyle = new GUIStyle(QStyles.GetStyle(Style.Text.Normal))
            {
                normal = { textColor = QUI.IsProSkin ? QColors.UnityLight.Color : QColors.UnityDark.Color },
                alignment = TextAnchor.MiddleLeft,
                fontSize = 18
            };

            miniTitleStyle = new GUIStyle(QStyles.GetStyle(Style.Text.Subtitle))
            {
                normal = { textColor = QUI.IsProSkin ? QColors.UnityLight.Color : QColors.UnityDark.Color },
                alignment = TextAnchor.MiddleLeft,
                fontSize = 10
            };

            miniSubtitleStyle = new GUIStyle(QStyles.GetStyle(Style.Text.Subtitle))
            {
                normal = { textColor = QUI.IsProSkin ? QColors.UnityLight.Color : QColors.UnityDark.Color },
                alignment = TextAnchor.MiddleLeft,
                fontSize = 7
            };
        }

        void InitPooly()
        {
            CreateCustomStylesForPooly();
            GeneratePoolyInfoMessages();

            editStats = new AnimBool(false, Repaint);
            poolyTempContent = new GUIContent();
            poolyTempContentSize = Vector2.zero;
        }

        void PoolyOnDisable()
        {

        }
        void PoolyOnFocus()
        {

        }
        void PoolyOnLostFocus()
        {

        }

        void DrawPooly()
        {
            poolyQLabel = new QLabel();

            DrawPageHeader("POOLY", QColors.Green, "Professional Pooling System", QUI.IsProSkin ? QColors.UnityLight : QColors.UnityMild, EZResources.IconPooly);

            QUI.Space(6);

            DrawPoolyAddRemoveButtons(WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth);

            QUI.Space(SPACE_16);

            DrawPoolyStatistics(WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth);
        }

        void DrawPoolyAddRemoveButtons(float width)
        {
            QUI.BeginHorizontal(width);
            {
                QUI.FlexibleSpace();
                DrawPoolyTabButtonAddPooly((width - SPACE_8) / 2, 20);
                DrawPoolyTabButtonAddPoolyExtension((width - SPACE_8) / 2, 20);
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
        }
        void DrawPoolyTabButtonAddPooly(float buttonWidth, float buttonHeight)
        {
            if(Pooly == null)
            {
                if(QUI.SlicedButton("Add Pooly to Scene", QColors.Color.Green, buttonWidth, buttonHeight))
                {
                    Undo.RegisterCreatedObjectUndo(new GameObject("Pooly", typeof(Pooly.Pooly)), "AddPoolyToScene");
                    Selection.activeObject = Pooly.gameObject;
                }
            }
            else
            {
                if(QUI.SlicedButton("Remove Pooly from Scene", QColors.Color.Red, buttonWidth, buttonHeight))
                {
                    if(QUI.DisplayDialog("Remove Pooly",
                                                   "Are you sure you want to remove (delete) the Pooly gameObject from the current scene?" +
                                                   "\n\n\n" +
                                                   "You will lose all the references and values you set in the inspector.",
                                                   "Ok",
                                                   "Cancel"))
                    {
                        Undo.DestroyObjectImmediate(Pooly.gameObject);
                    }
                }
            }
        }
        void DrawPoolyTabButtonAddPoolyExtension(float buttonWidth, float buttonHeight)
        {
            if(PoolyExtension == null)
            {
                if(QUI.SlicedButton("Add PoolyExtension to Scene", QColors.Color.Blue, buttonWidth, buttonHeight))
                {
                    Undo.RegisterCreatedObjectUndo(new GameObject("PoolyExtension", typeof(PoolyExtension)), "AddPoolyExtensionToScene");
                    Selection.activeObject = PoolyExtension.gameObject;
                }
            }
            else
            {
                if(QUI.SlicedButton("Remove PoolyExtension from Scene", QColors.Color.Red, buttonWidth, buttonHeight))
                {
                    if(QUI.DisplayDialog("Remove PoolyExtension",
                                                   "Are you sure you want to remove (delete) the PoolyExtension gameObject from the current scene?" +
                                                   "\n\n\n" +
                                                   "You will lose all the references and values you set in the inspector.",
                                                   "Ok",
                                                   "Cancel"))
                    {
                        Undo.DestroyObjectImmediate(PoolyExtension.gameObject);
                    }
                }
            }
        }

        void DrawPoolyStatistics(float width)
        {
            DrawPoolyStatisticsOptions(width);  //Draw Options
            QUI.Space(SPACE_4);
            if(graphNumberStyle == null || itemDetailsStyle == null || miniTitleStyle == null || miniSubtitleStyle == null)
            {
                CreateCustomStylesForPooly();
                return;
            }
            DrawPoolyStatisticsDatabase(width); //Draw Database
        }

        void DrawPoolyStatisticsOptions(float width)
        {
            QUI.BeginHorizontal(width);
            {
                QUI.Space(SPACE_8);
                QUI.BeginChangeCheck();
                PoolySettings.enableStatistics = QUI.Toggle(PoolySettings.enableStatistics);
                if(QUI.EndChangeCheck())
                {
                    QUI.SetDirty(PoolySettings);
                    QUI.SetDirty(WindowSettings);
                    AssetDatabase.SaveAssets();
                    WindowSettings.pageWidthExtraSpace.target = Ez.Pooly.Pooly.PoolySettings.enableStatistics ? Ez.Pooly.Statistics.PoolyStatistics.Instance.poolyWidth : 0;
                }
                QUI.Space(SPACE_2);
                poolyQLabel.text = "Enable Statistics";
                poolyQLabel.style = Style.Text.Normal;
                QUI.Label(poolyQLabel.text, Style.Text.Normal, poolyQLabel.x, 12);
                QUI.FlexibleSpace();

                if(PoolySettings.enableStatistics)
                {
                    if(QUI.GhostButton("Edit Stats", QColors.Color.Orange, 80, 20, editStats.target))
                    {
                        editStats.target = !editStats.target;
                    }
                    if(editStats.faded > 0.05f)
                    {
                        if(QUI.GhostButton("Clear ALL Statistics", QColors.Color.Orange, PS.databaseClearStatisticsButtonWidth * editStats.faded, 20))
                        {
                            if(QUI.DisplayDialog("Clear ALL Statistics", "Are you sure you want to clear ALL the recorded statistics?\n\nOperation cannot be undone!", "Yes", "No"))
                            {

                                if(PS.pools == null) { PS.pools = new List<PoolyStatistics.StatisticsPool>(); }
                                if(PS.pools.Count == 0) { return; }
                                for(int poolIndex = 0; poolIndex < PS.pools.Count; poolIndex++) { PS.pools[poolIndex].ClearStatistics(); }
                                QUI.SetDirty(PS);
                                AssetDatabase.SaveAssets();
                            }
                        }
                        if(QUI.GhostButton("Delete ALL", QColors.Color.Orange, PS.databaseDeleteButtonWidth * editStats.faded, 20))
                        {
                            if(QUI.DisplayDialog("Delete ALL Statistics", "Are you sure you want to delete ALL DATA from the statistics database?\n\nOperation cannot be undone!", "Yes", "No"))
                            {
                                if(PS.pools == null) { PS.pools = new List<PoolyStatistics.StatisticsPool>(); }
                                if(PS.pools.Count == 0) { return; }
                                PS.pools.Clear();
                                QUI.SetDirty(PS);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }
                }
            }
            QUI.EndHorizontal();

            if(!PoolySettings.enableStatistics)
            {
                QUI.Space((position.height - WindowSettings.pageHeaderHeight - 200) / 2);
                QUI.BeginHorizontal(width, 115);
                {
                    QUI.Space((width - 300) / 2);
                    QUI.DrawTexture(EZResources.imagePoolyStatisticsAreDisabled.texture);
                    QUI.Space((width - 300) / 2);
                }
                QUI.EndHorizontal();
            }
        }

        void DrawPoolyStatisticsDatabase(float width)
        {
            if(!PoolySettings.enableStatistics) { return; }

            if(PS.pools == null) { PS.pools = new List<PoolyStatistics.StatisticsPool>(); }

            if(PS.pools.Count == 0)
            {
                QUI.BeginHorizontal(width);
                {
                    QUI.Space(SPACE_8);
                    DrawInfoMessage("PoolyNoStatistics", width - SPACE_8);
                }
                QUI.EndHorizontal();
                return;
            }

            for(int poolIndex = 0; poolIndex < PS.pools.Count; poolIndex++)
            {
                DrawPoolyStatisticsDatabasePool(PS.pools[poolIndex], width);
                QUI.Space(PS.databaseLineVerticalSpacing * 2);
            }
        }
        void DrawPoolyStatisticsDatabasePool(PoolyStatistics.StatisticsPool pool, float width)
        {
            QUI.BeginHorizontal(width);
            {
                if(QUI.GhostBar((pool.poolType == PoolyStatistics.PoolType.MainPool ? "MAIN POOL" : "POOL EXTENSION") + "    /    scene: " + pool.sceneName,
                                 pool.poolType == PoolyStatistics.PoolType.MainPool ? QColors.Color.Green : QColors.Color.Blue,
                                 pool.isExpanded,
                                 (width - (PS.databaseClearStatisticsButtonWidth + PS.databaseDeleteButtonWidth) * editStats.faded),
                                 24))
                {
                    pool.isExpanded.target = !pool.isExpanded.target;
                    if(!pool.isExpanded.target)
                    {
                        pool.ClosePool();
                        QUI.SetDirty(PS);
                        AssetDatabase.SaveAssets();
                    }
                }
                if(editStats.target)
                {

                    if(QUI.GhostButton("Clear Pool Statistics", pool.poolType == PoolyStatistics.PoolType.MainPool ? QColors.Color.Green : QColors.Color.Blue, PS.databaseClearStatisticsButtonWidth * editStats.faded, 24, pool.isExpanded.value))
                    {
                        if(QUI.DisplayDialog("Clear Pool Statistics", "Are you sure you want to clear all the recorded statistics for this pool (" + (pool.poolType == PoolyStatistics.PoolType.MainPool ? "MAIN POOL" : "POOL EXTENSION") + " / scene: " + pool.sceneName + ")?\n\nOperation cannot be undone!", "Yes", "No"))
                        {
                            pool.ClearStatistics();
                            QUI.SetDirty(PS);
                            AssetDatabase.SaveAssets();
                        }
                    }
                    if(QUI.GhostButton("Delete Pool", pool.poolType == PoolyStatistics.PoolType.MainPool ? QColors.Color.Green : QColors.Color.Blue, PS.databaseDeleteButtonWidth * editStats.faded, 24, pool.isExpanded.value))
                    {
                        if(QUI.DisplayDialog("Delete Pool Statistics", "Are you sure you want to delete this pool (" + (pool.poolType == PoolyStatistics.PoolType.MainPool ? "MAIN POOL" : "POOL EXTENSION") + " / scene: " + pool.sceneName + ") from the statistics database?\n\nOperation cannot be undone!", "Yes", "No"))
                        {
                            PS.DeletePool(pool);
                            QUI.SetDirty(PS);
                            AssetDatabase.SaveAssets();
                        }

                    }
                }
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();

            if(QUI.BeginFadeGroup(pool.isExpanded.faded))
            {
                QUI.BeginVertical(width);
                {
                    if(pool.categories == null || pool.categories.Count == 0)
                    {
                        DrawInfoMessage("PoolyEmptyPool", width);
                    }
                    else
                    {
                        for(int categoryIndex = 0; categoryIndex < pool.categories.Count; categoryIndex++)
                        {
                            QUI.Space(PS.databaseLineVerticalSpacing);
                            QUI.BeginHorizontal(width);
                            {
                                QUI.Space(PS.databaseLineHorizontalIndent * pool.isExpanded.faded);
                                DrawPoolyStatisticsDatabasePoolCategory(pool, pool.categories[categoryIndex], width - PS.databaseLineHorizontalIndent);
                            }
                            QUI.EndHorizontal();
                        }
                    }
                }
                QUI.EndVertical();
            }
            QUI.EndFadeGroup();
        }
        void DrawPoolyStatisticsDatabasePoolCategory(PoolyStatistics.StatisticsPool pool, PoolyStatistics.StatisticsPoolCategory category, float width)
        {
            QUI.BeginVertical(width);
            {
                QUI.BeginHorizontal(width);
                {
                    if(QUI.GhostBar(category.categoryName,
                                    QColors.Color.Gray,
                                    category.isExpanded,
                                    width - (PS.databaseClearStatisticsButtonWidth + PS.databaseDeleteButtonWidth) * editStats.faded,
                                    20))
                    {
                        category.isExpanded.target = !category.isExpanded.target;
                        if(!category.isExpanded.target)
                        {
                            category.CloseCategory();
                            QUI.SetDirty(PS);
                            AssetDatabase.SaveAssets();
                        }
                    }

                    if(category.items != null && category.items.Count > 0)
                    {
                        QUI.Space(-60);
                        QUI.BeginHorizontal(60, 20);
                        {
                            QUI.FlexibleSpace();

                            category.UpdateMessageFlags();

                            if(category.hasInfoMessage)
                            {
                                QUI.Space(-SPACE_8);
                                QUI.BeginVertical(16, 20);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Space(SPACE_2);
                                    QUI.Label(QResources.iconInfo.texture, 16, 16);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();
                            }


                            if(category.hasWarningMessage)
                            {
                                QUI.Space(-SPACE_8);
                                QUI.BeginVertical(16, 20);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Space(SPACE_2);
                                    QUI.Label(QResources.iconWarning.texture, 16, 16);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();
                            }

                            if(category.hasErrorMessage)
                            {
                                QUI.Space(-SPACE_8);
                                QUI.BeginVertical(16, 20);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Space(SPACE_2);
                                    QUI.Label(QResources.iconError.texture, 16, 16);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();
                            }
                        }
                        QUI.EndHorizontal();
                    }

                    if(editStats.target)
                    {
                        if(QUI.GhostButton("Clear Category Statistics", QColors.Color.Gray, PS.databaseClearStatisticsButtonWidth * editStats.faded, 20, category.isExpanded.value))
                        {
                            if(QUI.DisplayDialog("Clear Category Statistics", "Are you sure you want to clear all the recorded statistics for this category (" + category.categoryName + ")?\n\nOperation cannot be undone!", "Yes", "No"))
                            {
                                category.ClearStatistics();
                                QUI.SetDirty(PS);
                                AssetDatabase.SaveAssets();
                            }
                        }
                        if(QUI.GhostButton("Delete Category", QColors.Color.Gray, PS.databaseDeleteButtonWidth * editStats.faded, 20, category.isExpanded.value))
                        {
                            if(QUI.DisplayDialog("Delete Category Statistics", "Are you sure you want to delete this category (" + category.categoryName + ") from the statistics database?\n\nOperation cannot be undone!", "Yes", "No"))
                            {
                                pool.DeleteCategory(category);
                                QUI.SetDirty(PS);
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();

                if(QUI.BeginFadeGroup(category.isExpanded.faded))
                {
                    QUI.BeginVertical(width);
                    {
                        if(category.items == null || category.items.Count == 0)
                        {
                            DrawInfoMessage("PoolyEmptyCategory", width);
                        }
                        else
                        {
                            QUI.Space(PS.databaseLineVerticalSpacing * 2);
                            for(int itemIndex = 0; itemIndex < category.items.Count; itemIndex++)
                            {
                                QUI.BeginHorizontal(width);
                                {
                                    QUI.Space(PS.databaseLineHorizontalIndent * category.isExpanded.faded);
                                    DrawPoolyStatisticsDatabasePoolCategoryItem(category, category.items[itemIndex], width - PS.databaseLineHorizontalIndent);
                                }
                                QUI.EndHorizontal();
                                QUI.Space(PS.databaseLineVerticalSpacing / 4);
                            }
                        }
                    }
                    QUI.EndVertical();
                }
                QUI.EndFadeGroup();
            }
            QUI.EndVertical();
        }
        void DrawPoolyStatisticsDatabasePoolCategoryItem(PoolyStatistics.StatisticsPoolCategory category, PoolyStatistics.StatisticsItem item, float width)
        {

            QUI.BeginVertical(width);
            {
                QUI.Space(4 * item.warningsEnabled.faded * item.showInfoMessage.faded);

                float backgroundHeight = PS.databaseLineHeight + 6 + PS.graphHeight * item.showGraph.faded;
                if(item.warningsEnabled.target && item.showInfoMessage.target)
                {
                    backgroundHeight += 25 * item.warningsEnabled.faded * item.showInfoMessage.faded;
                }

                QUI.BeginHorizontal(width);
                {
                    QUI.Space(-2);
                    QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), width + 2, backgroundHeight); //Draw the ITEM line background
                }
                QUI.EndHorizontal();

                QUI.Space(-backgroundHeight);

                if(item.warningsEnabled.target)
                {
                    QUI.Space(-1);
                    QUI.BeginHorizontal(width);
                    {
                        QUI.Space(-3 * item.warningsEnabled.faded);
                        DrawPoolyStatisticsDatabasePoolCategoryItemInfoMessage(item, width + 3 * item.warningsEnabled.faded);
                        QUI.FlexibleSpace();
                    }
                    QUI.EndHorizontal();
                }

                QUI.Space(SPACE_4 - (item.warningsEnabled.target ? 0 : 1));

                QUI.BeginHorizontal(width, PS.databaseLineHeight);
                {
                    QUI.Space(1);
                    if(QUI.GhostButton(item.prefabName, QColors.Color.Gray, PS.databaseItemButtonWidth, PS.databaseLineHeight))
                    {
                        if(item.prefab != null)
                        {
                            EditorGUIUtility.PingObject(item.prefab.gameObject);
                            Selection.activeGameObject = item.prefab.gameObject;
                        }
                    }

                    QUI.Space(PS.databaseElementHorizontalSpacing * 10 * category.isExpanded.faded);

                    if(item.data == null || item.data.Count == 0)
                    {
                        QUI.BeginVertical(width - PS.databaseItemButtonWidth - PS.databaseElementHorizontalSpacing * 3 - PS.databaseDeleteButtonWidth * editStats.faded, PS.databaseLineHeight);
                        {
                            QUI.FlexibleSpace();
                            QUI.Space(-SPACE_4);
                            DrawInfoMessage("PoolyEmptyItem", width - PS.databaseItemButtonWidth - PS.databaseElementHorizontalSpacing * 3 - PS.databaseDeleteButtonWidth * editStats.faded);
                            QUI.FlexibleSpace();
                        }
                        QUI.EndVertical();
                        QUI.FlexibleSpace();
                        QUI.Space(-SPACE_4);
                        if(editStats.target)
                        {
                            if(QUI.GhostButton("Delete Item", QColors.Color.Gray, (PS.databaseDeleteButtonWidth - 2) * editStats.faded, PS.databaseLineHeight))
                            {
                                if(QUI.DisplayDialog("Delete Item Statistics", "Are you sure you want to delete this item (" + item.prefabName + ") from the statistics database?\n\nOperation cannot be undone!", "Yes", "No"))
                                {
                                    category.DeleteItem(item);
                                    QUI.SetDirty(PS);
                                    AssetDatabase.SaveAssets();
                                }
                            }
                            QUI.Space(SPACE_2);
                        }
                    }
                    else
                    {
                        QUI.BeginVertical(20, PS.databaseLineHeight);
                        {
                            QUI.FlexibleSpace();
                            if(QUI.ButtonGraph(item.showGraph.value))
                            {
                                item.showGraph.target = !item.showGraph.target;
                                QUI.SetDirty(PS);
                                AssetDatabase.SaveAssets();
                            }
                            QUI.FlexibleSpace();
                        }
                        QUI.EndVertical();

                        if(item.lowWarningThreshold < 0) { item.lowWarningThreshold = 0; }
                        if(item.highWarningThreshold > 100) { item.highWarningThreshold = 100; }
                        if(item.lowWarningThreshold > item.highWarningThreshold) { item.lowWarningThreshold = item.highWarningThreshold; }
                        else if(item.highWarningThreshold < item.lowWarningThreshold) { item.highWarningThreshold = item.lowWarningThreshold; }

                        QUI.BeginVertical(width - PS.databaseItemButtonWidth - PS.databaseElementHorizontalSpacing - 20 - PS.databaseElementHorizontalSpacing - 4, PS.databaseLineHeight);
                        {
                            QUI.Space(-4);
                            QUI.BeginHorizontal();
                            {
                                QUI.Space(PS.databaseElementHorizontalSpacing * 8 * category.isExpanded.faded);

                                poolyTempContent.text = "PRELOADED CLONE COUNT";
                                poolyTempContentSize = miniTitleStyle.CalcSize(poolyTempContent);
                                QUI.BeginVertical(poolyTempContentSize.x, PS.databaseLineHeight);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Label(poolyTempContent.text, miniTitleStyle, poolyTempContentSize.x, 14);
                                    QUI.Space(-7);
                                    QUI.Label("LAST RECORDED", miniSubtitleStyle, poolyTempContentSize.x, 12);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();

                                poolyTempContent.text = item.LastRecordedPreloadedClones.ToString();
                                poolyTempContentSize = itemDetailsStyle.CalcSize(poolyTempContent);
                                QUI.BeginVertical(poolyTempContentSize.x, PS.databaseLineHeight);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Space(1);
                                    QUI.Label(poolyTempContent, itemDetailsStyle, poolyTempContentSize.x, 18);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();

                                QUI.Space(PS.databaseElementHorizontalSpacing * 8 * category.isExpanded.faded);

                                poolyTempContent.text = "HIGHEST SPAWN COUNT";
                                poolyTempContentSize = miniTitleStyle.CalcSize(poolyTempContent);
                                QUI.BeginVertical(poolyTempContentSize.x, PS.databaseLineHeight);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Label(poolyTempContent.text, miniTitleStyle, poolyTempContentSize.x, 14);
                                    QUI.Space(-7);
                                    QUI.Label("EVER RECORDED", miniSubtitleStyle, poolyTempContentSize.x, 12);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();

                                poolyTempContent.text = item.alltimeMaxSpawnCount.ToString();
                                poolyTempContentSize = itemDetailsStyle.CalcSize(poolyTempContent);
                                QUI.BeginVertical(poolyTempContentSize.x, PS.databaseLineHeight);
                                {
                                    QUI.FlexibleSpace();
                                    QUI.Space(1);
                                    QUI.Label(poolyTempContent, itemDetailsStyle, poolyTempContentSize.x, 18);
                                    QUI.FlexibleSpace();
                                }
                                QUI.EndVertical();

                                QUI.FlexibleSpace();

                                if(editStats.faded < 0.95f)
                                {
                                    QUI.BeginVertical(72 * (1 - editStats.faded), PS.databaseLineHeight);
                                    {
                                        QUI.FlexibleSpace();
                                        QUI.Space(4);
                                        if(QUI.SlicedButton("WARNINGS", QColors.Color.Gray, 72 * (1 - editStats.faded), PS.databaseLineHeight, item.warningsEnabled.value))
                                        {
                                            item.warningsEnabled.target = !item.warningsEnabled.target;
                                        }
                                        QUI.FlexibleSpace();
                                    }
                                    QUI.EndVertical();

                                    if(item.warningsEnabled.faded > 0.05f)
                                    {
                                        QUI.Space(SPACE_2 * (1 - editStats.faded) * item.warningsEnabled.faded);
                                        poolyTempContent.text = "LOW";
                                        poolyTempContentSize = miniTitleStyle.CalcSize(poolyTempContent);
                                        QUI.BeginVertical(poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                        {
                                            QUI.FlexibleSpace();
                                            QUI.Space(3);
                                            QUI.SetGUIColor(QUI.IsProSkin ? QColors.Blue.Color : QColors.BlueLight.Color);
                                            QUI.Label(poolyTempContent.text, miniTitleStyle, poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                            QUI.ResetColors();
                                            QUI.FlexibleSpace();
                                        }
                                        QUI.EndVertical();

                                        QUI.Space(-SPACE_4);

                                        QUI.BeginVertical(30 * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                        {
                                            QUI.FlexibleSpace();
                                            QUI.Space(3);
                                            QUI.SetGUIBackgroundColor(QUI.IsProSkin ? QColors.Blue.Color : QColors.BlueLight.Color);
                                            item.lowWarningThreshold = EditorGUILayout.DelayedFloatField(item.lowWarningThreshold, GUILayout.Width(30 * (1 - editStats.faded) * item.warningsEnabled.faded));
                                            QUI.ResetColors();
                                            QUI.FlexibleSpace();
                                        }
                                        QUI.EndVertical();

                                        QUI.Space(-6);

                                        poolyTempContent.text = "%";
                                        poolyTempContentSize = miniTitleStyle.CalcSize(poolyTempContent);
                                        QUI.BeginVertical(poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                        {
                                            QUI.FlexibleSpace();
                                            QUI.Space(3);
                                            QUI.SetGUIColor(QUI.IsProSkin ? QColors.Blue.Color : QColors.BlueLight.Color);
                                            QUI.Label(poolyTempContent.text, miniTitleStyle, poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                            QUI.ResetColors();
                                            QUI.FlexibleSpace();
                                        }
                                        QUI.EndVertical();

                                        QUI.Space(SPACE_2 * (1 - editStats.faded) * item.warningsEnabled.faded);

                                        poolyTempContent.text = "HIGH";
                                        poolyTempContentSize = miniTitleStyle.CalcSize(poolyTempContent);
                                        QUI.BeginVertical(poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                        {
                                            QUI.FlexibleSpace();
                                            QUI.Space(3);
                                            QUI.SetGUIColor(QUI.IsProSkin ? QColors.Orange.Color : QColors.OrangeLight.Color);
                                            QUI.Label(poolyTempContent.text, miniTitleStyle, poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                            QUI.ResetColors();
                                            QUI.FlexibleSpace();
                                        }
                                        QUI.EndVertical();

                                        QUI.Space(-SPACE_4);

                                        QUI.BeginVertical(30 * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                        {
                                            QUI.FlexibleSpace();
                                            QUI.Space(3);
                                            QUI.SetGUIBackgroundColor(QUI.IsProSkin ? QColors.Orange.Color : QColors.OrangeLight.Color);
                                            item.highWarningThreshold = EditorGUILayout.DelayedFloatField(item.highWarningThreshold, GUILayout.Width(30 * (1 - editStats.faded) * item.warningsEnabled.faded));
                                            QUI.ResetColors();
                                            QUI.FlexibleSpace();
                                        }
                                        QUI.EndVertical();

                                        QUI.Space(-6);

                                        poolyTempContent.text = "%";
                                        poolyTempContentSize = miniTitleStyle.CalcSize(poolyTempContent);
                                        QUI.BeginVertical(poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                        {
                                            QUI.FlexibleSpace();
                                            QUI.Space(3);
                                            QUI.SetGUIColor(QUI.IsProSkin ? QColors.Orange.Color : QColors.OrangeLight.Color);
                                            QUI.Label(poolyTempContent.text, miniTitleStyle, poolyTempContentSize.x * (1 - editStats.faded) * item.warningsEnabled.faded, PS.databaseLineHeight);
                                            QUI.ResetColors();
                                            QUI.FlexibleSpace();
                                        }
                                        QUI.EndVertical();
                                    }
                                    QUI.Space(12 + 3 * (1 - item.warningsEnabled.faded));
                                }
                                if(editStats.target)
                                {
                                    QUI.BeginVertical((PS.databaseClearStatisticsButtonWidth - 1) * editStats.faded, PS.databaseLineHeight);
                                    {
                                        QUI.FlexibleSpace();
                                        QUI.Space(4);
                                        if(QUI.GhostButton("Clear Item Statistics", QColors.Color.Gray, (PS.databaseClearStatisticsButtonWidth - 1) * editStats.faded, PS.databaseLineHeight))
                                        {
                                            if(QUI.DisplayDialog("Clear Item Statistics", "Are you sure you want to clear all the recorded statistics for this item (" + item.prefabName + ")?\n\nOperation cannot be undone!", "Yes", "No"))
                                            {
                                                item.ClearStatistics();
                                                QUI.SetDirty(PS);
                                                AssetDatabase.SaveAssets();
                                            }
                                        }
                                        QUI.FlexibleSpace();
                                    }
                                    QUI.EndVertical();
                                    QUI.BeginVertical((PS.databaseDeleteButtonWidth - 2) * editStats.faded, PS.databaseLineHeight);
                                    {
                                        QUI.FlexibleSpace();
                                        QUI.Space(4);
                                        if(QUI.GhostButton("Delete Item", QColors.Color.Gray, (PS.databaseDeleteButtonWidth - 2) * editStats.faded, PS.databaseLineHeight))
                                        {
                                            if(QUI.DisplayDialog("Delete Item Statistics", "Are you sure you want to delete this item (" + item.prefabName + ") from the statistics database?\n\nOperation cannot be undone!", "Yes", "No"))
                                            {
                                                category.DeleteItem(item);
                                                QUI.SetDirty(PS);
                                                AssetDatabase.SaveAssets();
                                            }
                                        }
                                        QUI.FlexibleSpace();
                                    }
                                    QUI.EndVertical();
                                }
                                QUI.Space(15 * editStats.faded);
                            }
                            QUI.EndHorizontal();
                        }
                        QUI.EndVertical();
                    }
                }
                QUI.EndHorizontal();

                if(item.data != null && item.data.Count > 0)
                {
                    QUI.Space(SPACE_2);

                    if(QUI.BeginFadeGroup(item.showGraph.faded))
                    {
                        QUI.BeginVertical();
                        {
                            QUI.Space(SPACE_2 + PS.graphHeight * item.showGraph.faded);
                            DrawPoolyStatisticsDatabasePoolCategoryItemGraph(item, width);
                            QUI.Space(SPACE_8 * item.showGraph.faded);
                        }
                        QUI.EndVertical();
                    }
                    QUI.EndFadeGroup();
                }
                else
                {
                    item.showGraph.value = false;
                }

                QUI.Space(8 * item.warningsEnabled.faded * item.showInfoMessage.faded);
            }
            QUI.EndVertical();
        }

        void DrawPoolyStatisticsDatabasePoolCategoryItemGraph(PoolyStatistics.StatisticsItem item, float width)
        {
            Rect rect = new Rect(GUILayoutUtility.GetLastRect());
            if(PS.graphHeight <= 24) { PS.graphHeight = 24; }
            rect.width = width - 1;
            rect.height = PS.graphHeight * item.showGraph.faded;

            QUI.Box(rect, QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray)); //Draw Graph Background

            if(item.warningsEnabled.target)
            {
                QUI.DrawLine(new Rect(rect.x,
                                         rect.y + (PS.graphHeight - PS.graphHeight * (item.highWarningThreshold / 100)),
                                         rect.width,
                                         1),
                             QColors.Color.Orange); //Draw Graph LINE (orange) for MAX Warning Threshold

                QUI.SetGUIColor(QUI.IsProSkin ? QColors.Orange.Color : QColors.OrangeDark.Color);
                poolyTempContent.text = "HIGH: " + (int)(item.highWarningThreshold * item.LastRecordedPreloadedClones / 100) + " clones"; //Draw Graph LABEL for MAX Warning Threshold
                poolyTempContentSize = QStyles.CalcSize(poolyTempContent, Style.Text.Small);
                GUI.Label(new Rect(rect.x + width - poolyTempContentSize.x - SPACE_8,
                                   rect.y + SPACE_2 + (PS.graphHeight - PS.graphHeight * (item.highWarningThreshold / 100)),
                                   poolyTempContentSize.x, poolyTempContentSize.y),
                          poolyTempContent,
                          QStyles.GetStyle(Style.Text.Small));
                QUI.ResetColors();
            }

            rect.y += PS.graphHeight * item.showGraph.faded;

            if(item.warningsEnabled.target)
            {
                QUI.DrawLine(new Rect(rect.x,
                                      rect.y - (PS.graphHeight * (item.lowWarningThreshold / 100)) - 1,
                                      rect.width,
                                      1),
                             QColors.Color.Blue); //Draw Graph LINE (blue) for MIN Warning Threshold

                QUI.SetGUIColor(QUI.IsProSkin ? QColors.Blue.Color : QColors.BlueDark.Color);
                poolyTempContent.text = "LOW: " + (int)(item.lowWarningThreshold * item.LastRecordedPreloadedClones / 100) + " clones"; //Draw Graph LABEL for MIN Warning Threshold
                poolyTempContentSize = QStyles.CalcSize(poolyTempContent, Style.Text.Small);
                GUI.Label(new Rect(rect.x + width - poolyTempContentSize.x - SPACE_8,
                                   rect.y - SPACE_2 - poolyTempContentSize.y - (PS.graphHeight * (item.lowWarningThreshold / 100)),
                                   poolyTempContentSize.x,
                                   poolyTempContentSize.y),
                          poolyTempContent,
                          QStyles.GetStyle(Style.Text.Small));
                QUI.ResetColors();
            }


            rect.y -= PS.graphPointSize / 2;
            rect.x += PS.graphPointSize / 2 + SPACE_8;
            float maxHeight = rect.y;
            float minHeight = rect.y - (PS.graphHeight * item.showGraph.faded - PS.graphPointSize);

            float labelX;
            float labelY;

            if(item.data.Count >= 2)
            {
                for(int dataIndex = 0; dataIndex < item.data.Count; dataIndex++) //Draw Graph Lines
                {
                    if(dataIndex > 0)
                    {
                        QDrawing.DrawLine(new Vector2(rect.x + PS.graphPointToPointDistance * (dataIndex - 1),
                                                      Mathf.Clamp(rect.y - (item.data[dataIndex - 1].maxSpawnCount * (PS.graphHeight * item.showGraph.faded - PS.graphPointSize)) / item.data[item.data.Count - 1].preloadedClones, minHeight, maxHeight)), //formula is: x = (current maximumUsage * grahpHeight) / last preloadedClones
                                          new Vector2(rect.x + PS.graphPointToPointDistance * dataIndex,
                                                      Mathf.Clamp(rect.y - (item.data[dataIndex].maxSpawnCount * (PS.graphHeight * item.showGraph.faded - PS.graphPointSize)) / item.data[item.data.Count - 1].preloadedClones, minHeight, maxHeight)),
                                          QUI.IsProSkin ? QColors.UnityMild.Color : QColors.UnityMild.Color,
                                          PS.graphLineWidth,
                                          true);
                    }
                }
            }

            for(int dataIndex = 0; dataIndex < item.data.Count; dataIndex++) //Draw Graph Point Icons
            {
                float x = rect.x - PS.graphPointSize / 2 + PS.graphPointToPointDistance * dataIndex;
                float y = rect.y - PS.graphPointSize / 2 - (item.data[dataIndex].maxSpawnCount * (PS.graphHeight * item.showGraph.faded - PS.graphPointSize)) / item.data[item.data.Count - 1].preloadedClones;
                QUI.DrawTexture(new Rect(x + (PS.graphPointSize / 2) * editStats.faded,
                                         Mathf.Clamp(y, minHeight - PS.graphPointSize / 2, maxHeight - PS.graphPointSize / 2) + (PS.graphPointSize / 2) * editStats.faded,
                                         PS.graphPointSize * (1 - editStats.faded),
                                         PS.graphPointSize * (1 - editStats.faded)),
                                GetPoolyStatisticsGraphPointIcon(item, dataIndex));

                poolyTempContent.text = item.data[dataIndex].maxSpawnCount.ToString();
                poolyTempContentSize = QStyles.CalcSize(poolyTempContent, Style.Text.Small);

                labelX = x + PS.graphPointSize;
                labelY = y + PS.graphPointSize / 2;
                labelY = Mathf.Clamp(labelY, minHeight, maxHeight - PS.graphPointSize / 2);

                QUI.Label(new Rect(labelX,
                                   labelY,
                                   poolyTempContentSize.x,
                                   poolyTempContentSize.y),
                          item.data[dataIndex].maxSpawnCount.ToString(),
                          Style.Text.Small);

                if(editStats.faded > 0.1f)
                {
                    if(GUI.Button(new Rect(x + (PS.graphPointSize / 2) * (1 - editStats.faded),
                                         Mathf.Clamp(y, minHeight - PS.graphPointSize / 2, maxHeight - PS.graphPointSize / 2) + (PS.graphPointSize / 2) * (1 - editStats.faded),
                                         PS.graphPointSize * editStats.faded,
                                         PS.graphPointSize * editStats.faded),
                                  "",
                                  QStyles.GetStyle(Style.QuickButton.Cancel)))
                    {
                        item.data.RemoveAt(dataIndex);
                        QUI.ExitGUI();
                    }
                }
            }
        }

        Color GetPoolyStatisticsGraphLineColor(PoolyStatistics.StatisticsItem item, int dataIndex)
        {
            if(item.data == null || item.data.Count == 0) { return QColors.Purple.Color; }                                                                     //No stats (data is null or empty)
            if(item.data[dataIndex].maxSpawnCount <= item.LastRecordedPreloadedClones * (item.lowWarningThreshold / 100)) { return QColors.Blue.Color; }       //Warning Threshold - UNDER LOW WARNING THRESHOLD
            if(item.data[dataIndex].maxSpawnCount > item.LastRecordedPreloadedClones) { return QColors.Red.Color; }                                            //EXCEEDED PRELOADED CLONE COUNT
            if(item.data[dataIndex].maxSpawnCount >= item.LastRecordedPreloadedClones * (item.highWarningThreshold / 100)) { return QColors.Orange.Color; }    //Warning Threshold - OVER HIGH WARNING THRESHOLD
            return QColors.Green.Color;                                                                                                                        //Optimum Settings (No Warning)
        }
        Texture GetPoolyStatisticsGraphPointIcon(PoolyStatistics.StatisticsItem item, int dataIndex)
        {
            if(!item.warningsEnabled.value) return QResources.backgroundHighBlue.texture;
            if(item.data[dataIndex].maxSpawnCount < item.LastRecordedPreloadedClones * (item.lowWarningThreshold / 100)) { return QResources.iconInfo.normal; }        //Warning Threshold - UNDER LOW WARNING THRESHOLD
            if(item.data[dataIndex].maxSpawnCount >= item.LastRecordedPreloadedClones) { return QResources.iconError.texture; }                                        //EXCEEDED PRELOADED CLONE COUNT
            if(item.data[dataIndex].maxSpawnCount > item.LastRecordedPreloadedClones * (item.highWarningThreshold / 100)) { return QResources.iconWarning.normal; }    //Warning Threshold - OVER HIGH WARNING THRESHOLD
            return QResources.iconOk.texture;                                                                                                                          //Optimum Settings (No Warning)
        }
        Color GetPoolyStatisticsItemStatusColor(PoolyStatistics.StatisticsItem item)
        {
            switch(item.GetItemStatus())
            {
                case PoolyStatistics.StatisticsItem.ItemStatus.OptimumSettings: return QColors.Green.Color;
                case PoolyStatistics.StatisticsItem.ItemStatus.UnderLowThreshold: return QColors.Blue.Color;
                case PoolyStatistics.StatisticsItem.ItemStatus.OverHighThreshold: return QColors.Orange.Color;
                case PoolyStatistics.StatisticsItem.ItemStatus.ExceededPreloadCount: return QColors.Red.Color;
                default: return Color.white;
            }
        }
        QColors.Color GetPoolyStatisticsItemStatusButtonColor(PoolyStatistics.StatisticsItem item)
        {
            switch(item.GetItemStatus())
            {
                case PoolyStatistics.StatisticsItem.ItemStatus.OptimumSettings: return QColors.Color.Green;
                case PoolyStatistics.StatisticsItem.ItemStatus.UnderLowThreshold: return QColors.Color.Blue;
                case PoolyStatistics.StatisticsItem.ItemStatus.OverHighThreshold: return QColors.Color.Orange;
                case PoolyStatistics.StatisticsItem.ItemStatus.ExceededPreloadCount: return QColors.Color.Red;
                default: return QColors.Color.Gray;
            }
        }
        void DrawPoolyStatisticsDatabasePoolCategoryItemInfoMessage(PoolyStatistics.StatisticsItem item, float width)
        {
            switch(item.GetItemStatus())
            {
                case PoolyStatistics.StatisticsItem.ItemStatus.OptimumSettings: //Optimum Settings (No Warning)
                    item.showInfoMessage.target = false;
                    break;
                case PoolyStatistics.StatisticsItem.ItemStatus.UnderLowThreshold: //Warning Threshold - UNDER MINIMUM WARNING THRESHOLD
                    item.showInfoMessage.target = true;
                    QUI.DrawInfoMessage(new InfoMessage() { title = "You may be pooling too many clones for this prefab... (LOW Warning Threshold: " + (int)(item.LastRecordedPreloadedClones * (item.lowWarningThreshold / 100)) + " clones / " + item.lowWarningThreshold + "% threshold)", show = item.showInfoMessage, type = InfoMessageType.Info }, width);
                    break;
                case PoolyStatistics.StatisticsItem.ItemStatus.OverHighThreshold: //Warning Threshold - OVER MAXIMUM WARNING THRESHOLD
                    item.showInfoMessage.target = true;
                    QUI.DrawInfoMessage(new InfoMessage() { title = "Consider increasing the preloaded clones count... (HIGH Warning Threshold: " + (int)(item.LastRecordedPreloadedClones * (item.highWarningThreshold / 100)) + " clones / " + item.highWarningThreshold + "% threshold)", show = item.showInfoMessage, type = InfoMessageType.Warning }, width);
                    break;
                case PoolyStatistics.StatisticsItem.ItemStatus.ExceededPreloadCount: //EXCEEDED PRELOADED CLONE COUNT
                    item.showInfoMessage.target = true;
                    QUI.DrawInfoMessage(new InfoMessage() { title = "Increase the preloaded clones count! The maximum usage has exceeded the set preload clone count (in the pool)!", show = item.showInfoMessage, type = InfoMessageType.Error }, width);
                    break;
                case PoolyStatistics.StatisticsItem.ItemStatus.NoStats:
                    item.showInfoMessage.target = false;
                    break;
            }
        }

        void DrawHorizontalLine(float width, float height = 1f)
        {
            QUI.BeginHorizontal(width, height);
            {
                QUI.DrawTexture(QResources.backgroundSidebar.texture, width, height);
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
        }
        void DrawHorizontalLine(Color color, float width, float height = 1f)
        {
            QUI.SetGUIColor(color);
            DrawHorizontalLine(width, height);
            QUI.ResetColors();
        }
#endif
    }
}
