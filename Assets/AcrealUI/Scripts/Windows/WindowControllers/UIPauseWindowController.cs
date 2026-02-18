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

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    public class UIPauseWindowController : DaggerfallPauseOptionsWindow, IWindowController
    {
        #region Variables
        private UIPauseWindow _pauseWindowInstance = null;
        private Dictionary<string, string> _allPrimaryKeybindsDict = null;
        private Dictionary<string, string> _allSecondaryKeybindsDict = null;
        private Dictionary<string, InputManager.Actions> _stringToActionEnumDict = null;
        private Dictionary<string, InputManager.AxisActions> _stringToAxisActionEnumDict = null;
        private Dictionary<string, InputManager.JoystickUIActions> _stringToJoystickActionEnumDict = null;
        private Dictionary<int, Resolution> _instanceIdToResolutionDict = null;
        private Dictionary<int, int> _instanceIdToRefreshRateDict = null;
        private Dictionary<int, string> _toggleIdToActionStringDict = null;
        private string[] _localizedStrings_weaponSwingModes = null;
        private string[] _localizedStrings_shadowResolutionModes = null;
        #endregion


        #region Initalization/Cleanup
        public UIPauseWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            _allPrimaryKeybindsDict = new Dictionary<string, string>(64);
            _allSecondaryKeybindsDict = new Dictionary<string, string>(64);

            _stringToActionEnumDict = new Dictionary<string, InputManager.Actions>(45);
            _stringToAxisActionEnumDict = new Dictionary<string, InputManager.AxisActions>(4);
            _stringToJoystickActionEnumDict = new Dictionary<string, InputManager.JoystickUIActions>(4);

            _instanceIdToResolutionDict = new Dictionary<int, Resolution>();
            _instanceIdToRefreshRateDict = new Dictionary<int, int>();
            _toggleIdToActionStringDict = new Dictionary<int, string>();

            _localizedStrings_weaponSwingModes = TextManager.Instance.GetLocalizedTextList("weaponSwingModes", TextCollections.TextSettings);
            _localizedStrings_shadowResolutionModes = TextManager.Instance.GetLocalizedTextList("shadowResolutionModes", TextCollections.TextSettings);

            ResetKeybindDict();
            InitializePauseWindow();
        }

        private void InitializePauseWindow()
        {
            if (_pauseWindowInstance == null)
            {
                if (UIManager.referenceManager.prefab_pauseWindow != null)
                {
                    _pauseWindowInstance = Object.Instantiate(UIManager.referenceManager.prefab_pauseWindow);
                    _pauseWindowInstance.Initialize();

                    _pauseWindowInstance.Event_ButtonClick_PrevWindow += OnBackButtonClicked;

                    _pauseWindowInstance.Event_ButtonClick_CloseWindow += () =>
                    {
                        CancelWindow();
                    };

                    //////////////////////////////////////////////////////////////////////////////////////////////
                    // PAUSED PANEL
                    //////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelPaused != null)
                    {
                        _pauseWindowInstance.panelPaused.gameObject.SetActive(true);

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_Continue += () =>
                        {
                            CancelWindow();
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_SaveGame += () =>
                        {
                            uiManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { uiManager, DaggerfallUnitySaveGameWindow.Modes.SaveGame, this, false }));
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_LoadGame += () =>
                        {
                            uiManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { uiManager, DaggerfallUnitySaveGameWindow.Modes.LoadGame, this, false }));
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_Settings += () =>
                        {
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings);
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_ExitGame += () =>
                        {
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Confirmation);

                            DaggerfallMessageBox confirmExitBox = new DaggerfallMessageBox(uiManager, DaggerfallMessageBox.CommonMessageBoxButtons.YesNo, strAreYouSure, this);
                            confirmExitBox.OnButtonClick += (DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton) =>
                            {
                                sender.CloseWindow();

                                if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
                                {
                                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiExitGame);
                                    CancelWindow();
                                }
                                else
                                {
                                    _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Paused);
                                }
                            };
                            confirmExitBox.Show();
                        };
                    }

                    //////////////////////////////////////////////////////////////////////////////////////////////
                    // SETTINGS PANEL
                    //////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelSettings != null)
                    {
                        _pauseWindowInstance.panelSettings.gameObject.SetActive(false);

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_GeneralSettings += () =>
                        {
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_General);
                        };

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_VideoSettings += () =>
                        {
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_Video);
                        };

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_AudioSettings += () =>
                        {
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_Audio);
                        };

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_ControlSettings += () =>
                        {
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_Controls);
                        };
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    // GENERAL SETTINGS PANEL
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelGeneralSettings != null)
                    {
                        _pauseWindowInstance.panelGeneralSettings.gameObject.SetActive(false);

                        #region Gameplay Settings
                        UIScrollListGroup generalGroup = _pauseWindowInstance.panelGeneralSettings.GetOrAddScrollListGroup("Gameplay"); // TODO(Acreal): localize this string

                        UIToggle headbobToggle = generalGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        headbobToggle.SetDisplayName("Head Bobbing"); // TODO(Acreal): localize this string
                        headbobToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.HeadBobbing; };
                        headbobToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.HeadBobbing = toggle.isToggledOn;
                        };
                        #endregion
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    // VIDEO SETTINGS PANEL
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelVideoSettings != null)
                    {
                        _pauseWindowInstance.panelVideoSettings.gameObject.SetActive(false);

                        #region Window
                        UIScrollListGroup windowGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Window"); // TODO(Acreal): localize this string
                        windowGroup.Collapse();

                        UIToggle fullScreenToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        fullScreenToggle.SetDisplayName("Fullscreen");  // TODO(Acreal): localize this string
                        fullScreenToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.Fullscreen; };
                        fullScreenToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.Fullscreen = toggle.isToggledOn;
                        };

                        UIToggle exclusiveToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        exclusiveToggle.SetDisplayName("Exclusive Fullscreen");  // TODO(Acreal): localize this string
                        exclusiveToggle.isDisabled = !DaggerfallUnity.Settings.Fullscreen;
                        exclusiveToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ExclusiveFullscreen; };
                        exclusiveToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ExclusiveFullscreen = toggle.isToggledOn;
                        };

                        UIToggle vsyncToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        vsyncToggle.SetDisplayName("Vsync");  // TODO(Acreal): localize this string
                        vsyncToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.VSync; };
                        vsyncToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.VSync = toggle.isToggledOn;
                        };
                        #endregion

                        #region Resolution/Refresh Rate Settings
                        Resolution[] allResolutions = Screen.resolutions;
                        
                        UIScrollListGroup resolutionGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Resolution"); // TODO(Acreal): localize this string
                        resolutionGroup.Collapse();

                        UIToggleGroup resolutionToggleGroup = resolutionGroup.groupParent.gameObject.AddComponent<UIToggleGroup>();
                        resolutionToggleGroup.canBeToggledOff = false;
                        resolutionToggleGroup.Initialize();

                        UIScrollListGroup refreshRateGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Refresh Rate"); // TODO(Acreal): localize this string
                        refreshRateGroup.Collapse();

                        UIToggleGroup refreshRateToggleGroup = refreshRateGroup.groupParent.gameObject.AddComponent<UIToggleGroup>();
                        refreshRateToggleGroup.canBeToggledOff = false;
                        refreshRateToggleGroup.Initialize();

                        #region Find All Valid Resolutions and Refresh Rates
                        List<Resolution> resolutions = new List<Resolution>();
                        UIToggle selectedResolutionEntry = null;

                        List<int> refreshRates = new List<int>();
                        UIToggle selectedRefreshRateEntry = null;
                        int maxRefreshRate = 0;

                        for (int i = 0; i < allResolutions.Length; i++)
                        {
                            #region Resolution
                            {
                                bool foundResolution = false;

                                foreach (Resolution resolution in resolutions)
                                {
                                    if (resolution.width == allResolutions[i].width && resolution.height == allResolutions[i].height)
                                    {
                                        foundResolution = true;
                                        break;
                                    }
                                }

                                if (!foundResolution)
                                {
                                    resolutions.Add(allResolutions[i]);
                                }
                            }
                            #endregion

                            #region Refresh Rates
                            if(allResolutions[i].refreshRate < 30) { continue; }

                            bool foundRefreshRate = false;
                            foreach (int refreshRate in refreshRates)
                            {
                                if (refreshRate == allResolutions[i].refreshRate)
                                {
                                    foundRefreshRate = true;
                                    break;
                                }
                            }

                            if (!foundRefreshRate)
                            {
                                refreshRates.Add(allResolutions[i].refreshRate);
                                if(allResolutions[i].refreshRate > maxRefreshRate)
                                {
                                    maxRefreshRate = allResolutions[i].refreshRate;
                                }
                            }
                            #endregion
                        }

                        if(refreshRates.Count == 0)
                        {
                            refreshRates.Add(30);
                            maxRefreshRate = 30;
                        }

                        resolutions.Reverse();
                        refreshRates.Sort((x, y) => { return y.CompareTo(x); });
                        #endregion
                        
                        #region Add Resolution Entries
                        for (int i = 0; i < resolutions.Count; i++)
                        {
                            UIToggle resolutionEntry = resolutionGroup.AddElement(UIManager.referenceManager.prefab_resolutionSettingEntry) as UIToggle;
                            resolutionToggleGroup.AddToggle(resolutionEntry);
                            _instanceIdToResolutionDict[resolutionEntry.GetInstanceID()] = resolutions[i];
                            resolutionEntry.SetDisplayName(resolutions[i].width + "x" + resolutions[i].height);

                            if (selectedResolutionEntry == null &&
                                resolutions[i].width == Screen.currentResolution.width &&
                                resolutions[i].height == Screen.currentResolution.height)
                            {
                                selectedResolutionEntry = resolutionEntry;
                            }

                            resolutionEntry.DataSource_IsToggledOn = (GameObject sender) =>
                            {
                                UIToggle toggle = sender != null ? sender.GetComponent<UIToggle>() : null;
                                if (toggle != null)
                                {
                                    if (_instanceIdToResolutionDict.TryGetValue(toggle.GetInstanceID(), out Resolution res))
                                    {
                                        Resolution curRes = Screen.currentResolution;
                                        return res.width == curRes.width && res.height == curRes.height;
                                    }
                                }
                                return false;
                            };

                            resolutionEntry.Event_OnToggledOn += (UIToggle toggle) =>
                            {
                                saveSettings = true;

                                Resolution res = new Resolution();
                                _instanceIdToResolutionDict.TryGetValue(toggle.GetInstanceID(), out res);
                                res.refreshRate = Screen.currentResolution.refreshRate;

                                DaggerfallUnity.Settings.ResolutionWidth = res.width;
                                DaggerfallUnity.Settings.ResolutionHeight = res.height;
                                ApplyResolution(res);
                            };
                        }

                        if (selectedResolutionEntry != null)
                        {
                            resolutionToggleGroup.SetActiveToggle(selectedResolutionEntry);
                        }
                        #endregion

                        #region Add Refresh Rate Entries
                        for (int i = 0; i < refreshRates.Count; i++)
                        {
                            UIToggle refreshRateEntry = refreshRateGroup.AddElement(UIManager.referenceManager.prefab_resolutionSettingEntry) as UIToggle;
                            refreshRateToggleGroup.AddToggle(refreshRateEntry);
                            _instanceIdToRefreshRateDict[refreshRateEntry.GetInstanceID()] = refreshRates[i];
                            refreshRateEntry.SetDisplayName(refreshRates[i].ToString("N0"));

                            if (selectedRefreshRateEntry == null && refreshRates[i] == Screen.currentResolution.refreshRate)
                            {
                                selectedRefreshRateEntry = refreshRateEntry;
                            }

                            refreshRateEntry.DataSource_IsToggledOn = (GameObject sender) => 
                            {
                                UIToggle toggle = sender != null ? sender.GetComponent<UIToggle>() : null;
                                if (toggle != null && _instanceIdToRefreshRateDict.TryGetValue(toggle.GetInstanceID(), out int refreshRate))
                                {
                                    return refreshRate == Screen.currentResolution.refreshRate;
                                }
                                return false;
                            };

                            refreshRateEntry.Event_OnToggledOn += (UIToggle toggle) =>
                            {
                                saveSettings = true;

                                int refreshRate = 0;
                                _instanceIdToRefreshRateDict.TryGetValue(toggle.GetInstanceID(), out refreshRate);
                                if (refreshRate < 30) { refreshRate = maxRefreshRate; }

                                Resolution res = Screen.currentResolution;
                                res.refreshRate = refreshRate;
                                ApplyResolution(res);
                            };
                        }

                        if (selectedRefreshRateEntry != null)
                        {
                            refreshRateToggleGroup.SetActiveToggle(selectedRefreshRateEntry);
                        }
                        #endregion
                        #endregion

                        #region Camera Settings
                        UIScrollListGroup cameraGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Camera"); // TODO(Acreal): localize this string
                        cameraGroup.Collapse();

                        UISlider fovSlider = cameraGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        fovSlider.SetTextTitle("Field of View"); // TODO(Acreal): localize this string
                        fovSlider.SetSliderMinMax(60, 120, true);
                        fovSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.FieldOfView; };
                        fovSlider.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.FieldOfView.ToString("N0"); };
                        fovSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            DaggerfallUnity.Settings.FieldOfView = slider.GetSliderValueAsInt();

                            Camera camera = GameManager.Instance.MainCamera;
                            if (camera != null)
                            {
                                camera.fieldOfView = DaggerfallUnity.Settings.FieldOfView;
                            }
                        };
                        #endregion

                        #region Shadow Settings
                        UIScrollListGroup shadowGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Shadows"); // TODO(Acreal): localize this string
                        shadowGroup.Collapse();

                        UISlider shadowRes = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowRes.SetTextTitle("Shadow Resolution"); // TODO(Acreal): localize this string
                        shadowRes.SetSliderMinMax(0, 3, true);
                        shadowRes.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.ShadowResolutionMode; };
                        shadowRes.DataSource_SliderValueString = (_) => { return _localizedStrings_shadowResolutionModes[DaggerfallUnity.Settings.ShadowResolutionMode]; };
                        shadowRes.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ShadowResolutionMode = (int)slider.GetSliderValue();
                        };

                        UISlider shadowDistExt = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowDistExt.SetTextTitle("Exterior Shadow Distance"); // TODO(Acreal): localize this string
                        shadowDistExt.SetSliderMinMax(0.1f, 150f, false);
                        shadowDistExt.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.ExteriorShadowDistance; };
                        shadowDistExt.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.ExteriorShadowDistance.ToString("F2"); };
                        shadowDistExt.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ExteriorShadowDistance = slider.GetSliderValue();
                        };

                        UISlider shadowDistInt = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowDistInt.SetTextTitle("Interior Shadow Distance"); // TODO(Acreal): localize this string
                        shadowDistInt.SetSliderMinMax(0.1f, 50f, false);
                        shadowDistInt.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.InteriorShadowDistance; };
                        shadowDistInt.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.InteriorShadowDistance.ToString("F2"); };
                        shadowDistInt.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.InteriorShadowDistance = slider.GetSliderValue();
                        };

                        UISlider shadowDistDungeon = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowDistDungeon.SetTextTitle("Dungeon Shadow Distance"); // TODO(Acreal): localize this string
                        shadowDistDungeon.SetSliderMinMax(0.1f, 50f, false);
                        shadowDistDungeon.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.DungeonShadowDistance; };
                        shadowDistDungeon.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.DungeonShadowDistance.ToString("F2"); };
                        shadowDistDungeon.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.DungeonShadowDistance = slider.GetSliderValue();
                        };

                        UIToggle exteriorLightShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        exteriorLightShadows.SetDisplayName("Exterior Lights Cast Shadows"); // TODO(Acreal): localize this string
                        exteriorLightShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ExteriorLightShadows; };
                        exteriorLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ExteriorLightShadows = toggle.isToggledOn;
                        };

                        UIToggle interiorLightShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        interiorLightShadows.SetDisplayName("Interior Lights Cast Shadows"); // TODO(Acreal): localize this string
                        interiorLightShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.InteriorLightShadows; };
                        interiorLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.InteriorLightShadows = toggle.isToggledOn;
                        };

                        UIToggle dungeonLightShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        dungeonLightShadows.SetDisplayName("Dungeon Lights Cast Shadows"); // TODO(Acreal): localize this string
                        dungeonLightShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.DungeonLightShadows; };
                        dungeonLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.DungeonLightShadows = toggle.isToggledOn;
                        };

                        UIToggle npcShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        npcShadows.SetDisplayName("Dynamic NPCs Cast Shadows"); // TODO(Acreal): localize this string
                        npcShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.MobileNPCShadows; };
                        npcShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.MobileNPCShadows = toggle.isToggledOn;
                        };

                        UIToggle billboardShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        billboardShadows.SetDisplayName("Object Billboards Cast Shadows"); // TODO(Acreal): localize this string
                        billboardShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.GeneralBillboardShadows; };
                        billboardShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.GeneralBillboardShadows = toggle.isToggledOn;
                        };

                        UIToggle natureBillboardShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        natureBillboardShadows.SetDisplayName("Foliage Billboards Cast Shadows"); // TODO(Acreal): localize this string
                        natureBillboardShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.NatureBillboardShadows; };
                        natureBillboardShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.NatureBillboardShadows = toggle.isToggledOn;
                        };
                        #endregion

                        // TODO(Acreal): finish adding video options
                        // SLIDER: fullscreen mode
                        // SLIDER: retro rendering mode
                        // SLIDER: main filter mode
                        // SLIDER: quality level
                        // TOGGLE: random dungeon textures
                        // TOGGLE: vsync
                        // TOGGLE: ambient lit interiors
                        // TOGGLE: texture array
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    // AUDIO SETTINGS PANEL
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelAudioSettings != null)
                    {
                        _pauseWindowInstance.panelAudioSettings.gameObject.SetActive(false);

                        #region Volume Settings
                        UIScrollListGroup volumeGroup = _pauseWindowInstance.panelAudioSettings.GetOrAddScrollListGroup("Volume"); // TODO(Acreal): localize this string

                        UISlider soundVolume = volumeGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        soundVolume.SetTextTitle("Sound"); // TODO(Acreal): localize this string
                        soundVolume.SetSliderMinMax(0f, 100f, true);
                        soundVolume.DataSource_SliderValue = (_) => { return Mathf.RoundToInt(DaggerfallUnity.Settings.SoundVolume * 100f); };
                        soundVolume.DataSource_SliderValueString = (_) =>
                        {
                            Debug.Log("Sound Volume: " + DaggerfallUnity.Settings.SoundVolume);
                            return ((int)(DaggerfallUnity.Settings.SoundVolume * 100f)).ToString("N0") + "%";
                        };
                        soundVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.SoundVolume = slider.GetSliderValue() * 0.01f;
                        };

                        UISlider musicVolume = volumeGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        musicVolume.SetTextTitle("Music"); // TODO(Acreal): localize this string
                        musicVolume.SetSliderMinMax(0f, 100f, true);
                        musicVolume.DataSource_SliderValue = (_) => { return Mathf.RoundToInt(DaggerfallUnity.Settings.MusicVolume * 100f); };
                        musicVolume.DataSource_SliderValueString = (_) =>
                        {
                            Debug.Log("Music Volume: " + DaggerfallUnity.Settings.MusicVolume);
                            return ((int)(DaggerfallUnity.Settings.MusicVolume * 100f)).ToString("N0") + "%";
                        };
                        musicVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.MusicVolume = slider.GetSliderValue() * 0.01f;
                        };
                        #endregion
                    }

                    //////////////////////////////////////////////////////////////////////////////////////////////
                    // CONTROLS PANEL
                    //////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelControlSettings != null)
                    {
                        _pauseWindowInstance.panelControlSettings.gameObject.SetActive(false);

                        _pauseWindowInstance.panelControlSettings.Event_OnButtonClicked_Default += () =>
                        {
                            AllowCancel = false;
                            UIManager.ShowConfirmationWindow("Set Defaults",
                                                             "Reset keybinds to default values?",
                                                             
                                                             () => // Confirm
                                                             {
                                                                 UIUtilityFunctions.SetDefaultKeybinds();
                                                                 ResetKeybindDict();
                                                                 CheckDuplicates();
                                                                 _pauseWindowInstance.panelControlSettings.Refresh();
                                                                 UIManager.HideConfirmationWindow();
                                                                 UIManager.ExecuteDelayed(0.2f, () => { AllowCancel = true; });
                                                             },

                                                             () => // Cancel
                                                             {
                                                                 UIManager.HideConfirmationWindow();
                                                                 UIManager.ExecuteDelayed(0.2f, () => { AllowCancel = true; });
                                                             });
                        };

                        #region Movement Control Settings
                        UIScrollListGroup movementGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("Movement"); // TODO(Acreal): localize this string
                        movementGroup.Collapse();

                        UIToggle invertLookToggle = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        invertLookToggle.SetDisplayName("Invert Look"); // TODO(Acreal): localize this string
                        invertLookToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.InvertMouseVertical; };
                        invertLookToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.InvertMouseVertical = toggle.isToggledOn;
                        };

                        UIToggle movementAccelerationToggle = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        movementAccelerationToggle.SetDisplayName("Movement Acceleration"); // TODO(Acreal): localize this string
                        movementAccelerationToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.MovementAcceleration; };
                        movementAccelerationToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.MovementAcceleration = toggle.isToggledOn;
                        };

                        UIToggle bowsDrawAndRelease = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        bowsDrawAndRelease.SetDisplayName("Bows Draw and Release"); // TODO(Acreal): localize this string
                        bowsDrawAndRelease.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.BowDrawback; };
                        bowsDrawAndRelease.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.BowDrawback = toggle.isToggledOn;
                        };

                        UIToggle sneakToggle = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        sneakToggle.SetDisplayName("Toggle Sneak"); // TODO(Acreal): localize this string
                        sneakToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ToggleSneak; };
                        sneakToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ToggleSneak = toggle.isToggledOn;
                        };

                        List<InputManager.Actions> moveActions = new List<InputManager.Actions>()
                        {
                            InputManager.Actions.MoveForwards,
                            InputManager.Actions.MoveBackwards,
                            InputManager.Actions.TurnLeft,
                            InputManager.Actions.MoveLeft,
                            InputManager.Actions.TurnRight,
                            InputManager.Actions.MoveRight,
                            InputManager.Actions.Jump,
                            InputManager.Actions.Crouch,
                            InputManager.Actions.Run,
                            InputManager.Actions.AutoRun,
                            InputManager.Actions.Slide,
                            InputManager.Actions.Sneak,
                            InputManager.Actions.FloatUp,
                            InputManager.Actions.FloatDown,
                            InputManager.Actions.LookUp,
                            InputManager.Actions.LookDown,
                            InputManager.Actions.CenterView,
                        };

                        for(int i = 0; i < moveActions.Count; i++)
                        {
                            string actionStr = moveActions[i].ToString();
                            UIKeyCodeBindingEntry keyCodeBindingEntry = movementGroup.AddElement(UIManager.referenceManager.prefab_keyCodeBindEntry) as UIKeyCodeBindingEntry;
                            keyCodeBindingEntry.SetActionEnum(moveActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValueString(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValueString(actionStr, false));

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                            keyCodeBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
                        }
                        #endregion

                        #region Combat Control Settings
                        UIScrollListGroup combatGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("Combat"); // TODO(Acreal): localize this string
                        combatGroup.Collapse();

                        UISlider slider_weaponSwingMode = combatGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_weaponSwingMode.SetTextTitle(TextManager.Instance.GetText("MainMenu", "weaponSwingMode"));
                        slider_weaponSwingMode.SetSliderMinMax(0, _localizedStrings_weaponSwingModes.Length - 1, true);
                        slider_weaponSwingMode.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.WeaponSwingMode; };
                        slider_weaponSwingMode.DataSource_SliderValueString = (_) => { return _localizedStrings_weaponSwingModes[DaggerfallUnity.Settings.WeaponSwingMode]; };

                        slider_weaponSwingMode.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.WeaponSwingMode = (int)slider.GetSliderValue();
                        };

                        List<InputManager.Actions> combatActions = new List<InputManager.Actions>()
                        {
                            InputManager.Actions.ReadyWeapon,
                            InputManager.Actions.SwingWeapon,
                            InputManager.Actions.SwitchHand,

                            InputManager.Actions.CastSpell,
                            InputManager.Actions.RecastSpell,
                            InputManager.Actions.AbortSpell,
                            InputManager.Actions.UseMagicItem,
                        };

                        for (int i = 0; i < combatActions.Count; i++)
                        {
                            string actionStr = combatActions[i].ToString();
                            UIKeyCodeBindingEntry keyCodeBindingEntry = combatGroup.AddElement(UIManager.referenceManager.prefab_keyCodeBindEntry) as UIKeyCodeBindingEntry;
                            keyCodeBindingEntry.SetActionEnum(combatActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValueString(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValueString(actionStr, false));

                            keyCodeBindingEntry.DataSource_PrimaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, true); };
                            keyCodeBindingEntry.DataSource_SecondaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, false); };

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                            keyCodeBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
                        }
                        #endregion

                        #region Interaction Control Settings
                        UIScrollListGroup interactGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("Interaction"); // TODO(Acreal): localize this string
                        interactGroup.Collapse();

                        List<InputManager.Actions> interactActions = new List<InputManager.Actions>()
                        {
                            InputManager.Actions.ActivateCenterObject,
                            InputManager.Actions.ActivateCursor,
                            InputManager.Actions.StealMode,
                            InputManager.Actions.GrabMode,
                            InputManager.Actions.InfoMode,
                            InputManager.Actions.TalkMode,
                        };

                        for (int i = 0; i < interactActions.Count; i++)
                        {
                            string actionStr = interactActions[i].ToString();
                            UIKeyCodeBindingEntry keyCodeBindingEntry = interactGroup.AddElement(UIManager.referenceManager.prefab_keyCodeBindEntry) as UIKeyCodeBindingEntry;
                            keyCodeBindingEntry.SetActionEnum(interactActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValueString(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValueString(actionStr, false));

                            keyCodeBindingEntry.DataSource_PrimaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, true); };
                            keyCodeBindingEntry.DataSource_SecondaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, false); };

                            keyCodeBindingEntry.Event_OnRebind  += Controls_OnRebind;
                            keyCodeBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
                        }
                        #endregion

                        #region Interface Control Settings
                        UIScrollListGroup interfaceGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("Interface"); // TODO(Acreal): localize this string
                        interfaceGroup.Collapse();

                        List<InputManager.Actions> interfaceActions = new List<InputManager.Actions>()
                        {
                            InputManager.Actions.Rest,
                            InputManager.Actions.Transport,
                            InputManager.Actions.Status,
                            InputManager.Actions.CharacterSheet,
                            InputManager.Actions.Inventory,
                            InputManager.Actions.LogBook,
                            InputManager.Actions.NoteBook,
                            InputManager.Actions.AutoMap,
                            InputManager.Actions.TravelMap,
                        };

                        for (int i = 0; i < interfaceActions.Count; i++)
                        {
                            string actionStr = interfaceActions[i].ToString();
                            UIKeyCodeBindingEntry keyCodeBindingEntry = interfaceGroup.AddElement(UIManager.referenceManager.prefab_keyCodeBindEntry) as UIKeyCodeBindingEntry;
                            keyCodeBindingEntry.SetActionEnum(interfaceActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValueString(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValueString(actionStr, false));

                            keyCodeBindingEntry.DataSource_PrimaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, true); };
                            keyCodeBindingEntry.DataSource_SecondaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, false); };

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                            keyCodeBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
                        }
                        #endregion

                        #region System Control Settings
                        UIScrollListGroup systemGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("System"); // TODO(Acreal): localize this string
                        systemGroup.Collapse();

                        List<InputManager.Actions> systemActions = new List<InputManager.Actions>()
                        {
                            InputManager.Actions.ToggleConsole,
                            InputManager.Actions.QuickSave,
                            InputManager.Actions.QuickLoad,
                            InputManager.Actions.PrintScreen,
                        };

                        for (int i = 0; i < systemActions.Count; i++)
                        {
                            string actionStr = systemActions[i].ToString();
                            UIKeyCodeBindingEntry keyCodeBindingEntry = systemGroup.AddElement(UIManager.referenceManager.prefab_keyCodeBindEntry) as UIKeyCodeBindingEntry;
                            keyCodeBindingEntry.SetActionEnum(systemActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValueString(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValueString(actionStr, false));

                            keyCodeBindingEntry.DataSource_PrimaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, true); };
                            keyCodeBindingEntry.DataSource_SecondaryKeybindDisplayValue = (GameObject sender) => { return Controls_KeyCodeBindingDataSource(sender, false); };

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                            keyCodeBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
                        }
                        #endregion

                        #region Axis Control Settings
                        UIScrollListGroup axisGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("Axis Bindings"); // TODO(Acreal): localize this string
                        axisGroup.Collapse();

                        List<InputManager.AxisActions> axisActions = new List<InputManager.AxisActions>()
                        {
                            InputManager.AxisActions.MovementHorizontal,
                            InputManager.AxisActions.MovementVertical,
                            InputManager.AxisActions.CameraHorizontal,
                            InputManager.AxisActions.CameraVertical,
                        };

                        for (int i = 0; i < axisActions.Count; i++)
                        {
                            UIScrollListRow scrollRow = axisGroup.AddRow();

                            string actionStr = axisActions[i].ToString();

                            // BINDING //////////////////////////////////////////////////////////////////////////////////////////
                            UIAxisBindingEntry axisBindingEntry = scrollRow.AddElement(UIManager.referenceManager.prefab_axisBindEntry) as UIAxisBindingEntry;
                            axisBindingEntry.boundAction = axisActions[i];
                            axisBindingEntry.SetActionEnumString(actionStr);
                            axisBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));

                            // axis binds only have a primary binding
                            axisBindingEntry.DataSource_PrimaryKeybindDisplayValue = (GameObject entry) =>
                            {
                                UIAxisBindingEntry axisEntry = entry != null ? entry.GetComponent<UIAxisBindingEntry>() : null;
                                if (axisEntry != null)
                                {
                                    return Controls_GetControlBindValueString(axisEntry.actionEnumAsString, true);
                                }
                                return null;
                            };

                            axisBindingEntry.Event_OnRebind += Controls_OnRebind;
                            axisBindingEntry.Event_OnClearBinding += (UIControlBindingEntry entry, bool _) =>
                            {
                                UIAxisBindingEntry axisEntry = entry as UIAxisBindingEntry;
                                if (axisEntry != null)
                                {
                                    InputManager.Instance.ClearAxisBinding(axisEntry.boundAction);
                                    _allPrimaryKeybindsDict[axisEntry.actionEnumAsString] = string.Empty;
                                    InputManager.Instance.SaveKeyBinds();
                                    axisEntry.Refresh();
                                    CheckDuplicates();
                                }
                            };

                            // INVERT TOGGLE //////////////////////////////////////////////////////////////////////////////////////////
                            UIToggle invertToggle = scrollRow.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            invertToggle.SetDisplayName("Invert"); // TODO(Acreal): localize this string
                            
                            LayoutElement layoutElem = invertToggle.GetComponent<LayoutElement>();
                            if(layoutElem != null) { layoutElem.flexibleWidth = 0f; }
                            
                            int toggleID = invertToggle.GetInstanceID();
                            _toggleIdToActionStringDict[invertToggle.GetInstanceID()] = actionStr;

                            invertToggle.DataSource_IsToggledOn = (GameObject toggleObj) =>
                            {
                                UIToggle toggle = toggleObj.GetComponent<UIToggle>();
                                if (toggle != null)
                                {
                                    int id = toggle.GetInstanceID();
                                    _toggleIdToActionStringDict.TryGetValue(id, out actionStr);
                                    if (!string.IsNullOrWhiteSpace(actionStr))
                                    {
                                        if (_stringToAxisActionEnumDict.TryGetValue(actionStr, out InputManager.AxisActions actionEnum))
                                        {
                                            return InputManager.Instance.GetAxisActionInversion(actionEnum);
                                        }
                                    }
                                }
                                return false;
                            };

                            invertToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                int id = toggle.GetInstanceID();
                                _toggleIdToActionStringDict.TryGetValue(id, out actionStr);
                                if (!string.IsNullOrWhiteSpace(actionStr))
                                {
                                    if (_stringToAxisActionEnumDict.TryGetValue(actionStr, out InputManager.AxisActions actionEnum))
                                    {
                                        saveSettings = true;
                                        InputManager.Instance.SetAxisActionInversion(actionEnum, toggle.isToggledOn);
                                    }
                                }
                            };

                            axisBindingEntry.Refresh();
                            invertToggle.Refresh();
                        }
                        #endregion

                        #region Joystick Control Settings
                        UIScrollListGroup joystickGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup("Joystick Controls"); // TODO(Acreal): localize this string
                        joystickGroup.Collapse();

                        UISlider slider_lookSensitivity = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_lookSensitivity.SetTextTitle("Look Sensitivity"); // TODO(Acreal): localize this string
                        slider_lookSensitivity.SetSliderMinMax(0f, 1f, false);
                        slider_lookSensitivity.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickLookSensitivity; };
                        slider_lookSensitivity.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickLookSensitivity.ToString("F2"); };
                        slider_lookSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.JoystickLookSensitivity = slider.GetSliderValue();
                        };

                        UISlider slider_uiMouseSensitivity = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_uiMouseSensitivity.SetTextTitle("UI Mouse Sensitivity"); // TODO(Acreal): localize this string
                        slider_uiMouseSensitivity.SetSliderMinMax(0f, 1f, false);
                        slider_uiMouseSensitivity.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MouseLookSensitivity; };
                        slider_uiMouseSensitivity.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.MouseLookSensitivity.ToString("F2"); };
                        slider_uiMouseSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.MouseLookSensitivity = slider.GetSliderValue();
                        };

                        UISlider slider_uiMouseSmoothing = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_uiMouseSmoothing.SetTextTitle("Mouse Smoothing Factor"); // TODO(Acreal): localize this string
                        slider_uiMouseSmoothing.SetSliderMinMax(0f, 0.9f, false);
                        slider_uiMouseSmoothing.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MouseLookSmoothingFactor; };
                        slider_uiMouseSmoothing.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.MouseLookSmoothingFactor.ToString("F2"); };
                        slider_uiMouseSmoothing.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.MouseLookSmoothingFactor = slider.GetSliderValue();
                        };

                        UISlider slider_maxMovementThreshold = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_maxMovementThreshold.SetTextTitle("Maximum Movement Threshold"); // TODO(Acreal): localize this string
                        slider_maxMovementThreshold.SetSliderMinMax(0f, 1f, false);
                        slider_maxMovementThreshold.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickMovementThreshold; };
                        slider_maxMovementThreshold.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickMovementThreshold.ToString("F2"); };
                        slider_maxMovementThreshold.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.JoystickMovementThreshold = slider.GetSliderValue();
                        };

                        UISlider slider_deadzone = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_deadzone.SetTextTitle("Deadzone"); // TODO(Acreal): localize this string
                        slider_deadzone.SetSliderMinMax(0f, 1f, false);
                        slider_deadzone.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickDeadzone; };
                        slider_deadzone.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickDeadzone.ToString("F2"); };
                        slider_deadzone.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClickSound();
                            DaggerfallUnity.Settings.JoystickDeadzone = slider.GetSliderValue();
                        };

                        List<InputManager.JoystickUIActions> joystickActions = new List<InputManager.JoystickUIActions>()
                        {
                            InputManager.JoystickUIActions.LeftClick,
                            InputManager.JoystickUIActions.MiddleClick,
                            InputManager.JoystickUIActions.RightClick,
                            InputManager.JoystickUIActions.Back,
                        };

                        for (int i = 0; i < joystickActions.Count; i++)
                        {
                            string actionStr = joystickActions[i].ToString();
                            UIJoystickKeyBindingEntry joystickBindingEntry = joystickGroup.AddElement(UIManager.referenceManager.prefab_joystickBindEntry) as UIJoystickKeyBindingEntry;
                            joystickBindingEntry.SetActionEnum(joystickActions[i]);
                            joystickBindingEntry.SetActionEnumString(actionStr);  // TODO(Acreal): localize this string
                            joystickBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            joystickBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValueString(actionStr, true));
                            joystickBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValueString(actionStr, false));

                            // joystick binds only have a primary binding
                            joystickBindingEntry.DataSource_PrimaryKeybindDisplayValue = Controls_JoystickBindingDataSource;

                            joystickBindingEntry.Event_OnRebind += Controls_OnRebind;
                            joystickBindingEntry.Event_OnClearBinding += (UIControlBindingEntry entry, bool _) =>
                            {
                                UIJoystickKeyBindingEntry joyEntry = entry as UIJoystickKeyBindingEntry;
                                if (joyEntry != null)
                                {
                                    InputManager.Instance.ClearJoystickUIBinding(joyEntry.boundAction);
                                    _allPrimaryKeybindsDict[joyEntry.actionEnumAsString] = string.Empty;
                                    InputManager.Instance.SaveKeyBinds();
                                    joyEntry.Refresh();
                                    CheckDuplicates();
                                }
                            };
                        }
                        #endregion
                    }
                }
            }
        }
        #endregion


        #region Base Class Overrides
        public override void CancelWindow()
        {
            if(_pauseWindowInstance != null)
            {
                _pauseWindowInstance.Hide();
            }
            base.CancelWindow();
        }

        public override void OnPush()
        {
            base.OnPush();

            GameManager.Instance.SaveLoadManager.EnumerateSaves();
            ShowWindow();
        }

        public override void OnPop()
        {
            if (saveSettings)
            {
                DaggerfallUnity.Settings.SaveSettings();
                saveSettings = false;
            }

            HideWindow();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
        }

        protected override void Setup()
        {
            ParentPanel.BackgroundColor = ScreenDimColor;
        }

        public override void Update()
        {
            if (AllowCancel)
            {
                if (DaggerfallUI.Instance.HotkeySequenceProcessed == HotkeySequence.HotkeySequenceProcessStatus.NotFound)
                {
                    // Toggle window closed with same hotkey used to open it
                    if (InputManager.Instance.GetKeyUp(toggleClosedBinding) || InputManager.Instance.GetBackButtonUp())
                    {
                        OnBackButtonClicked();
                    }
                }
            }
        }

        public override void Draw()
        {
            //don't need this
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            AllowCancel = true;

            if (_pauseWindowInstance != null)
            {
                _pauseWindowInstance.panelGeneralSettings?.Refresh();
                _pauseWindowInstance.panelVideoSettings?.Refresh();
                _pauseWindowInstance.panelAudioSettings?.Refresh();
                _pauseWindowInstance.panelControlSettings?.Refresh();

                _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Paused);
                _pauseWindowInstance.Show();
            }
        }

        public void HideWindow()
        {
            if (_pauseWindowInstance != null)
            {
                _pauseWindowInstance.Hide();
            }
        }
        #endregion


        #region Pause Panel Callbacks/Delegates
        private void OnBackButtonClicked()
        {
            switch (_pauseWindowInstance.currentState)
            {
                case UIPauseWindow.PauseWindowState.Settings:
                    _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Paused);
                    break;

                case UIPauseWindow.PauseWindowState.Settings_Audio:
                case UIPauseWindow.PauseWindowState.Settings_Controls:
                case UIPauseWindow.PauseWindowState.Settings_General:
                case UIPauseWindow.PauseWindowState.Settings_Video:
                    _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings);
                    break;

                case UIPauseWindow.PauseWindowState.Paused:
                default:
                    CancelWindow();
                    break;
            }
        }

        //private void Paused_OnDetailChanged(float detailVal)
        //{
        //    saveSettings = true;
        //    UIUtilityFunctions.PlayButtonClickSound();
        //    QualitySettings.SetQualityLevel(DaggerfallUnity.Settings.QualityLevel = Mathf.FloorToInt(detailVal));
        //    GameManager.UpdateShadowDistance();
        //    GameManager.UpdateShadowResolution();
        //}

        //private void Paused_OnFullScreenChanged(bool enableFullscreen)
        //{
        //    saveSettings = true;
        //    DaggerfallUnity.Settings.Fullscreen = !DaggerfallUnity.Settings.Fullscreen;
        //}
        #endregion


        #region Controls Panel Callbacks/Delegates
        private string Controls_KeyCodeBindingDataSource(GameObject entry, bool primary)
        {
            UIControlBindingEntry bindingEntry = entry != null ? entry.GetComponent<UIControlBindingEntry>() : null;
            if (bindingEntry != null)
            {
                return Controls_GetControlBindValueString(bindingEntry.actionEnumAsString, primary);
            }
            return null;
        }

        private string Controls_JoystickBindingDataSource(GameObject entry)
        {
            UIJoystickKeyBindingEntry joyEntry = entry.GetComponent<UIJoystickKeyBindingEntry>();
            if(joyEntry != null)
            {
                return Controls_GetControlBindValueString(joyEntry.actionEnumAsString, true);
            }
            return null;
        }

        private void Controls_OnRebind(UIControlBindingEntry bindingEntry, bool isPrimaryBinding)
        {
            InputManager.Instance.StartCoroutine(WaitForKeyPressRoutine(bindingEntry, isPrimaryBinding));
        }

        private void Controls_OnClearBinding(UIControlBindingEntry entry, bool primary)
        {
            UIKeyCodeBindingEntry keyCodeEntry = entry as UIKeyCodeBindingEntry;
            if (keyCodeEntry != null)
            {
                if (_stringToActionEnumDict.TryGetValue(keyCodeEntry.actionEnumAsString, out InputManager.Actions inputAction))
                {
                    InputManager.Instance.ClearBinding(inputAction, primary);
                    if (primary) { _allPrimaryKeybindsDict[keyCodeEntry.actionEnumAsString] = string.Empty; }
                    else { _allSecondaryKeybindsDict[keyCodeEntry.actionEnumAsString] = string.Empty; }
                    InputManager.Instance.SaveKeyBinds();
                    keyCodeEntry.Refresh();
                    CheckDuplicates();
                }
            }
        }

        private string Controls_GetControlBindName(string actionEnumAsString)
        {
            if (_stringToActionEnumDict.TryGetValue(actionEnumAsString, out InputManager.Actions inputAction))
            {
                // TODO(Acreal): Get localized versions of these strings
                //just not sure where to find them yet...
                return actionEnumAsString;
            }
            else if (_stringToAxisActionEnumDict.TryGetValue(actionEnumAsString, out InputManager.AxisActions axisInputAction))
            {
                return TextManager.Instance.GetText("GameSettings", AxisActionToLocalizationKey(axisInputAction));
            }
            else if (_stringToJoystickActionEnumDict.TryGetValue(actionEnumAsString, out InputManager.JoystickUIActions joystickInputAction))
            {
                return TextManager.Instance.GetText("GameSettings", JoystickActionToLocalizationKey(joystickInputAction));
            }
            else
            {
                return actionEnumAsString;
            }
        }

        private string Controls_GetControlBindValueString(string actionEnumAsString, bool primaryBinding)
        {
            if(string.IsNullOrEmpty(actionEnumAsString))
            {
                return string.Empty;
            }

            if (_stringToActionEnumDict.TryGetValue(actionEnumAsString, out InputManager.Actions inputAction))
            {
                KeyCode keyCode = InputManager.Instance.GetBinding(inputAction, primaryBinding);
                return ControlsConfigManager.Instance.GetButtonText(keyCode);
            }
            else if (_stringToAxisActionEnumDict.TryGetValue(actionEnumAsString, out InputManager.AxisActions axisInputAction))
            {
                return InputManager.Instance.GetAxisBinding(axisInputAction);
            }
            else if (_stringToJoystickActionEnumDict.TryGetValue(actionEnumAsString, out InputManager.JoystickUIActions joystickInputAction))
            {
                KeyCode keyCode = InputManager.Instance.GetJoystickUIBinding(joystickInputAction);
                return ControlsConfigManager.Instance.GetButtonText(keyCode);
            }
            else
            {
                return "???";
            }
        }
        #endregion


        #region Utility
        private void ApplyResolution(Resolution resolution)
        {
            if (DaggerfallUnity.Settings.Fullscreen && DaggerfallUnity.Settings.ExclusiveFullscreen)
            {
                Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen, resolution.refreshRate);
            }
            else
            {
                Screen.SetResolution(resolution.width, resolution.height, DaggerfallUnity.Settings.Fullscreen, resolution.refreshRate);
            }
        }
        #endregion


        #region Key Rebinding
        private void ResetKeybindDict()
        {
            foreach (InputManager.Actions a in System.Enum.GetValues(typeof(InputManager.Actions)))
            {
                string actionString = a.ToString();
                _stringToActionEnumDict[actionString] = a;

                KeyCode primaryBinding = InputManager.Instance.GetBinding(a, true);
                string primaryBindingString = ControlsConfigManager.Instance.GetButtonText(primaryBinding);
                _allPrimaryKeybindsDict[actionString] = primaryBindingString;

                KeyCode secondaryBinding = InputManager.Instance.GetBinding(a, false);
                string secondaryBindingString = ControlsConfigManager.Instance.GetButtonText(secondaryBinding);
                _allSecondaryKeybindsDict[actionString] = secondaryBindingString;
            }

            foreach (InputManager.AxisActions a in System.Enum.GetValues(typeof(InputManager.AxisActions)))
            {
                string actionString = a.ToString();
                string keyString = InputManager.Instance.GetAxisBinding(a);
                _stringToAxisActionEnumDict[actionString] = a;
                _allPrimaryKeybindsDict[actionString] = keyString;
            }

            foreach (InputManager.JoystickUIActions a in System.Enum.GetValues(typeof(InputManager.JoystickUIActions)))
            {
                KeyCode code = InputManager.Instance.GetJoystickUIBinding(a);
                string actionString = a.ToString();
                string keyString = ControlsConfigManager.Instance.GetButtonText(code);
                _stringToJoystickActionEnumDict[actionString] = a;
                _allPrimaryKeybindsDict[actionString] = keyString;
            }
        }

        private void CheckDuplicates()
        {
            IEnumerable<string> values = _allPrimaryKeybindsDict.Values.Concat(_allSecondaryKeybindsDict.Values);
            HashSet<string> duplicateSet = ControlsConfigManager.Instance.GetDuplicates(values);
            _pauseWindowInstance.panelControlSettings.UpdateDuplicateBindings(duplicateSet);
        }

        private string AxisActionToLocalizationKey(InputManager.AxisActions action)
        {
            switch (action)
            {
                case InputManager.AxisActions.MovementHorizontal: return "movementH";
                case InputManager.AxisActions.MovementVertical: return "movementV";
                case InputManager.AxisActions.CameraHorizontal: return "cameraH";
                case InputManager.AxisActions.CameraVertical: return "cameraV";
                default: return null;
            }
        }

        private string JoystickActionToLocalizationKey(InputManager.JoystickUIActions action)
        {
            switch (action)
            {
                case InputManager.JoystickUIActions.LeftClick: return "leftClickString";
                case InputManager.JoystickUIActions.MiddleClick: return "middleClickString";
                case InputManager.JoystickUIActions.RightClick: return "rightClickString";
                case InputManager.JoystickUIActions.Back: return "backString";
                default: return null;
            }
        }

        private IEnumerator<float> WaitForKeyPressRoutine(UIControlBindingEntry bindingEntry, bool isPrimaryBinding)
        {
            GameManager.Instance.PlayerMouseLook.enabled = false;
            InputManager.Instance.CursorVisible = false;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            ControlsConfigManager.Instance.UsingPrimary = isPrimaryBinding;
            AllowCancel = false;

            bool isAxisAction = bindingEntry is UIAxisBindingEntry;
            bool isJoystickAction = !isAxisAction && bindingEntry is UIJoystickKeyBindingEntry;

            KeyCode code = KeyCode.None;
            string currentLabel = isPrimaryBinding ? bindingEntry.primaryBindingString : bindingEntry.secondaryBindingString;

            if (isPrimaryBinding) { bindingEntry.SetPrimaryBindingValue(string.Empty); }
            else { bindingEntry.SetSecondaryBindingValue(string.Empty); }

            string primOrSec = isPrimaryBinding ? "Primary" : "Secondary";
            UIManager.ShowConfirmationWindow("Waiting For Input", 
                                             "Rebinding " + primOrSec + " Action: " + bindingEntry.actionEnumAsString + "\nPress Any Key...\n('Esc' to Cancel)", 
                                             null, null);

            float t = 0.2f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0f;
            }

            while ((!isAxisAction && (code = InputManager.Instance.GetAnyKeyDownIgnoreAxisBinds(true)) == KeyCode.None)
                || (isAxisAction && (code = InputManager.Instance.GetAnyKeyDown(true)) == KeyCode.None))
            {
                yield return 0f;
            }

            UIManager.HideConfirmationWindow();

            if (code != KeyCode.Escape && InputManager.Instance.ReservedKeys.FirstOrDefault(x => x == code) == KeyCode.None)
            {
                if (isAxisAction)
                {
                    UIAxisBindingEntry axisEntry = bindingEntry as UIAxisBindingEntry;
                    string axisText = InputManager.Instance.AxisKeyCodeToInputAxis((int)code);
                    if (!string.IsNullOrEmpty(axisText))
                    {
                        //there are no secondary keybinds for axis actions
                        bindingEntry.SetPrimaryBindingValue(axisText);
                        _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = axisText;

                        // TODO(Acreal): add tooltips to keybindings
                        //bindingEntry.SuppressToolTip = bindingEntry.Label.Text != ControlsConfigManager.ElongatedButtonText;
                        //bindingEntry.ToolTipText = ControlsConfigManager.Instance.GetButtonText(code, true);

                        InputManager.Instance.SetAxisBinding(axisText, axisEntry.boundAction);
                        InputManager.Instance.SaveKeyBinds();
                    }
                }
                else if (isJoystickAction)
                {
                    UIJoystickKeyBindingEntry joystickEntry = bindingEntry as UIJoystickKeyBindingEntry;
                    KeyCode boundKey = InputManager.Instance.GetJoystickUIBinding(joystickEntry.boundAction);
                    string joystickText = ControlsConfigManager.Instance.GetButtonText(boundKey);
                    if (!string.IsNullOrEmpty(joystickText))
                    {
                        //there are no secondary keybinds for joystick actions
                        bindingEntry.SetPrimaryBindingValue(joystickText);
                        _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = joystickText;

                        // TODO(Acreal): add tooltips to keybindings
                        //bindingEntry.SuppressToolTip = bindingEntry.Label.Text != ControlsConfigManager.ElongatedButtonText;
                        //bindingEntry.ToolTipText = ControlsConfigManager.Instance.GetButtonText(code, true);

                        InputManager.Instance.SetJoystickUIBinding(code, joystickEntry.boundAction);
                        InputManager.Instance.SaveKeyBinds();
                    }
                }
                else
                {
                    UIKeyCodeBindingEntry keyCodeBindingEntry = bindingEntry as UIKeyCodeBindingEntry;
                    if (keyCodeBindingEntry != null)
                    {
                        string keyText = ControlsConfigManager.Instance.GetButtonText(code);
                        if (isPrimaryBinding)
                        {
                            bindingEntry.SetPrimaryBindingValue(keyText);
                            _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = keyText;
                        }
                        else
                        {
                            bindingEntry.SetSecondaryBindingValue(keyText);
                            _allSecondaryKeybindsDict[bindingEntry.actionEnumAsString] = keyText;
                        }

                        // TODO(Acreal): add tooltips to keybindings
                        //bindingEntry.SuppressToolTip = bindingEntry.Label.Text != ControlsConfigManager.ElongatedButtonText;
                        //bindingEntry.ToolTipText = ControlsConfigManager.Instance.GetButtonText(code, true);

                        InputManager.Instance.SetBinding(code, keyCodeBindingEntry.boundAction, isPrimaryBinding);
                        InputManager.Instance.SaveKeyBinds();
                    }
                }

                CheckDuplicates();
            }
            else
            {
                if (isPrimaryBinding) { bindingEntry.SetPrimaryBindingValue(currentLabel); }
                else { bindingEntry.SetSecondaryBindingValue(currentLabel); }
            }

            t = 0.2f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0f;
            }

            AllowCancel = true;
            GameManager.Instance.PlayerMouseLook.enabled = true;
            InputManager.Instance.CursorVisible = true;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        #endregion
    }
}