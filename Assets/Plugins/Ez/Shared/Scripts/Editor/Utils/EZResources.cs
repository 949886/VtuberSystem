// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEditor;

namespace Ez
{
    public partial class EZResources
    {
        private static string _IMAGES;
        public static string IMAGES { get { if(string.IsNullOrEmpty(_IMAGES)) { _IMAGES = EZT.PATH + "/Shared/Images/"; } return _IMAGES; } }

        private static string _ADS;
        public static string ADS { get { if(string.IsNullOrEmpty(_ADS)) { _ADS = IMAGES + "Ads/"; } return _ADS; } }

        private static string _EDITORHEADERS;
        public static string EDITORHEADERS { get { if(string.IsNullOrEmpty(_EDITORHEADERS)) { _EDITORHEADERS = IMAGES + "EditorHeaders/"; } return _EDITORHEADERS; } }

        private static string _GENERAL;
        public static string GENERAL { get { if(string.IsNullOrEmpty(_GENERAL)) { _GENERAL = IMAGES + "General/"; } return _GENERAL; } }

        private static string _ICONS;
        public static string ICONS { get { if(string.IsNullOrEmpty(_ICONS)) { _ICONS = IMAGES + "Icons/"; } return _ICONS; } }

        private static string _PAGEICONS;
        public static string PAGEICONS { get { if(string.IsNullOrEmpty(_PAGEICONS)) { _PAGEICONS = IMAGES + "PageIcons/"; } return _PAGEICONS; } }

        private static string _PLACEHOLDERS;
        public static string PLACEHOLDERS { get { if(string.IsNullOrEmpty(_PLACEHOLDERS)) { _PLACEHOLDERS = IMAGES + "Placeholders/"; } return _PLACEHOLDERS; } }

        //CONTROL PANEL
        public static QTexture sidebarLogo0 = new QTexture(GENERAL, "sidebarLogo0");
        public static QTexture sidebarLogo1 = new QTexture(GENERAL, "sidebarLogo1");
        public static QTexture sidebarLogo2 = new QTexture(GENERAL, "sidebarLogo2");
        public static QTexture sidebarLogo3 = new QTexture(GENERAL, "sidebarLogo3");
        public static QTexture sidebarLogo4 = new QTexture(GENERAL, "sidebarLogo4");
        public static QTexture sidebarLogo5 = new QTexture(GENERAL, "sidebarLogo5");
        public static QTexture sidebarLogo6 = new QTexture(GENERAL, "sidebarLogo6");
        public static QTexture sidebarLogo7 = new QTexture(GENERAL, "sidebarLogo7");
        public static QTexture sidebarLogo8 = new QTexture(GENERAL, "sidebarLogo8");
        public static QTexture sidebarLogo9 = new QTexture(GENERAL, "sidebarLogo9");
        public static QTexture sidebarLogo10 = new QTexture(GENERAL, "sidebarLogo10");

