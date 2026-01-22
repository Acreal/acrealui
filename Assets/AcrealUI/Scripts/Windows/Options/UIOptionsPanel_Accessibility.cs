/*
Copyright (c) 2025-2026 Acreal (https://github.com/acreal)

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
documentation files (the "Software"), to deal in the Software without restriction, including without 
limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of 
the Software, and to permit persons to whom the Software is furnished to do so, subject to the following 
conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions 
of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;

namespace AcrealUI
{
    public class UIOptionsPanel_Accessibility : UIOptionsPanel
    {
        public override string panelTitle => "Accessibility";


        #region Editor Variables
        [SerializeField] private UIButton button_resetAutomapSettings = null;
        [SerializeField] private UIButton button_resetDungeonMapSettings = null;
        [SerializeField] private UIToggle toggle_minimapQoLToggle_off = null;
        [SerializeField] private UIToggle toggle_minimapQoLToggle_on = null;
        [SerializeField] private UIColorPicker colorPicker_guildHallColor = null;
        [SerializeField] private UIColorPicker colorPicker_shopColor = null;
        [SerializeField] private UIColorPicker colorPicker_tavernColor = null;
        [SerializeField] private UIColorPicker colorPicker_residenceColor = null;
        [SerializeField] private UIColorPicker colorPicker_innerBlockColor = null;
        [SerializeField] private UIColorPicker colorPicker_borderBlockColor = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            button_resetAutomapSettings.Event_OnClicked += OnResetAutomapSettings;
            button_resetDungeonMapSettings.Event_OnClicked += OnResetDungeonMapSettings;

            toggle_minimapQoLToggle_off.Event_OnToggledOn += OnMinimapQoLToggledOff;
            toggle_minimapQoLToggle_on.Event_OnToggledOn += OnMinimapQoLToggledOn;

            colorPicker_guildHallColor.Initialize();
            colorPicker_guildHallColor.onColorChanged += OnGuildHallColorChanged;

            colorPicker_shopColor.Initialize();
            colorPicker_shopColor.onColorChanged += OnShopColorChanged;

            colorPicker_tavernColor.Initialize();
            colorPicker_tavernColor.onColorChanged += OnTavernColorChanged;

            colorPicker_residenceColor.Initialize();
            colorPicker_residenceColor.onColorChanged += OnResidenceColorChanged;

            colorPicker_innerBlockColor.Initialize();
            colorPicker_innerBlockColor.onColorChanged += OnInnerBlockColorChanged;

            colorPicker_borderBlockColor.Initialize();
            colorPicker_borderBlockColor.onColorChanged += OnBorderBlockColorChanged;
        }
        #endregion


        #region Show/Hide
        public override void Show()
        {
            base.Show();

            toggle_minimapQoLToggle_on.isToggledOn = DaggerfallUnity.Settings.DungeonMicMapQoL;
            toggle_minimapQoLToggle_off.isToggledOn = !DaggerfallUnity.Settings.DungeonMicMapQoL;

            colorPicker_innerBlockColor.currentColor = DaggerfallUnity.Settings.DunMicMapInnerColor;
            colorPicker_borderBlockColor.currentColor = DaggerfallUnity.Settings.DunMicMapBorderColor;

            colorPicker_guildHallColor.currentColor = DaggerfallUnity.Settings.AutomapTempleColor;
            colorPicker_shopColor.currentColor = DaggerfallUnity.Settings.AutomapShopColor;
            colorPicker_tavernColor.currentColor = DaggerfallUnity.Settings.AutomapTavernColor;
            colorPicker_residenceColor.currentColor = DaggerfallUnity.Settings.AutomapHouseColor;
        }

        public override void Hide()
        {
            base.Hide();
        }
        #endregion


        #region Input Handling
        private void OnResetAutomapSettings(UIButton button)
        {
            colorPicker_innerBlockColor.currentColor = DaggerfallUI.DaggerfallDefaultMicMapInnerQoLColor;
            colorPicker_borderBlockColor.currentColor = DaggerfallUI.DaggerfallDefaultMicMapBorderQoLColor;
        }

        private void OnResetDungeonMapSettings(UIButton button)
        {
            colorPicker_guildHallColor.currentColor = DaggerfallUI.DaggerfallDefaultTempleAutomapColor;
            colorPicker_shopColor.currentColor = DaggerfallUI.DaggerfallDefaultShopAutomapColor;
            colorPicker_tavernColor.currentColor = DaggerfallUI.DaggerfallDefaultTavernAutomapColor;
            colorPicker_residenceColor.currentColor = DaggerfallUI.DaggerfallDefaultHouseAutomapColor;
        }

        private void OnMinimapQoLToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.DungeonMicMapQoL = false;
        }

        private void OnMinimapQoLToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.DungeonMicMapQoL = true;
        }

        private void OnGuildHallColorChanged(Color color)
        {
            DaggerfallUnity.Settings.AutomapTempleColor = color;
        }

        private void OnShopColorChanged(Color color)
        {
            DaggerfallUnity.Settings.AutomapShopColor = color;
        }

        private void OnTavernColorChanged(Color color)
        {
            DaggerfallUnity.Settings.AutomapTavernColor = color;
        }

        private void OnResidenceColorChanged(Color color)
        {
            DaggerfallUnity.Settings.AutomapHouseColor = color;
        }

        private void OnInnerBlockColorChanged(Color color)
        {
            DaggerfallUnity.Settings.DunMicMapInnerColor = color;
        }

        private void OnBorderBlockColorChanged(Color color)
        {
            DaggerfallUnity.Settings.DunMicMapBorderColor = color;
        }
        #endregion
    }
}