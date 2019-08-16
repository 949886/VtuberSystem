// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Ez.Internal;
using QuickEditor;
using QuickEngine.Core;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Ez
{
    public partial class ControlPanelWindow : QWindow
    {
        EzModuleVersion _defineSymbolsModuleVersion;
        EzModuleVersion DefineSymbolsModuleVersion
        {
            get
            {
#if !EZ_DEFINE_SYMBOLS
                return null;
#endif

#pragma warning disable CS0162 // Unreachable code detected
                if(_defineSymbolsModuleVersion == null)
                {
                    _defineSymbolsModuleVersion = Q.GetResource<EzModuleVersion>(EZT.RESOURCES_PATH_DEFINE_SYMBOLS_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);

                    if(_defineSymbolsModuleVersion == null)
                    {
                        _defineSymbolsModuleVersion = Q.CreateAsset<EzModuleVersion>(EZT.RELATIVE_PATH_DEFINE_SYMBOLS_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);
                    }
                }
                return _defineSymbolsModuleVersion;
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        EzModuleVersion _dataManagerModuleVersion;
        EzModuleVersion DataManagerModuleVersion
        {
            get
            {
#if !EZ_DATA_MANAGER
                return null;
#endif

#pragma warning disable CS0162 // Unreachable code detected
                if(_dataManagerModuleVersion == null)
                {
                    _dataManagerModuleVersion = Q.GetResource<EzModuleVersion>(EZT.RESOURCES_PATH_DATA_MANAGER_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);

                    if(_dataManagerModuleVersion == null)
                    {
                        _dataManagerModuleVersion = Q.CreateAsset<EzModuleVersion>(EZT.RELATIVE_PATH_DATA_MANAGER_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);
                    }
                }
                return _dataManagerModuleVersion;
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        EzModuleVersion _bindModuleVersion;
        EzModuleVersion BindModuleVersion
        {
            get
            {
#if !EZ_BIND
                return null;
#endif

#pragma warning disable CS0162 // Unreachable code detected
                if(_bindModuleVersion == null)
                {
                    _bindModuleVersion = Q.GetResource<EzModuleVersion>(EZT.RESOURCES_PATH_BIND_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);

                    if(_bindModuleVersion == null)
                    {
                        _bindModuleVersion = Q.CreateAsset<EzModuleVersion>(EZT.RELATIVE_PATH_BIND_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);
                    }
                }
                return _bindModuleVersion;
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        EzModuleVersion _poolyModuleVersion;
        EzModuleVersion PoolyModuleVersion
        {
            get
            {
#if !EZ_POOLY
                return null;
#endif

#pragma warning disable CS0162 // Unreachable code detected
                if(_poolyModuleVersion == null)
                {
                    _poolyModuleVersion = Q.GetResource<EzModuleVersion>(EZT.RESOURCES_PATH_POOLY_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);

                    if(_poolyModuleVersion == null)
                    {
                        _poolyModuleVersion = Q.CreateAsset<EzModuleVersion>(EZT.RELATIVE_PATH_POOLY_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);
                    }
                }
                return _poolyModuleVersion;
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        EzModuleVersion _adsModuleVersion;
        EzModuleVersion AdsModuleVersion
        {
            get
            {
#if !EZ_ADS
                return null;
#endif

#pragma warning disable CS0162 // Unreachable code detected
                if(_adsModuleVersion == null)
                {
                    _adsModuleVersion = Q.GetResource<EzModuleVersion>(EZT.RESOURCES_PATH_ADS_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);

                    if(_adsModuleVersion == null)
                    {
                        _adsModuleVersion = Q.CreateAsset<EzModuleVersion>(EZT.RELATIVE_PATH_ADS_VERSION, EzModuleVersion.MODULE_VERSION_FILENAME);
                    }
                }
                return _adsModuleVersion;
#pragma warning restore CS0162 // Unreachable code detected
            }
        }

        AnimBool showDefineSymbolsReleaseNotes;
        AnimBool showDataManagerReleaseNotes;
        AnimBool showBindReleaseNotes;
        AnimBool showPoolyReleaseNotes;
        AnimBool showAdsReleaseNotes;

        AnimBool editModuleAnimBool;

        Page selectedModuleToEdit = Page.None;

        void InitAbout()
        {
            InitAboutAnimBools();

            selectedModuleToEdit = Page.None;
        }

        void InitAboutAnimBools()
        {
            showDefineSymbolsReleaseNotes = new AnimBool(false, Repaint);
            showDataManagerReleaseNotes = new AnimBool(false, Repaint);
            showBindReleaseNotes = new AnimBool(false, Repaint);
            showPoolyReleaseNotes = new AnimBool(false, Repaint);
            showAdsReleaseNotes = new AnimBool(false, Repaint);

            editModuleAnimBool = new AnimBool(false, Repaint);
        }

        void DrawAbout()
        {
            DrawPageHeader("ABOUT", QColors.Green, "Module Versions & Release Notes", QUI.IsProSkin ? QColors.UnityLight : QColors.UnityMild, EZResources.IconAbout);
            QUI.Space(6);

            editModuleAnimBool.target = selectedModuleToEdit != Page.None;

            DrawSourceData(WindowSettings.CurrentPageContentWidth + 16);
        }


        void DrawSourceData(float width)
        {
            QUI.Space(SPACE_16);
            DrawModuleVersion("EZ DEFINE SYMBOLS", Page.DefineSymbols, DefineSymbolsModuleVersion, showDefineSymbolsReleaseNotes, width);
            QUI.Space(SPACE_8);
            DrawModuleVersion("EZ DATA MANAGER", Page.DataManager, DataManagerModuleVersion, showDataManagerReleaseNotes, width);
            QUI.Space(SPACE_8);
            DrawModuleVersion("EZ BIND", Page.DataBind, BindModuleVersion, showBindReleaseNotes, width);
            QUI.Space(SPACE_8);
            DrawModuleVersion("POOLY", Page.Pooly, PoolyModuleVersion, showPoolyReleaseNotes, width);
            QUI.Space(SPACE_8);
            DrawModuleVersion("EZ ADS", Page.Ads, AdsModuleVersion, showAdsReleaseNotes, width);
            QUI.Space(SPACE_8);
        }

        void DrawModuleVersion(string moduleName, Page targetModule, EzModuleVersion emv, AnimBool show, float width)
        {
            if(QUI.GhostBar(moduleName + (emv == null ? " has not been installed!" : (" version " + emv.versionNumber)), emv == null ? QColors.Color.Gray : QColors.Color.Green, show, width, 24))
            {
                if(emv == null)
                {
                    show.target = false;
                    WindowSettings.currentPage = targetModule;
                }
                else
                {
                    show.target = !show.target;
                    if(selectedModuleToEdit == targetModule)
                    {
                        selectedModuleToEdit = Page.None;
                        editModuleAnimBool.target = false;
                    }
                }
            }

            if(emv == null)
            {
                return;
            }

            if(QUI.BeginFadeGroup(show.faded))
            {
                QUI.BeginVertical(width);
                {

                    QUI.Space(-9);

                    QUI.BeginHorizontal(width);
                    {
                        EditorGUILayout.LabelField(emv.releaseNotes, QStyles.GetInfoMessageMessageStyle(Style.InfoMessage.Help));
                    }
                    QUI.EndHorizontal();

#if EZ_SOURCE
#pragma warning disable CS0162 // Unreachable code detected

                    QUI.Space(13);
                    QUI.Space(8 * (selectedModuleToEdit == targetModule ? editModuleAnimBool.faded : 0));
                    QUI.BeginHorizontal(width);
                    {
                        QUI.FlexibleSpace();
                        if(QUI.GhostButton("Edit Version", QColors.Color.Gray, 100, 16, selectedModuleToEdit == targetModule)
                            || QUI.DetectKeyUp(Event.current, KeyCode.Escape))
                        {
                            if(selectedModuleToEdit == targetModule)
                            {
                                selectedModuleToEdit = Page.None;
                                editModuleAnimBool.target = false;
                            }
                            else
                            {
                                selectedModuleToEdit = targetModule;
                                editModuleAnimBool.target = true;
                            }
                        }

                        if(selectedModuleToEdit == targetModule && editModuleAnimBool.faded > 0.4f)
                        {
                            QUI.Space(SPACE_4 * editModuleAnimBool.faded);

                            if(QUI.GhostButton("Save Changes", QColors.Color.Green, 100 * editModuleAnimBool.faded, 16))
                            {
                                QUI.SetDirty(emv);
                                AssetDatabase.SaveAssets();
                                selectedModuleToEdit = Page.None;
                                editModuleAnimBool.value = false;
                            }
                        }
                    }
                    QUI.EndHorizontal();


                    if(selectedModuleToEdit == targetModule)
                    {
                        if(QUI.BeginFadeGroup(editModuleAnimBool.faded))
                        {
                            QUI.BeginVertical(width);
                            {
                                QUI.BeginHorizontal(width);
                                {
                                    EditorGUILayout.LabelField("version", QStyles.GetStyle(QStyles.GetStyleName(Style.Text.Small)), GUILayout.Width(80));
                                    QUI.Space(SPACE_2);
                                    EditorGUILayout.LabelField("release notes", QStyles.GetStyle(QStyles.GetStyleName(Style.Text.Small)));
                                }
                                QUI.EndHorizontal();

                                QUI.Space(-SPACE_4);

                                QUI.BeginHorizontal(width);
                                {
                                    emv.versionNumber = EditorGUILayout.TextField(emv.versionNumber, GUILayout.Width(80));
                                    QUI.Space(SPACE_2);
                                    emv.releaseNotes = EditorGUILayout.TextArea(emv.releaseNotes);
                                }
                                QUI.EndHorizontal();

                                QUI.Space(SPACE_16);
                            }
                            QUI.EndVertical();
                        }
                        QUI.EndFadeGroup();
                    }
#endif
                    QUI.Space(9 + 16);
                }
                QUI.EndVertical();
            }
            QUI.EndFadeGroup();


#pragma warning restore CS0162 // Unreachable code detected
        }
    }
}
