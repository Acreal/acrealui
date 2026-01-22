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

using TMPro;
using UnityEngine;

namespace AcrealUI
{
    public class UIDaggerfallSetupPanelAdvanced : UIDaggerfallSetupPanel
    {
        #region Editor Variables
        [SerializeField] private TextMeshProUGUI text_title = null;

        [SerializeField] private UIOptionsPanel optionsPanel_gameplay = null;
        [SerializeField] private UIOptionsPanel optionsPanel_interface = null;
        [SerializeField] private UIOptionsPanel optionsPanel_enhancements = null;
        [SerializeField] private UIOptionsPanel optionsPanel_video = null;
        [SerializeField] private UIOptionsPanel optionsPanel_accessibility = null;

        [SerializeField] private UIToggle toggle_gameplay = null;
        [SerializeField] private UIToggle toggle_interface = null;
        [SerializeField] private UIToggle toggle_enhancements = null;
        [SerializeField] private UIToggle toggle_video = null;
        [SerializeField] private UIToggle toggle_accessibility = null;
        #endregion


        #region Private Variables
        private UIOptionsPanel currentPanel = null;
        #endregion


        #region MonoBehaviour
        protected override void Awake()
        {
            base.Awake();

            optionsPanel_gameplay.Initialize();
            optionsPanel_interface.Initialize();
            optionsPanel_enhancements.Initialize();
            optionsPanel_video.Initialize();
            optionsPanel_accessibility.Initialize();

            toggle_gameplay.Event_OnToggledOn += OnGameplayToggled;
            toggle_interface.Event_OnToggledOn += OnInterfaceToggled;
            toggle_enhancements.Event_OnToggledOn += OnEnhancementsToggled;
            toggle_video.Event_OnToggledOn += OnVideoToggled;
            toggle_accessibility.Event_OnToggledOn += OnAccessibilityToggled;
        }
        #endregion


        #region Panels
        private void SetCurrentPanel(UIOptionsPanel panel)
        {
            if(currentPanel != null)
            {
                currentPanel.Hide();
            }

            currentPanel = panel;

            if(currentPanel != null)
            {
                if (text_title != null) { text_title.text = currentPanel.panelTitle; }
                currentPanel.Show();
            }
            else
            {
                if (text_title != null) { text_title.text = null; }
            }
        }
        #endregion


        #region Input Handling
        private void OnGameplayToggled(UIToggle toggle)
        {
            SetCurrentPanel(optionsPanel_gameplay);
        }

        private void OnInterfaceToggled(UIToggle toggle)
        {
            SetCurrentPanel(optionsPanel_interface);
        }

        private void OnEnhancementsToggled(UIToggle toggle)
        {
            SetCurrentPanel(optionsPanel_enhancements);
        }

        private void OnVideoToggled(UIToggle toggle)
        {
            SetCurrentPanel(optionsPanel_video);
        }

        private void OnAccessibilityToggled(UIToggle toggle)
        {
            SetCurrentPanel(optionsPanel_accessibility);
        }
        #endregion
    }
}