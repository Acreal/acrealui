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

using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AcrealUI
{
    public class UIDaggerfallSetupPanelMain : UIDaggerfallSetupPanel
    {
        #region Editor Variables
        [SerializeField] private UIButton button_restartGame = null;
        [SerializeField] private UIButton button_playGame = null;
        [SerializeField] private UIButton button_mods = null;
        [SerializeField] private UIButton button_advanced = null;
        [SerializeField] private UIToggle toggle_alwaysShowWindow_on = null;
        [SerializeField] private UIToggle toggle_alwaysShowWindow_off = null;
        [SerializeField] private UIToggle toggle_verticalSync_on = null;
        [SerializeField] private UIToggle toggle_verticalSync_off = null;
        [SerializeField] private UIToggle toggle_swapHealthFatigue_on = null;
        [SerializeField] private UIToggle toggle_swapHealthFatigue_off = null;
        [SerializeField] private UIToggle toggle_invertMouse_on = null;
        [SerializeField] private UIToggle toggle_invertMouse_off = null;
        [SerializeField] private UIToggle toggle_leftHandWeapons_on = null;
        [SerializeField] private UIToggle toggle_leftHandWeapons_off = null;
        [SerializeField] private UIToggle toggle_playerNudity_on = null;
        [SerializeField] private UIToggle toggle_playerNudity_off = null;
        [SerializeField] private UIToggle toggle_sdfFontRendering_on = null;
        [SerializeField] private UIToggle toggle_sdfFontRendering_off = null;
        [SerializeField] private UIToggle toggle_enableController_on = null;
        [SerializeField] private UIToggle toggle_enableController_off = null;
        [SerializeField] private UIArrowSelection arrowSelection_mouseLookSmoothing = null;
        [SerializeField] private UIArrowSelection arrowSelection_weaponSwingMode = null;
        #endregion


        #region Private Variables
        private List<string> mouseSmoothingOptions = null;
        #endregion


        #region MonoBehaviour
        protected override void Awake()
        {
            base.Awake();

            if (mouseSmoothingOptions == null)
            {
                mouseSmoothingOptions = new List<string>()
                {
                    TextManager.Instance.GetLocalizedText("none"),
                    TextManager.Instance.GetLocalizedText("lowest"),
                    TextManager.Instance.GetLocalizedText("low"),
                    TextManager.Instance.GetLocalizedText("medium"),
                    TextManager.Instance.GetLocalizedText("high"),
                    TextManager.Instance.GetLocalizedText("ultra")
                };
            }

            if (mouseSmoothingOptions != null && mouseSmoothingOptions.Count > 0)
            {
                for (int i = 0; i < mouseSmoothingOptions.Count; i++)
                {
                    arrowSelection_mouseLookSmoothing.PushTextOption(mouseSmoothingOptions[i]);
                }
            }

            string[] weaponSwingModes = TextManager.Instance.GetLocalizedTextList("weaponSwingModes", TextCollections.TextSettings);
            if (weaponSwingModes != null && weaponSwingModes.Length > 0)
            {
                for (int i = 0; i < weaponSwingModes.Length; i++)
                {
                    arrowSelection_weaponSwingMode.PushTextOption(weaponSwingModes[i]);
                }
            }

            button_playGame.Event_OnClicked += OnButtonClick_PlayGame;
            button_restartGame.Event_OnClicked += OnButtonClick_RestartGame;
            button_mods.Event_OnClicked += OnButtonClick_Mods;
            button_advanced.Event_OnClicked += OnButtonClick_Advanced;

            toggle_alwaysShowWindow_on.Event_OnToggledOn += OnAlwaysShowWindowChanged;
            toggle_alwaysShowWindow_off.Event_OnToggledOn += OnAlwaysShowWindowChanged;
            toggle_verticalSync_on.Event_OnToggledOn += OnVerticalSyncChanged;
            toggle_verticalSync_off.Event_OnToggledOn += OnVerticalSyncChanged;
            toggle_swapHealthFatigue_on.Event_OnToggledOn += OnSwapHealthFatigueChanged;
            toggle_swapHealthFatigue_off.Event_OnToggledOn += OnSwapHealthFatigueChanged;
            toggle_invertMouse_on.Event_OnToggledOn += OnInvertMouseChanged;
            toggle_invertMouse_off.Event_OnToggledOn += OnInvertMouseChanged;
            toggle_leftHandWeapons_on.Event_OnToggledOn += OnLeftHandWeaponsChanged;
            toggle_leftHandWeapons_off.Event_OnToggledOn += OnLeftHandWeaponsChanged;
            toggle_playerNudity_on.Event_OnToggledOn += OnPlayerNudityChanged;
            toggle_playerNudity_off.Event_OnToggledOn += OnPlayerNudityChanged;
            toggle_sdfFontRendering_on.Event_OnToggledOn += OnSDFFontRenderingChanged;
            toggle_sdfFontRendering_off.Event_OnToggledOn += OnSDFFontRenderingChanged;
            toggle_enableController_on.Event_OnToggledOn += OnEnableControllerChanged;
            toggle_enableController_off.Event_OnToggledOn += OnEnableControllerChanged;

            arrowSelection_mouseLookSmoothing.onSelectionChanged += OnMouseLookSmoothingChanged;
            arrowSelection_weaponSwingMode.onSelectionChanged += OnWeaponSwingModeChanged;
        }

        protected override void Start()
        {
            base.Start();

            toggle_alwaysShowWindow_off.isToggledOn = !DaggerfallUnity.Settings.ShowOptionsAtStart;
            toggle_alwaysShowWindow_on.isToggledOn = DaggerfallUnity.Settings.ShowOptionsAtStart;

            toggle_verticalSync_off.isToggledOn = !DaggerfallUnity.Settings.VSync;
            toggle_verticalSync_on.isToggledOn = DaggerfallUnity.Settings.VSync;

            toggle_swapHealthFatigue_off.isToggledOn = !DaggerfallUnity.Settings.SwapHealthAndFatigueColors;
            toggle_swapHealthFatigue_on.isToggledOn = DaggerfallUnity.Settings.SwapHealthAndFatigueColors;

            toggle_invertMouse_off.isToggledOn = !DaggerfallUnity.Settings.InvertMouseVertical;
            toggle_invertMouse_on.isToggledOn = DaggerfallUnity.Settings.InvertMouseVertical;

            toggle_leftHandWeapons_off.isToggledOn = DaggerfallUnity.Settings.Handedness != 1;
            toggle_leftHandWeapons_on.isToggledOn = DaggerfallUnity.Settings.Handedness == 1;

            toggle_playerNudity_off.isToggledOn = !DaggerfallUnity.Settings.PlayerNudity;
            toggle_playerNudity_on.isToggledOn = DaggerfallUnity.Settings.PlayerNudity;

            toggle_sdfFontRendering_off.isToggledOn = !DaggerfallUnity.Settings.SDFFontRendering;
            toggle_sdfFontRendering_on.isToggledOn = DaggerfallUnity.Settings.SDFFontRendering;

            toggle_enableController_off.isToggledOn = !DaggerfallUnity.Settings.EnableController;
            toggle_enableController_on.isToggledOn = DaggerfallUnity.Settings.EnableController;

            if (arrowSelection_mouseLookSmoothing != null)
            {
                int index = 0;
                float[] mouseSmoothingValues = SettingsManager.GetMouseLookSmoothingFactors();
                for (int i = 0; i < mouseSmoothingValues.Length; i++)
                {
                    if (DaggerfallUnity.Settings.MouseLookSmoothingFactor <= mouseSmoothingValues[i])
                    {
                        index = i;
                        break;
                    }
                }
                arrowSelection_mouseLookSmoothing.SetIndexNoCallback(index);
            }

            arrowSelection_weaponSwingMode.SetIndexNoCallback(DaggerfallUnity.Settings.WeaponSwingMode);
        }
        #endregion


        #region Input Handling
        private void OnButtonClick_PlayGame(UIButton button)
        {
            DaggerfallUnity.Settings.SaveSettings();
            SceneManager.LoadScene(SceneControl.GameSceneIndex);
        }

        private void OnButtonClick_RestartGame(UIButton button)
        {
            DaggerfallUnity.Settings.MyDaggerfallPath = string.Empty;
            SceneManager.LoadScene(SceneControl.StartupSceneIndex);
        }

        private void OnButtonClick_Mods(UIButton button)
        {
            ModLoaderInterfaceWindow modLoaderWindow = new ModLoaderInterfaceWindow(DaggerfallUI.UIManager);
            DaggerfallUI.UIManager.PushWindow(modLoaderWindow);
            Hide();
        }

        private void OnButtonClick_Advanced(UIButton button)
        {
            //AdvancedSettingsWindow advancedSettingsWindow = new AdvancedSettingsWindow(DaggerfallUI.UIManager);
            //DaggerfallUI.UIManager.PushWindow(advancedSettingsWindow);
            Hide();
        }

        private void OnAlwaysShowWindowChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_alwaysShowWindow_on)
                    {
                        DaggerfallUnity.Settings.ShowOptionsAtStart = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.ShowOptionsAtStart = false;
                    }
                }
            }
        }

        private void OnVerticalSyncChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_verticalSync_on)
                    {
                        DaggerfallUnity.Settings.VSync = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.VSync = false;
                    }
                }
            }
        }

        private void OnSwapHealthFatigueChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_swapHealthFatigue_on)
                    {
                        DaggerfallUnity.Settings.SwapHealthAndFatigueColors = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.SwapHealthAndFatigueColors = false;
                    }
                }
            }
        }

        private void OnInvertMouseChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_invertMouse_on)
                    {
                        DaggerfallUnity.Settings.InvertMouseVertical = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.InvertMouseVertical = false;
                    }
                }
            }
        }

        private void OnMouseLookSmoothingChanged(UIArrowSelection arrowSelection)
        {
            float[] mouseSmoothingFactors = SettingsManager.GetMouseLookSmoothingFactors();
            if (mouseSmoothingFactors != null && mouseSmoothingFactors.Length > 0)
            {
                DaggerfallUnity.Settings.MouseLookSmoothingFactor = SettingsManager.GetMouseLookSmoothingFactor(arrowSelection_mouseLookSmoothing.index);
            }
        }

        private void OnLeftHandWeaponsChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_leftHandWeapons_on)
                    {
                        DaggerfallUnity.Settings.Handedness = 1;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.Handedness = 0;
                    }
                }
            }
        }

        private void OnPlayerNudityChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_playerNudity_on)
                    {
                        DaggerfallUnity.Settings.PlayerNudity = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.PlayerNudity = false;
                    }
                }
            }
        }

        private void OnSDFFontRenderingChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_sdfFontRendering_on)
                    {
                        DaggerfallUnity.Settings.SDFFontRendering = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.SDFFontRendering = false;
                    }
                }
            }
        }

        private void OnEnableControllerChanged(UIToggle toggle)
        {
            if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (toggle == toggle_enableController_on)
                    {
                        DaggerfallUnity.Settings.EnableController = true;
                    }
                    else
                    {
                        DaggerfallUnity.Settings.EnableController = false;
                    }
                }
            }
        }

        private void OnWeaponSwingModeChanged(UIArrowSelection arrowSelection)
        {
            if (arrowSelection != null)
            {
                string[] weaponSwingModes = TextManager.Instance.GetLocalizedTextList("weaponSwingModes", TextCollections.TextSettings);
                if (weaponSwingModes != null && weaponSwingModes.Length > 0)
                {
                    if (arrowSelection.index < weaponSwingModes.Length)
                    {
                        DaggerfallUnity.Settings.WeaponSwingMode = arrowSelection.index;
                    }
                }
            }
        }
        #endregion
    }
}