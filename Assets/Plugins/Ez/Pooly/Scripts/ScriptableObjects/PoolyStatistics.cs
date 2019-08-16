// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

#if UNITY_EDITOR
using QuickEngine.Core;
using System;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
#endif
using UnityEngine;

namespace Ez.Pooly.Statistics
{
    public class PoolyStatistics : ScriptableObject
    {
#if UNITY_EDITOR
        public const int MAXIMUM_NUMBER_OF_RECORDS = 12;

        public int poolyWidth = 350;

        public int defaultLowWarningThreshold = 5;
        public int defaultHighWarningThreshold = 95;

        public int databaseLineHorizontalIndent = 8;
        public int databaseElementHorizontalSpacing = 2;
        public int databaseLineVerticalSpacing = 2;
        public int databaseLineHeight = 16;

        public int databaseItemButtonWidth = 180;
        public int databaseClearStatisticsButtonWidth = 160;
        public int databaseDeleteButtonWidth = 130;

        public int graphLineWidth = 2;
        public int graphPointSize = 16;
        public int graphHeight = 128;
        public int graphPointToPointDistance = 76;

        public enum PoolType
        {
            MainPool,
            PoolExtension
        }

        /// <summary>
        /// List of all the pools (MainPool and all the PoolExtenstions) that were instantiated at runtime.
        /// </summary>
        public List<StatisticsPool> pools = new List<StatisticsPool>();

