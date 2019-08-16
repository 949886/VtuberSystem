// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System.Collections.Generic;
using QuickEditor;
using QuickEngine.Extensions;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Ez.Pooly
{
    [CustomEditor(typeof(PoolySpawner))]
    [DisallowMultipleComponent]
    public class PoolySpawnerEditor : QEditor
    {
        PoolySpawner poolySpawner { get { return (PoolySpawner)target; } }

        SerializedProperty
            autoStart, despawnWhenFinished, spawnStartDelay,
            spawnCount, spawnForever,
            spawnPositions, spawnPoints,
            spawnInterval, spawnAtRandomIntervals, spawnAtRandomIntervalMinimum, spawnAtRandomIntervalMaximum,
            matchTransformRotation, matchTransformScale, reparentUnderTransform,
            useSpawnerAsSpawnLocation,
            prefabSpawnType, spawnAt, locationSpawnType,
            showOnSpawnStarted, OnSpawnStarted,
            showOnSpawnStopped, OnSpawnStopped,
            showOnSpawnResumed, OnSpawnResumed,
            showOnSpawnPaused, OnSpawnPaused,
            showOnSpawnFinished, OnSpawnFinished;

        AnimBool
            animBoolDespawnWhenFinished,
            animBoolSpawnRandomPrefab,
            animBoolRandomSpawnLocation,
            animBoolShowSpawnLocationsOptions,
            animBoolShowOnSpawnStarted, animBoolShowOnSpawnStopped, animBoolShowOnSpawnResumed, animBoolShowOnSpawnPaused, animBoolShowOnSpawnFinished,
            animBoolShowEditorSettings;

        Color defaultHandlesColor;
        Color AccentColorOrange { get { return QUI.IsProSkin ? QColors.Orange.Color : QColors.OrangeLight.Color; } }
        Color AccentColorBlue { get { return QUI.IsProSkin ? QColors.Blue.Color : QColors.BlueLight.Color; } }
        Color AccentColorPurple { get { return QUI.IsProSkin ? QColors.Purple.Color : QColors.PurpleLight.Color; } }

        GUIContent tempContent;
        Vector2 tempContentSize;

        bool savePoolySettings = false;

        protected override void SerializedObjectFindProperties()
        {
            base.SerializedObjectFindProperties();

            autoStart = serializedObject.FindProperty("autoStart");
            despawnWhenFinished = serializedObject.FindProperty("despawnWhenFinished");
            spawnStartDelay = serializedObject.FindProperty("spawnStartDelay");
            spawnCount = serializedObject.FindProperty("spawnCount");
            spawnPositions = serializedObject.FindProperty("spawnPositions");
            spawnPoints = serializedObject.FindProperty("spawnPoints");
            spawnForever = serializedObject.FindProperty("spawnForever");
            spawnInterval = serializedObject.FindProperty("spawnInterval");
            spawnAtRandomIntervals = serializedObject.FindProperty("spawnAtRandomIntervals");
            spawnAtRandomIntervalMinimum = serializedObject.FindProperty("spawnAtRandomIntervalMinimum");
            spawnAtRandomIntervalMaximum = serializedObject.FindProperty("spawnAtRandomIntervalMaximum");
            matchTransformRotation = serializedObject.FindProperty("matchTransformRotation");
            matchTransformScale = serializedObject.FindProperty("matchTransformScale");
            reparentUnderTransform = serializedObject.FindProperty("reparentUnderTransform");
            useSpawnerAsSpawnLocation = serializedObject.FindProperty("useSpawnerAsSpawnLocation");
            prefabSpawnType = serializedObject.FindProperty("prefabSpawnType");
            spawnAt = serializedObject.FindProperty("spawnAt");
            locationSpawnType = serializedObject.FindProperty("locationSpawnType");
            showOnSpawnStarted = serializedObject.FindProperty("showOnSpawnStarted");
            OnSpawnStarted = serializedObject.FindProperty("OnSpawnStarted");
            showOnSpawnStopped = serializedObject.FindProperty("showOnSpawnStopped");
            OnSpawnStopped = serializedObject.FindProperty("OnSpawnStopped");
            showOnSpawnResumed = serializedObject.FindProperty("showOnSpawnResumed");
            OnSpawnResumed = serializedObject.FindProperty("OnSpawnResumed");
            showOnSpawnPaused = serializedObject.FindProperty("showOnSpawnPaused");
            OnSpawnPaused = serializedObject.FindProperty("OnSpawnPaused");
            showOnSpawnFinished = serializedObject.FindProperty("showOnSpawnFinished");
            OnSpawnFinished = serializedObject.FindProperty("OnSpawnFinished");
        }

        protected override void InitAnimBools()
        {
            base.InitAnimBools();

            animBoolDespawnWhenFinished = new AnimBool(autoStart.enumValueIndex == (int)PoolySpawner.AutoStart.OnSpawned, Repaint);
            animBoolSpawnRandomPrefab = new AnimBool(prefabSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random, Repaint);
            animBoolRandomSpawnLocation = new AnimBool(locationSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random, Repaint);
            animBoolShowSpawnLocationsOptions = new AnimBool(useSpawnerAsSpawnLocation.boolValue, Repaint);
            animBoolShowOnSpawnStarted = new AnimBool(showOnSpawnStarted.boolValue, Repaint);
            animBoolShowOnSpawnStopped = new AnimBool(showOnSpawnStopped.boolValue, Repaint);
            animBoolShowOnSpawnResumed = new AnimBool(showOnSpawnResumed.boolValue, Repaint);
            animBoolShowOnSpawnPaused = new AnimBool(showOnSpawnPaused.boolValue, Repaint);
            animBoolShowOnSpawnFinished = new AnimBool(showOnSpawnFinished.boolValue, Repaint);
            animBoolShowEditorSettings = new AnimBool(false, Repaint);
        }

        protected override void GenerateInfoMessages()
        {
            base.GenerateInfoMessages();

            infoMessage.Add("AutoStartNever", 
                            new InfoMessage()
                            {
                                title = "Auto Start Never",
                                message = "The spawner will not automatically start spawning." +
                                          "\n" +
                                          "It's your responsability to start the spawning cycle for this spawner.",
                                show = new AnimBool(false, Repaint),
                                type = InfoMessageType.Help
                            });

            infoMessage.Add("SpawnForever",
                            new InfoMessage()
                            {
                                title = "Spawn Forever",
                                message = "The spawner will spawn prefabs forever and it will stop only when you call either StopSpawn or PauseSpawn methods." +
                                          "\n" +
                                          "It's your responsability to stop or pause the spawning cycle for this spawner.",
                                show = new AnimBool(false, Repaint),
                                type = InfoMessageType.Help
                            });

            infoMessage.Add("NoSpawnPoint", 
                            new InfoMessage()
                            {
                                title = "",
                                message = "Set at least one spawn point by referencing a Transform from the scene or set Use Spawner As Spawn Location as true.",
                                show = new AnimBool(false, Repaint),
                                type = InfoMessageType.Info
                            });

            infoMessage.Add("NoPrefabs", 
                            new InfoMessage()
                            {
                                title = "SPAWNER IS DISABLED",
                                message = "Reference at least one prefab.",
                                show = new AnimBool(false, Repaint),
                                type = InfoMessageType.Error
                            });
        }

        private void GetInitialHandlesColors()
        {
            defaultHandlesColor = Handles.color;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            requiresContantRepaint = true;

            GetInitialHandlesColors();

            tempContent = new GUIContent();
        }

        protected override void OnDisable()
        {
            if(savePoolySettings)
            {
                SavePoolySettings();
            }
        }

        public static Rect GetScreenRect(Vector2 screenPoint, float width, float height)
        {
            Rect rect = new Rect
            {
                x = EditorGUIUtility.ScreenToGUIPoint(screenPoint).x,
                y = EditorGUIUtility.ScreenToGUIPoint(screenPoint).y,
                width = width,
                height = height
            };
            return rect;
        }

        private void OnSceneGUI()
        {
            if(useSpawnerAsSpawnLocation.boolValue) { return; }
            if(!PoolySpawner.PoolySettings.showDottedLines) { return; }
            switch(poolySpawner.spawnAt)
            {
                case PoolySpawner.SpawnAt.Position:
                    if(poolySpawner.spawnPositions != null && poolySpawner.spawnPositions.Count > 0)
                    {
                        for(int i = 0; i < poolySpawner.spawnPositions.Count; i++)
                        {
                            if(PoolySpawner.PoolySettings.showLabels)
                            {
                                Handles.Label(new Vector3(poolySpawner.spawnPositions[i].spawnPosition.x + 0.05f,
                                                         poolySpawner.spawnPositions[i].spawnPosition.y - 0.05f,
                                                         poolySpawner.spawnPositions[i].spawnPosition.z),
                                                         "Spawn Position " + i + " " + poolySpawner.spawnPositions[i].spawnPosition.ToString(PoolySpawner.PoolySettings.decimalPoins));
                            }
                            QUI.BeginChangeCheck();
                            Vector3 updatedSpawnPosition = Handles.PositionHandle(poolySpawner.spawnPositions[i].spawnPosition, Quaternion.identity);
                            if(QUI.EndChangeCheck())
                            {
                                Undo.RecordObject(poolySpawner, "Changed Spawn Position " + i);
                                poolySpawner.spawnPositions[i].spawnPosition = updatedSpawnPosition.Round(PoolySpawner.PoolySettings.decimalPoins);
                            }
                            Handles.color = PoolySpawner.PoolySettings.dottedLinesColor;
                            Handles.DrawDottedLine(poolySpawner.transform.position, poolySpawner.spawnPositions[i].spawnPosition, PoolySpawner.PoolySettings.dottedLinesScreenSpaceSize);
                            Handles.color = defaultHandlesColor;
                        }
                    }
                    break;
                case PoolySpawner.SpawnAt.Transform:
                    if(poolySpawner.spawnPoints != null && poolySpawner.spawnPoints.Count > 0)
                    {
                        for(int i = 0; i < poolySpawner.spawnPoints.Count; i++)
                        {
                            if(poolySpawner.spawnPoints[i].spawnPoint == null) { continue; }
                            if(PoolySpawner.PoolySettings.showLabels)
                            {
                                Handles.Label(new Vector3(poolySpawner.spawnPoints[i].spawnPoint.position.x + 0.05f,
                                                          poolySpawner.spawnPoints[i].spawnPoint.position.y - 0.05f,
                                                          poolySpawner.spawnPoints[i].spawnPoint.position.z),
                                                          "Spawn Point " + i + " " + poolySpawner.spawnPoints[i].spawnPoint.position.ToString(PoolySpawner.PoolySettings.decimalPoins));
                            }
                            QUI.BeginChangeCheck();
                            Vector3 updatedSpawnPointPosition = Handles.PositionHandle(poolySpawner.spawnPoints[i].spawnPoint.position, Quaternion.identity);
                            if(QUI.EndChangeCheck())
                            {
                                Undo.RecordObject(poolySpawner, "Moved " + poolySpawner.spawnPoints[i].spawnPoint.name);
                                poolySpawner.spawnPoints[i].spawnPoint.position = updatedSpawnPointPosition.Round(PoolySpawner.PoolySettings.decimalPoins);
                            }
                            Handles.color = PoolySpawner.PoolySettings.dottedLinesColor;
                            Handles.DrawDottedLine(poolySpawner.transform.position, poolySpawner.spawnPoints[i].spawnPoint.position, PoolySpawner.PoolySettings.dottedLinesScreenSpaceSize);
                            Handles.color = defaultHandlesColor;
                        }
                    }
                    break;
            }
        }

        public override void OnInspectorGUI()
        {
            DrawHeader(EZResources.editorHeaderPoolySpawner.texture, WIDTH_420, 42);
            serializedObject.Update();
            DrawMainSettings();
            DrawPrefabs();
            DrawLocations();
            DrawOnSpawnEvents();
            DrawEditorSettings();
            serializedObject.ApplyModifiedProperties();
            QUI.Space(SPACE_4);
        }

        private void DrawMainSettings()
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.SetGUIBackgroundColor(AccentColorOrange);
                tempContent.text = "Auto Start On";
                QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                QUI.PropertyField(autoStart, 104);
                QUI.FlexibleSpace();
                if(!(autoStart.enumValueIndex == (int)PoolySpawner.AutoStart.Never))
                {
                    tempContent.text = "Auto Start After";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.PropertyField(spawnStartDelay, 40);
                    if(spawnStartDelay.floatValue < 0) { spawnStartDelay.floatValue = 0; }
                    tempContent.text = "seconds";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                }
                QUI.ResetColors();
            }
            QUI.EndHorizontal();
            infoMessage["AutoStartNever"].show.target = PoolySpawner.PoolySettings.showInfoMessages && autoStart.enumValueIndex == (int)PoolySpawner.AutoStart.Never;
            DrawInfoMessage("AutoStartNever", WIDTH_420);
            QUI.Space(SPACE_4 * infoMessage["AutoStartNever"].show.faded);
            animBoolDespawnWhenFinished.target = autoStart.enumValueIndex == (int)PoolySpawner.AutoStart.OnSpawned;
            if(QUI.BeginFadeGroup(animBoolDespawnWhenFinished.faded))
            {
                QUI.BeginHorizontal(WIDTH_420);
                {
                    QUI.SetGUIBackgroundColor(AccentColorOrange);
                    QUI.Toggle(despawnWhenFinished);
                    QUI.ResetColors();
                    QUI.Label("Despawn When Finished Spawning", Style.Text.Normal, WIDTH_420);
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
            }
            QUI.EndFadeGroup();
            QUI.BeginHorizontal(WIDTH_420);
            {
                if(!spawnForever.boolValue)
                {
                    tempContent.text = "Spawn Cycles";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.SetGUIBackgroundColor(AccentColorOrange);
                    QUI.PropertyField(spawnCount, 104);
                    QUI.ResetColors();
                    if(spawnCount.intValue < 0) { spawnCount.intValue = 0; }
                    QUI.FlexibleSpace();
                }
                QUI.SetGUIBackgroundColor(QColors.OrangeLight.Color);
                QUI.Toggle(spawnForever);
                QUI.ResetColors();
                tempContent.text = "Spawn Forever";
                QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                if(spawnForever.boolValue) { QUI.FlexibleSpace(); }
            }
            QUI.EndHorizontal();
            infoMessage["SpawnForever"].show.target = PoolySpawner.PoolySettings.showInfoMessages && spawnForever.boolValue;
            DrawInfoMessage("SpawnForever", WIDTH_420);
            QUI.Space(SPACE_4 * infoMessage["SpawnForever"].show.faded);
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.SetGUIBackgroundColor(AccentColorOrange);
                if(spawnAtRandomIntervals.boolValue)
                {
                    QUI.Toggle(spawnAtRandomIntervals);
                    tempContent.text = "Random Spawn Interval Between";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.FlexibleSpace();
                    tempContent.text = "Min";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.PropertyField(spawnAtRandomIntervalMinimum, 40);
                    if(spawnAtRandomIntervalMinimum.floatValue < 0) { spawnAtRandomIntervalMinimum.floatValue = 0; }
                    QUI.Label("-", Style.Text.Normal, 8);
                    tempContent.text = "Max";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.PropertyField(spawnAtRandomIntervalMaximum, 40);
                    if(spawnAtRandomIntervalMaximum.floatValue < spawnAtRandomIntervalMinimum.floatValue) { spawnAtRandomIntervalMaximum.floatValue = spawnAtRandomIntervalMinimum.floatValue; }
                    tempContent.text = "seconds";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                }
                else
                {
                    tempContent.text = "Spawn Every";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.PropertyField(spawnInterval, 56);
                    tempContent.text = "seconds";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.FlexibleSpace();
                    QUI.Toggle(spawnAtRandomIntervals);
                    tempContent.text = "Set Random Spawn Interval";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                }
                QUI.ResetColors();
            }
            QUI.EndHorizontal();

            if(useSpawnerAsSpawnLocation.boolValue ||
               (!useSpawnerAsSpawnLocation.boolValue && poolySpawner.spawnAt == PoolySpawner.SpawnAt.Transform))
            {
                QUI.SetGUIBackgroundColor(AccentColorOrange);
                QUI.BeginHorizontal(WIDTH_420);
                {
                    tempContent.text = "Extra Spawn Settings";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                }
                QUI.EndHorizontal();

                QUI.BeginHorizontal(WIDTH_420);
                {
                    QUI.Space(SPACE_8);
                    QUI.Toggle(matchTransformRotation);
                    tempContent.text = "Match " + (useSpawnerAsSpawnLocation.boolValue ? "Spawner" : "Target") + " Transform Rotation";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
                QUI.BeginHorizontal(WIDTH_420);
                {
                    QUI.Space(SPACE_8);
                    QUI.Toggle(matchTransformScale);
                    tempContent.text = "Match " + (useSpawnerAsSpawnLocation.boolValue ? "Spawner" : "Target") + " Transform Scale";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
                QUI.BeginHorizontal(WIDTH_420);
                {
                    QUI.Space(SPACE_8);
                    QUI.Toggle(reparentUnderTransform);
                    tempContent.text = "Reparent Under " + (useSpawnerAsSpawnLocation.boolValue ? "Spawner" : "Target") + " Transform";
                    QUI.Label(tempContent.text, Style.Text.Normal, QStyles.CalcSize(tempContent, Style.Text.Normal).x);
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
                QUI.ResetColors();
            }
        }

        private void DrawPrefabs()
        {
            QUI.Space(SPACE_8);
            QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Orange), WIDTH_420, 20);
            QUI.Space(-20);
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Space(SPACE_4);
                QUI.Label("PREFAB" + ((poolySpawner.spawnPrefabs != null && poolySpawner.spawnPrefabs.Count > 1) ? "S" : "") + " TO SPAWN", Style.Text.Normal, 130, HEIGHT_16);
                QUI.Space(SPACE_4);
                QUI.FlexibleSpace();
                if(poolySpawner.spawnPrefabs != null && poolySpawner.spawnPrefabs.Count > 1)
                {
                    QUI.Label("Spawn Type", Style.Text.Normal, 68);
                    QUI.SetGUIBackgroundColor(AccentColorOrange);
                    QUI.Popup(prefabSpawnType, 90);
                    QUI.ResetColors();
                }
                QUI.Space(SPACE_8);
            }
            QUI.EndHorizontal();
            QUI.Space(SPACE_4);
            if(poolySpawner.spawnPrefabs == null || poolySpawner.spawnPrefabs.Count == 0) { poolySpawner.spawnPrefabs = new List<PoolySpawner.SpawnPrefab> { new PoolySpawner.SpawnPrefab(null) }; }
            if(poolySpawner.spawnPrefabs.Count == 1 && poolySpawner.prefabSpawnType == PoolySpawner.SpawnType.Random) { poolySpawner.prefabSpawnType = PoolySpawner.SpawnType.Sequential; }
            infoMessage["NoPrefabs"].show.target = !poolySpawner.HasPrefabs();
            DrawInfoMessage("NoPrefabs", WIDTH_420);
            for(int i = 0; i < poolySpawner.spawnPrefabs.Count; i++)
            {
                QUI.Space(SPACE_2);
                QUI.BeginHorizontal(WIDTH_420);
                {
                    tempContent = new GUIContent(i.ToString());
                    tempContentSize = QStyles.CalcSize(tempContent, Style.Text.Small);
                    QUI.Label(tempContent.text, Style.Text.Small, tempContentSize.x);
                    QUI.BeginChangeCheck();
                    Transform prefabObject = poolySpawner.spawnPrefabs[i].prefab;
                    QUI.SetGUIBackgroundColor(AccentColorOrange);
                    prefabObject = (Transform)QUI.ObjectField(prefabObject, typeof(Transform), false, (WIDTH_420 - SPACE_2 - tempContentSize.x - SPACE_8 - SPACE_16) - 240 * animBoolSpawnRandomPrefab.faded);
                    QUI.ResetColors();
                    if(QUI.EndChangeCheck())
                    {
                        Undo.RecordObject(poolySpawner, "Changed Value For Prefab Slot " + i);
                        poolySpawner.spawnPrefabs[i].prefab = prefabObject;
                    }
                    if(QUI.ButtonMinus())
                    {
                        if(poolySpawner.spawnPrefabs.Count > 1)
                        {
                            Undo.RecordObject(poolySpawner, "Removed Prefab Slot " + i);
                            poolySpawner.spawnPrefabs.RemoveAt(i);
                        }
                        else if(poolySpawner.spawnPrefabs == null || poolySpawner.spawnPrefabs[0].prefab != null)
                        {
                            Undo.RecordObject(poolySpawner, "Reset Prefab Slot");
                            poolySpawner.spawnPrefabs = new List<PoolySpawner.SpawnPrefab> { new PoolySpawner.SpawnPrefab(null) };
                        }
                        QUI.SetDirty(poolySpawner);
                        QUI.ExitGUI();
                    }
                    animBoolSpawnRandomPrefab.target = prefabSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random;
                    if(poolySpawner.spawnPrefabs != null && poolySpawner.spawnPrefabs.Count > 1 && prefabSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random)
                    {
                        QUI.Space(SPACE_2);
                        QUI.Label("weight", Style.Text.Small, 50 * animBoolSpawnRandomPrefab.faded);
                        QUI.Space(-SPACE_8);
                        QUI.BeginChangeCheck();
                        int spawnPrefabWeight = poolySpawner.spawnPrefabs[i].weight;
                        QUI.SetGUIBackgroundColor(AccentColorOrange);
                        spawnPrefabWeight = EditorGUILayout.IntSlider(spawnPrefabWeight, 0, 100);
                        QUI.ResetColors();
                        if(QUI.EndChangeCheck())
                        {
                            Undo.RecordObject(poolySpawner, "Changed Weight Of Prefab " + i);
                            poolySpawner.spawnPrefabs[i].weight = spawnPrefabWeight;
                        }
                    }
                    QUI.FlexibleSpace();
                }
                QUI.EndHorizontal();
            }
            QUI.Space(SPACE_2);
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.FlexibleSpace();
                if(QUI.ButtonPlus())
                {
                    Undo.RecordObject(poolySpawner, "Added New Prefab Slot");
                    poolySpawner.spawnPrefabs.Add(new PoolySpawner.SpawnPrefab(null));
                    QUI.SetDirty(poolySpawner);
                    QUI.ExitGUI();
                }
                QUI.Space(3 + 240 * animBoolSpawnRandomPrefab.faded);
            }
            QUI.EndHorizontal();
        }

        private void DrawLocations()
        {
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.SetGUIBackgroundColor(AccentColorBlue);
                QUI.BeginChangeCheck();
                QUI.Toggle(useSpawnerAsSpawnLocation);
                if(QUI.EndChangeCheck())
                {
                    HandleUtility.Repaint();
                    SceneView.RepaintAll();
                    Repaint();
                }
                QUI.ResetColors();
                if(useSpawnerAsSpawnLocation.boolValue && poolySpawner.spawnPositions != null && poolySpawner.spawnPositions.Count > 0)
                {
                    spawnPositions.ClearArray();
                    spawnPositions.InsertArrayElementAtIndex(0);
                    spawnPositions.GetArrayElementAtIndex(0).FindPropertyRelative("spawnPosition").vector3Value = new Vector3(0, 1, 0);

                    serializedObject.ApplyModifiedProperties();
                    HandleUtility.Repaint();
                    SceneView.RepaintAll();
                    Repaint();
                }
                if(useSpawnerAsSpawnLocation.boolValue && poolySpawner.spawnPoints != null && poolySpawner.spawnPoints.Count > 0)
                {
                    spawnPoints.ClearArray();
                    spawnPoints.InsertArrayElementAtIndex(0);

                    serializedObject.ApplyModifiedProperties();
                    HandleUtility.Repaint();
                    SceneView.RepaintAll();
                    Repaint();
                }
                QUI.Label("Use Spawner As Spawn Location", Style.Text.Normal, 200);
            }
            QUI.EndHorizontal();
            if(useSpawnerAsSpawnLocation.boolValue) { return; }
            else { QUI.Space(-SPACE_4); }
            animBoolShowSpawnLocationsOptions.target = !useSpawnerAsSpawnLocation.boolValue;
            if(QUI.BeginFadeGroup(animBoolShowSpawnLocationsOptions.faded))
            {
                QUI.Space(SPACE_8);
                QUI.BeginVertical();
                {
                    QUI.Space(SPACE_4);
                    QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Blue), WIDTH_420, 20);
                    QUI.Space(-20);
                    QUI.BeginHorizontal(WIDTH_420);
                    {
                        QUI.Space(SPACE_4);
                        QUI.Label("SPAWN AT", Style.Text.Normal, 60);
                        QUI.SetGUIBackgroundColor(AccentColorBlue);
                        QUI.BeginChangeCheck();
                        {
                            QUI.Popup(spawnAt, 70);
                        }
                        if(QUI.EndChangeCheck())
                        {

                            Undo.RecordObject(poolySpawner, "SpawnAt Change");

                            spawnPositions.ClearArray();
                            spawnPositions.InsertArrayElementAtIndex(0);
                            spawnPositions.GetArrayElementAtIndex(0).FindPropertyRelative("spawnPosition").vector3Value = new Vector3(0, 1, 0);

                            spawnPoints.ClearArray();
                            spawnPoints.InsertArrayElementAtIndex(0);
                            serializedObject.ApplyModifiedProperties();
                        }
                        QUI.ResetColors();
                        QUI.FlexibleSpace();
                        if((poolySpawner.spawnPositions != null && poolySpawner.spawnPositions.Count > 1 && poolySpawner.spawnAt == PoolySpawner.SpawnAt.Position) ||
                           (poolySpawner.spawnPoints != null && poolySpawner.spawnPoints.Count > 1 && poolySpawner.spawnAt == PoolySpawner.SpawnAt.Transform))
                        {
                            QUI.Label("Spawn Type", Style.Text.Normal, 68);
                            QUI.SetGUIBackgroundColor(AccentColorBlue);
                            QUI.Popup(locationSpawnType, 90);
                            QUI.ResetColors();
                        }
                        QUI.Space(SPACE_8);
                    }
                    QUI.EndHorizontal();
                    QUI.Space(SPACE_4);
                    switch(poolySpawner.spawnAt)
                    {
                        case PoolySpawner.SpawnAt.Position:
                            if(poolySpawner.spawnPositions == null || poolySpawner.spawnPositions.Count == 0) { poolySpawner.spawnPositions = new List<PoolySpawner.SpawnPosition> { new PoolySpawner.SpawnPosition(new Vector3(0, 1, 0)) }; }
                            if(poolySpawner.spawnPositions.Count == 1 && poolySpawner.locationSpawnType == PoolySpawner.SpawnType.Random) { poolySpawner.locationSpawnType = PoolySpawner.SpawnType.Sequential; }
                            for(int i = 0; i < poolySpawner.spawnPositions.Count; i++)
                            {
                                QUI.BeginHorizontal(WIDTH_420);
                                {
                                    tempContent = new GUIContent(i.ToString());
                                    tempContentSize = QStyles.CalcSize(tempContent, Style.Text.Small);
                                    QUI.Label(tempContent.text, Style.Text.Small, tempContentSize.x);
                                    QUI.BeginChangeCheck();
                                    Vector3 spawnPosition = poolySpawner.spawnPositions[i].spawnPosition;
                                    QUI.SetGUIBackgroundColor(AccentColorBlue);
                                    spawnPosition = QUI.Vector3(spawnPosition, (WIDTH_420 - SPACE_2 - tempContentSize.x - SPACE_8 - SPACE_16 - SPACE_4) - 240 * animBoolRandomSpawnLocation.faded);
                                    QUI.ResetColors();
                                    if(QUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(poolySpawner, "Changed Spawn Position " + i);
                                        poolySpawner.spawnPositions[i].spawnPosition = spawnPosition;
                                        QUI.SetDirty(poolySpawner);
                                    }
                                    if(QUI.ButtonMinus())
                                    {
                                        if(poolySpawner.spawnPositions.Count > 1)
                                        {
                                            Undo.RecordObject(poolySpawner, "Removed Spawn Position " + i);
                                            poolySpawner.spawnPositions.RemoveAt(i);
                                        }
                                        else if(poolySpawner.spawnPositions == null || poolySpawner.spawnPositions[0].spawnPosition != new Vector3(poolySpawner.transform.position.x, poolySpawner.transform.position.y + 1, poolySpawner.transform.position.z))
                                        {
                                            Undo.RecordObject(poolySpawner, "Reset Spawn Position");
                                            poolySpawner.spawnPositions = new List<PoolySpawner.SpawnPosition> { new PoolySpawner.SpawnPosition(new Vector3(poolySpawner.transform.position.x, poolySpawner.transform.position.y + 1, poolySpawner.transform.position.z)) };
                                        }
                                        QUI.SetDirty(poolySpawner);
                                        QUI.ExitGUI();
                                    }
                                    animBoolRandomSpawnLocation.target = locationSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random;
                                    if(poolySpawner.spawnPositions != null && poolySpawner.spawnPositions.Count > 1 && locationSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random)
                                    {
                                        QUI.Space(SPACE_2);
                                        QUI.Label("weight", Style.Text.Small, 40 * animBoolRandomSpawnLocation.faded);
                                        QUI.Space(-SPACE_8);
                                        QUI.BeginChangeCheck();
                                        int spawnPositionWeight = poolySpawner.spawnPositions[i].weight;
                                        QUI.SetGUIBackgroundColor(AccentColorBlue);
                                        spawnPositionWeight = EditorGUILayout.IntSlider(spawnPositionWeight, 0, 100);
                                        QUI.ResetColors();
                                        if(QUI.EndChangeCheck())
                                        {
                                            Undo.RecordObject(poolySpawner, "Changed Weight Of Spawn Position " + i);
                                            poolySpawner.spawnPositions[i].weight = spawnPositionWeight;
                                        }
                                    }
                                    QUI.FlexibleSpace();
                                    QUI.Space(SPACE_8);
                                }
                                QUI.EndHorizontal();
                                QUI.Space(1);
                            }
                            QUI.Space(SPACE_2);
                            QUI.BeginHorizontal(WIDTH_420);
                            {
                                QUI.FlexibleSpace();
                                if(QUI.ButtonPlus())
                                {
                                    Undo.RecordObject(poolySpawner, "Added Spawn Position");
                                    poolySpawner.spawnPositions.Add(new PoolySpawner.SpawnPosition(new Vector3(poolySpawner.transform.position.x, poolySpawner.transform.position.y + 1, poolySpawner.transform.position.z)));
                                    QUI.SetDirty(poolySpawner);
                                    QUI.ExitGUI();
                                }
                                QUI.Space(3 + 240 * animBoolRandomSpawnLocation.faded);
                            }
                            QUI.EndHorizontal();
                            break;
                        case PoolySpawner.SpawnAt.Transform:
                            if(poolySpawner.spawnPoints == null || poolySpawner.spawnPoints.Count == 0) { poolySpawner.spawnPoints = new List<PoolySpawner.SpawnPoint> { new PoolySpawner.SpawnPoint(null) }; }
                            if(poolySpawner.spawnPoints.Count == 1 && poolySpawner.locationSpawnType == PoolySpawner.SpawnType.Random) { poolySpawner.locationSpawnType = PoolySpawner.SpawnType.Sequential; }
                            infoMessage["NoSpawnPoint"].show.target = PoolySpawner.PoolySettings.showInfoMessages && !poolySpawner.HasSpawnPoints();
                            DrawInfoMessage("NoSpawnPoint", WIDTH_420);
                            for(int i = 0; i < poolySpawner.spawnPoints.Count; i++)
                            {
                                QUI.BeginHorizontal(WIDTH_420);
                                {
                                    tempContent = new GUIContent(i.ToString());
                                    tempContentSize = QStyles.CalcSize(tempContent, Style.Text.Small);
                                    QUI.Label(tempContent.text, Style.Text.Small, tempContentSize.x);
                                    QUI.BeginChangeCheck();
                                    Transform spawnPoint = poolySpawner.spawnPoints[i].spawnPoint;
                                    QUI.SetGUIBackgroundColor(AccentColorBlue);
                                    spawnPoint = (Transform)QUI.ObjectField(spawnPoint, typeof(Transform), true, (WIDTH_420 - SPACE_2 - tempContentSize.x - SPACE_8 - SPACE_16 - SPACE_4) - 240 * animBoolRandomSpawnLocation.faded);
                                    QUI.ResetColors();
                                    if(QUI.IsPersistent(spawnPoint)) { spawnPoint = poolySpawner.spawnPoints[i].spawnPoint; }
                                    if(QUI.EndChangeCheck())
                                    {
                                        Undo.RecordObject(poolySpawner, "Changed Reference For Spawn Point Slot " + i);
                                        poolySpawner.spawnPoints[i].spawnPoint = spawnPoint;
                                        QUI.SetDirty(poolySpawner);
                                    }
                                    QUI.BeginVertical(16);
                                    {
                                        QUI.Space(SPACE_2);
                                        if(QUI.ButtonMinus())
                                        {
                                            if(poolySpawner.spawnPoints.Count > 1)
                                            {
                                                Undo.RecordObject(poolySpawner, "Removed Spawn Point Slot " + i);
                                                poolySpawner.spawnPoints.RemoveAt(i);
                                            }
                                            else if(poolySpawner.spawnPoints == null || poolySpawner.spawnPoints[0].spawnPoint != null)
                                            {
                                                Undo.RecordObject(poolySpawner, "Reset Spawn Point Slot");
                                                poolySpawner.spawnPoints = new List<PoolySpawner.SpawnPoint> { new PoolySpawner.SpawnPoint(null) };
                                            }
                                            QUI.SetDirty(poolySpawner);
                                            QUI.ExitGUI();
                                        }
                                    }
                                    QUI.EndVertical();
                                    animBoolRandomSpawnLocation.target = locationSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random;
                                    if(poolySpawner.spawnPoints != null && poolySpawner.spawnPoints.Count > 1 && locationSpawnType.enumValueIndex == (int)PoolySpawner.SpawnType.Random)
                                    {
                                        QUI.Space(SPACE_2);
                                        QUI.Label("weight", Style.Text.Small, 40);
                                        QUI.Space(-SPACE_8);
                                        QUI.BeginChangeCheck();
                                        int spawnPointWeight = poolySpawner.spawnPoints[i].weight;
                                        QUI.SetGUIBackgroundColor(AccentColorBlue);
                                        spawnPointWeight = EditorGUILayout.IntSlider(spawnPointWeight, 0, 100);
                                        QUI.ResetColors();
                                        if(QUI.EndChangeCheck())
                                        {
                                            Undo.RecordObject(poolySpawner, "Changed Weight Of Spawn Point " + i);
                                            poolySpawner.spawnPoints[i].weight = spawnPointWeight;
                                        }
                                    }
                                    QUI.FlexibleSpace();
                                    QUI.Space(SPACE_8);
                                }
                                QUI.EndHorizontal();
                                QUI.Space(1);
                            }
                            QUI.Space(SPACE_2);
                            QUI.BeginHorizontal(WIDTH_420);
                            {
                                QUI.FlexibleSpace();
                                if(QUI.ButtonPlus())
                                {
                                    Undo.RecordObject(poolySpawner, "Added Spawn Point Slot");
                                    poolySpawner.spawnPoints.Add(new PoolySpawner.SpawnPoint(null));
                                    QUI.SetDirty(poolySpawner);
                                    QUI.ExitGUI();
                                }
                                QUI.Space(3 + 240 * animBoolRandomSpawnLocation.faded);
                            }
                            QUI.EndHorizontal();
                            break;
                    }
                }
                QUI.EndVertical();
            }
            QUI.EndFadeGroup();
        }

        private void DrawOnSpawnEvents()
        {
            QUI.Space(SPACE_8);
            QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Purple), WIDTH_420, 40);
            QUI.Space(-40);
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Space(SPACE_4);
                QUI.Label("UnityEvents Callbacks", Style.Text.Normal, 240, HEIGHT_16);
                QUI.FlexibleSpace();
            }
            QUI.EndHorizontal();
            QUI.BeginHorizontal(WIDTH_420);
            {
                QUI.Space(SPACE_4);
                QUI.SetGUIBackgroundColor(QColors.PurpleLight.Color);
                QUI.Label("OnSpawn", Style.Text.Normal, 66);
                QUI.FlexibleSpace();

                QUI.Toggle(showOnSpawnStarted);
                QUI.Space(-SPACE_2);
                tempContent.text = "Started";
                QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);


                QUI.Space(SPACE_8);

                QUI.Toggle(showOnSpawnStopped);
                QUI.Space(-SPACE_2);
                tempContent.text = "Stopped";
                QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                QUI.Space(SPACE_8);

                QUI.Toggle(showOnSpawnPaused);
                QUI.Space(-SPACE_2);
                tempContent.text = "Paused";
                QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                QUI.Space(SPACE_8);

                QUI.Toggle(showOnSpawnResumed);
                QUI.Space(-SPACE_2);
                tempContent.text = "Resumed";
                QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                QUI.Space(SPACE_8);

                QUI.Toggle(showOnSpawnFinished);
                QUI.Space(-SPACE_2);
                tempContent.text = "Finished";
                QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                QUI.Space(SPACE_8);

                QUI.ResetColors();
            }
            QUI.EndHorizontal();
            if(showOnSpawnStarted.boolValue || showOnSpawnPaused.boolValue || showOnSpawnFinished.boolValue)
            {
                QUI.Space(SPACE_4);
            }
            QUI.SetGUIBackgroundColor(AccentColorPurple);
            animBoolShowOnSpawnStarted.target = showOnSpawnStarted.boolValue;
            if(QUI.BeginFadeGroup(animBoolShowOnSpawnStarted.faded))
            {
                QUI.PropertyField(OnSpawnStarted, new GUIContent("OnSpawnStarted"), WIDTH_420 - 5);
            }
            QUI.EndFadeGroup();
            animBoolShowOnSpawnStopped.target = showOnSpawnStopped.boolValue;
            if(QUI.BeginFadeGroup(animBoolShowOnSpawnStopped.faded))
            {
                QUI.PropertyField(OnSpawnStopped, new GUIContent("OnSpawnStopped"), WIDTH_420 - 5);
            }
            QUI.EndFadeGroup();
            animBoolShowOnSpawnPaused.target = showOnSpawnPaused.boolValue;
            if(QUI.BeginFadeGroup(animBoolShowOnSpawnPaused.faded))
            {
                QUI.PropertyField(OnSpawnPaused, new GUIContent("OnSpawnPaused"), WIDTH_420 - 5);
            }
            QUI.EndFadeGroup();
            animBoolShowOnSpawnResumed.target = showOnSpawnResumed.boolValue;
            if(QUI.BeginFadeGroup(animBoolShowOnSpawnResumed.faded))
            {
                QUI.PropertyField(OnSpawnResumed, new GUIContent("OnSpawnResumed"), WIDTH_420 - 5);
            }
            QUI.EndFadeGroup();
            animBoolShowOnSpawnFinished.target = showOnSpawnFinished.boolValue;
            if(QUI.BeginFadeGroup(animBoolShowOnSpawnFinished.faded))
            {
                QUI.PropertyField(OnSpawnFinished, new GUIContent("OnSpawnfinished"), WIDTH_420 - 5);
            }
            QUI.EndFadeGroup();
            QUI.ResetColors();
        }

        private void DrawEditorSettings()
        {
            QUI.Space(SPACE_8);
            if(QUI.GhostBar("Editor Settings", QColors.Color.Gray, animBoolShowEditorSettings, WIDTH_420))
            {
                animBoolShowEditorSettings.target = !animBoolShowEditorSettings.target;
            }
            if(QUI.BeginFadeGroup(animBoolShowEditorSettings.faded))
            {
                QUI.Space(SPACE_8);
                QUI.BeginVertical();
                {

                    QUI.BeginHorizontal(WIDTH_420);
                    {
                        QUI.Label("Icons", Style.Text.Normal, 40);

                        QUI.FlexibleSpace();

                        QUI.BeginChangeCheck();
                        bool tempAlwaysShowIcons = PoolySpawner.PoolySettings.alwaysShowIcons;
                        tempAlwaysShowIcons = QUI.Toggle(tempAlwaysShowIcons);
                        if(QUI.EndChangeCheck())
                        {
                            savePoolySettings = true;
                            Undo.RecordObject(this, "Toggled AlwaysShowIcons");
                            PoolySpawner.PoolySettings.alwaysShowIcons = tempAlwaysShowIcons;
                            SceneView.RepaintAll();
                        }

                        tempContent.text = "Always Show";
                        QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                        QUI.Space(SPACE_8);

                        QUI.BeginChangeCheck();
                        bool tempShowSpawnerIcon = PoolySpawner.PoolySettings.showSpawnerIcon;
                        tempShowSpawnerIcon = QUI.Toggle(tempShowSpawnerIcon);
                        if(QUI.EndChangeCheck())
                        {
                            savePoolySettings = true;
                            Undo.RecordObject(this, "Toggled ShowSpawnerIcon");
                            PoolySpawner.PoolySettings.showSpawnerIcon = tempShowSpawnerIcon;
                            SceneView.RepaintAll();
                        }

                        tempContent.text = "Spawner";
                        QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                        QUI.Space(SPACE_8);

                        if(!useSpawnerAsSpawnLocation.boolValue)
                        {
                            QUI.BeginChangeCheck();
                            bool tempShowSpawnLocationsIcons = PoolySpawner.PoolySettings.showSpawnLocationsIcons;
                            tempShowSpawnLocationsIcons = QUI.Toggle(tempShowSpawnLocationsIcons);
                            if(QUI.EndChangeCheck())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Toggled ShowSpawnLocationsIcons");
                                PoolySpawner.PoolySettings.showSpawnLocationsIcons = tempShowSpawnLocationsIcons;
                                SceneView.RepaintAll();
                            }

                            tempContent.text = "Spawn Locations";
                            QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                            QUI.Space(SPACE_8);
                        }

                        QUI.BeginChangeCheck();
                        bool tempAllowIconScaling = PoolySpawner.PoolySettings.allowIconScaling;
                        tempAllowIconScaling = QUI.Toggle(tempAllowIconScaling);
                        if(QUI.EndChangeCheck())
                        {
                            savePoolySettings = true;
                            Undo.RecordObject(this, "Toggled AllowIconScaling");
                            PoolySpawner.PoolySettings.allowIconScaling = tempAllowIconScaling;
                            SceneView.RepaintAll();
                        }

                        tempContent.text = "Allow Scaling";
                        QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                        if(QUI.ButtonReset())
                        {
                            savePoolySettings = true;
                            Undo.RecordObject(this, "Reset icons settings");
                            PoolySpawner.PoolySettings.ResetIconsSetting();
                            SceneView.RepaintAll();
                        }
                    }
                    QUI.EndHorizontal();
                    if(QUI.BeginFadeGroup(animBoolShowSpawnLocationsOptions.faded))
                    {
                        QUI.Space(SPACE_4);
                        QUI.BeginHorizontal(WIDTH_420);
                        {
                            QUI.Label("Labels", Style.Text.Normal, 40);
                            QUI.FlexibleSpace();

                            QUI.BeginChangeCheck();
                            bool tempShowLabels = PoolySpawner.PoolySettings.showLabels;
                            tempShowLabels = QUI.Toggle(tempShowLabels);
                            if(QUI.EndChangeCheck())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Toggled ShowLabels");
                                PoolySpawner.PoolySettings.showLabels = tempShowLabels;
                                SceneView.RepaintAll();
                            }

                            tempContent.text = "Show";
                            QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                            QUI.Space(SPACE_8);

                            tempContent.text = "Decimal Points";
                            QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                            QUI.Space(-SPACE_2);
                            QUI.BeginChangeCheck();
                            int tempDecimalPoins = PoolySpawner.PoolySettings.decimalPoins;
                            tempDecimalPoins = EditorGUILayout.IntSlider(tempDecimalPoins, 0, 4);
                            if(QUI.EndChangeCheck())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Changed DecimalPoins");
                                PoolySpawner.PoolySettings.decimalPoins = tempDecimalPoins;
                                SceneView.RepaintAll();
                            }

                            if(QUI.ButtonReset())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Reset label settings");
                                PoolySpawner.PoolySettings.ResetLabelSettings();
                                SceneView.RepaintAll();
                            }
                        }
                        QUI.EndHorizontal();
                        QUI.Space(SPACE_4);
                        QUI.BeginHorizontal(WIDTH_420);
                        {
                            QUI.Label("Dotted Lines", Style.Text.Normal, 70);
                            QUI.FlexibleSpace();

                            QUI.BeginChangeCheck();
                            bool tempShowDottedLines = PoolySpawner.PoolySettings.showDottedLines;
                            tempShowDottedLines = QUI.Toggle(tempShowDottedLines);
                            if(QUI.EndChangeCheck())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Toggled dotted lines visiblility");
                                PoolySpawner.PoolySettings.showDottedLines = tempShowDottedLines;
                                SceneView.RepaintAll();
                            }

                            tempContent.text = "Show";
                            QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                            QUI.Space(SPACE_8);

                            QUI.BeginChangeCheck();
                            Color tempLinesColor = PoolySpawner.PoolySettings.dottedLinesColor;
                            tempLinesColor = QUI.ColorField(tempLinesColor, false, false, true, 20);
                            if(QUI.EndChangeCheck())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Changed dotted lines color");
                                PoolySpawner.PoolySettings.dottedLinesColor = tempLinesColor;
                                SceneView.RepaintAll();
                            }

                            tempContent.text = "Color";
                            QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                            QUI.Space(SPACE_8);

                            tempContent.text = "Size";
                            QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);

                            QUI.Space(-SPACE_2);
                            QUI.BeginChangeCheck();
                            float tempDottedLineScreenSpaceSize = PoolySpawner.PoolySettings.dottedLinesScreenSpaceSize;
                            tempDottedLineScreenSpaceSize = EditorGUILayout.Slider(tempDottedLineScreenSpaceSize, 1, 10);
                            if(QUI.EndChangeCheck())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Changed dotted lines size");
                                PoolySpawner.PoolySettings.dottedLinesScreenSpaceSize = tempDottedLineScreenSpaceSize;
                                SceneView.RepaintAll();
                            }

                            if(QUI.ButtonReset())
                            {
                                savePoolySettings = true;
                                Undo.RecordObject(this, "Reset dotted lines settings");
                                PoolySpawner.PoolySettings.ResetDottedLinesSettings();
                                SceneView.RepaintAll();
                            }
                        }
                        QUI.EndHorizontal();
                    }
                    QUI.EndFadeGroup();
                    QUI.Space(SPACE_4);
                    QUI.BeginHorizontal(WIDTH_420);
                    {
                        QUI.Label("Messages", Style.Text.Normal, 60, HEIGHT_16);
                        QUI.FlexibleSpace();

                        QUI.BeginChangeCheck();
                        bool tempShowInfoMessages = PoolySpawner.PoolySettings.showInfoMessages;
                        tempShowInfoMessages = QUI.Toggle(tempShowInfoMessages);
                        if(QUI.EndChangeCheck())
                        {
                            savePoolySettings = true;
                            Undo.RecordObject(this, "Toggled ShowInfoMessages");
                            PoolySpawner.PoolySettings.showInfoMessages = tempShowInfoMessages;
                            SceneView.RepaintAll();
                        }

                        tempContent.text = "Show Info Messages";
                        QUI.Label(tempContent.text, Style.Text.Small, QStyles.CalcSize(tempContent, Style.Text.Small).x);
                    }
                    QUI.EndHorizontal();
                }
                QUI.EndVertical();
            }
            QUI.EndFadeGroup();
            QUI.Space(SPACE_4);
        }

        private void SavePoolySettings()
        {
            QUI.SetDirty(PoolySpawner.PoolySettings);
            AssetDatabase.SaveAssets();
        }
    }
}
