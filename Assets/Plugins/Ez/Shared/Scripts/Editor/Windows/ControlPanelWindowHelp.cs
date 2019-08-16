// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Ez.Internal;
using QuickEditor;
using QuickEngine.Core;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace Ez
{
    public partial class ControlPanelWindow : QWindow
    {
        AnimBool editHelpAnimBool;

        Page selectedHelpModuleToEdit = Page.None;

        void InitHelp()
        {
            InitHelpAnimBools();

            selectedHelpModuleToEdit = Page.None;
        }

        void InitHelpAnimBools()
        {
            editHelpAnimBool = new AnimBool(false, Repaint);
        }
        void DrawHelp()
        {
            DrawPageHeader("Help", QColors.Green, "Owner's Manuals & Video Tutorials", QUI.IsProSkin ? QColors.UnityLight : QColors.UnityMild, EZResources.IconHelp);
            QUI.Space(6);

            editHelpAnimBool.target = selectedHelpModuleToEdit != Page.None;

            QUI.Space(2);

            DrawHelpModule("Ez Define Symbols", Page.DefineSymbols, EZResources.IconDefineSymbols, EzSourceData.Instance.defineSymbolsHelpButtons, WindowSettings.CurrentPageContentWidth);
            QUI.Space(SPACE_8);

            DrawHelpModule("Ez Data Manager", Page.DataManager, EZResources.IconDataManager, EzSourceData.Instance.dataManagerHelpButtons, WindowSettings.CurrentPageContentWidth);
            QUI.Space(SPACE_8);

            DrawHelpModule("Ez Bind", Page.DataBind, EZResources.IconBind, EzSourceData.Instance.bindHelpButtons, WindowSettings.CurrentPageContentWidth);
            QUI.Space(SPACE_8);

            DrawHelpModule("Pooly", Page.Pooly, EZResources.IconPooly, EzSourceData.Instance.poolyHelpButtons, WindowSettings.CurrentPageContentWidth);
            QUI.Space(SPACE_8);

            DrawHelpModule("Ez Ads", Page.Ads, EZResources.IconAds, EzSourceData.Instance.adsHelpButtons, WindowSettings.CurrentPageContentWidth);
            QUI.Space(SPACE_8);
        }


        void DrawHelpModule(string moduleName, Page targetModule, QTexture moduleIcon, List<LinkButtonData> list, float width)
        {
            QUI.DrawIconBar(moduleName, moduleIcon, QColors.Color.Gray, IconPosition.Right, width + 16, 24);
            QUI.Space(SPACE_4);
            QUI.DrawLinkButtonsList(list, SPACE_8, width);

#if EZ_SOURCE
            QUI.Space(SPACE_2);

            QUI.Space(6 * (selectedHelpModuleToEdit == targetModule ? editHelpAnimBool.faded : 0));
            QUI.BeginHorizontal(width + 16);
            {
                QUI.FlexibleSpace();
                if(QUI.GhostButton("Edit Links", QColors.Color.Gray, 100, 16, selectedHelpModuleToEdit == targetModule)
                    || QUI.DetectKeyUp(Event.current, KeyCode.Escape))
                {
                    if(selectedHelpModuleToEdit == targetModule)
                    {
                        selectedHelpModuleToEdit = Page.None;
                        editHelpAnimBool.target = false;
                    }
                    else
                    {
                        selectedHelpModuleToEdit = targetModule;
                        editHelpAnimBool.target = true;
                    }
                }

                if(selectedHelpModuleToEdit == targetModule && editHelpAnimBool.faded > 0.4f)
                {
                    QUI.Space(SPACE_4 * editHelpAnimBool.faded);

                    if(QUI.GhostButton("Save Changes", QColors.Color.Green, 100 * editHelpAnimBool.faded, 16))
                    {
                        QUI.SetDirty(EzSourceData.Instance);
                        AssetDatabase.SaveAssets();
                        selectedHelpModuleToEdit = Page.None;
                        editHelpAnimBool.value = false;
                    }
                }
            }
            QUI.EndHorizontal();


            if(selectedHelpModuleToEdit == targetModule)
            {
                if(QUI.BeginFadeGroup(editHelpAnimBool.faded))
                {
                    QUI.BeginVertical(width);
                    {
                        DrawEditableLinkButtons(list, width + 16);

                        QUI.Space(SPACE_16 * (selectedHelpModuleToEdit == targetModule ? editHelpAnimBool.faded : 0));
                    }
                    QUI.EndVertical();
                }
                QUI.EndFadeGroup();
            }

            QUI.Space(SPACE_8);
#endif
            QUI.Space(SPACE_8);
        }

    }
}
