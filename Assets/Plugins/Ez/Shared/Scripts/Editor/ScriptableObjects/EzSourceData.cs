// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEditor;
using QuickEngine.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Ez.Internal
{
    [Serializable]
    public class EzSourceData : ScriptableObject
    {
        public static EzSourceData _instance;
        public static EzSourceData Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = Q.GetResource<EzSourceData>(EZT.RESOURCES_PATH_SOURCE_DATA, "EzSourceData");

#if UNITY_EDITOR
                    if(_instance == null)
                    {
                        _instance = Q.CreateAsset<EzSourceData>(EZT.RELATIVE_PATH_SOURCE_DATA, "EzSourceData");
                    }
#endif
                }
                return _instance;
            }
        }

        public List<LinkButtonData> defineSymbolsMissingModuleButtons = new List<LinkButtonData>();
        public List<LinkButtonData> dataManagerMissingModuleButtons = new List<LinkButtonData>();
        public List<LinkButtonData> bindMissingModuleButtons = new List<LinkButtonData>();
        public List<LinkButtonData> poolyMissingModuleButtons = new List<LinkButtonData>();
        public List<LinkButtonData> adsMissingModuleButtons = new List<LinkButtonData>();
        [Space(20)]
        public List<LinkButtonData> defineSymbolsHelpButtons = new List<LinkButtonData>();
        public List<LinkButtonData> dataManagerHelpButtons = new List<LinkButtonData>();
        public List<LinkButtonData> bindHelpButtons = new List<LinkButtonData>();
        public List<LinkButtonData> poolyHelpButtons = new List<LinkButtonData>();
        public List<LinkButtonData> adsHelpButtons = new List<LinkButtonData>();
    }
}
