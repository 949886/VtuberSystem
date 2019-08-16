// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Ez.Editor;
using Ez.Internal;
using QuickEditor;
using QuickEngine.Core;
using QuickEngine.Extensions;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Ez
{
    public partial class ControlPanelWindow : QWindow
    {
        public static ControlPanelWindow Instance;

        GUIStyle _pageHeaderTitleStyle;
        GUIStyle PageHeaderTitleStyle
        {
            get
            {
                if(_pageHeaderTitleStyle == null)
                {
                    _pageHeaderTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        font = QResources.GetFont(FontName.Sansation.Regular),
                        fontSize = 26,
                        alignment = TextAnchor.MiddleLeft,
                        wordWrap = false,
                        margin = new RectOffset(0, 0, 0, 0),
                        border = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0),
                    };
                }
                return _pageHeaderTitleStyle;
            }
        }

        GUIStyle _pageHeaderSubtitleStyle;
        GUIStyle PageHeaderSubtitleStyle
        {
            get
            {
                if(_pageHeaderSubtitleStyle == null)
                {
                    _pageHeaderSubtitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        font = QResources.GetFont(FontName.Sansation.Light),
                        fontSize = 16,
                        alignment = TextAnchor.MiddleLeft,
                        wordWrap = false,
                        margin = new RectOffset(0, 0, 0, 0),
                        border = new RectOffset(0, 0, 0, 0),
                        padding = new RectOffset(0, 0, 0, 0)
                    };
                }
                return _pageHeaderSubtitleStyle;
            }
        }

        public static bool Selected = false;
#pragma warning disable 0414
        private static bool needsRefresh = false;
