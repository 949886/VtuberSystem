// Copyright (c) 2016 - 2018 Ez Entertainment SRL. All Rights Reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement
// A Copy of the EULA APPENDIX 1 is available at http://unity3d.com/company/legal/as_terms

using QuickEditor;
using QuickEngine.Extensions;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Ez
{
    public class EZStyles
    {
        public enum General
        {
            SideButtonCollapseSideBar,
            SideButtonExpandSideBar,

            SideButtonControlPanel,
            SideButtonDefineSymbols,
            SideButtonDataManager,
            SideButtonDataBind,
            SideButtonPooly,
            SideButtonAdsManager,
            SideButtonHelpBtnHelp,
            SideButtonAbout,

            SideButtonSelectedControlPanel,
            SideButtonSelectedDefineSymbols,
            SideButtonSelectedDataManager,
            SideButtonSelectedDataBind,
            SideButtonSelectedPooly,
            SideButtonSelectedAdsManager,
            SideButtonSelectedHelpBtnHelp,
            SideButtonSelectedAbout,

            SideButtonTwitter,
            SideButtonFacebook,
            SideButtonYoutube,
        }


        private static GUISkin skin;
        public static GUISkin Skin { get { if(skin == null) { skin = GetSkin(); } return skin; } }

        /// <summary>
        /// Returns a style that has been added to the skin.
        /// </summary>
        public static GUIStyle GetStyle(string styleName) { return Skin.GetStyle(styleName); }
        /// <summary>
        /// Returns a style that has been added to the skin.
        /// This method is to be used paired with the enums in the Style class.
        /// </summary>
        public static GUIStyle GetStyle<T>(T styleName) { return Skin.GetStyle(QStyles.GetStyleName(styleName)); }

        private static GUISkin GetSkin()
        {
            GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
            List<GUIStyle> styles = new List<GUIStyle>();
            styles.AddRange(GeneralStyles());

            skin.customStyles = styles.ToArray();
            return skin;
        }

        private static void UpdateSkin()
        {
            skin = null;
            skin = GetSkin();
        }

        public static void AddStyle(GUIStyle style)
        {
            if(style == null) { return; }
            List<GUIStyle> customStyles = new List<GUIStyle>();
            customStyles.AddRange(Skin.customStyles);
            if(customStyles.Contains(style)) { return; }
            customStyles.Add(style);
            Skin.customStyles = customStyles.ToArray();
        }

        public static void RemoveStyle(GUIStyle style)
        {
            if(style == null) { return; }
            List<GUIStyle> customStyles = new List<GUIStyle>();
            customStyles.AddRange(Skin.customStyles);
            if(!customStyles.Contains(style)) { return; }
            customStyles.Remove(style);
            Skin.customStyles = customStyles.ToArray();
        }


        private static List<GUIStyle> GeneralStyles()
        {
            List<GUIStyle> styles = new List<GUIStyle>
            {
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonCollapseSideBar), EZResources.sideButtonCollapseSideBar, QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(2, 28, 2, 2), new RectOffset(2, 32, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonExpandSideBar), EZResources.sideButtonExpandSideBar, QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(2, 28, 2, 2), new RectOffset(2, 32, 2, 4)),

                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonControlPanel), EZResources.sideButtonControlPanel, QUI.IsProSkin ? QColors.BlueLight.Color : QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonDefineSymbols), EZResources.sideButtonDefineSymbols, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonDataManager), EZResources.sideButtonDataManager, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color, QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonDataBind), EZResources.sideButtonDataBind, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonPooly), EZResources.sideButtonPooly, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonAdsManager), EZResources.sideButtonAdsManager, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color, QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonHelpBtnHelp), EZResources.sideButtonHelp, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonAbout), EZResources.sideButtonAbout, QUI.IsProSkin ? QColors.BlueLight.Color :QColors.BlueDark.Color, QColors.BlueDark.Color,  QColors.GreenLight.Color , new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),

                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonTwitter), EZResources.sideButtonTwitter, ColorExtensions.ColorFrom256(128,128,128), ColorExtensions.ColorFrom256(242,242,242),  ColorExtensions.ColorFrom256(0,153,209), new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4), 14),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonFacebook), EZResources.sideButtonFacebook, ColorExtensions.ColorFrom256(128,128,128), ColorExtensions.ColorFrom256(242,242,242), ColorExtensions.ColorFrom256(74,112,186), new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4), 14),
                GetSideButtonStyle(QStyles.GetStyleName(General.SideButtonYoutube), EZResources.sideButtonYoutube, ColorExtensions.ColorFrom256(128,128,128),  ColorExtensions.ColorFrom256(242,242,242),  ColorExtensions.ColorFrom256(255, 102,102), new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4), 14),

                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedControlPanel), EZResources.sideButtonControlPanelSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedDefineSymbols), EZResources.sideButtonDefineSymbolsSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedDataManager), EZResources.sideButtonDataManagerSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedDataBind), EZResources.sideButtonDataBindSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedPooly), EZResources.sideButtonPoolySelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedAdsManager), EZResources.sideButtonAdsManagerSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedHelpBtnHelp), EZResources.sideButtonHelpSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
                GetSideButtonSelectedStyle(QStyles.GetStyleName(General.SideButtonSelectedAbout), EZResources.sideButtonAboutSelected, QColors.GreenLight.Color, new RectOffset(28, 2, 2, 2), new RectOffset(32, 2, 2, 4)),
            };
            return styles;
        }

        private static GUIStyle GetSideButtonStyle(string styleName, QTexture qTexture, Color normalTextColor, Color hoverTextColor, Color activeTextColor, RectOffset border, RectOffset padding, int fontSize = 20)
        {
            return new GUIStyle()
            {
                name = styleName,
                normal = { background = qTexture.normal2D, textColor = normalTextColor },
                onNormal = { background = qTexture.normal2D, textColor = normalTextColor },
                hover = { background = qTexture.hover2D, textColor = hoverTextColor },
                onHover = { background = qTexture.hover2D, textColor = hoverTextColor },
                active = { background = qTexture.active2D, textColor = activeTextColor },
                onActive = { background = qTexture.active2D, textColor = activeTextColor },
                border = border,
                padding = padding,
                fontSize = fontSize,
                alignment = TextAnchor.MiddleLeft,
                font = QResources.GetFont(FontName.Sansation.Regular)
            };
        }
        private static GUIStyle GetSideButtonSelectedStyle(string styleName, QTexture qTexture, Color normalTextColor, RectOffset border, RectOffset padding)
        {
            return new GUIStyle()
            {
                name = styleName.ToString(),
                normal = { background = qTexture.normal2D, textColor = normalTextColor },
                onNormal = { background = qTexture.normal2D, textColor = normalTextColor },
                border = border,
                padding = padding,
                fontSize = 20,
                alignment = TextAnchor.MiddleLeft,
                font = QResources.GetFont(FontName.Sansation.Regular)
            };
        }
    }
}
