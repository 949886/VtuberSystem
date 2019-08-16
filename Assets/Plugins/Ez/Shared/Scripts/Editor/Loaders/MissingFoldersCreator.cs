// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using UnityEditor;

namespace Ez.Internal
{
    [InitializeOnLoad]
    public class MissingFoldersCreator
    {
        static MissingFoldersCreator()
        {
            EditorApplication.update += RunOnce;
        }

        static void RunOnce()
        {
            EditorApplication.update -= RunOnce;
            CreateMissingFolders();
        }

        static void CreateMissingFolders()
        {
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Shared/Editor")) { AssetDatabase.CreateFolder(EZT.PATH + "/Shared", "Editor"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Shared/Editor/Resources")) { AssetDatabase.CreateFolder(EZT.PATH + "/Shared/Editor", "Resources"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Shared/Editor/Resources/EZT")) { AssetDatabase.CreateFolder(EZT.PATH + "/Shared/Editor/Resources", "EZT"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Shared/Editor/Resources/EZT/Shared")) { AssetDatabase.CreateFolder(EZT.PATH + "/Shared/Editor/Resources/EZT", "Shared"); }
            if(!AssetDatabase.IsValidFolder(EZT.PATH + "/Shared/Editor/Resources/EZT/Shared/Settings")) { AssetDatabase.CreateFolder(EZT.PATH + "/Shared/Editor/Resources/EZT/Shared", "Settings"); }
        }
    }
}