        private static PoolyStatistics _instance;
        public static PoolyStatistics Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Q.GetResource<PoolyStatistics>(EZT.RESOURCES_PATH_POOLY_STATISTICS, "PoolyStatistics");
                    if(_instance == null)
                    {
                        _instance = Q.CreateAsset<PoolyStatistics>(EZT.RELATIVE_PATH_POOLY_STATISTICS, "PoolyStatistics");
                    }
                }
                return _instance;
            }
        }

        public static StatisticsItem GetStatisticsItem(PoolyExtension originPool, string category, string prefabName, Transform prefab, int preloadedClones)
        {
            //See if this scene is in the pools list (by default we consider this the MainPool)
            string targetSceneName = Pooly.mainPoolSceneName;
            PoolType targetPoolType = PoolType.MainPool;

            if(originPool != null) //this is a pool extension
            {
                targetSceneName = originPool.gameObject.scene.name;
                targetPoolType = PoolType.PoolExtension;
            }

            StatisticsPool statisticsPool = null;
            StatisticsPoolCategory statisticsPoolCategory = null;
            StatisticsItem statisticsItem = null;

            if(Instance.pools == null) //the pools list null
            {
                Instance.pools = new List<StatisticsPool>(); //initialize the pools list
            }
            if(Instance.pools.Count == 0) //the pools list empty
            {
                statisticsPool = new StatisticsPool(targetSceneName, targetPoolType); //create a new StatisticsPool
                statisticsPoolCategory = new StatisticsPoolCategory(category); //create a new category for the new StatisticsPool
                statisticsPool.categories.Add(statisticsPoolCategory); //add the new category to the new StatisticsPool
                statisticsItem = new StatisticsItem(prefabName, prefab);
                statisticsItem.data.Add(new StatisticsItemData(preloadedClones, 0)); //add initial item data
                statisticsPoolCategory.items.Add(statisticsItem); //add the StatisticsItem to the new category under the new StatisticsPool
                Instance.pools.Add(statisticsPool); //add the newly created StatisticsPool to the pools list
                return statisticsItem; //return the reference to the StatisticsItem in order to quickly update its data values
            }

            if(statisticsPool == null) // a new StatisticsPool was NOT created
            {
                for(int i = 0; i < Instance.pools.Count; i++) //look for the pool where this prefab should be in
                {
                    if(Instance.pools[i].sceneName == targetSceneName && Instance.pools[i].poolType == targetPoolType)
                    {
                        statisticsPool = Instance.pools[i];
                        break;
                    }
                }
            }

            if(statisticsPool == null) //the StatisticsPool we were looking for does not exist, it needs to be created
            {
                statisticsPool = new StatisticsPool(targetSceneName, targetPoolType); //create a new StatisticsPool
                statisticsPoolCategory = new StatisticsPoolCategory(category); //create a new category for the new StatisticsPool
                statisticsPool.categories.Add(statisticsPoolCategory); //add the new category to the new StatisticsPool
                statisticsItem = new StatisticsItem(prefabName, prefab); //create a new StatisticsItem
                if(statisticsItem.data.Count == MAXIMUM_NUMBER_OF_RECORDS) { statisticsItem.data.RemoveAt(0); }
                statisticsItem.data.Add(new StatisticsItemData(preloadedClones, 0)); //add initial item data
                statisticsPoolCategory.items.Add(statisticsItem); //add the StatisticsItem to the new category under the new StatisticsPool
                Instance.pools.Add(statisticsPool); //add the newly created StatisticsPool to the pools list
                return statisticsItem; //return the reference to the StatisticsItem in order to quickly update its data values
            }

            //if we got here then we found a StatisticsPool where this prefab should be in
            //start searching for the prefab category

            int categoryIndex = statisticsPool.HasCategory(category);
            if(categoryIndex != -1) //the category exists in this pool
            {
                statisticsItem = statisticsPool.categories[categoryIndex].GetStatisticsItem(prefabName, prefab);
                if(statisticsItem != null) //the StatisticsItem was found
                {
                    if(statisticsItem.data.Count == MAXIMUM_NUMBER_OF_RECORDS) { statisticsItem.data.RemoveAt(0); }
                    statisticsItem.data.Add(new StatisticsItemData(preloadedClones, 0)); //add initial item data
                    return statisticsItem;  //return the reference to the StatisticsItem in order to quickly update its data values
                }

                //the item does not exist in this category, we need to add it
                statisticsItem = new StatisticsItem(prefabName, prefab); //create a new StatisticsItem
                if(statisticsItem.data.Count == MAXIMUM_NUMBER_OF_RECORDS) { statisticsItem.data.RemoveAt(0); }
                statisticsItem.data.Add(new StatisticsItemData(preloadedClones, 0)); //add initial item data
                statisticsPool.categories[categoryIndex].items.Add(statisticsItem);
                return statisticsItem; //return the reference to the StatisticsItem in order to quickly update its data values
            }

            //the category does not exist, we need to create it
            statisticsPoolCategory = new StatisticsPoolCategory(category); //create a new category for the new StatisticsPool
            statisticsPool.categories.Add(statisticsPoolCategory); //add the new category to the new StatisticsPool
            statisticsItem = new StatisticsItem(prefabName, prefab); //create a new StatisticsItem
            if(statisticsItem.data.Count == MAXIMUM_NUMBER_OF_RECORDS) { statisticsItem.data.RemoveAt(0); }
            statisticsItem.data.Add(new StatisticsItemData(preloadedClones, 0)); //add initial item data
            statisticsPoolCategory.items.Add(statisticsItem); //add the StatisticsItem to the new category under the new StatisticsPool
            return statisticsItem; //return the reference to the StatisticsItem in order to quickly update its data values
        }

        public void DeletePool(StatisticsPool pool)
        {
            if(pools == null || pools.Count == 0 || !pools.Contains(pool)) { return; }
            pools.Remove(pool);
        }

        [Serializable]
        public class StatisticsPool
        {
            /// <summary>
            /// The scene this pool can be found in.
            /// </summary>
            public string sceneName = "Unknown Scene";
            /// <summary>
            /// Pooly type: MainPool or PoolExtension
            /// </summary>
            public PoolType poolType = PoolType.MainPool;
            /// <summary>
            /// Remembers this pool's open state in the ControlPanelWindow.
            /// </summary>
            public AnimBool isExpanded = new AnimBool(false);
            /// <summary>
            /// List of all the categories found in this pool.
            /// </summary>
            public List<StatisticsPoolCategory> categories = new List<StatisticsPoolCategory>();

            public StatisticsPool(string sceneName, PoolType poolType)
            {
                this.sceneName = sceneName;
                this.poolType = poolType;
                this.isExpanded = new AnimBool(false);
                this.categories = new List<StatisticsPoolCategory>();
            }

            /// <summary>
            /// If it returns -1 then the category was not found.
            /// </summary>
            public int HasCategory(string category)
            {
                if(categories == null || categories.Count == 0) { return -1; }
                for(int i = 0; i < categories.Count; i++)
                {
                    if(categories[i].categoryName == category)
                    {
                        return i;
                    }
                }
                return -1;
            }

            /// <summary>
            /// If it returns null then the StatisticsItem was not fround.
            /// </summary>
            public StatisticsItem GetStatisticsItem(string prefabName, Transform prefab)
            {
                if(categories == null || categories.Count == 0) { return null; }
                for(int i = 0; i < categories.Count; i++)
                {
                    if(categories[i].GetStatisticsItem(prefabName, prefab) != null)
                    {
                        return categories[i].GetStatisticsItem(prefabName, prefab);
                    }
                }
                return null;
            }

            public void ClosePool()
            {
                if(categories != null && categories.Count > 0)
                {
                    for(int categoryIndex = 0; categoryIndex < categories.Count; categoryIndex++)
                    {
                        isExpanded.target = false;
                        categories[categoryIndex].CloseCategory();
                    }
                }
            }

            public void ClearStatistics()
            {
                if(categories == null) { categories = new List<StatisticsPoolCategory>(); }
                for(int i = 0; i < categories.Count; i++)
                {
                    categories[i].ClearStatistics();
                }

            }

            public void DeleteCategory(StatisticsPoolCategory category)
            {
                if(categories == null || categories.Count == 0 || !categories.Contains(category)) { return; }
                categories.Remove(category);
            }
        }

        [Serializable]
        public class StatisticsPoolCategory
        {
            /// <summary>
            /// The category this item belongs to.
            /// </summary>
            public string categoryName = string.Empty;
            /// <summary>
            /// Remembers this category's open state in the ControlPanelWindow.
            /// </summary>
            public AnimBool isExpanded = new AnimBool(false);
            /// <summary>
            /// List of all the items found in this category.
            /// </summary>
            public List<StatisticsItem> items = new List<StatisticsItem>();

            public bool hasInfoMessage = false;
            public bool hasWarningMessage = false;
            public bool hasErrorMessage = false;

            public StatisticsPoolCategory(string categoryName)
            {
                this.categoryName = categoryName;
                this.isExpanded = new AnimBool(false);
                this.items = new List<StatisticsItem>();
                this.hasInfoMessage = false;
                this.hasWarningMessage = false;
                this.hasErrorMessage = false;
            }

            public void UpdateMessageFlags()
            {
                hasInfoMessage = false;
                hasWarningMessage = false;
                hasErrorMessage = false;

                if(items == null || items.Count == 0)
                {
                    return;
                }

                for(int i = 0; i < items.Count; i++)
                {
                    if(!items[i].warningsEnabled.target) { continue; }
                    if(items[i].GetItemStatus() == StatisticsItem.ItemStatus.UnderLowThreshold) { hasInfoMessage = true; }
                    else if(items[i].GetItemStatus() == StatisticsItem.ItemStatus.OverHighThreshold) { hasWarningMessage = true; }
                    else if(items[i].GetItemStatus() == StatisticsItem.ItemStatus.ExceededPreloadCount) { hasErrorMessage = true; }
                }

            }

            /// <summary>
            /// If it returns null then the StatisticsItem was not fround.
            /// </summary>
            public StatisticsItem GetStatisticsItem(string prefabName, Transform prefab)
            {
                if(items == null || items.Count == 0) { return null; }
                for(int i = 0; i < items.Count; i++)
                {
                    if(items[i].prefabName == prefabName && items[i].prefab == prefab)
                    {
                        return items[i];
                    }
                }
                return null;
            }

            public void CloseCategory()
            {
                if(items != null && items.Count > 0)
                {
                    for(int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                    {
                        isExpanded.target = false;
                        items[itemIndex].CloseItem();
                    }
                }
            }

            public void ClearStatistics()
            {
                if(items == null) { items = new List<StatisticsItem>(); }
                for(int i = 0; i < items.Count; i++)
                {
                    items[i].ClearStatistics();
                }
            }

            public void DeleteItem(StatisticsItem item)
            {
                if(items == null || items.Count == 0 || !items.Contains(item)) { return; }
                items.Remove(item);
            }
        }

        [Serializable]
        public class StatisticsItem
        {
            public enum ItemStatus
            {
                OptimumSettings,
                UnderLowThreshold,
                OverHighThreshold,
                ExceededPreloadCount,
                NoStats
            }

            /// <summary>
            /// The name of the prefab.
            /// </summary>
            public string prefabName = string.Empty;
            /// <summary>
            /// Reference to the prefab from which all clones are made from.
            /// </summary>
            public Transform prefab = null;
            /// <summary>
            /// The maximum number of clones that were ever spawned at once time during runtime.
            /// </summary>
            public int alltimeMaxSpawnCount = 0;

            /// <summary>
            /// List of recorded statistics item data duting runtime.
            /// </summary>
            public List<StatisticsItemData> data = new List<StatisticsItemData>();

            /// <summary>
            /// Shows the graph view.
            /// </summary>
            public AnimBool showGraph = new AnimBool(false);
            /// <summary>
            /// Toggles the warning threshholds
            /// </summary>
            public AnimBool warningsEnabled = new AnimBool(true);
            public float lowWarningThreshold = Instance.defaultLowWarningThreshold;
            public float highWarningThreshold = Instance.defaultHighWarningThreshold;

            /// <summary>
            /// Shows the warning threshold message. If there is a message to show.
            /// </summary>
            public AnimBool showInfoMessage = new AnimBool(false);

            /// <summary>
            /// Returns the last recorded value for the preloded clones count (the last entry in the data list).
            /// </summary>
            public int LastRecordedPreloadedClones
            {
                get
                {
                    return data == null || data.Count == 0 ? 0 : data[data.Count - 1].preloadedClones;
                }
            }

            /// <summary>
            /// Returns the average maximum spawn count for the recorded stats (usually the last 10 entries as there is a cap to the maximum number of records set to 10).
            /// </summary>
            public int AverageSpawnCount
            {
                get
                {
                    if(data == null || data.Count == 0)
                    {
                        return 0;
                    }
                    else
                    {
                        float sum = 0;
                        for(int i = 0; i < data.Count; i++)
                        {
                            sum += data[i].maxSpawnCount;
                        }
                        return (int) sum / data.Count;
                    }
                }
            }

            public StatisticsItem(string prefabName, Transform prefab)
            {
                this.prefabName = prefabName;
                this.prefab = prefab;
                this.data = new List<StatisticsItemData>();
                this.lowWarningThreshold = Instance.defaultLowWarningThreshold;
                this.highWarningThreshold = Instance.defaultHighWarningThreshold;
                this.showGraph = new AnimBool(false);
                this.warningsEnabled = new AnimBool(true);
                this.warningsEnabled.speed = 4;
                this.showInfoMessage = new AnimBool(false);
                this.showInfoMessage.speed = 4;
            }

            public void UpdateActiveCloneCount(int preloadCloneCount, int newActiveCloneCount)
            {
                int index = data.Count - 1;

                data[index].preloadedClones = preloadCloneCount; //update the preloadedClones count (in case the developer changed it)
                data[index].maxSpawnCount = Mathf.Max(data[index].maxSpawnCount, newActiveCloneCount); //update the maximumUsage if that is the case
                alltimeMaxSpawnCount = Mathf.Max(alltimeMaxSpawnCount, newActiveCloneCount);
            }

            public ItemStatus GetItemStatus()
            {
                if(data == null || data.Count == 0) { return ItemStatus.NoStats; }                                                                      //No stats (data is null or empty)
                if(alltimeMaxSpawnCount <= LastRecordedPreloadedClones * (lowWarningThreshold / 100)) { return ItemStatus.UnderLowThreshold; }        //Warning Threshold - UNDER LOW WARNING THRESHOLD
                if(alltimeMaxSpawnCount > LastRecordedPreloadedClones) { return ItemStatus.ExceededPreloadCount; }                                      //EXCEEDED PRELOADED CLONE COUNT
                if(alltimeMaxSpawnCount >= LastRecordedPreloadedClones * (highWarningThreshold / 100)) { return ItemStatus.OverHighThreshold; }        //Warning Threshold - OVER HIGH WARNING THRESHOLD
                return ItemStatus.OptimumSettings;                                                                                                      //Optimum Settings (No Warning)
            }

            public void CloseItem()
            {
                showGraph.target = false;
            }

            public void ClearStatistics()
            {
                if(data == null) { data = new List<StatisticsItemData>(); }
                data.Clear();
                alltimeMaxSpawnCount = 0;
            }

            public void ResetMaximumSpawnCount()
            {
                alltimeMaxSpawnCount = 0;
            }
        }

        [Serializable]
        public class StatisticsItemData
        {
            /// <summary>
            /// The number of clones that were created at start, for this particular run through.
            /// </summary>
            public int preloadedClones = 0;
            /// <summary>
            /// The maximum number of clones that were spawned at once time during runtime, for this particular run through.
            /// </summary>
            public int maxSpawnCount = 0;

            public StatisticsItemData(int preloadedClones, int maximumUsage)
            {
                this.preloadedClones = preloadedClones;
                this.maxSpawnCount = maximumUsage;
            }
        }
#endif  
    }
}