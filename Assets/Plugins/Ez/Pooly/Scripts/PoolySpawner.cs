// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEngine.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Ez.Pooly
{
    public class PoolySpawner : MonoBehaviour
    {
#if UNITY_EDITOR

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

        void OnDrawGizmos() { if(PoolySettings.alwaysShowIcons) { DrawIcons(); } }
        void OnDrawGizmosSelected() { if(!PoolySettings.alwaysShowIcons) { DrawIcons(); } }
        void DrawIcons()
        {
            if(PoolySettings.showSpawnerIcon) { DrawSpawnerIcon(transform.position, PoolySettings.allowIconScaling); }
            if(useSpawnerAsSpawnLocation) { return; }
            if(!PoolySettings.showSpawnLocationsIcons) { return; }
            switch(spawnAt)
            {
                case SpawnAt.Position:
                    if(spawnPositions != null && spawnPositions.Count > 0)
                    {
                        for(int i = 0; i < spawnPositions.Count; i++)
                        {
                            DrawSpawnLocationsIcons(spawnPositions[i].spawnPosition, PoolySettings.allowIconScaling);
                        }
                    }
                    break;
                case SpawnAt.Transform:
                    if(spawnPoints != null && spawnPoints.Count > 0)
                    {
                        for(int i = 0; i < spawnPoints.Count; i++)
                        {
                            if(spawnPoints[i].spawnPoint == null) { continue; }
                            DrawSpawnLocationsIcons(spawnPoints[i].spawnPoint.position, PoolySettings.allowIconScaling);
                        }
                    }
                    break;
            }
        }
        void DrawSpawnerIcon(Vector3 position, bool allowScaling = true) { Gizmos.DrawIcon(transform.position, "Pooly/sceneIconPoolySpawner", allowScaling); }
        void DrawSpawnLocationsIcons(Vector3 position, bool allowScaling = true) { Gizmos.DrawIcon(position, "Pooly/sceneIconPoolySpawnerSpawnLocation", allowScaling); }
#endif

        #region SpawnPrefab / SpawnPoint / SpawnPosition
        [Serializable]
        public class SpawnPrefab
        {
            public Transform prefab;
            public int weight = 100;

            public SpawnPrefab(Transform prefab, int weight = 100)
            {
                this.prefab = prefab;
                this.weight = weight;
            }
        }

        [Serializable]
        public class SpawnPoint
        {
            public Transform spawnPoint = null;
            public int weight = 100;

            public SpawnPoint(Transform spawnPoint, int weight = 100)
            {
                this.spawnPoint = spawnPoint;
                this.weight = weight;
            }
        }

        [Serializable]
        public class SpawnPosition
        {
            public Vector3 spawnPosition = new Vector3(0, 1, 0);
            public int weight = 100;

            public SpawnPosition(Vector3 spawnPosition, int weight = 100)
            {
                this.spawnPosition = spawnPosition;
                this.weight = weight;
            }
        }

        private class SpawnParameters
        {
            public Vector3 spawnPosition = Vector3.zero;
            public Quaternion spawnRotation = Quaternion.identity;
            public Vector3 spawnScale = Vector3.one;
            public Transform parent = null;
        }
        #endregion

        /// <summary>
        /// When should the spawner automatically start
        /// </summary>
        public enum AutoStart
        {
            /// <summary>
            /// The spawner will automatically start in the Start method
            /// </summary>
            Start = 0,
            /// <summary>
            /// The spawner will automatically start in the OnSpawned method
            /// </summary>
            OnSpawned = 1,
            /// <summary>
            /// The spawner not start automatically. You are responsible to call the StartSpawn method yourself when needed.
            /// </summary>
            Never = 2
        }
        /// <summary>
        /// Set the spawner behaviour. Should it automatically start on Start, OnSpawned or Never.
        /// </summary>
        public AutoStart autoStart = AutoStart.Start;
        /// <summary>
        /// Start spawning after a set time delay.
        /// </summary>
        public float spawnStartDelay = 0f;
        /// <summary>
        /// Spawn cycles. How many prefabs will be spawned before the spawner stops spawning.
        /// </summary>
        public int spawnCount = 1;
        /// <summary>
        /// Spawner will spawn prefabs forever and it will stop only when you call either StopSpawn or PauseSpawn methods.
        /// <para>If true, it's your responsability to stop or pause the spawning for this spawner.</para>
        /// </summary>
        public bool spawnForever = false;
        /// <summary>
        /// Spawned cycles. How many prefabs have been spawned since the spawner started spawning.
        /// </summary>
        private int spawnedCount = 0;
        /// <summary>
        /// Fixed time interval between spawns.
        /// </summary>
        public float spawnInterval = 1f;
        /// <summary>
        /// Should the spawner spawn prefabs at set random time intervals (min and max).
        /// </summary>
        public bool spawnAtRandomIntervals = false;
        /// <summary>
        /// Minimum value for random time spawn interval.
        /// </summary>
        public float spawnAtRandomIntervalMinimum = 0.5f;
        /// <summary>
        /// Maximum value for random time spawn interval.
        /// </summary>
        public float spawnAtRandomIntervalMaximum = 2f;
        /// <summary>
        /// If autoStart is set to OnSpawned, should the spawner despawn itself when spawning finished (default:true).
        /// </summary>
        public bool despawnWhenFinished = true;
        /// <summary>
        /// Should the spawner also be used as the spawn location or should it spawn to one or more other set positions/points.
        /// </summary>
        public bool useSpawnerAsSpawnLocation = true;
        /// <summary>
        /// UnityEvent triggered when spawning has started.
        /// </summary>
        public UnityEvent OnSpawnStarted = new UnityEvent();
        /// <summary>
        /// Toggles visibility of the OnSpawnStarted UnityEvent in the PoolySpawner inspector.
        /// </summary>
        public bool showOnSpawnStarted = false;
        /// <summary>
        /// UnityEvent triggered when spawning has stopped.
        /// </summary>
        public UnityEvent OnSpawnStopped = new UnityEvent();
        /// <summary>
        /// Toggles visibility of the OnSpawnStopped UnityEvent in the PoolySpawner inspector.
        /// </summary>
        public bool showOnSpawnStopped = false;
        /// <summary>
        /// UnityEvent triggered when spawning was paused.
        /// </summary>
        public UnityEvent OnSpawnPaused = new UnityEvent();
        /// <summary>
        /// Toggles visibility of the OnSpawnPaused UnityEvent in the PoolySpawner inspector.
        /// </summary>
        public bool showOnSpawnPaused = false;
        /// <summary>
        /// UnityEvent triggered when spawning was resumed.
        /// </summary>
        public UnityEvent OnSpawnResumed = new UnityEvent();
        /// <summary>
        /// Toggles visibility of the OnSpawnResumed UnityEvent in the PoolySpawner inspector.
        /// </summary>
        public bool showOnSpawnResumed = false;
        /// <summary>
        /// UnityEvent triggered when spawning has finished.
        /// </summary>
        public UnityEvent OnSpawnFinished = new UnityEvent();
        /// <summary>
        /// Toggles visibility of the OnSpawnFinidhed UnityEvent in the PoolySpawner inspector.
        /// </summary>
        public bool showOnSpawnFinished = false;
        /// <summary>
        /// List of prefabs that this spawner will spawn
        /// </summary>
        public List<SpawnPrefab> spawnPrefabs = new List<SpawnPrefab>();
        /// <summary>
        /// This index is used by the spawner when there are more than one prefabs referenced and the prefabSpawnType is set to Sequential.
        /// </summary>
        public enum SpawnAt
        {
            Position,
            Transform
        }
        /// <summary>
        /// Is useSpawnerAsSpawnLocation is set to true, this will determine if the spawner spawns at set Vector3 coordinates (Positions) or at set points (Transforms).
        /// </summary>
        public SpawnAt spawnAt = SpawnAt.Position;
        public enum SpawnType
        {
            Sequential,
            Random
        }
        /// <summary>
        /// If there are more than one prefabs references, this determines how the next prefab to spawn is selected.
        /// <para>Sequential will start with the first prefab and then select the next one, and so on.</para>
        /// <para>Random will select a random prefab each time, taking into account each prefab's weight.</para>
        /// </summary>
        public SpawnType prefabSpawnType = SpawnType.Sequential;
        /// <summary>
        /// If there are more than one spawn locations (Positions or Transforms) set, this determines how the next spawn location is selected.
        /// <para>Sequential will start with the first spawn location and then select the next one, and so on.</para>
        /// <para>Random will select a random spawn location each time, taking into account each spawn's location weight.</para>
        /// </summary>
        public SpawnType locationSpawnType = SpawnType.Sequential;
        private int lastSpawnedPrefabIndex = -1;
        /// <summary>
        /// List of spawn positions(Vector3) that this spawner will spawn to.
        /// </summary>
        public List<SpawnPosition> spawnPositions = new List<SpawnPosition>();
        /// <summary>
        /// This index is used by the spawner when there are more than one spawn positions(Vector3) set and the locationSpawnType is set to Sequential.
        /// </summary>
        private int lastSpawnedPositionIndex = -1;
        /// <summary>
        /// If enabled, the spawned clone's rotation will match the rotation of the target transform. 
        /// </summary>
        public bool matchTransformRotation = false;
        /// <summary>
        /// If enabled, the spawned clone's scale will match the scale of the target transform.
        /// </summary>
        public bool matchTransformScale = false;
        /// <summary>
        /// If enabled, the spawned clone will be reparented under the target transform.
        /// </summary>
        public bool reparentUnderTransform = false;
        /// <summary>
        /// List of spawn points(Transforms) that this spawner will spawn to.
        /// </summary>
        public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        /// <summary>
        /// This index is used by the spawner when there are more than one spawn points(Transforms) referenced and the locationSpawnType is set to Sequential.
        /// </summary>
        private int lastSpawnedPointIndex = -1;
        /// <summary>
        /// Reference to the spawner's Coroutine. This manages the spawning operations as start/pause/stop spawn.
        /// </summary>
        private Coroutine cSpawn;
        /// <summary>
        /// WaitForSeconds used on spawn start to avoid GC.
        /// </summary>
        private WaitForSeconds startSpawnWaitForSeconds = null;
        /// <summary>
        /// Current duration for the startSpawnWaitForSeconds. Used to generate a new startSpawnWaitForSeconds if the start delay changes.
        /// </summary>
        private float startSpawnWFSCurrentDuration = 0f;
        /// <summary>
        /// WaitForSeconds used between spawns to avoid GC.
        /// </summary>
        private WaitForSeconds spawnIntervalWaitForSeconds = null;
        /// <summary>
        /// Current duration for the spawnIntervalWaitForSeconds. Used to generate a new spawnIntervalWaitForSeconds if the spawn delay changes.
        /// </summary>
        private float spawnIntervalWFSCurrentDuration = 0f;
        /// <summary>
        /// Auxiliary variable holding the data to be used when spawning sequentially.
        /// </summary>
        private SpawnParameters nextSpawnParams = new SpawnParameters();
        /// <summary>
        /// Auxiliary list that holds weights when calculating a random prefab to spawn or a random spawn position/point.
        /// </summary>
        private List<int> spawnChances = new List<int>();
        /// <summary>
        /// Returns true is the spawner is currently running a spawning cycle.
        /// </summary>
        public bool isSpawning { get { return cSpawn != null; } }

        bool AreSpawnerSettingsValid()
        {
            if(spawnPrefabs == null || spawnPrefabs.Count < 1) { Debug.LogWarning("[Pooly][PoolySpawner] Link at least one prefab, to spawn, for the '" + name + "' Pooly Spawner. Spawner disabled..."); return false; }
            if(autoStart != AutoStart.Never && spawnCount < 1) { Debug.LogWarning("[Pooly][PoolySpawner] The spawnCount should be greater than zero (0) for the '" + name + "' Pooly Spawner. Either increase the spawnCount, or set autoStart to Never. Spawner disabled..."); return false; }
            if(spawnAt == SpawnAt.Transform && !useSpawnerAsSpawnLocation)
            {
                if(spawnPoints == null || spawnPoints.Count < 1)
                {
                    Debug.LogWarning("[Pooly][PoolySpawner] Link at least one transform, to be used as a spawn point, to the '" + name + "' Pooly Spawner. Or set useSpawnerAsSpawnLocation as true. Spawner disabled...");
                    return false;
                }
            }
            return true;
        }

        bool IsAtLeastOnePrefabInTheSpawnPool()
        {
            return false;
        }

        void Start()
        {
            if(autoStart == AutoStart.Start) { StartSpawn(spawnStartDelay); }
        }

        void OnSpawned()
        {
            if(autoStart == AutoStart.OnSpawned) { StartSpawn(spawnStartDelay); }
        }

        void OnDespawned()
        {
            if(cSpawn != null) { InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnFinished); }
            InternalStopSpawn(true);
        }

        void OnDestroy()
        {
            if(cSpawn != null) { InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnFinished); }
            InternalStopSpawn(true);
        }

        /// <summary>
        /// Checks if a prefab has been referenced to the spawner.
        /// </summary>
        /// <param name="prefab">The target prefab.</param>
        bool IsRegisteredPrefab(Transform prefab)
        {
            return IndexOfPrefab(prefab) != -1;
        }
        /// <summary>
        /// Returns the index, in the spawner, of the prefab
        /// <para>Returns -1 if the prefab was not found referenced to the spawner.</para>
        /// </summary>
        /// <param name="prefab">The target prefab.</param>
        int IndexOfPrefab(Transform prefab)
        {
            if(prefab == null) { return -1; }
            if(spawnPrefabs == null || spawnPrefabs.Count == 0) { return -1; }
            for(int i = 0; i < spawnPrefabs.Count; i++) { if(spawnPrefabs[i].prefab == prefab) { return i; } }
            return -1;
        }
        /// <summary>
        /// Checks if a Transform exists in the spawnPoints list.
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <returns></returns>
        bool IsRegisteredSpawnPoint(Transform spawnPoint)
        {
            return IndexOfSpawnPoint(spawnPoint) != -1;
        }
        /// <summary>
        /// Returns the index of a Transform in the spawnPoints list.
        /// <para>Returns -1 if the it was not found.</para>
        /// </summary>
        /// <param name="spawnPoint"></param>
        /// <returns></returns>
        int IndexOfSpawnPoint(Transform spawnPoint)
        {
            if(spawnPoint == null || spawnPoints == null || spawnPoints.Count == -1) { return -1; }
            for(int i = 0; i < spawnPoints.Count; i++) { if(spawnPoint == spawnPoints[i].spawnPoint) { return i; } }
            return -1;
        }
        /// <summary>
        /// Adds a new prefab to the spawner.
        /// <para>(Optional) With a specified weight (default: 100).</para>
        /// </summary>
        /// <param name="prefab">The target prefab you want to add. Make sure it has been added to the Pooly pool, in order to be able to spawn it.</param>
        /// <param name="weight">The prefab's weight value if the prefabSpawnType is set to Random.</param>
        public void AddPrefabToSpawner(Transform prefab, int weight = 100)
        {
            if(prefab == null) { return; }
            weight = Mathf.Clamp(weight, 0, 100);
            if(spawnPrefabs == null) { spawnPrefabs = new List<SpawnPrefab>(); }
            if(!IsRegisteredPrefab(prefab)) { spawnPrefabs.Add(new SpawnPrefab(prefab, weight)); }
        }
        /// <summary>
        /// Removes a prefab from the spawner. If the prefab was not found, nothing will happen.
        /// </summary>
        /// <param name="prefab">The targe prefab you want to remove.</param>
        public void RemovePrefabFromSpawner(Transform prefab)
        {
            if(spawnPrefabs == null) { return; }
            if(!IsRegisteredPrefab(prefab)) { return; }
            spawnPrefabs.RemoveAt(IndexOfPrefab(prefab));
        }
        /// <summary>
        /// Replaces a prefab, that is referenced to the spawner, with a another one.
        /// </summary>
        /// <param name="oldPrefab">The old prefab you want replaced.</param>
        /// <param name="newPrefab">The new prefab.</param>
        public void ReplacePrefabFromSpawner(Transform oldPrefab, Transform newPrefab)
        {
            if(oldPrefab == null || newPrefab == null) { return; }
            if(spawnPrefabs == null || spawnPrefabs.Count == 0) { return; }
            if(IsRegisteredPrefab(oldPrefab)) { spawnPrefabs[IndexOfPrefab(oldPrefab)].prefab = newPrefab; }
        }
        /// <summary>
        /// Updates the weight of a prefab.
        /// <para>Note: The prefab value you pass should already be referenced to the spawner, as this method will not add a new entry from it.</para>
        /// </summary>
        /// <param name="prefab">The target prefab.</param>
        /// <param name="newWeight">The new weight value. Enter a value between 0 and 100 (any other values will get clamped).</param>
        public void UpdateSpawnChanceForPrefab(Transform prefab, int newWeight = 100)
        {
            if(prefab == null) { return; }
            if(spawnPrefabs == null || spawnPrefabs.Count == 0) { return; }
            if(!IsRegisteredPrefab(prefab)) { return; }
            newWeight = Mathf.Clamp(newWeight, 0, 100);
            spawnPrefabs[IndexOfPrefab(prefab)].weight = newWeight;
        }
        /// <summary>
        /// Updates the weight of a spawn position.
        /// </summary>
        /// <param name="positionIndex">The index of the position to be updated.</param>
        /// <param name="newWeight">The new weight value. Enter a value between 0 and 100 (any other values will get clamped).</param>
        public void UpdateSpawnChanceForSpawnPosition(int positionIndex, int newWeight)
        {
            if(positionIndex < 0 || positionIndex >= spawnPositions.Count) { return; }
            newWeight = Mathf.Clamp(newWeight, 0, 100);
            spawnPositions[positionIndex].weight = newWeight;
        }
        /// <summary>
        /// Updates the weight of a spawn point.
        /// </summary>
        /// <param name="spawnPoint">Spawn point to be updated.</param>
        /// <param name="newWeight">The new weight value. Enter a value between 0 and 100 (any other values will get clamped).</param>
        public void UpdateSpawnChanceForSpawnTransform(Transform spawnPoint, int newWeight)
        {
            if(spawnPoint == null || spawnPoints == null || spawnPoints.Count == 0) { return; }
            if(!IsRegisteredSpawnPoint(spawnPoint)) { return; }
            newWeight = Mathf.Clamp(newWeight, 0, 100);
            spawnPoints[IndexOfSpawnPoint(spawnPoint)].weight = newWeight;
        }
        /// <summary>
        /// Starts the spawn cycle instantly.
        /// </summary>
        public void StartSpawn()
        {
            StartSpawn(0, true, false);
        }
        /// <summary>
        /// Starts the spawn cycle, after the set time delay. This method is primarily meant to be used by UnityEvents in the Inspector.
        /// </summary>
        /// <param name="spawnStartDelay">Delay before spawning starts.</param>
        public void StartSpawnWithDelay(float spawnStartDelay)
        {
            StartSpawn(spawnStartDelay, true, false);
        }
        /// <summary>
        /// Starts the spawn cycle, after the set time delay.
        /// </summary>
        /// <param name="spawnStartDelay">Delay before spawning starts.</param>
        /// <param name="resetSpawnedCount">If the spawn cycle was previously paused (using the PauseSpawn method), you can force a restart by setting this value as true. Otherwise it will continue spawning from where it was paused.</param>
        public void StartSpawn(float spawnStartDelay, bool resetSpawnedCount = true, bool isResumeSpawn = false)
        {
            if(!AreSpawnerSettingsValid()) { return; }
            InternalStopSpawn(resetSpawnedCount);
            if(!isResumeSpawn) { ResetSpawnIndex(); }
            cSpawn = StartCoroutine(iStartSpawn(spawnStartDelay, isResumeSpawn));
        }
        /// <summary>
        /// Internal method to stop the spawn cycle.
        /// </summary>
        /// <param name="resetSpawnedCount">If set to false, it will act as PauseSpawn and won't not reset the spawnedCount counter.</param>
        /// <param name="shouldSendEvent">If set to true, then the OnSpawnStopped event is triggered.</param>
        private void InternalStopSpawn(bool resetSpawnedCount = true, bool shouldSendEvent = false)
        {
            if(cSpawn != null)
            {
                StopCoroutine(cSpawn);
                cSpawn = null;
            };
            if(resetSpawnedCount) { spawnedCount = 0; ResetSpawnIndex(); }
            if(shouldSendEvent) { InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnStopped); }
        }
        /// <summary>
        /// Stops the spawn cycle.
        /// </summary>
        /// <param name="resetSpawnedCount">If set to false, it will act as PauseSpawn and won't not reset the spawnedCount counter.</param>
        public void StopSpawn(bool resetSpawnedCount = true)
        {
            InternalStopSpawn(resetSpawnedCount, true);
        }
        /// <summary>
        /// Pauses the spawn cycle, without resetting the spawnedCount counter.
        /// </summary>
        public void PauseSpawn()
        {
            InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnPaused);
            InternalStopSpawn(false);
        }
        /// <summary>
        /// If the spawn cycle was previously paused (using the PauseSpawn method) it will continue spawning where it left off. Otherwise it will start a new spawn cycle ignoring the spawnStartDelay setting.
        /// </summary>
        public void ResumeSpawn()
        {
            InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnResumed);
            StartSpawn(0, false, true);
        }
        /// <summary>
        /// Spawns a prefab at one of the set locations without modifying the spawnedCount value.
        /// </summary>
        private Transform Spawn()
        {
            int prefabIndex = GetNextSpawnIndex();
            if(prefabIndex == -1) { Debug.LogWarning("[Pooly][PoolySpawner] The " + name + " PoolySpawner is trying to spawn a null prefab. Check your referenced prefab slots, one or more may be empty."); return null; }
            GenerateNextSpawnParametersSet();
            if(!matchTransformScale) { return Pooly.Spawn(spawnPrefabs[prefabIndex].prefab, nextSpawnParams.spawnPosition, nextSpawnParams.spawnRotation, nextSpawnParams.parent); }
            else { return Pooly.Spawn(spawnPrefabs[prefabIndex].prefab, nextSpawnParams.spawnPosition, nextSpawnParams.spawnRotation, nextSpawnParams.spawnScale, nextSpawnParams.parent); }
        }
        /// <summary>
        /// Spawns a prefab at one of the set locations if spawnForever is enabled or if spawnedCount smaller than spawnCount.
        /// <para>This also increases the spawnedCount</para>
        /// <para>This method is useful if you want to manually trigger the spawn of the next prefab in the spawn cycle.</para>
        /// </summary>
        public void SpawnNext()
        {
            if(CanSpawn)
            {
                spawnedCount++;
                Spawn();
            }
        }
        /// <summary>
        /// Spawns the prefab at one of the set locations if spawnForever is enabled or if spawnedCount is smaller than spawnCount.
        /// <para>This also increases the spawnedCount</para>
        /// <para>This method is useful if you want to manually trigger the spawn of the next prefab in the spawn cycle.</para>
        /// <para>Note: The prefab has to be registered to this spawner in order to work (and in the Pooly pool as well)</para>
        /// </summary>
        /// <param name="prefab">Target prefab.</param>
        public void SpawnNext(Transform prefab)
        {
            if(CanSpawn)
            {
                spawnedCount++;
                Spawn(prefab);
            }
        }
        /// <summary>
        /// Spawns the given prefab at one of the set locations without modifying the spawnedCount value.
        /// <para>Note: The prefab has to be registered to this spawner in order to work (and in the Pooly pool as well)</para>
        /// </summary>
        /// <param name="prefab">Target prefab.</param>
        private Transform Spawn(Transform prefab)
        {
            if(prefab == null) { return null; }
            if(!IsRegisteredPrefab(prefab)) { return null; }
            GenerateNextSpawnParametersSet();
            if(!matchTransformScale) { return Pooly.Spawn(prefab, nextSpawnParams.spawnPosition, nextSpawnParams.spawnRotation, nextSpawnParams.parent); }
            else { return Pooly.Spawn(prefab, nextSpawnParams.spawnPosition, nextSpawnParams.spawnRotation, nextSpawnParams.spawnScale, nextSpawnParams.parent); }
        }
        /// <summary>
        /// Spawns a prefab at one of the set locations if spawnForever is enabled or if spawnedCount smaller than spawnCount and returns a reference to the spawned clone.
        /// <para>This also increases the spawnedCount</para>
        /// <para>This method is useful if you want to manually trigger the spawn of the next prefab in the spawn cycle and you need a reference to the spawned clone.</para>
        /// </summary>
        public Transform SpawnNextAndGetReference()
        {
            if(CanSpawn)
            {
                spawnedCount++;
                return Spawn();
            }
            return null;
        }
        /// <summary>
        /// Spawns the prefab at one of the set locations if spawnForever is enabled or if spawnedCount is smaller than spawnCount and returns a reference to the spawned clone.
        /// <para>This also increases the spawnedCount</para>
        /// <para>This method is useful if you want to manually trigger the spawn of the next prefab in the spawn cycle and you need a reference to the spawned clone.</para>
        /// <para>Note: The prefab has to be registered in the spawner and in Pooly's pool as well!</para>
        /// </summary>
        /// <param name="prefab">Target prefab.</param>
        public Transform SpawnNextAndGetReference(Transform prefab)
        {
            if(CanSpawn)
            {
                spawnedCount++;
                return Spawn(prefab);
            }
            return null;
        }

        IEnumerator iStartSpawn(float startDelay, bool isResumeSpawn)
        {
            yield return null;
            if(startSpawnWaitForSeconds == null || startDelay != startSpawnWFSCurrentDuration)
            { startSpawnWaitForSeconds = new WaitForSeconds(startDelay); startSpawnWFSCurrentDuration = startDelay; }

            if(startDelay > 0) { yield return startSpawnWaitForSeconds; }
            if(!isResumeSpawn) { InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnStarted); }
            while(CanSpawn)
            {
                Spawn();
                spawnedCount++;
                if(GetSpawnInterval() > 0f) { yield return spawnIntervalWaitForSeconds; }
            }
            cSpawn = null;
            InvokeOnSpawnEvent(OnSpawnEvent.OnSpawnFinished);
            if(autoStart == AutoStart.OnSpawned && despawnWhenFinished) { Despawn(); }
        }
        /// <summary>
        /// Returns the current spawn interval value between spawns, taking into account if the spawning interval is set to be either a fixed or a random interval value.
        /// </summary>
        private float GetSpawnInterval()
        {
            if(spawnAtRandomIntervals)
            {
                if(spawnAtRandomIntervalMinimum < 0) { spawnAtRandomIntervalMinimum = 0; }
                if(spawnAtRandomIntervalMaximum < 0) { spawnAtRandomIntervalMaximum = 0; }
                if(spawnAtRandomIntervalMinimum == spawnAtRandomIntervalMaximum) { spawnInterval = spawnAtRandomIntervalMinimum; }
                else { spawnInterval = UnityEngine.Random.Range(spawnAtRandomIntervalMinimum, spawnAtRandomIntervalMaximum); }
            }

            if(spawnIntervalWaitForSeconds == null || spawnIntervalWFSCurrentDuration != spawnInterval)
            { spawnIntervalWaitForSeconds = new WaitForSeconds(spawnInterval); spawnIntervalWFSCurrentDuration = spawnInterval; }
            return spawnIntervalWFSCurrentDuration;
        }
        /// <summary>
        /// Returns the next prefab's index that will get spawned by the spawner, taking into account the prefabSpawnType setting.
        /// </summary>
        int GetNextSpawnIndex()
        {
            if(spawnPrefabs == null || spawnPrefabs.Count == 0) { return -1; }
            switch(prefabSpawnType)
            {
                case SpawnType.Sequential:
                    if(lastSpawnedPrefabIndex == -1 || lastSpawnedPrefabIndex == spawnPrefabs.Count - 1) { lastSpawnedPrefabIndex = 0; }
                    else { lastSpawnedPrefabIndex++; }
                    if(spawnPrefabs[lastSpawnedPrefabIndex].prefab == null) { return -1; }
                    return lastSpawnedPrefabIndex;
                case SpawnType.Random:
                    int index = GetRandomPrefabIndex();
                    if(spawnPrefabs[index].prefab == null) { return -1; }
                    return index;
            }
            return -1;
        }
        /// <summary>
        /// Resets the index of the last used prefab, point/position.
        /// </summary>
        private void ResetSpawnIndex()
        {
            lastSpawnedPrefabIndex = lastSpawnedPointIndex = lastSpawnedPositionIndex = -1;
        }
        /// <summary>
        /// The next spawn will have its spawp point/rotation/scale matched to the given Transform.
        /// </summary>
        private void GetSpawnParamsFromTransform(Transform tr)
        {
            nextSpawnParams.spawnPosition = tr.position;
            if(matchTransformRotation) { nextSpawnParams.spawnRotation = tr.rotation; }
            if(matchTransformScale) { nextSpawnParams.spawnScale = tr.localScale; }
            if(reparentUnderTransform) { nextSpawnParams.parent = tr; }
        }
        /// <summary>
        /// Returns the next spawn location, taking into account both the spawnAt and the locationSpawnType settings.
        /// </summary>
        private void GenerateNextSpawnParametersSet()
        {
            if(useSpawnerAsSpawnLocation) { GetSpawnParamsFromTransform(transform); return; }

            switch(locationSpawnType)
            {
                case SpawnType.Sequential:
                    switch(spawnAt)
                    {
                        case SpawnAt.Position:
                            if(spawnPositions == null || spawnPositions.Count == 0) { nextSpawnParams.spawnPosition = transform.position; return; } // spawning at points, only position matters
                            if(lastSpawnedPositionIndex == -1 || lastSpawnedPositionIndex == spawnPositions.Count - 1) { lastSpawnedPositionIndex = 0; }
                            else { lastSpawnedPositionIndex++; }
                            nextSpawnParams.spawnPosition = spawnPositions[lastSpawnedPositionIndex].spawnPosition;
                            return;
                        case SpawnAt.Transform:
                            if(spawnPoints == null || spawnPoints.Count == 0) { GetSpawnParamsFromTransform(transform); return; }
                            if(lastSpawnedPointIndex == -1 || lastSpawnedPointIndex == spawnPoints.Count - 1) { lastSpawnedPointIndex = 0; }
                            else { lastSpawnedPointIndex++; }
                            if(spawnPoints[lastSpawnedPointIndex].spawnPoint == null) { GetSpawnParamsFromTransform(transform); return; }
                            GetSpawnParamsFromTransform(spawnPoints[lastSpawnedPointIndex].spawnPoint);
                            return;
                    }
                    break;
                case SpawnType.Random:
                    switch(spawnAt)
                    {
                        case SpawnAt.Position:
                            if(spawnPositions == null || spawnPositions.Count == 0) { nextSpawnParams.spawnPosition = transform.position; return; } // spawning at points, only position matters
                            nextSpawnParams.spawnPosition = spawnPositions[GetRandomSpawnPositionIndex()].spawnPosition;
                            return;
                        case SpawnAt.Transform:
                            if(spawnPoints == null || spawnPoints.Count == 0) { GetSpawnParamsFromTransform(transform); return; }
                            int index = GetRandomSpawnPointIndex();
                            if(spawnPoints[index].spawnPoint == null) { GetSpawnParamsFromTransform(transform); return; }
                            GetSpawnParamsFromTransform(spawnPoints[index].spawnPoint);
                            return;
                    }
                    break;
            }
        }
        /// <summary>
        /// Returns a random prefab index, by doing a lottery that takes into account every prefab's weight setting.
        /// </summary>
        int GetRandomPrefabIndex()
        {
            if(spawnPrefabs == null || spawnPrefabs.Count == 0) { return -1; }
            spawnChances.Clear();
            int maxChance = 0;
            for(int i = 0; i < spawnPrefabs.Count; i++)
            {
                if(spawnPrefabs[i].weight == 0)
                {
                    spawnChances.Add(-1);
                }
                else
                {
                    maxChance = maxChance + spawnPrefabs[i].weight;
                    spawnChances.Add(maxChance);
                }
            }
            int randomChance = UnityEngine.Random.Range(0, maxChance);
            for(int i = 0; i < spawnChances.Count; i++)
            {
                if(spawnChances[i] == -1) { continue; }
                if(spawnChances[i] >= randomChance) { return i; }
            }
            return -1;
        }
        /// <summary>
        /// Returns a random spawn point index, by doing a lottery that takes into account every spawn point's weight setting.
        /// <para>WARNING: Method does not check for NULL or empty list!</para>
        /// </summary>
        int GetRandomSpawnPointIndex()
        {
            //if(spawnPoints == null || spawnPoints.Count == 0) { return -1; }
            spawnChances.Clear();
            int maxChance = 0;
            for(int i = 0; i < spawnPoints.Count; i++)
            {
                if(spawnPoints[i].weight == 0)
                {
                    spawnChances.Add(-1);
                }
                else
                {
                    maxChance = maxChance + spawnPoints[i].weight;
                    spawnChances.Add(maxChance);
                }
            }
            int randomChance = UnityEngine.Random.Range(0, maxChance);
            for(int i = 0; i < spawnChances.Count; i++)
            {
                if(spawnChances[i] == -1) { continue; }
                if(spawnChances[i] >= randomChance) { return i; }
            }
            return -1;
        }
        /// <summary>
        /// Returns a random spawn position index, by doing a lottery that takes into account every spawn position's weight setting.
        /// <para>WARNING: Method does not check for NULL or empty list!</para>
        /// </summary>
        int GetRandomSpawnPositionIndex()
        {
            //if(spawnPositions == null || spawnPositions.Count == 0) { return -1; }
            spawnChances.Clear();
            int maxChance = 0;
            for(int i = 0; i < spawnPositions.Count; i++)
            {
                if(spawnPositions[i].weight == 0)
                {
                    spawnChances.Add(-1);
                }
                else
                {
                    maxChance = maxChance + spawnPositions[i].weight;
                    spawnChances.Add(maxChance);
                }
            }
            int randomChance = UnityEngine.Random.Range(0, maxChance);
            for(int i = 0; i < spawnChances.Count; i++)
            {
                if(spawnChances[i] == -1) { continue; }
                if(spawnChances[i] >= randomChance) { return i; }
            }
            return -1;
        }
        /// <summary>
        /// Returns true if there is at least one prefab referenced to the spawnPrefabs list.
        /// </summary>
        public bool HasPrefabs()
        {
            if(spawnPrefabs == null || spawnPrefabs.Count == 0) { return false; }
            for(int i = 0; i < spawnPrefabs.Count; i++)
            {
                if(spawnPrefabs[i] != null && spawnPrefabs[i].prefab != null) { return true; }
            }
            return false;
        }
        /// <summary>
        /// Returns true if there is at least one transform referenced to the spawnPoints list.
        /// </summary>
        public bool HasSpawnPoints()
        {
            if(spawnPoints == null || spawnPoints.Count == 0) { return false; }
            for(int i = 0; i < spawnPoints.Count; i++)
            {
                if(spawnPoints[i] != null && spawnPoints[i].spawnPoint != null) { return true; }
            }
            return false;
        }
        public enum OnSpawnEvent { OnSpawnStarted, OnSpawnStopped, OnSpawnResumed, OnSpawnPaused, OnSpawnFinished }
        /// <summary>
        /// Add a listener for the specified spawn event callback.
        /// </summary>
        /// <param name="onSpawnEvent">Which callback should invoke the listener function.</param>
        /// <param name="listener">Listener to be invoked by the callback.</param>
        public void AddOnSpawnCallbackListener(OnSpawnEvent onSpawnEvent, UnityAction listener)
        {
            switch(onSpawnEvent)
            {
                case OnSpawnEvent.OnSpawnStarted: OnSpawnStarted.AddListener(listener); break;
                case OnSpawnEvent.OnSpawnStopped: OnSpawnStopped.AddListener(listener); break;
                case OnSpawnEvent.OnSpawnResumed: OnSpawnResumed.AddListener(listener); break;
                case OnSpawnEvent.OnSpawnPaused: OnSpawnPaused.AddListener(listener); break;
                case OnSpawnEvent.OnSpawnFinished: OnSpawnFinished.AddListener(listener); break;
            }
        }
        /// <summary>
        /// Remove a listener for the specified spawn event callback.
        /// </summary>
        /// <param name="onSpawnEvent">Which callback should stop invoking the listener function.</param>
        /// <param name="listener">Listener to be removed.</param>
        public void RemoveOnSpawnCallbackListener(OnSpawnEvent onSpawnEvent, UnityAction listener)
        {
            switch(onSpawnEvent)
            {
                case OnSpawnEvent.OnSpawnStarted: OnSpawnStarted.RemoveListener(listener); break;
                case OnSpawnEvent.OnSpawnStopped: OnSpawnStopped.RemoveListener(listener); break;
                case OnSpawnEvent.OnSpawnResumed: OnSpawnResumed.RemoveListener(listener); break;
                case OnSpawnEvent.OnSpawnPaused: OnSpawnPaused.RemoveListener(listener); break;
                case OnSpawnEvent.OnSpawnFinished: OnSpawnFinished.RemoveListener(listener); break;
            }
        }
        /// <summary>
        /// Invokes the selected onSpawnEvent
        /// </summary>
        /// <param name="onSpawnEvent">Which UnityEvent should get invoked.</param>
        private void InvokeOnSpawnEvent(OnSpawnEvent onSpawnEvent)
        {
            switch(onSpawnEvent)
            {
                case OnSpawnEvent.OnSpawnStarted: OnSpawnStarted.Invoke(); break;
                case OnSpawnEvent.OnSpawnStopped: OnSpawnStopped.Invoke(); break;
                case OnSpawnEvent.OnSpawnResumed: OnSpawnResumed.Invoke(); break;
                case OnSpawnEvent.OnSpawnPaused: OnSpawnPaused.Invoke(); break;
                case OnSpawnEvent.OnSpawnFinished: OnSpawnFinished.Invoke(); break;
            }
        }
        /// <summary>
        /// Despawns this gameObject by returning it to the Pooly pool
        /// This method is useful only if you have this gameObject spawned from Pooly, thus it exists in the pool.
        /// </summary>
        public void Despawn()
        {
            Pooly.Despawn(transform);
        }
        /// <summary>
        /// Retruns true if spawnForever is enabled or if spawnedCount is smaller than spawnCount
        /// </summary>
        public bool CanSpawn
        {
            get
            {
                return spawnForever || spawnedCount < spawnCount;
            }
        }
    }
}