#pragma warning restore 0414
        public static void NeedsRefresh(bool value = true) { needsRefresh = value; }

        public enum Page
        {
            None,
            General,
            DefineSymbols,
            DataManager,
            DataBind,
            Pooly,
            Ads,
            Help,
            About
        }

        public static Page PreviousPage = Page.None;
        public static bool refreshData = true;

        private static Vector2 PageScrollPosition = Vector2.zero;
        private static Vector2 PageLastScrollPosition = Vector2.zero;

        private static bool _utility = true;
        private static string _title = "EzTools";
        private static bool _focus = true;


        /// <summary>
        /// Prevent OnInspector() forcing a repaint every time it's called.
        /// </summary>
        int inspectorUpdateFrame = 0;

        public static ControlPanelWindowSettings WindowSettings
        {
            get
            {
                return ControlPanelWindowSettings.Instance;
            }
        }

        private string SearchPattern = string.Empty;
        private static AnimBool SearchPatternAnimBool = new AnimBool(false);

        private float tempFloat;
        private string tempLabel = string.Empty;
        private Rect tempRect;
        [MenuItem("Tools/EZ/Control Panel &E", false, 0)]
        static void Init()
        {
            Instance = GetWindow<ControlPanelWindow>(_utility, _title, _focus);
            Instance.SetupWindow();
        }

        private void SetupWindow()
        {
            titleContent = new GUIContent(_title);
            WindowSettings.sidebarIsExpanded.speed = 2;
            WindowSettings.sidebarIsExpanded.valueChanged.RemoveAllListeners();
            WindowSettings.sidebarIsExpanded.valueChanged.AddListener(Repaint);
            WindowSettings.sidebarIsExpanded.valueChanged.AddListener(UpdateWindowSize);

            WindowSettings.pageWidthExtraSpace.valueChanged.RemoveAllListeners();
            WindowSettings.pageWidthExtraSpace.valueChanged.AddListener(Repaint);
            WindowSettings.pageWidthExtraSpace.valueChanged.AddListener(UpdateWindowSize);

            UpdateWindowSize();
            CenterWindow();
        }

        private void UpdateWindowSize()
        {
            minSize = new Vector2(WindowSettings.windowMinimumWidth, WindowSettings.windowMinimumHeight);
            maxSize = new Vector2(minSize.x, Screen.currentResolution.height);
            //maxSize = minSize;
        }

        public static void Open(Page page)
        {
            Init();
#if UNITY_EDITOR_OSX
            QUI.ExitGUI();
#endif
            WindowSettings.currentPage = page;
            refreshData = true;
        }

        private void OnEnable()
        {
            autoRepaintOnSceneChange = true;
            requiresContantRepaint = true;
            PreviousPage = Page.None;
            InitPages();
        }
        private void OnDisable()
        {
            switch(WindowSettings.currentPage)
            {
                case Page.None:
                    break;
                case Page.General:
                    break;
                case Page.DefineSymbols:
                    break;
                case Page.DataManager:
                    break;
                case Page.DataBind:
                    break;
                case Page.Pooly:
#if EZ_POOLY
                    PoolyOnDisable();
#endif
                    break;
                case Page.Ads:
                    break;
                case Page.Help:
                    break;
                case Page.About:
                    break;
            }
            QUI.SetDirty(WindowSettings);
            AssetDatabase.SaveAssets();
        }

        private void OnFocus()
        {
            switch(WindowSettings.currentPage)
            {
                case Page.None:
                    break;
                case Page.General:
                    break;
                case Page.DefineSymbols:
                    break;
                case Page.DataManager:
                    break;
                case Page.DataBind:
                    break;
                case Page.Pooly:
#if EZ_POOLY
                    PoolyOnFocus();
#endif
                    break;
                case Page.Ads:
                    break;
                case Page.Help:
                    break;
                case Page.About:
                    break;
            }
        }
        private void OnLostFocus()
        {
            switch(WindowSettings.currentPage)
            {
                case Page.None:
                    break;
                case Page.General:
                    break;
                case Page.DefineSymbols:
                    break;
                case Page.DataManager:
                    break;
                case Page.DataBind:
                    break;
                case Page.Pooly:
#if EZ_POOLY
                    PoolyOnLostFocus();
#endif
                    break;
                case Page.Ads:
                    break;
                case Page.Help:
                    break;
                case Page.About:
                    break;
            }
            QUI.SetDirty(WindowSettings);
            AssetDatabase.SaveAssets();
        }

        private void OnInspectorUpdate()
        {
            switch(WindowSettings.currentPage)
            {
                case Page.General:
                    break;
                case Page.DefineSymbols:
                    break;
                case Page.DataManager:
#if EZ_DATA_MANAGER
                    DataManagerOnInspectorUpdate();
#endif
                    break;
                case Page.DataBind:
#if EZ_BIND
                    BindRefreshEzBindComponentsInfoInCurrentScene();
#endif
                    break;
                case Page.Pooly:
                    break;
                case Page.Ads:
                    break;
                case Page.Help:
                    break;
                case Page.About:
                    break;
                default:
                    break;
            }

            if(inspectorUpdateFrame % 10 == 0) //once a second (every 10th frame)
            {
                Repaint(); //force the window to repaint
            }

            inspectorUpdateFrame++; //track what frame we're on, so we can call code less often
        }

        private void OnGUI()
        {
            UpdateWindowSize();

            DrawBackground();

            QUI.BeginHorizontal(position.width);
            {
                DrawSideBar();
                QUI.Space(WindowSettings.pageShadowWidth);
                DrawPages();
            }
            QUI.EndHorizontal();

            if(Event.current.type != EventType.Layout)
            {
                if(PageScrollPosition != PageLastScrollPosition) //if the user has scrolled, deselect - this is because control IDs within carousel will change when scrolled so we'd end up with the wrong box selected.
                {
                    GUI.FocusControl(""); //deselect
                    PageLastScrollPosition = PageScrollPosition;
                }
            }

            SearchPatternAnimBool.target = !SearchPattern.IsNullOrEmpty();

            Repaint();
        }

        void DrawBackground()
        {
            tempRect = new Rect(0, 0, position.width, position.height);
            QUI.DrawTexture(new Rect(tempRect.x, tempRect.y, WindowSettings.SidebarCurrentWidth, tempRect.height), QResources.backgroundSidebar.texture);
            tempRect.x += WindowSettings.SidebarCurrentWidth;
            QUI.DrawTexture(new Rect(tempRect.x, tempRect.y, WindowSettings.pageShadowWidth, tempRect.height), QResources.backgroundContentShadowLeft.texture);
            tempRect.x += WindowSettings.pageShadowWidth;
            QUI.DrawTexture(new Rect(tempRect.x, tempRect.y, WindowSettings.pageShadowWidth + WindowSettings.CurrentPageContentWidth + WindowSettings.scrollbarSize, tempRect.height), QResources.backgroundContent.texture);
            //tempRect.x += Settings.CurrentPageContentWidth;
            //QUI.DrawTexture(new Rect(tempRect.x, tempRect.y, Settings.scrollbarSize, tempRect.height), QResources.backgroundContent.texture);
        }

        void DrawSideBar()
        {
            QUI.BeginVertical(WindowSettings.SidebarCurrentWidth);
            {
                DrawSideBarLogo();
                DrawBtnExpandCollapseSideBar();
                QUI.Space(WindowSettings.sidebarVerticalSpacing);
                //DrawBtnControlPanel();
                //QUI.Space(WindowSettings.sidebarVerticalSpacing);
                DrawBtnDefineSymbols();
                DrawBtnDataManager();
                DrawBtnDataBind();
                DrawBtnPooly();
                DrawBtnAdsManager();
                QUI.Space(WindowSettings.sidebarVerticalSpacing);
                DrawBtnHelp();
                DrawBtnAbout();
                QUI.FlexibleSpace();
                DrawSocialButtons();
            }
            QUI.EndVertical();
        }

        void DrawSideBarLogo()
        {
            if(WindowSettings.sidebarIsExpanded.faded <= 0.05f) { QUI.DrawTexture(EZResources.sidebarLogo10.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.1f) { QUI.DrawTexture(EZResources.sidebarLogo9.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.2f) { QUI.DrawTexture(EZResources.sidebarLogo8.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.3f) { QUI.DrawTexture(EZResources.sidebarLogo7.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.4f) { QUI.DrawTexture(EZResources.sidebarLogo6.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.5f) { QUI.DrawTexture(EZResources.sidebarLogo5.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.6f) { QUI.DrawTexture(EZResources.sidebarLogo4.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.7f) { QUI.DrawTexture(EZResources.sidebarLogo3.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.8f) { QUI.DrawTexture(EZResources.sidebarLogo2.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else if(WindowSettings.sidebarIsExpanded.faded <= 0.9f) { QUI.DrawTexture(EZResources.sidebarLogo1.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }
            else { QUI.DrawTexture(EZResources.sidebarLogo0.texture, WindowSettings.sidebarExpandedWidth, WindowSettings.sidebarLogoHeight); }

        }

        void DrawBtnExpandCollapseSideBar()
        {
            if(QUI.Button("", EZStyles.GetStyle(WindowSettings.sidebarIsExpanded.faded < 0.9f ? EZStyles.General.SideButtonExpandSideBar : EZStyles.General.SideButtonCollapseSideBar), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                WindowSettings.sidebarIsExpanded.target = !WindowSettings.sidebarIsExpanded.target;
            }
        }
        void DrawBtnControlPanel()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Control Panel";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.General ? EZStyles.General.SideButtonSelectedControlPanel : EZStyles.General.SideButtonControlPanel), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.General)
                {
                    WindowSettings.currentPage = Page.General;
                    ResetPageView();
                }
            }
        }
        void DrawBtnDefineSymbols()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Define Symbols";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.DefineSymbols ? EZStyles.General.SideButtonSelectedDefineSymbols : EZStyles.General.SideButtonDefineSymbols), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.DefineSymbols)
                {
                    WindowSettings.currentPage = Page.DefineSymbols;
                    ResetPageView();
                }
            }
        }
        void DrawBtnDataManager()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Data Manager";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.DataManager ? EZStyles.General.SideButtonSelectedDataManager : EZStyles.General.SideButtonDataManager), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.DataManager)
                {
                    WindowSettings.currentPage = Page.DataManager;
                    ResetPageView();
                }
            }
        }
        void DrawBtnDataBind()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Data Bind";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.DataBind ? EZStyles.General.SideButtonSelectedDataBind : EZStyles.General.SideButtonDataBind), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.DataBind)
                {
                    WindowSettings.currentPage = Page.DataBind;
                    ResetPageView();
                }
            }
        }
        void DrawBtnPooly()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Pooly";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.Pooly ? EZStyles.General.SideButtonSelectedPooly : EZStyles.General.SideButtonPooly), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.Pooly)
                {
                    WindowSettings.currentPage = Page.Pooly;
                    ResetPageView();
                }
            }
        }
        void DrawBtnAdsManager()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Ads Manager";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.Ads ? EZStyles.General.SideButtonSelectedAdsManager : EZStyles.General.SideButtonAdsManager), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.Ads)
                {
                    WindowSettings.currentPage = Page.Ads;
                    ResetPageView();
                }
            }
        }
        void DrawBtnHelp()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "Help";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.Help ? EZStyles.General.SideButtonSelectedHelpBtnHelp : EZStyles.General.SideButtonHelpBtnHelp), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.Help)
                {
                    WindowSettings.currentPage = Page.Help;
                    ResetPageView();
                }
            }
        }
        void DrawBtnAbout()
        {
            tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.6f ? "" : "About";
            if(QUI.Button(tempLabel, EZStyles.GetStyle(WindowSettings.currentPage == Page.About ? EZStyles.General.SideButtonSelectedAbout : EZStyles.General.SideButtonAbout), WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight))
            {
                if(WindowSettings.currentPage != Page.About)
                {
                    WindowSettings.currentPage = Page.About;
                    ResetPageView();
                }
            }
        }

        void DrawSocialButtons()
        {
            if(WindowSettings.sidebarIsExpanded.faded < 0.3f)
            {
                QUI.BeginVertical(WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight * 3);
            }
            else
            {
                QUI.BeginHorizontal(WindowSettings.SidebarCurrentWidth, WindowSettings.sidebarButtonHeight);
            }
            {
                tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.8f ? "" : "Twitter";
                if(QUI.Button(tempLabel, EZStyles.GetStyle(EZStyles.General.SideButtonTwitter), WindowSettings.sidebarExpandedWidth / 3 >= WindowSettings.SidebarCurrentWidth ? WindowSettings.SidebarCurrentWidth : WindowSettings.SidebarCurrentWidth / 3, WindowSettings.sidebarButtonHeight))
                {
                    Application.OpenURL("https://twitter.com/EzGamesStudio");
                }
                tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.8f ? "" : "Facebook";
                if(QUI.Button(tempLabel, EZStyles.GetStyle(EZStyles.General.SideButtonFacebook), WindowSettings.sidebarExpandedWidth / 3 >= WindowSettings.SidebarCurrentWidth ? WindowSettings.SidebarCurrentWidth : WindowSettings.SidebarCurrentWidth / 3, WindowSettings.sidebarButtonHeight))
                {
                    Application.OpenURL("https://www.facebook.com/ezentertainmentstudio");
                }
                tempLabel = WindowSettings.sidebarIsExpanded.faded < 0.8f ? "" : "YouTube";
                if(QUI.Button(tempLabel, EZStyles.GetStyle(EZStyles.General.SideButtonYoutube), WindowSettings.sidebarExpandedWidth / 3 >= WindowSettings.SidebarCurrentWidth ? WindowSettings.SidebarCurrentWidth : WindowSettings.SidebarCurrentWidth / 3, WindowSettings.sidebarButtonHeight))
                {
                    Application.OpenURL("https://www.youtube.com/channel/UCfGuilFEHW0CQ-cU9D0ik6A");
                }
                QUI.FlexibleSpace();
            }
            if(WindowSettings.sidebarIsExpanded.faded < 0.3f)
            {
                QUI.EndVertical();
            }
            else
            {
                QUI.EndHorizontal();
            }
            //QUI.Space(Settings.sidebarButtonHeight / 4);
        }

        void ResetPageView()
        {
            PageScrollPosition = Vector2.zero; //reset scroll
            PageLastScrollPosition = PageScrollPosition;

            SearchPattern = ""; //reset search pattern
            SearchPatternAnimBool.value = false; //reset ui for search pattern
        }

        void InitPages()
        {
            switch(WindowSettings.currentPage)
            {
                case Page.General:
                    WindowSettings.pageWidthExtraSpace.target = 0;
                    break;
                case Page.DefineSymbols:
#if EZ_DEFINE_SYMBOLS
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitDefineSymbols(); }
#endif
                    WindowSettings.pageWidthExtraSpace.target = 0;
                    break;
                case Page.DataManager:
#if EZ_DATA_MANAGER
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitDataManager(); }
#endif
                    WindowSettings.pageWidthExtraSpace.target = 0;
                    break;
                case Page.DataBind:
