// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuickEngine.Core;
#if UNITY_EDITOR
using Ez.Pooly.Statistics;
#endif

namespace Ez.Pooly
{
    [DisallowMultipleComponent]
    public partial class Pooly : MonoBehaviour
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
#endif
        #region Constants: DEFAULT_CATEGORY_NAME, METHOD_NAME_ONSPAWNED, METHOD_NAME_ONDESPAWNED
        public const string DEFAULT_CATEGORY_NAME = "No Category";
        private const string METHOD_NAME_ONSPAWNED = "OnSpawned";
        private const string METHOD_NAME_ONDESPAWNED = "OnDespawned";
        #endregion
        #region TriggerOnSpawned, TriggerOnDespawned
        /// <summary>
        /// Trigger the 'OnSpawned' method on the target's components.
        /// </summary>
        private static void TriggerOnSpawned(Transform target) { target.BroadcastMessage(METHOD_NAME_ONSPAWNED, SendMessageOptions.DontRequireReceiver); }
        /// <summary>
        /// Trigger the 'OnDespawned' method on the target's components.
        /// </summary>
        private static void TriggerOnDespawned(Transform target) { target.BroadcastMessage(METHOD_NAME_ONDESPAWNED, SendMessageOptions.DontRequireReceiver); }
        #endregion
        #region Variables
        /// <summary>
        /// Prints all the relevant log messages to console.
        /// </summary>
        public bool debug = false;
        /// <summary>
        /// Adds new items, at runtime, to the pool if you try to Spawn/Despawn prefabs that are not in the pool.
        /// </summary>
        public bool autoAddMissingItems = false;
        private static bool Initialized = false;
        /// <summary>
        /// List of Items that contains all the references and settings for the pool items.
        /// </summary>
        public List<Item> items = new List<Item>();
        /// <summary>
        /// Dictionary populated at runtime with all the pooled items.
        /// </summary>
        public static Dictionary<string, PooledItem> Pool = new Dictionary<string, PooledItem>(StringComparer.OrdinalIgnoreCase);
        private static Transform poolTransform;
        /// <summary>
        /// Returns the transfrom of this pool.
        /// </summary>
        public static Transform PoolTransform { get { if(poolTransform == null) poolTransform = Instance.transform; return poolTransform; } }
        /// <summary>
        /// Dictionary of all the categories transforms that are used to parent the all of the prefabs clones
        /// </summary>
        public static Dictionary<string, Transform> Categories = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// Counter for active coroutines. If != 0 the instantiation of clones is not yet finished.
        /// </summary>
        private static int runningCoroutines = 0;
        #region Auxiliaries
        /// <summary>
        /// Auxiliary variable used in Spawn() to avoid GC.
        /// </summary>
        private static PooledItem auxSpawnPooledItem;
        /// <summary>
        /// Auxiliary variable used in Despawn() to avoid GC.
        /// </summary>
        private static PooledItem auxDespawnPooledItem;
        /// <summary>
        /// Auxiliary variable used in clone creation to avoid GC.
        /// </summary>
        private static Transform auxCloneTransform;
        /// <summary>
        /// Auxiliary variable used to manipulate a clone after it was spawned to avoid GC.
        /// </summary>
        private static Transform spawnedCloneTransform;
        /// <summary>
        /// Auxiliary variable that keeps a record of the scene where the Main Pool was created.
        /// </summary>
        public static string mainPoolSceneName;
        #endregion
        #endregion

        #region Singleton
        protected Pooly() { }
        private static Pooly _instance;
        public static Pooly Instance
        {
            get
            {
                if(_instance == null)
                {
                    if(applicationIsQuitting) { return null; }
                    GameObject singleton = new GameObject("(singleton) " + typeof(Pooly).ToString());
                    _instance = singleton.AddComponent<Pooly>();
                    DontDestroyOnLoad(singleton);
                }
                return _instance;
            }
        }