        public static QTexture sideButtonCollapseSideBar = new QTexture(GENERAL, "sideButtonCollapseSideBar" + QResources.IsProSkinTag);
        public static QTexture sideButtonExpandSideBar = new QTexture(GENERAL, "sideButtonExpandSideBar" + QResources.IsProSkinTag);
        public static QTexture sideButtonControlPanel = new QTexture(GENERAL, "sideButtonControlPanel" + QResources.IsProSkinTag);
        public static QTexture sideButtonControlPanelSelected = new QTexture(GENERAL, "sideButtonControlPanel" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonDefineSymbols = new QTexture(GENERAL, "sideButtonDefineSymbols" + QResources.IsProSkinTag);
        public static QTexture sideButtonDefineSymbolsSelected = new QTexture(GENERAL, "sideButtonDefineSymbols" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonDataManager = new QTexture(GENERAL, "sideButtonDataManager" + QResources.IsProSkinTag);
        public static QTexture sideButtonDataManagerSelected = new QTexture(GENERAL, "sideButtonDataManager" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonDataBind = new QTexture(GENERAL, "sideButtonDataBind" + QResources.IsProSkinTag);
        public static QTexture sideButtonDataBindSelected = new QTexture(GENERAL, "sideButtonDataBind" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonPooly = new QTexture(GENERAL, "sideButtonPooly" + QResources.IsProSkinTag);
        public static QTexture sideButtonPoolySelected = new QTexture(GENERAL, "sideButtonPooly" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonAdsManager = new QTexture(GENERAL, "sideButtonAdsManager" + QResources.IsProSkinTag);
        public static QTexture sideButtonAdsManagerSelected = new QTexture(GENERAL, "sideButtonAdsManager" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonHelp = new QTexture(GENERAL, "sideButtonHelp" + QResources.IsProSkinTag);
        public static QTexture sideButtonHelpSelected = new QTexture(GENERAL, "sideButtonHelp" + QResources.IsProSkinTag + "Selected");
        public static QTexture sideButtonAbout = new QTexture(GENERAL, "sideButtonAbout" + QResources.IsProSkinTag);
        public static QTexture sideButtonAboutSelected = new QTexture(GENERAL, "sideButtonAbout" + QResources.IsProSkinTag + "Selected");

        public static QTexture sideButtonTwitter = new QTexture(GENERAL, "sideButtonTwitter" + QResources.IsProSkinTag);
        public static QTexture sideButtonFacebook = new QTexture(GENERAL, "sideButtonFacebook" + QResources.IsProSkinTag);
        public static QTexture sideButtonYoutube = new QTexture(GENERAL, "sideButtonYoutube" + QResources.IsProSkinTag);

        //ICONS
        public static QTexture IconDefineSymbols = new QTexture(ICONS, "IconDefineSymbols"); /// 128x128

        public static QTexture IconDataManager = new QTexture(ICONS, "IconDataManager"); /// 128x128

        public static QTexture IconBind = new QTexture(ICONS, "IconBind"); /// 128x128
        public static QTexture IconBindExtension = new QTexture(ICONS, "IconBindExtension"); /// 128x128
        public static QTexture IconBindAddObserver = new QTexture(ICONS, "IconBindAddObserver"); /// 128x128
        public static QTexture IconBindAddSource = new QTexture(ICONS, "IconBindAddSource"); /// 128x128

        public static QTexture IconPooly = new QTexture(ICONS, "IconPooly"); /// 128x128
        public static QTexture IconPoolyExtension = new QTexture(ICONS, "IconPoolyExtension"); /// 128x128
        public static QTexture IconPoolySpawner = new QTexture(ICONS, "IconPoolySpawner"); /// 128x128
        public static QTexture IconPoolyDespawnerTrigger = new QTexture(ICONS, "IconPoolyDespawnerTrigger"); /// 128x128
        public static QTexture IconPoolyDespawnerCollision = new QTexture(ICONS, "IconPoolyDespawnerCollision"); /// 128x128
        public static QTexture IconPoolyDespawnerEffectPlayed = new QTexture(ICONS, "IconPoolyDespawnerEffectPlayed"); /// 128x128
        public static QTexture IconPoolyDespawnerSoundPlayed = new QTexture(ICONS, "IconPoolyDespawnerSoundPlayed"); /// 128x128
        public static QTexture IconPoolyDespawnerTime = new QTexture(ICONS, "IconPoolyDespawnerTime"); /// 128x128

        public static QTexture IconAds = new QTexture(ICONS, "IconAds"); /// 128x128

        public static QTexture IconAbout = new QTexture(ICONS, "IconAbout"); /// 128x128

        public static QTexture IconHelp = new QTexture(ICONS, "IconHelp"); /// 128x128

        //PLACEHOLDERS
        public static QTexture imagePoolyStatisticsAreDisabled = new QTexture(PLACEHOLDERS, "imagePoolyStatisticsAreDisabled" + QResources.IsProSkinTag); // 300x115

        //ADS
        public static QTexture AdDefineSymbols = new QTexture(ADS, "AdDefineSymbols" + QResources.IsProSkinTag);
        public static QTexture AdDataManager = new QTexture(ADS, "AdDataManager" + QResources.IsProSkinTag);
        public static QTexture AdBind = new QTexture(ADS, "AdBind" + QResources.IsProSkinTag);
        public static QTexture AdPooly = new QTexture(ADS, "AdPooly" + QResources.IsProSkinTag);
        public static QTexture AdAds = new QTexture(ADS, "AdAds" + QResources.IsProSkinTag);
    }
}