#if EZ_BIND
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitBind(); }
                    WindowSettings.pageWidthExtraSpace.target = 200;
#else
                    WindowSettings.pageWidthExtraSpace.target = 0;
#endif
                    break;
                case Page.Pooly:
#if EZ_POOLY
                    WindowSettings.pageWidthExtraSpace.target = Ez.Pooly.Pooly.PoolySettings.enableStatistics ? Ez.Pooly.Statistics.PoolyStatistics.Instance.poolyWidth : 0;
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitPooly(); }
#else
                    WindowSettings.pageWidthExtraSpace.target = 0;
#endif
                    break;
                case Page.Ads:
#if EZ_ADS
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitAds(); }
                    WindowSettings.pageWidthExtraSpace.target = -200;
#else
                    WindowSettings.pageWidthExtraSpace.target = 0;
#endif
                    break;
                case Page.Help:
                    WindowSettings.pageWidthExtraSpace.target = -140;
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitHelp(); }
                    break;
                case Page.About:
                    WindowSettings.pageWidthExtraSpace.target = 0;
                    if(PreviousPage != WindowSettings.currentPage || refreshData) { InitAbout(); }
                    break;
            }
        }

        void DrawPages()
        {
            InitPages();

            PageScrollPosition = QUI.BeginScrollView(PageScrollPosition);
            {
                QUI.BeginVertical(WindowSettings.CurrentPageContentWidth);
                {
                    switch(WindowSettings.currentPage)
                    {
                        case Page.General: break;
                        case Page.DefineSymbols: DrawDefineSymbols(); break;
                        case Page.DataManager: DrawDataManager(); break;
                        case Page.DataBind: DrawBind(); break;
                        case Page.Pooly: DrawPooly(); break;
                        case Page.Ads: DrawAds(); break;
                        case Page.Help: DrawHelp(); break;
                        case Page.About: DrawAbout(); break;
                    }
                    QUI.FlexibleSpace();
                }
                QUI.EndVertical();
                QUI.Space(16);
            }
            QUI.EndScrollView();

            if(PreviousPage != WindowSettings.currentPage || refreshData)
            {
                PreviousPage = WindowSettings.currentPage;
                refreshData = false;
            }
        }

        void DrawPageHeader(string title, QColor titleColor, string subtitle, QColor subtitleColor, QTexture iconQTexture)
        {
            QUI.Space(SPACE_2);
            QUI.Box(QStyles.GetBackgroundStyle(Style.BackgroundType.Low, QColors.Color.Gray), WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth, WindowSettings.pageHeaderHeight);
            QUI.Space(-WindowSettings.pageHeaderHeight + (WindowSettings.pageHeaderHeight - WindowSettings.pageHeaderHeight * 0.8f) / 2);
            QUI.BeginHorizontal(WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth, WindowSettings.pageHeaderHeight * 0.8f);
            {
                QUI.Space((WindowSettings.pageHeaderHeight - WindowSettings.pageHeaderHeight * 0.8f));
                QUI.BeginVertical((WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth) / 2, WindowSettings.pageHeaderHeight * 0.8f);
                {
                    QUI.FlexibleSpace();
                    if(!title.IsNullOrEmpty())
                    {
                        QUI.Space(-SPACE_2);
                        QUI.SetGUIColor(titleColor.Color);
                        QUI.Label(title, PageHeaderTitleStyle, (WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth) / 2, 26);
                        QUI.ResetColors();
                    }

                    if(!subtitle.IsNullOrEmpty())
                    {
                        QUI.Space(-SPACE_2);
                        QUI.SetGUIColor(subtitleColor.Color);
                        QUI.Label(subtitle, PageHeaderSubtitleStyle, (WindowSettings.CurrentPageContentWidth + WindowSettings.pageShadowWidth) / 2, 18);
                        QUI.ResetColors();
                    }
                    QUI.FlexibleSpace();
                }
                QUI.EndVertical();
                QUI.FlexibleSpace();
                QUI.DrawTexture(iconQTexture.texture, WindowSettings.pageHeaderHeight * 0.8f, WindowSettings.pageHeaderHeight * 0.8f);
                QUI.Space((WindowSettings.pageHeaderHeight - WindowSettings.pageHeaderHeight * 0.8f) / 2);
            }
            QUI.EndHorizontal();
        }

        public static void DrawEditableLinkButtons(List<LinkButtonData> list, float width)
        {
#if !EZ_SOURCE
            return;
#endif
#pragma warning disable CS0162 // Unreachable code detected
            QUI.Space(SPACE_2);

            QUI.DrawIconBar("Edit Link Buttons", QResources.iconInfo, QColors.Color.Blue, IconPosition.Right, width, 20);
            if(list != null && list.Count > 0)
            {
                QUI.BeginChangeCheck();
                for(int i = 0; i < list.Count; i++)
                {
                    QUI.BeginHorizontal(width);
                    {
                        list[i].text = EditorGUILayout.DelayedTextField(list[i].text, GUILayout.Width(width * 0.4f));
                        QUI.Space(SPACE_2);
                        list[i].url = EditorGUILayout.DelayedTextField(list[i].url);
                        QUI.Space(SPACE_2);
                        list[i].linkButton = (Style.LinkButton)EditorGUILayout.EnumPopup(list[i].linkButton, GUILayout.Width(width * 0.1f));
                        QUI.Space(SPACE_2);
                        if(QUI.ButtonMinus())
                        {
                            list.RemoveAt(i);
                            QUI.SetDirty(EzSourceData.Instance);
                            AssetDatabase.SaveAssets();
                            QUI.ExitGUI();
                        }
                        QUI.Space(10);
                    }
                    QUI.EndHorizontal();
                }


                if(QUI.EndChangeCheck())
                {
                    QUI.SetDirty(EzSourceData.Instance);
                    AssetDatabase.SaveAssets();
                }
            }
            else
            {
                QUI.BeginHorizontal(width);
                {
                    QLabel q = new QLabel("There are no link buttons defined... This is not right!", Style.Text.Help);
                    QUI.Label(q);
                }
                QUI.EndHorizontal();
            }

            QUI.BeginHorizontal(width);
            {
                QUI.FlexibleSpace();
                if(QUI.ButtonPlus())
                {
                    list.Add(new LinkButtonData());
                    QUI.SetDirty(EzSourceData.Instance);
                    AssetDatabase.SaveAssets();
                }
                QUI.Space(6);
            }
            QUI.EndHorizontal();
#pragma warning restore CS0162 // Unreachable code detected
        }

    }
}
