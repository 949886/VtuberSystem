// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEditor;
using UnityEditor;

#pragma warning disable 0162
namespace Ez.Internal
{
    [InitializeOnLoad]
    public class PoolySymbolLoader
    {
        static PoolySymbolLoader()
        {
            EditorApplication.update += RunOnce;
        }

        static void RunOnce()
        {
            EditorApplication.update -= RunOnce;
            CreateMissingFolders();
            LoadSymbol();
        }

        static void CreateMissingFolders()
        {
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly", "Editor"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor/Resources")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly/Editor", "Resources"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly/Editor/Resources", "EZT"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT", "Pooly"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly/Settings")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly", "Settings"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly/Statistics")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly", "Statistics"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly/Version")) { AssetDatabase.CreateFolder(EZT.PATH + "/Pooly/Editor/Resources/EZT/Pooly", "Version"); }
        }

        static void LoadSymbol()
        {
#if EZ_SOURCE
            return;
#endif
            QUtils.AddScriptingDefineSymbol(EZT.SYMBOL_EZ_POOLY);
        }
    }
}
#pragma warning restore 0162