        private static bool applicationIsQuitting = false;
        private void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }
        #endregion

        void Awake()
        {
            if(_instance != null)
            {
                if(debug) { Debug.Log("[Pooly] There cannot be two Main Pools active at the same time. Destryoing this one!"); }
                Destroy(gameObject);
                return;
            }
            mainPoolSceneName = gameObject.scene.name;
            _instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }

        /// <summary>
        /// Performs the initial setup for Pooly
        /// </summary>
        public static void Initialize()
        {
            Initialized = false;
            Instance.CreateCategories();
            Pool.Clear();
            for(int i = 0; i < Instance.items.Count; i++)
            { Instance.CreatePooledItem(Instance.items[i], true); }
            Initialized = true;
        }

        /// <summary>
        /// This function allows checking if Pooly is currently instantiating prefab clones (regardless if they are its own clones or extension clones).
        /// </summary>
        /// <returns>True, as long as Pooly is instantiating prefab clones</returns>
        public static bool IsLoading()
        {
            return !(Initialized && (runningCoroutines == 0));
        }

        #region Spawn
        /// <summary>
        /// Spawns a clone of the given prefab as a child of the target parent, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Vector3 rotation, Transform parent) { return Spawn(prefab, position, Quaternion.Euler(rotation), parent); }
        /// <summary>
        /// Spawns a clone of the given prefab inside the pool, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Vector3 rotation) { return Spawn(prefab, position, Quaternion.Euler(rotation), null); }
        /// <summary>
        /// Spawns a clone of the given prefab inside the pool, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Quaternion rotation) { return Spawn(prefab, position, rotation, null); }
        /// <summary>
        /// Spawns a clone of the given prefab as a child of the target parent, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if(!Initialized)
            { Debug.LogWarning("[Pooly] Not has not finished it's initialization yet."); return null; }
            if(prefab == null)
            { Debug.LogWarning("[Pooly] Cannot spawn a null prefab."); return null; }
            if(Instance == null)
            { return null; } // Scene change in progress
            var itemName = GetPrefabNameFromClone(prefab);
            if(Pool.ContainsKey(itemName))
            { return Spawn(itemName, position, rotation, parent); }
            if(!Instance.autoAddMissingItems)
            { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); return null; }
            CreateMissingPooledItem(prefab, itemName, true);
            return Spawn(itemName, position, rotation, parent);
        }
        /// <summary>
        /// Spawns a clone of the given itemName as a child of the target parent, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Vector3 rotation, Transform parent) { return Spawn(itemName, position, Quaternion.Euler(rotation), parent); }
        /// <summary>
        /// Spawns a clone of the given itemName inside the pool, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Vector3 rotation) { return Spawn(itemName, position, Quaternion.Euler(rotation), null); }
        /// <summary>
        /// Spawns a clone of the given itemName inside the pool, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Quaternion rotation) { return Spawn(itemName, position, rotation, null); }
        /// <summary>
        /// Spawns a clone of the given itemName as a child of the target parent, at target position and rotated to the target rotation. 
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Quaternion rotation, Transform parent)
        {
            if(!Initialized)
            { Debug.LogWarning("[Pooly] Not has not initialized yet."); return null; }
            if(Instance == null || itemName == null)
            { return null; } // Scene change in progress
            if(!Pool.TryGetValue(itemName, out auxSpawnPooledItem))
            { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); return null; }
            if(auxSpawnPooledItem == null)
            { Pool.Remove(itemName); return null; }
            Transform clone = null;
            if(auxSpawnPooledItem.DisabledClones.Count == 0)
            {
                if(auxSpawnPooledItem.allowInstantiateMore)
                {
                    int cloneNumber = GetCloneCount(auxSpawnPooledItem);
                    if(auxSpawnPooledItem.limitCloneCount && cloneNumber >= auxSpawnPooledItem.cloneCountLimit)
                    { Debug.LogWarning("[Pooly] Object '" + itemName + "' reached it's clone count limit. Increase the Clone Count Limit or set Limit Clone Count as FALSE."); return null; }
                    else
                    { clone = CreateClone(auxSpawnPooledItem.prefab, cloneNumber + 1, auxSpawnPooledItem.categoryTransform); auxSpawnPooledItem.DisabledClones.Add(clone); }
                    if(Instance.debug || auxSpawnPooledItem.debug)
                    { Debug.Log("[Pooly] Created a new clone for object '" + itemName + "'."); }
                }
                else
                {
                    if(auxSpawnPooledItem.allowRecycleClones)
                    { clone = auxSpawnPooledItem.ActiveClones[0]; Despawn(clone); }
                    else
                    { Debug.LogWarning("[Pooly] Object '" + itemName + "' reached it's pooled limit. Increase Preload Clone Count or set Allow Instantiate More as TRUE or set Allow Recycle Clones as TRUE."); return null; }
                }
            }
            if(clone == null)
            { clone = auxSpawnPooledItem.DisabledClones[0]; } // else { TriggerOnDespawned(clone); }
            if(clone == null)
            { Debug.LogWarning("[Pooly] One or more clones of '" + itemName + "' prefab were destroyed. Please use Despawn instead of Destroy when disposing of a pooled object."); return null; }
            clone.SetPositionAndRotation(position, rotation);
            if(parent != null)
            { SetParent(clone, parent); }
            SetActive(clone, true);
            TriggerOnSpawned(clone);
            auxSpawnPooledItem.DisabledClones.Remove(clone);
            auxSpawnPooledItem.ActiveClones.Add(clone);
            if(Instance.debug || auxSpawnPooledItem.debug)
            { Debug.Log("[Pooly] Spawned a clone of '" + itemName + "'."); }
