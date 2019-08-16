// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEngine.Extensions;
using System;
using UnityEngine;

namespace Ez
{
    [Serializable]
    public partial class EZT
    {
        public const string SYMBOL_EZ_DEFINE_SYMBOLS = "EZ_DEFINE_SYMBOLS";
        public const string SYMBOL_EZ_DATA_MANAGER = "EZ_DATA_MANAGER";
        public const string SYMBOL_EZ_BIND = "EZ_BIND";
        public const string SYMBOL_EZ_POOLY = "EZ_POOLY";
        public const string SYMBOL_EZ_ADS = "EZ_ADS";
        public const string SYMBOL_EZ_ADS_UNITYADS = "EZ_ADS_UNITYADS";
        public const string SYMBOL_EZ_ADS_ADMOB = "EZ_ADS_ADMOB";
        public const string SYMBOL_EZ_ADS_HEYZAP = "EZ_ADS_HEYZAP";
        public const string SYMBOL_EZ_PLAYMAKER_SUPPORT = "EZ_PLAYMAKER_SUPPORT";
        public const string SYMBOL_EZ_BOLT_SUPPORT = "EZ_BOLT_SUPPORT";

        public const string RESOURCES_PATH_SOURCE_DATA = "EZT/Shared/Settings/";
        public const string RESOURCES_PATH_CONTROL_PANEL_WINDOW_SETTINGS = "EZT/Shared/Settings/";
        public const string RESOURCES_PATH_DEFINE_SYMBOLS_PRESETS = "EZT/DefineSymbols/Presets/";
        public const string RESOURCES_PATH_DATA_MANAGER_SETTINGS = "EZT/DataManager/Settings/";
        public const string RESOURCES_PATH_DATA_MANAGER_KEYS = "EZT/DataManager/Keys/";
        public const string RESOURCES_PATH_POOLY_SETTINGS = "EZT/Pooly/Settings/";
        public const string RESOURCES_PATH_POOLY_STATISTICS = "EZT/Pooly/Statistics/";
        public const string RESOURCES_PATH_ADS_SETTINGS = "EZT/Ads/Settings/";

        public const string RESOURCES_PATH_DEFINE_SYMBOLS_VERSION = "EZT/DefineSymbols/Version/";
        public const string RESOURCES_PATH_DATA_MANAGER_VERSION = "EZT/DataManager/Version/";
        public const string RESOURCES_PATH_BIND_VERSION = "EZT/Bind/Version/";
        public const string RESOURCES_PATH_POOLY_VERSION = "EZT/Pooly/Version/";
        public const string RESOURCES_PATH_ADS_VERSION = "EZT/Ads/Version/";


        private static string _EZ_PATH = "";
        public static string PATH
        {
            get
            {
                if(_EZ_PATH.IsNullOrEmpty())
                {
                    _EZ_PATH = QuickEngine.IO.File.GetRelativeDirectoryPath("Ez");
                }
                return _EZ_PATH;
            }
        }

        public static string RELATIVE_PATH_SOURCE_DATA { get { return PATH + "/Shared/Editor/Resources/" + RESOURCES_PATH_SOURCE_DATA; } }
        public static string RELATIVE_PATH_CONTROL_PANEL_WINDOW_SETTINGS { get { return PATH + "/Shared/Editor/Resources/" + RESOURCES_PATH_CONTROL_PANEL_WINDOW_SETTINGS; } }
        public static string RELATIVE_PATH_DEFINE_SYMBOLS_PRESETS { get { return PATH + "/DefineSymbols/Editor/Resources/" + RESOURCES_PATH_DEFINE_SYMBOLS_PRESETS; } }
        public static string RELATIVE_PATH_DATA_MANAGER { get { return PATH + "/DataManager/"; } }
        public static string RELATIVE_PATH_DATA_MANAGER_SETTINGS { get { return PATH + "/DataManager/Editor/Resources/" + RESOURCES_PATH_DATA_MANAGER_SETTINGS; } }
        public static string RELATIVE_PATH_DATA_MANAGER_KEYS { get { return PATH + "/DataManager/Resources/" + RESOURCES_PATH_DATA_MANAGER_KEYS; } }
        public static string RELATIVE_PATH_POOLY_SETTINGS { get { return PATH + "/Pooly/Editor/Resources/" + RESOURCES_PATH_POOLY_SETTINGS; } }
        public static string RELATIVE_PATH_POOLY_STATISTICS { get { return PATH + "/Pooly/Editor/Resources/" + RESOURCES_PATH_POOLY_STATISTICS; } }
        public static string RELATIVE_PATH_ADS_SETTINGS { get { return PATH + "/Ads/Resources/" + RESOURCES_PATH_ADS_SETTINGS; } }

        public static string RELATIVE_PATH_DEFINE_SYMBOLS_VERSION { get { return PATH + "/DefineSymbols/Editor/Resources/" + RESOURCES_PATH_DEFINE_SYMBOLS_VERSION; } }
        public static string RELATIVE_PATH_DATA_MANAGER_VERSION { get { return PATH + "/DataManager/Editor/Resources/" + RESOURCES_PATH_DATA_MANAGER_VERSION; } }
        public static string RELATIVE_PATH_BIND_VERSION { get { return PATH + "/Bind/Editor/Resources/" + RESOURCES_PATH_BIND_VERSION; } }
        public static string RELATIVE_PATH_POOLY_VERSION { get { return PATH + "/Pooly/Editor/Resources/" + RESOURCES_PATH_POOLY_VERSION; } }
        public static string RELATIVE_PATH_ADS_VERSION { get { return PATH + "/Ads/Editor/Resources/" + RESOURCES_PATH_ADS_VERSION; } }

        public enum AssetName { DefineSymbols, DataManager, Bind, Pooly, Ads }
        public static void DebugLog(AssetName assetName, string message) { Debug.Log("[EZ][" + assetName + "] " + message); }
        public static void DebugWarning(AssetName assetName, string message) { Debug.LogWarning("[EZ][" + assetName + "] " + message); }
        public static void DebugError(AssetName assetName, string message) { Debug.LogError("[EZ][" + assetName + "] " + message); }
    }
}
