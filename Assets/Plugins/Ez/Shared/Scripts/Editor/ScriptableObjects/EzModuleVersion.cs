// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using System;
using UnityEngine;

namespace Ez.Internal
{
    [Serializable]
    public class EzModuleVersion : ScriptableObject
    {
        public const string MODULE_VERSION_FILENAME = "ModuleVersion";

        public string versionNumber;
        public string releaseNotes;
    }
}