#if UNITY_EDITOR
            if(PoolySettings.enableStatistics)
            {
                auxSpawnPooledItem.ownStats.UpdateActiveCloneCount(auxSpawnPooledItem.preloadCloneCount, auxSpawnPooledItem.ActiveClones.Count);
            }
#endif
            return clone;
        }

        /// <summary>
        /// Spawns a clone of the given prefab as a child of the target parent, at target position, rotated to the target rotation with the specified localScale.
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Vector3 rotation, Vector3 localScale, Transform parent)
        {
            spawnedCloneTransform = Spawn(prefab, position, Quaternion.Euler(rotation), parent);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given prefab inside the pool, at target position, rotated to the target rotation with the specified localScale.
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            spawnedCloneTransform = Spawn(prefab, position, Quaternion.Euler(rotation), null);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given prefab inside the pool,  at target position, rotated to the target rotation with the specified localScale. 
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            spawnedCloneTransform = Spawn(prefab, position, rotation, null);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given prefab as a child of the target parent,  at target position, rotated to the target rotation with the specified localScale. 
        /// </summary>
        public static Transform Spawn(Transform prefab, Vector3 position, Quaternion rotation, Vector3 localScale, Transform parent)
        {
            spawnedCloneTransform = Spawn(prefab, position, rotation, parent);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given itemName as a child of the target parent, at target position, rotated to the target rotation with the specified localScale.  
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Vector3 rotation, Vector3 localScale, Transform parent)
        {
            spawnedCloneTransform = Spawn(itemName, position, Quaternion.Euler(rotation), parent);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given itemName inside the pool, at target position, rotated to the target rotation with the specified localScale. 
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Vector3 rotation, Vector3 localScale)
        {
            spawnedCloneTransform = Spawn(itemName, position, Quaternion.Euler(rotation), null);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given itemName inside the pool, at target position, rotated to the target rotation with the specified localScale.
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Quaternion rotation, Vector3 localScale)
        {
            spawnedCloneTransform = Spawn(itemName, position, rotation, null);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        /// <summary>
        /// Spawns a clone of the given itemName as a child of the target parent, at target position, rotated to the target rotation with the specified localScale.
        /// </summary>
        public static Transform Spawn(string itemName, Vector3 position, Quaternion rotation, Vector3 localScale, Transform parent)
        {
            spawnedCloneTransform = Spawn(itemName, position, rotation, parent);
            spawnedCloneTransform.localScale = localScale;
            return spawnedCloneTransform;
        }
        #endregion
        #region Despawn, DespawnAllPrefabs, DespawnAllPrefabsFromPool, DespawnAllPrefabsInCategory, DespawnAllClonesOfPrefab
        /// <summary>
        /// Despawn the given clone.
        /// </summary>
        public static void Despawn(Transform clone)
        {
            if(!Initialized)
            { Debug.LogWarning("[Pooly] Not has not finished its initialization yet."); return; }
            if(clone == null)
            { Debug.LogWarning("[Pooly] Cannot despawn a null prefab."); return; }
            if(Instance == null)
            { return; } // Scene change in progress
            string itemName = GetPrefabNameFromClone(clone);
            bool autoCreatedItem = false;
            if(!Pool.TryGetValue(itemName, out auxDespawnPooledItem))
            {
                if(Instance.autoAddMissingItems)
                { CreateMissingPooledItem(clone, itemName, false); auxDespawnPooledItem = Pool[itemName]; autoCreatedItem = true; }
                else
                { Debug.Log("[Pooly] The object '" + itemName + "', you are trying to Despawn, does not exist in the pool."); return; }
            }
            if(auxDespawnPooledItem.ActiveClones.Remove(clone) || autoCreatedItem)
            {
                if(clone.parent != auxDespawnPooledItem.categoryTransform)
                { SetParent(clone, auxDespawnPooledItem.categoryTransform); }
                TriggerOnDespawned(clone);
                SetActive(clone, false);
                auxDespawnPooledItem.DisabledClones.Add(clone);
                if(Instance.debug || auxDespawnPooledItem.debug)
                { Debug.Log("[Pooly] Despawned a clone of '" + itemName + "'."); }
            }
            else
            {
                Debug.Log("[Pooly] Tried to despawn a clone of '" + itemName + "', but it was already despawned!");
            }
        }
        /// <summary>
        /// Despawns all the clones of all the prefabs from every pool.
        /// </summary>
        public static void DespawnAllClones()
        {
            if(Instance == null)
            { return; } // Scene change in progress
            foreach(var pooledItem in Pool.Values)
            { DespawnAllClonesOfPrefab(pooledItem.prefab); }
        }
        /// <summary>
        /// Despawns all the clones of all the prefabs from the given Pooly Extension
        /// </summary>
        public static void DespawnAllClonesFromPoolExtension(PoolyExtension poolExtension)
        {
            if(Instance == null)
            { return; } // Scene change in progress
            if(poolExtension == null)
            { return; }
            foreach(var pooledItem in poolExtension.Pool.Values)
            { DespawnAllClonesOfPrefab(Pool[pooledItem.prefab.name].prefab); }
        }
        /// <summary>
        /// Despawns all the clones of all the prefabs in the given categoy
        /// </summary>
        public static void DespawnAllClonesInCategory(string category)
        {
            if(Instance == null)
            { return; } // Scene change in progress
            if(string.IsNullOrEmpty(category))
            { Debug.LogWarning("[Pooly] Cannot Despawn All Clones In Category because you provided an empty/null category name."); return; }
            foreach(var pooledItem in Pool.Values)
            { if(category.Equals(pooledItem.category)) { DespawnAllClonesOfPrefab(pooledItem.prefab); } }
        }
        /// <summary>
        /// Despawns all the clones of all the prefabs from the given Pooly Extension in the given category
        /// </summary>
        public static void DespawnAllClonesInCategory(PoolyExtension poolExtension, string category)
        {
            if(Instance == null)
            { return; } // Scene change in progress
            if(poolExtension == null)
            { return; }
            if(string.IsNullOrEmpty(category))
            { Debug.LogWarning("[Pooly] Cannot Despawn All Clones In Category because you provided an empty/null category name."); return; }
            foreach(var pooledItem in poolExtension.Pool.Values)
            { if(category.Equals(pooledItem.category)) { DespawnAllClonesOfPrefab(Pool[pooledItem.prefab.name].prefab); } }
        }
        /// <summary>
        /// Despawns all the clones of the given prefab
        /// </summary>
        public static void DespawnAllClonesOfPrefab(Transform prefab)
        {
            if(prefab == null || Pool == null || Pool.Count == 0)
            { return; }
            var itemName = GetPrefabNameFromClone(prefab);
            PooledItem item;
            if(Pool.TryGetValue(itemName, out item))
            {
                for(int i = item.ActiveClones.Count - 1; i >= 0; i--)
                { Despawn(item.ActiveClones[i]); }
            }
        }

        /// <summary>
        /// Despawns all the clones of the prefab with the given name
        /// </summary>
        /// <param name="prefabName"></param>
        public static void DespawnAllClonesOfPrefab(string prefabName)
        {
            prefabName = prefabName.Trim();
            if(string.IsNullOrEmpty(prefabName) || Pool == null || Pool.Count == 0)
            { return; }
            PooledItem item;
            if(Pool.TryGetValue(prefabName, out item))
            {
                for(int i = item.ActiveClones.Count - 1; i >= 0; i--)
                { Despawn(item.ActiveClones[i]); }
            }
            else
            {
                Debug.LogWarning("[Pooly] There is no prefab with the name " + prefabName + " in the pool");
            }
        }
        #endregion
        #region GetActiveCloneCount, HasActiveClones, GetDisabledCloneCount, HasDisabledClones, GetCloneCount
        /// <summary>
        /// Returns the number of active (spawned) clones for the given prefab.
        /// Returns -1 if the prefab was not found in the pool.
        /// </summary>
        public static int GetActiveCloneCount(Transform prefab, bool debug = false)
        {
            var itemName = GetPrefabNameFromClone(prefab);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return -1; }
            return Pool[itemName].ActiveClones.Count;
        }
        /// <summary>
        /// Returns the number of active (spawned) clones for the given itemName.
        /// Returns -1 if the itemName was not found in the pool.
        /// </summary>
        public static int GetActiveCloneCount(string itemName, bool debug = false)
        {
            itemName = RemoveCloneSuffix(itemName);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return -1; }
            return Pool[itemName].ActiveClones.Count;
        }
        /// <summary>
        /// Returns true if the given prefab has active (spawned) clones.
        /// Returns false if the given prefab has no active (spawned) clones or if it was not found in the pool.
        /// </summary>
        public static bool HasActiveClones(Transform prefab, bool debug = false)
        {
            var itemName = GetPrefabNameFromClone(prefab);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return false; }
            return Pool[itemName].ActiveClones.Count > 0;
        }
        /// <summary>
        /// Returns true if the given itemName has active (spawned) clones.
        /// Returns false if the given itemName has no active (spawned) clones or if it was not found in the pool.
        /// </summary>
        public static bool HasActiveClones(string itemName, bool debug = false)
        {
            itemName = RemoveCloneSuffix(itemName);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return false; }
            return Pool[itemName].ActiveClones.Count > 0;
        }
        /// <summary>
        /// Returns the number of disabled (despawned) clones available for the given prefab.
        /// Returns -1 if the prefab was not found in the pool.
        /// </summary>
        public static int GetDisabledCloneCount(Transform prefab, bool debug = false)
        {
            var itemName = GetPrefabNameFromClone(prefab);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return -1; }
            return Pool[itemName].DisabledClones.Count;
        }
        /// <summary>
        /// Returns the number of disabled (despawned) clones available for the given itemName.
        /// Returns -1 if the itemName was not found in the pool.
        /// </summary>
        public static int GetDisabledCloneCount(string itemName, bool debug = false)
        {
            itemName = RemoveCloneSuffix(itemName);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return -1; }
            return Pool[itemName].DisabledClones.Count;
        }
        /// <summary>
        /// Returns true if the given prefab has disabled (despawned) clones available.
        /// Returns false if the given prefab has no disabled (despawned) clones available or if it was not found in the pool.
        /// </summary>
        public static bool HasDisabledClones(Transform prefab, bool debug = false)
        {
            var itemName = GetPrefabNameFromClone(prefab);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return false; }
            return Pool[itemName].DisabledClones.Count > 0;
        }
        /// <summary>
        /// Returns true if the given itemName has disabled (despawned) clones available.
        /// Returns false if the given itemName has no disabled (despawned) clones available or if it was not found in the pool.
        /// </summary>
        public static bool HasDisabledClones(string itemName, bool debug = false)
        {
            itemName = RemoveCloneSuffix(itemName);
            if(!Pool.ContainsKey(itemName))
            { if(debug) { Debug.Log("[Pooly] Object '" + itemName + "' was not found in the pool."); } return false; }
            return Pool[itemName].DisabledClones.Count > 0;
        }
        /// <summary>
        /// Returns the number of clones (active and disabled) for the given pooled item
        /// </summary>
        private static int GetCloneCount(PooledItem pooledItem, PoolyExtension poolyExtension = null)
        {
            if(!Initialized)
            { Debug.LogWarning("[Pooly] Not has not finished it's initialization yet."); }
            return Initialized ? pooledItem.CloneCount : -1;
        }
        #endregion
        #region CreateCategories, GetCategoryTransform
        /// <summary>
        /// Creates a gameobject for each category and the Categories dictionary
        /// </summary>
        private void CreateCategories()
        {
            if(Pool == null || Pool.Count == 0) { return; }
            GameObject categoryGameObject = null;
            foreach(var value in Pool.Values)
            {
                if(value.poolTransform != transform)
                { continue; }
                value.category = value.category.Trim();
                if(string.IsNullOrEmpty(value.category))
                { value.category = Pooly.DEFAULT_CATEGORY_NAME; }
                if(!Categories.ContainsKey(value.category))
                {
                    categoryGameObject = new GameObject(value.category);
                    categoryGameObject.transform.SetParent(PoolTransform);
                    Categories.Add(value.category, categoryGameObject.transform);
                    value.categoryTransform = categoryGameObject.transform;
                    categoryGameObject = null;
                }
                else
                {
                    value.categoryTransform = Categories[value.category];
                }
                value.poolTransform = PoolTransform;
            }
        }
        /// <summary>
        /// Returns the category transform for the given item
        /// </summary>
        private Transform GetCategoryTransform(Item item)
        {
            item.category = item.category.Trim();
            if(string.IsNullOrEmpty(item.category))
            { item.category = Pooly.DEFAULT_CATEGORY_NAME; }
            if(!Categories.ContainsKey(item.category))
            { var go = new GameObject(item.category); go.transform.SetParent(PoolTransform); Categories.Add(item.category, go.transform); }
            return Categories[item.category];
        }
        /// <summary>
        /// Returns the category transform for the given category name
        /// </summary>
        private Transform GetCategoryTransform(string category)
        {
            category = category.Trim();
            if(string.IsNullOrEmpty(category))
            { category = Pooly.DEFAULT_CATEGORY_NAME; }
            if(!Categories.ContainsKey(category))
            { var go = new GameObject(category); go.transform.SetParent(PoolTransform); Categories.Add(category, go.transform); }
            return Categories[category];
        }
        #endregion
        #region CreatePooledItem, RemovePooledItemFromPool, CreateMissingPooledItem, CreateClone, CreateClones
        /// <summary>
        /// Creates a new PooledItem and adds it to the main pool (Pooly)
        /// </summary>
        private void CreatePooledItem(Item item, bool onInitialize = false)
        {
            if(!onInitialize)
                Instance.items.Add(item);
            if(item.preloadCloneCount <= 0)
            { return; } //TODO: Add debug message
            if(item.prefab == null)
            { Debug.Log(onInitialize ? "[Pooly] There is an Item with no prefab referenced in " + item.category + "." : "[Pooly] Object cannot have a null prefab."); return; }
            if(Pooly.Pool.ContainsKey(item.prefab.name))
            { Debug.Log("[Pooly] Object '" + item.prefab.name + "' already exists in the pool."); return; }
            StartCoroutine(CreateClones(item, PoolTransform, GetCategoryTransform(item), null));
        }
        /// <summary>
        /// Removes the pooledItem, that has the speficied name, from the main pool.
        /// </summary>
        /// <param name="pooledItemName">The prefab name of the pooled item that will be removed from the main pool</param>
        public static void RemovePooledItemFromPool(string pooledItemName)
        {
            if(!Pool.ContainsKey(pooledItemName))
            { return; }
            Pool.Remove(pooledItemName);
        }
        /// <summary>
        /// Creates a missing PooledItem and adds it to the pool.
        /// </summary>
        private static void CreateMissingPooledItem(Transform prefab, string itemName, bool spawn)
        {
            var clones = new List<Transform>();
            if(spawn)
            { clones.Add(CreateClone(prefab, clones.Count + 1, Instance.GetCategoryTransform(DEFAULT_CATEGORY_NAME))); }
            var pooledItem = new PooledItem(clones)
            {
                prefab = prefab,
                poolTransform = PoolTransform,
                categoryTransform = Instance.GetCategoryTransform(DEFAULT_CATEGORY_NAME)
            };
            Instance.items.Add(new Item() { prefab = prefab });
            Pool.Add(itemName, pooledItem);
#if UNITY_EDITOR
            if(PoolySettings.enableStatistics)
            {
                //pooledItem.ownStats = PoolyStatistics.ActivateStatsForPrefab(itemName, DEFAULT_CATEGORY_NAME, prefab, 0, null);
                pooledItem.ownStats = PoolyStatistics.GetStatisticsItem(null, DEFAULT_CATEGORY_NAME, itemName, prefab, 0);
            }
#endif
            if(Instance.debug)
            { Debug.Log("[Pooly] Object " + itemName + " was added to the pool."); }
        }
        /// <summary>
        /// Creates a clone of the given prefab. The clone will be inactive by default.
        /// </summary>
        private static Transform CreateClone(Transform prefab, int cloneNumber, Transform parent)
        {
            Transform clone = Instantiate(prefab, parent.position, prefab.rotation) as Transform;
            clone.name = prefab.name + " (Clone " + cloneNumber + ")";
            SetParent(clone, parent);
            SetActive(clone, false);
            return clone;
        }
        /// <summary>
        /// Created a pooled item for the given item and all the clones that need to be preloaded, in the specified pattern.
        /// </summary>
        public IEnumerator CreateClones(Item item, Transform poolTransform, Transform categoryTransform, PoolyExtension poolyExtension)
        {
            runningCoroutines++;
            var pooledItem = new PooledItem(new List<Transform>())
            {
                category = item.category,
                prefab = item.prefab,
                preloadCloneCount = item.preloadCloneCount,
                limitCloneCount = item.limitCloneCount,
                cloneCountLimit = item.cloneCountLimit,
                limitCloneCreationPerFrame = item.limitCloneCreationPerFrame,
                clonesOnFirstFrame = item.clonesOnFirstFrame,
                clonesPerFrame = item.clonesPerFrame,
                delayCreatingClonesForFrames = item.delayCreatingClonesForFrames,
                allowInstantiateMore = item.allowInstantiateMore,
                allowRecycleClones = item.allowRecycleClones,
                debug = item.debug,
                poolTransform = poolTransform,
                categoryTransform = categoryTransform
            };

            Pooly.Pool.Add(item.prefab.name, pooledItem);
#if UNITY_EDITOR
            if(PoolySettings.enableStatistics)
            {
                //pooledItem.ownStats = PoolyStatistics.ActivateStatsForPrefab(item.prefab.name, item.category, item.prefab, item.preloadCloneCount, poolyExtension);
                pooledItem.ownStats = PoolyStatistics.GetStatisticsItem(poolyExtension, item.category, item.prefab.name, item.prefab, item.preloadCloneCount);
            }
#endif
            int cloneNumber = 1;
            auxCloneTransform = null;
            if(item.limitCloneCreationPerFrame)
            {
                if(item.clonesOnFirstFrame > 0)
                {
                    while(cloneNumber <= item.preloadCloneCount && cloneNumber <= item.clonesOnFirstFrame)
                    {
                        auxCloneTransform = CreateClone(item.prefab, cloneNumber, categoryTransform);
                        Pooly.Pool[item.prefab.name].DisabledClones.Add(auxCloneTransform);
                        cloneNumber++;
                    }
                }

                if(item.delayCreatingClonesForFrames > 0)
                {
                    int delayedFrames = 0;
                    while(delayedFrames <= item.delayCreatingClonesForFrames)
                    { yield return null; delayedFrames++; }
                }
                while(cloneNumber <= item.preloadCloneCount)
                {
                    auxCloneTransform = CreateClone(item.prefab, cloneNumber, categoryTransform);
                    Pooly.Pool[item.prefab.name].DisabledClones.Add(auxCloneTransform);
                    if(cloneNumber % item.clonesPerFrame == 0)
                    { yield return null; }
                    cloneNumber++;
                }
            }
            else
            {
                while(cloneNumber <= item.preloadCloneCount)
                {
                    auxCloneTransform = CreateClone(item.prefab, cloneNumber, categoryTransform);
                    Pooly.Pool[item.prefab.name].DisabledClones.Add(auxCloneTransform);
                    cloneNumber++;
                }
            }
            if(poolyExtension != null)
            { poolyExtension.Pool.Add(item.prefab.name, pooledItem); }
            runningCoroutines--;
        }
        #endregion
        #region GetPrefabNameFromClone, RemoveCloneSuffix, SetParent, SetActive
        /// <summary>
        /// Returns the prefab name of a clone.
        /// It does that by removing the ' (Clone n)' ending from the clone.name
        /// </summary>
        public static string GetPrefabNameFromClone(Transform clone)
        {
            if(clone == null)
            { return null; }
            return RemoveCloneSuffix(clone.name);
        }
        /// <summary>
        /// Removes the ' (Clone n)' ending from itemName
        /// </summary>
        private static string RemoveCloneSuffix(string itemName)
        {
            int prefabNameLength = itemName.IndexOf(" (Clone ", StringComparison.Ordinal);
            if(prefabNameLength > -1)
            { return itemName.Substring(0, prefabNameLength); }
            return itemName;
        }
        /// <summary>
        /// Sets the target's parent checking if the target has a RectTransform or not
        /// </summary>
        private static void SetParent(Transform target, Transform parent)
        {
            if(target.GetComponent<RectTransform>() == null)
            { target.SetParent(parent); }
            else
            { target.GetComponent<RectTransform>().SetParent(parent); }
        }
        /// <summary>
        /// Sets the target's gameobject active state.
        /// </summary>
        private static void SetActive(Transform target, bool isActive)
        {
            target.gameObject.SetActive(isActive);
        }
        #endregion
    }
}
