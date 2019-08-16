// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using Ez.Internal;
using QuickEditor;
using System.Collections.Generic;
using UnityEngine;

namespace Ez
{
    public partial class ControlPanelWindow : QWindow
    {
        float LinkButtonWidth { get { return WindowSettings.CurrentPageContentWidth + 32; } }

#if !EZ_DEFINE_SYMBOLS
        void DrawDefineSymbols()
        {
            DrawMissingModuleAd(EZResources.AdDefineSymbols.texture, EzSourceData.Instance.defineSymbolsMissingModuleButtons);
#if EZ_SOURCE
            QUI.Space(SPACE_4);
            DrawEditableLinkButtons(EzSourceData.Instance.defineSymbolsMissingModuleButtons, LinkButtonWidth);
#endif
        }
#endif

#if !EZ_DATA_MANAGER
        void DrawDataManager()
        {
            DrawMissingModuleAd(EZResources.AdDataManager.texture, EzSourceData.Instance.dataManagerMissingModuleButtons);
#if EZ_SOURCE
            QUI.Space(SPACE_4);
            DrawEditableLinkButtons(EzSourceData.Instance.dataManagerMissingModuleButtons, LinkButtonWidth);
#endif
        }
#endif

#if !EZ_BIND
        void DrawBind()
        {
            DrawMissingModuleAd(EZResources.AdBind.texture, EzSourceData.Instance.bindMissingModuleButtons);
#if EZ_SOURCE
            QUI.Space(SPACE_4);
            DrawEditableLinkButtons(EzSourceData.Instance.bindMissingModuleButtons, LinkButtonWidth);
#endif
        }
#endif

#if !EZ_POOLY
        void DrawPooly()
        {
            DrawMissingModuleAd(EZResources.AdPooly.texture, EzSourceData.Instance.poolyMissingModuleButtons); //change texture
#if EZ_SOURCE
            QUI.Space(SPACE_4);
            DrawEditableLinkButtons(EzSourceData.Instance.poolyMissingModuleButtons, LinkButtonWidth);
#endif
        }
#endif

#if !EZ_ADS
        void DrawAds()
        {
            DrawMissingModuleAd(EZResources.AdAds.texture, EzSourceData.Instance.adsMissingModuleButtons); //change texture
#if EZ_SOURCE
            QUI.Space(SPACE_4);
            DrawEditableLinkButtons(EzSourceData.Instance.adsMissingModuleButtons, LinkButtonWidth);
#endif
        }
#endif

        void DrawMissingModuleAd(Texture texture, List<LinkButtonData> list, float width = 656, float height = 608)
        {
            QUI.Space(SPACE_16);
            QUI.DrawTexture(texture, width, height);
            QUI.Space(-height);
            QUI.BeginVertical(width, height);
            {
                QUI.FlexibleSpace();
                QUI.DrawLinkButtonsList(list);
                QUI.Space(SPACE_16);
            }
            QUI.EndVertical();
        }

     
    }
}
