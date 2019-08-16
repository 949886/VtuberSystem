// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using Ez.Pooly.Statistics;
#endif

namespace Ez.Pooly
{
    public partial class Pooly
    {
        #region Item
        /// <summary>
        /// Contains specific settings for each pool object.
        /// </summary>
        [Serializable]
        public class Item
        {
            /// <summary>
            /// The category this item belongs to. This variable is used by the custom inspector to help sort the items and on runtime to create a gameobject, with the same name, as the item's parent.
            /// </summary>
            public string category = DEFAULT_CATEGORY_NAME;
            /// <summary>
            /// Reference to the prefab from which all clones are made from.
            /// </summary>
            public Transform prefab = null;
            /// <summary>
            /// The number of clones that are created at start.
            /// </summary>
            public int preloadCloneCount = 1;
            /// <summary>
            /// Limit the number of clones this prefab can have. (default: false)
            /// </summary>
            public bool limitCloneCount = false;
            /// <summary>
            /// If limitCloneCount is TRUE. This is the maximum number of clones this prefab can have.
            /// </summary>
            public int cloneCountLimit = 10;
            /// <summary>
            /// If TRUE, the preloadCloneCount clones will be created in two stages. (dafault: false)
            /// <para>1. In the first frame there will be created a clonesOnFirstFrame number of clones.</para>
            /// <para>2. In the following frames there will be created clonesPerFrame number of clones, each frame, until the preloadCloneCount has been reached.</para>
            /// </summary>
            public bool limitCloneCreationPerFrame = false;
            /// <summary>
            /// The number of clones that should get created in the first frame.
            /// </summary>
            public int clonesOnFirstFrame = 10;
            /// <summary>
            /// The number of clones that should get created each frame until the preloadCloneCount has been reached.
            /// </summary>
            public int clonesPerFrame = 20;
            /// <summary>
            /// The number of frames between when the clonesOnFirstFrames have been created and the clonesPerFrame start to get created.
            /// </summary>
            public int delayCreatingClonesForFrames = 0;
            /// <summary>
            /// Creates new clones be if all the preloadCloneCount are active and more are needed.
            /// </summary>
            public bool allowInstantiateMore = true;
            /// <summary>
            /// Recycles the oldest active clone. Useful when working with decals. You create a fixed number of clones and reuse them as needed.
            /// </summary>
            public bool allowRecycleClones = false;
            /// <summary>
            /// Prints all the relevant log messages for this particular item to console.
            /// </summary>
            public bool debug = false;

            /// <summary>
            /// Returns a deep clone of the Item
            /// </summary>
            public Item Clone()
            {
                return new Item
                {
                    category = category,
                    prefab = prefab,
                    preloadCloneCount = preloadCloneCount,
                    limitCloneCount = limitCloneCount,
                    cloneCountLimit = cloneCountLimit,
                    limitCloneCreationPerFrame = limitCloneCreationPerFrame,
                    clonesOnFirstFrame = clonesOnFirstFrame,
                    clonesPerFrame = clonesPerFrame,
                    delayCreatingClonesForFrames = delayCreatingClonesForFrames,
                    allowInstantiateMore = allowInstantiateMore,
                    allowRecycleClones = allowRecycleClones,
                    debug = debug,
                };
            }
        }
        #endregion
        #region PooledItem
        /// <summary>
        /// Contains specific settings for each pool object and info about it's active/disabled clones.
        /// </summary>
        [Serializable]
        public class PooledItem : Item
        {
            /// <summary>
            /// Reference to the transform component of the pool this pooled item belongs to.
            /// </summary>
            public Transform poolTransform = null;
            /// <summary>
            /// Reference to the transform component of the category this pooled item is parented to.
            /// </summary>
            public Transform categoryTransform = null;
            /// <summary>
            /// Spawned clones. All the clones that are activeInHierarchy.
            /// </summary>
            public List<Transform> ActiveClones = new List<Transform>();
            /// <summary>
            /// Despawned clones. All the clones that are not activeInHierarchy.
            /// </summary>
            public List<Transform> DisabledClones = new List<Transform>();
            /// <summary>
            /// Cleares the ActiveClones and the DisabledClones list and adds all the clones to the DisabledClones list.
            /// </summary>
            public PooledItem(List<Transform> clones) { ActiveClones.Clear(); DisabledClones.Clear(); DisabledClones.AddRange(clones); }
            /// <summary>
            /// Returns the number of clones (active and disabled) this pooled item has.
            /// </summary>
            public int CloneCount { get { return ActiveClones.Count + DisabledClones.Count; } }
#if UNITY_EDITOR
            //public PoolyStatistics.PrefabStats ownStats;
            public PoolyStatistics.StatisticsItem ownStats;
#endif
        }
        #endregion
    }
}
