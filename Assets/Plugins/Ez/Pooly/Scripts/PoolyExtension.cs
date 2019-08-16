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
    public class PoolyExtension : MonoBehaviour
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
        #region Variables
        public List<Pooly.Item> items = new List<Pooly.Item>();
        public Dictionary<string, Pooly.PooledItem> Pool = new Dictionary<string, Pooly.PooledItem>(StringComparer.OrdinalIgnoreCase);

        private Transform poolTransform;
        public Transform PoolTransform { get { if(poolTransform == null) poolTransform = transform; return poolTransform; } }

        public Dictionary<string, Transform> Categories = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);

        private bool registeredToPooly = false;
        private bool initialized = false;
        public bool Initialized { get { return initialized; } }
        #endregion

        private void Awake() { CreateCategories(); }
        private void OnEnable() { StartCoroutine("iRegisterItemsToPooly"); }
        private void OnDisable() { UnregisterItemsFromPooly(); }

        #region RegisterItemsToPooly, UnregisterItemsFromPooly
        /// <summary>
        /// Adds the items configured in this pool extension, as pooled items, to the main pool (Pooly)
        /// </summary>
        private void RegisterItemsToPooly()
        {
            if(registeredToPooly) { return; }
            if(items == null || items.Count == 0) { Debug.LogWarning("[Pooly] The '" + name + "' Pooly Extension is empty."); return; }
            for(int i = 0; i < items.Count; i++)
            {
                if(items[i].prefab == null)
                {
                    Debug.Log("[Pooly] There is an object with no prefab referenced in " + items[i].category + " in the " + name + " pool extension.");
                    continue;
                }
                if(Pooly.Pool.ContainsKey(items[i].prefab.name))
                {
                    Debug.Log("[Pooly] Object '" + items[i].prefab.name + "', found in the '" + name + "' pool extension, has already been added to the pool either by another Pooly Extension or by Pooly. No extra clones were created.");
                    continue;
                }
                if(Initialized)
                {
                    Pooly.Pool.Add(items[i].prefab.name, Pool[items[i].prefab.name]);
#if UNITY_EDITOR
                    if(PoolySettings.enableStatistics)
                    {
                        var pooledItem = Pool[items[i].prefab.name];
                        //pooledItem.ownStats = PoolyStatistics.ActivateStatsForPrefab(pooledItem.prefab.name, pooledItem.category, pooledItem.prefab, pooledItem.preloadCloneCount, this);
                        pooledItem.ownStats = PoolyStatistics.GetStatisticsItem(this, pooledItem.category, pooledItem.prefab.name, pooledItem.prefab, pooledItem.preloadCloneCount);
                    }
#endif
                }
                else
                {
                    Pooly.Instance.StartCoroutine(Pooly.Instance.CreateClones(items[i], PoolTransform, GetCategoryTransform(items[i]), this));
                }
            }
            registeredToPooly = true;
            initialized = true;
        }
        IEnumerator iRegisterItemsToPooly()
        {
            yield return new WaitForEndOfFrame();
            RegisterItemsToPooly();
        }
        /// <summary>
        /// Removes the pooled items that were registered by this pool extension
        /// </summary>
        private void UnregisterItemsFromPooly()
        {
            if(!registeredToPooly) { return; }
            if(Pool.Keys.Count > 0)
            {
                Pooly.DespawnAllClonesFromPoolExtension(this);
                foreach(var pooledItemName in Pool.Keys)
                {
                    Pooly.RemovePooledItemFromPool(pooledItemName);
                }
            }
            registeredToPooly = false;
        }
        #endregion
        #region GetCategoryTransform, CreateCategories
        /// <summary>
        /// Returns the category transform for the given item
        /// </summary>
        private Transform GetCategoryTransform(Pooly.Item item)
        {
            item.category = item.category.Trim();
            if(string.IsNullOrEmpty(item.category)) { item.category = Pooly.DEFAULT_CATEGORY_NAME; }
            if(!Categories.ContainsKey(item.category))
            {
                var go = new GameObject(item.category);
                go.transform.SetParent(PoolTransform);
                Categories.Add(item.category, go.transform);
            }
            return Categories[item.category];
        }
        /// <summary>
        /// Creates a gameobject for each category and the Categories dictionary
        /// </summary>
        private void CreateCategories()
        {
            if(Pool == null || Pool.Count == 0) { return; }
            GameObject categoryGameObject = null;
            foreach(var value in Pool.Values)
            {
                value.category = value.category.Trim();
                if(string.IsNullOrEmpty(value.category)) { value.category = Pooly.DEFAULT_CATEGORY_NAME; }
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
        #endregion
    }
}
