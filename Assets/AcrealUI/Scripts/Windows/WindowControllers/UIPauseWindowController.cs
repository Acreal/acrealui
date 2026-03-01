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

using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Localization;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace AcrealUI
{
    // TODO(Acreal): replace magic numbers used for settings with constant variables
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
        private Dictionary<int, string> _toggleIdToActionStringDict = null;
        private string[] _localizedStrings_weaponSwingModes = null;
        private string[] _localizedStrings_shadowResolutionModes = null;
        private string[] _localizedStrings_retroModes = null;
        private string[] _localizedStrings_filterModes = null;
        private string[] _localizedStrings_textureModes = null;
        private string[] _localizedStrings_antiAliasingMethods = null;
        private string[] _localizedStrings_smaaQualitySettings = null;
        private string[] _localizedStrings_ambientOcclusionMethods = null;
        private string[] _localizedStrings_aoQualityLevels = null;
        private string[] _localizedStrings_blurSizes = null;
        private string[] _localizedStrings_retroPostProcessModes = null;
        private string[] _localizedStrings_retroAspectCorrections = null;
        private int _maxFramerate = 30;
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
            _toggleIdToActionStringDict = new Dictionary<int, string>();

            _localizedStrings_weaponSwingModes = UIUtilityFunctions.GetLocalizedTextList("weaponSwingModes", TextCollections.TextSettings);
            _localizedStrings_shadowResolutionModes = UIUtilityFunctions.GetLocalizedTextList("shadowResolutionModes", TextCollections.TextSettings);
            _localizedStrings_filterModes = UIUtilityFunctions.GetLocalizedTextList("filterModes", TextCollections.TextSettings);
            _localizedStrings_textureModes = UIUtilityFunctions.GetLocalizedTextList("dungeonTextureModes", TextCollections.TextSettings);

            _localizedStrings_retroModes = new string[3]
            {
                UIUtilityFunctions.GetLocalizedText("retroModeOff"),
                UIUtilityFunctions.GetLocalizedText("retroMode320x200"),
                UIUtilityFunctions.GetLocalizedText("retroMode640x400"),
            };

            _localizedStrings_antiAliasingMethods = new string[]
            {
                UIUtilityFunctions.GetLocalizedText("none"),
                UIUtilityFunctions.GetLocalizedText("fxaa"),
                UIUtilityFunctions.GetLocalizedText("smaa"),
                UIUtilityFunctions.GetLocalizedText("taa")
            };

            _localizedStrings_smaaQualitySettings = new string[]
            {
                UIUtilityFunctions.GetLocalizedText("low"),
                UIUtilityFunctions.GetLocalizedText("medium"),
                UIUtilityFunctions.GetLocalizedText("high")
            };

            if (SystemInfo.graphicsShaderLevel >= 45)
            {
                _localizedStrings_ambientOcclusionMethods = new string[]
                {
                    UIUtilityFunctions.GetLocalizedText("scalableAmbient"),
                    UIUtilityFunctions.GetLocalizedText("multiScaleVolumetric")
                };
            }
            else
            {
                _localizedStrings_ambientOcclusionMethods = new string[]
                {
                    UIUtilityFunctions.GetLocalizedText("scalableAmbient"),
                };
            }

            _localizedStrings_aoQualityLevels = new string[]
            {
                UIUtilityFunctions.GetLocalizedText("lowest"),
                UIUtilityFunctions.GetLocalizedText("low"),
                UIUtilityFunctions.GetLocalizedText("medium"),
                UIUtilityFunctions.GetLocalizedText("high"),
                UIUtilityFunctions.GetLocalizedText("ultra")
            };

            _localizedStrings_blurSizes = new string[]
            {
                UIUtilityFunctions.GetLocalizedText("small"),
                UIUtilityFunctions.GetLocalizedText("medium"),
                UIUtilityFunctions.GetLocalizedText("large"),
                UIUtilityFunctions.GetLocalizedText("veryLarge"),
            };

            _localizedStrings_retroPostProcessModes = new string[]
            {
                TextManager.Instance.GetLocalizedText("off"),
                TextManager.Instance.GetLocalizedText("posterizationFull"),
                TextManager.Instance.GetLocalizedText("posterizationMinusSky"),
                TextManager.Instance.GetLocalizedText("palettizationFull"),
                TextManager.Instance.GetLocalizedText("palettizationMinusSky"),
            };

            _localizedStrings_retroAspectCorrections = new string[]
            {
                TextManager.Instance.GetLocalizedText("off"),
                TextManager.Instance.GetLocalizedText("FourThree"),
                TextManager.Instance.GetLocalizedText("SixteenTen")
            };

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
                            AllowCancel = false;

                            TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(1069);
                            string msg = DaggerfallStringTableImporter.ConvertRSCTokensToString(1069, tokens).Split('[')[0];

                            UIManager.ShowConfirmationWindow(null, msg,
                                () => 
                                {
                                    DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiExitGame);
                                }, 
                                () =>
                                {
                                    AllowCancel = true;
                                    UIManager.HideConfirmationWindow();
                                });
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

                        #region Find All Valid Resolutions and Refresh Rates
                        Resolution[] allResolutions = Screen.resolutions;
                        List<Resolution> resolutions = new List<Resolution>();
                        UIToggle selectedResolutionEntry = null;

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
                            if(allResolutions[i].refreshRate < 30)
                            {
                                continue;
                            }
                            else if (allResolutions[i].refreshRate > _maxFramerate)
                            {
                                _maxFramerate = allResolutions[i].refreshRate;
                            }
                            #endregion
                        }

                        resolutions.Reverse();
                        #endregion

                        #region Window Settings
                        UIScrollListGroup windowGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Window"); // TODO(Acreal): localize this string
                        windowGroup.Collapse();

                        #region Resolution Settings
                        UIScrollListGroup resolutionGroup = windowGroup.GetOrAddSubScrollListGroup("Resolution"); // TODO(Acreal): localize this string
                        resolutionGroup.Collapse();

                        UIToggleGroup resolutionToggleGroup = resolutionGroup.groupParent.gameObject.AddComponent<UIToggleGroup>();
                        resolutionToggleGroup.canBeToggledOff = false;
                        resolutionToggleGroup.Initialize();

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

                        #region Add Refresh Rate Entries **DISABLED**
                        /*
                        for (int i = 0; i < refreshRates.Count; i++)
                        {
                            UIToggle refreshRateEntry = refreshRateGroup.AddElement(UIManager.referenceManager.prefab_resolutionSettingEntry) as UIToggle;
                            refreshRateToggleGroup.AddToggle(refreshRateEntry);
                            _refreshRateToggleIdToRefreshRateValueDict[refreshRateEntry.GetInstanceID()] = refreshRates[i];
                            refreshRateEntry.SetDisplayName(refreshRates[i].ToString("N0"));

                            if (selectedRefreshRateEntry == null && refreshRates[i] == Screen.currentResolution.refreshRate)
                            {
                                selectedRefreshRateEntry = refreshRateEntry;
                            }

                            refreshRateEntry.DataSource_IsToggledOn = (GameObject sender) => 
                            {
                                UIToggle toggle = sender != null ? sender.GetComponent<UIToggle>() : null;
                                if (toggle != null && _refreshRateToggleIdToRefreshRateValueDict.TryGetValue(toggle.GetInstanceID(), out int refreshRate))
                                {
                                    Debug.LogError(sender.name + "." + toggle.GetInstanceID() + ".DataSource_IsToggledOn = " + (refreshRate == Screen.currentResolution.refreshRate) + "(Entry Rate: " + refreshRate + ", Screen Rate: " + Screen.currentResolution.refreshRate + ")");
                                    return refreshRate == Screen.currentResolution.refreshRate;
                                }
                                return false;
                            };

                            refreshRateEntry.Event_OnToggledOn += (UIToggle toggle) =>
                            {
                                saveSettings = true;

                                int refreshRate = 0;
                                _refreshRateToggleIdToRefreshRateValueDict.TryGetValue(toggle.GetInstanceID(), out refreshRate);
                                if (refreshRate < 30) { refreshRate = _maxFramerate; }

                                Resolution res = Screen.currentResolution;
                                res.refreshRate = refreshRate;
                                ApplyResolution(res);
                            };
                        }

                        if (selectedRefreshRateEntry != null)
                        {
                            refreshRateToggleGroup.SetActiveToggle(selectedRefreshRateEntry);
                        }
                        */
                        #endregion
                        #endregion

                        UIToggle fullScreenToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        fullScreenToggle.SetDisplayName("Fullscreen");  // TODO(Acreal): localize this string
                        fullScreenToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.Fullscreen; };
                        fullScreenToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.Fullscreen = toggle.isToggledOn;
                            ApplyResolution(Screen.currentResolution);
                        };

                        UIToggle exclusiveToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        exclusiveToggle.SetDisplayName("Exclusive Fullscreen");  // TODO(Acreal): localize this string
                        exclusiveToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ExclusiveFullscreen; };
                        exclusiveToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ExclusiveFullscreen = toggle.isToggledOn;
                            ApplyResolution(Screen.currentResolution);
                        };

                        UIToggle vsyncToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        vsyncToggle.SetDisplayName("Vsync");  // TODO(Acreal): localize this string
                        vsyncToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.VSync; };
                        vsyncToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.VSync = toggle.isToggledOn;
                            QualitySettings.vSyncCount = toggle.isToggledOn ? 1 : 0;
                        };

                        UISlider framerateSlider = windowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        framerateSlider.SetTitle("Max Framerate"); // TODO(Acreal): localize this string
                        framerateSlider.SetSliderMinMax(30f, _maxFramerate + 1, true);
                        framerateSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.TargetFrameRate == 0 ? _maxFramerate + 1 : DaggerfallUnity.Settings.TargetFrameRate; };
                        framerateSlider.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.TargetFrameRate == 0 ? "Unlimited" : DaggerfallUnity.Settings.TargetFrameRate.ToString("N0"); }; // TODO(Acreal): localize this string
                        framerateSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;

                            int val = slider.GetSliderValueAsInt();
                            if (val == _maxFramerate + 1)
                            {
                                val = 0;
                            }
                            DaggerfallUnity.Settings.TargetFrameRate = Application.targetFrameRate = val;
                        };
                        #endregion

                        #region Rendering Settings
                        UIScrollListGroup renderGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Rendering"); // TODO(Acreal): localize this string
                        renderGroup.Collapse();

                        UISlider qualitySlider = renderGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        qualitySlider.SetTitle("Render Quality"); // TODO(Acreal): localize this string
                        qualitySlider.SetSliderMinMax(0, QualitySettings.names.Length - 1, true);
                        qualitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.QualityLevel; };
                        qualitySlider.DataSource_SliderValueString = (_) => { return QualitySettings.names[DaggerfallUnity.Settings.QualityLevel]; }; // TODO(Acreal): localize quality level names?
                        qualitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            int val = slider.GetSliderValueAsInt();
                            DaggerfallUnity.Settings.QualityLevel = val;
                            QualitySettings.SetQualityLevel(val);
                            GameManager.UpdateShadowDistance();
                            GameManager.UpdateShadowResolution();
                        };

                        UIScrollListGroup textureGroup = renderGroup.GetOrAddSubScrollListGroup("Textures"); // TODO(Acreal): localize this string
                        textureGroup.Collapse();

                        UISlider mainFilterSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        mainFilterSlider.SetTitle("Main Filter"); // TODO(Acreal): localize this string
                        mainFilterSlider.SetSliderMinMax(0, 2, true);
                        mainFilterSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MainFilterMode; };
                        mainFilterSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_filterModes[DaggerfallUnity.Settings.MainFilterMode]; };
                        mainFilterSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.MainFilterMode = slider.GetSliderValueAsInt();
                        };

                        UISlider guiFilterSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        guiFilterSlider.SetTitle("GUI Filter"); // TODO(Acreal): localize this string
                        guiFilterSlider.SetSliderMinMax(0, 2, true);
                        guiFilterSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.GUIFilterMode; };
                        guiFilterSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_filterModes[DaggerfallUnity.Settings.GUIFilterMode]; };
                        guiFilterSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.GUIFilterMode = slider.GetSliderValueAsInt();
                        };

                        UISlider videoFilterSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        videoFilterSlider.SetTitle("Video Filter"); // TODO(Acreal): localize this string
                        videoFilterSlider.SetSliderMinMax(0, 2, true);
                        videoFilterSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.VideoFilterMode; };
                        videoFilterSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_filterModes[DaggerfallUnity.Settings.VideoFilterMode]; };
                        videoFilterSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.VideoFilterMode = slider.GetSliderValueAsInt();
                        };

                        UISlider dungeonTexSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        dungeonTexSlider.SetTitle("Dungeon Texture Mode"); // TODO(Acreal): localize this string
                        dungeonTexSlider.SetSliderMinMax(0, 2, true);
                        dungeonTexSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.RandomDungeonTextures; };
                        dungeonTexSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_textureModes[DaggerfallUnity.Settings.RandomDungeonTextures]; };
                        dungeonTexSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.RandomDungeonTextures = slider.GetSliderValueAsInt();
                        };

                        UIToggle texArrayToggle = textureGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        texArrayToggle.SetDisplayName("Enable Texture Arrays");  // TODO(Acreal): localize this string
                        texArrayToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.EnableTextureArrays; };
                        texArrayToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.EnableTextureArrays = toggle.isToggledOn;
                        };

                        UIScrollListGroup aaGroup = renderGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("antialiasing"));
                        aaGroup.Collapse();

                        UISlider aaModeSlider = aaGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        aaModeSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("method"));
                        //aaModeSlider.SetTooltip(UIUtilityFunctions.GetLocalizedText("antialiasingTip"));
                        aaModeSlider.SetSliderMinMax(0, 3, true);
                        aaModeSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.AntialiasingMethod; };
                        aaModeSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_antiAliasingMethods[DaggerfallUnity.Settings.AntialiasingMethod]; };
                        aaModeSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.AntialiasingMethod = slider.GetSliderValueAsInt();

                            if (GameManager.Instance.StartGameBehaviour != null)
                            {
                                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Antialiasing);
                            }

                            _pauseWindowInstance.panelVideoSettings.Refresh();
                        };

                        UISlider taaSharpnessSlider = aaGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        taaSharpnessSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("taaSharpness"));
                        taaSharpnessSlider.SetSliderMinMax(0.0f, 3.0f, false);
                        taaSharpnessSlider.DataSource_GameObjectActive = (_) => { return DaggerfallUnity.Settings.AntialiasingMethod == (int)AntiAliasingMethods.TAA; };
                        taaSharpnessSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.AntialiasingTAASharpness; };
                        taaSharpnessSlider.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.AntialiasingTAASharpness.ToString("F2"); };
                        taaSharpnessSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.AntialiasingTAASharpness = slider.GetSliderValue();

                            if (GameManager.Instance.StartGameBehaviour != null)
                            {
                                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Antialiasing);
                            }
                        };

                        UISlider smaaQualitySlider = aaGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        smaaQualitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("smaaQuality"));
                        smaaQualitySlider.SetSliderMinMax(0, 2, true);
                        smaaQualitySlider.DataSource_GameObjectActive = (_) => { return DaggerfallUnity.Settings.AntialiasingMethod == (int)AntiAliasingMethods.SMAA; };
                        smaaQualitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.AntialiasingSMAAQuality; };
                        smaaQualitySlider.DataSource_SliderValueString = (_) => { return _localizedStrings_smaaQualitySettings[DaggerfallUnity.Settings.AntialiasingSMAAQuality]; };
                        smaaQualitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.AntialiasingSMAAQuality = slider.GetSliderValueAsInt();

                            if (GameManager.Instance.StartGameBehaviour != null)
                            {
                                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Antialiasing);
                            }
                        };

                        UIToggle fastFxaaToggle = aaGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        fastFxaaToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("fxaaFastMode"));
                        fastFxaaToggle.DataSource_GameObjectActive = (_) => { return DaggerfallUnity.Settings.AntialiasingMethod == (int)AntiAliasingMethods.FXAA; };
                        fastFxaaToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.AntialiasingFXAAFastMode; };
                        fastFxaaToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.AntialiasingFXAAFastMode = toggle.isToggledOn;

                            if (GameManager.Instance.StartGameBehaviour != null)
                            {
                                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Antialiasing);
                            }
                        };

                        UIToggle ambientLitToggle = renderGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        ambientLitToggle.SetDisplayName("Ambient Lit Interiors"); // TODO(Acreal): localize this string
                        ambientLitToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.AmbientLitInteriors; };
                        ambientLitToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.AmbientLitInteriors = toggle.isToggledOn;
                        };
                        #endregion

                        #region PostFX Settings
                        UIScrollListGroup postFxGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup(UIUtilityFunctions.GetLocalizedText("postProcess")); // TODO(Acreal): localize this string
                        renderGroup.Collapse();

                        #region Retro Mode
                        UIScrollListGroup retroGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("retroMode"));
                        retroGroup.Collapse();

                        ////// RENDERING MODE ///////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider retroSlider = retroGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            retroSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("mode"));
                            retroSlider.SetSliderMinMax(0, _localizedStrings_retroModes.Length-1, true);
                            retroSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.RetroRenderingMode; };
                            retroSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_retroModes[DaggerfallUnity.Settings.RetroRenderingMode]; }; // TODO(Acreal): localize quality level names?
                            retroSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.RetroRenderingMode = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.RetroMode);
                                }
                            };
                        }

                        ////// POST-PROCESS MODE ///////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider retroSlider = retroGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            retroSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("postProcess"));
                            retroSlider.SetSliderMinMax(0, _localizedStrings_retroPostProcessModes.Length-1, true);
                            retroSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.PostProcessingInRetroMode; };
                            retroSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_retroPostProcessModes[DaggerfallUnity.Settings.PostProcessingInRetroMode]; }; // TODO(Acreal): localize quality level names?
                            retroSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.PostProcessingInRetroMode = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.RetroMode);
                                }
                            };
                        }

                        ////// ASPECT CORRECTION ///////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider retroSlider = retroGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            retroSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("retroModeAspectCorrection"));
                            retroSlider.SetSliderMinMax(0, _localizedStrings_retroAspectCorrections.Length - 1, true);
                            retroSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.RetroModeAspectCorrection; };
                            retroSlider.DataSource_SliderValueString = (_) => { return _localizedStrings_retroAspectCorrections[DaggerfallUnity.Settings.RetroModeAspectCorrection]; }; // TODO(Acreal): localize quality level names?
                            retroSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.RetroModeAspectCorrection = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.RetroMode);
                                }
                            };
                        }
                        #endregion

                        #region Depth of Field
                        UIScrollListGroup dofGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("depthOfField"));
                        dofGroup.Collapse();

                        ////// ENABLE //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle dofToggle = dofGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            dofToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("enable"));
                            dofToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.DepthOfFieldEnable; };
                            dofToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.DepthOfFieldEnable = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);
                                }
                            };
                        }

                        ////// FOCUS DISTANCE //////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider focusDistSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            focusDistSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("focusDistance"));
                            focusDistSlider.SetSliderMinMax(0.1f, 100f, false);

                            focusDistSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldFocusDistance;
                            };

                            focusDistSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldFocusDistance.ToString("N1");
                            };

                            focusDistSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.DepthOfFieldFocusDistance = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);
                                }
                            };
                        }

                        ////// APERTURE ////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider apertureSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            apertureSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("aperture"));
                            apertureSlider.SetSliderMinMax(0.1f, 32.0f, false);

                            apertureSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldAperture;
                            };

                            apertureSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldAperture.ToString("N1");
                            };

                            apertureSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.DepthOfFieldAperture = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);
                                }
                            };
                        }

                        ////// FOCAL LENGTH /////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider focalLengthSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            focalLengthSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("focalLength"));
                            focalLengthSlider.SetSliderMinMax(1, 300, true);

                            focalLengthSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldFocalLength;
                            };

                            focalLengthSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldFocalLength.ToString("N1");
                            };

                            focalLengthSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.DepthOfFieldFocalLength = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);
                                }
                            };
                        }

                        ////// MAX BLUR SIZE ///////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider focalLengthSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            focalLengthSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("maxBlurSize"));
                            focalLengthSlider.SetSliderMinMax(0, _localizedStrings_blurSizes.Length - 1, true);

                            focalLengthSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.DepthOfFieldMaxBlurSize;
                            };

                            focalLengthSlider.DataSource_SliderValueString = (_) =>
                            {
                                return _localizedStrings_blurSizes[DaggerfallUnity.Settings.DepthOfFieldMaxBlurSize];
                            };

                            focalLengthSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.DepthOfFieldMaxBlurSize = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.DepthOfField);
                                }
                            };
                        }
                        #endregion

                        #region Bloom
                        UIScrollListGroup bloomGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("bloom"));
                        bloomGroup.Collapse();

                        ////// ENABLE //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle bloomToggle = bloomGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            bloomToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("enable"));
                            bloomToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.BloomEnable; };
                            bloomToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.BloomEnable = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                                }
                            };
                        }

                        ////// INTENSITY ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider intensitySlider = bloomGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            intensitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("intensity"));
                            intensitySlider.SetSliderMinMax(0f, 1f, false);
                            intensitySlider.DataSource_SliderValue = (_) => { return Mathf.InverseLerp(0f, 50f, DaggerfallUnity.Settings.BloomIntensity); };

                            intensitySlider.DataSource_SliderValueString = (_) =>
                            {
                                return (Mathf.InverseLerp(0f, 50f, DaggerfallUnity.Settings.BloomIntensity) * 100f).ToString("N0") + "%";
                            };

                            intensitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                float val = slider.GetSliderValue();
                                DaggerfallUnity.Settings.BloomIntensity = Mathf.Lerp(0f, 50f, val);

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                                }
                            };
                        }

                        ////// THRESHOLD ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider intensitySlider = bloomGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            intensitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("threshold"));
                            intensitySlider.SetSliderMinMax(0.1f, 10f, false);
                            intensitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.BloomThreshold; };

                            intensitySlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.BloomThreshold.ToString("F1");
                            };

                            intensitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.BloomThreshold = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                                }
                            };
                        }

                        ////// DIFFUSION //////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider intensitySlider = bloomGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            intensitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("diffusion"));
                            intensitySlider.SetSliderMinMax(1f, 10f, false);
                            intensitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.BloomDiffusion; };

                            intensitySlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.BloomDiffusion.ToString("N1");
                            };

                            intensitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.BloomDiffusion = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                                }
                            };
                        }

                        ////// FAST MODE //////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle bloomToggle = bloomGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            bloomToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("bloomFastMode"));
                            bloomToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.BloomFastMode; };
                            bloomToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.BloomFastMode = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Bloom);
                                }
                            };
                        }
                        #endregion

                        #region Ambient Occlusion
                        UIScrollListGroup aoGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("ambientOcclusion"));
                        aoGroup.Collapse();

                        ////// ENABLE //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle aoToggle = aoGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            aoToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("enable"));
                            aoToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.AmbientOcclusionEnable; };
                            aoToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.AmbientOcclusionEnable = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.AmbientOcclusion);
                                }
                            };
                        }

                        ////// METHOD /////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider aoMethodSlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            aoMethodSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("method"));
                            aoMethodSlider.SetSliderMinMax(0f, _localizedStrings_ambientOcclusionMethods.Length-1, true);

                            aoMethodSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionMethod;
                            };

                            aoMethodSlider.DataSource_SliderValueString = (_) =>
                            {
                                return _localizedStrings_ambientOcclusionMethods[DaggerfallUnity.Settings.AmbientOcclusionMethod];
                            };

                            aoMethodSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.AmbientOcclusionMethod = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.AmbientOcclusion);
                                }

                                // refresh for the dependent settings
                                _pauseWindowInstance.panelVideoSettings.Refresh();
                            };
                        }

                        ////// INTENSITY //////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider aoIntensitySlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            aoIntensitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("intensity"));
                            aoIntensitySlider.SetSliderMinMax(0f, 1f, false);

                            aoIntensitySlider.DataSource_SliderValue = (_) =>
                            {
                                return Mathf.InverseLerp(0f, 4f, DaggerfallUnity.Settings.AmbientOcclusionIntensity);
                            };

                            aoIntensitySlider.DataSource_SliderValueString = (_) =>
                            {
                                return (Mathf.InverseLerp(0f, 4f, DaggerfallUnity.Settings.AmbientOcclusionIntensity) * 100f).ToString("N0") + "%";
                            };

                            aoIntensitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                float val = slider.GetSliderValue();
                                DaggerfallUnity.Settings.AmbientOcclusionIntensity = Mathf.Lerp(0f, 4f, val);

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.AmbientOcclusion);
                                }
                            };
                        }

                        ////// RADIUS ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider aoRadiusSlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            aoRadiusSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("radius"));
                            aoRadiusSlider.SetSliderMinMax(0f, 2f, false);

                            aoRadiusSlider.DataSource_GameObjectActive = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionMethod == (int)AmbientOcclusionMode.ScalableAmbientObscurance;
                            };

                            aoRadiusSlider.DataSource_SliderValue = (_) =>
                            {
                                return Mathf.InverseLerp(0f, 2f, DaggerfallUnity.Settings.AmbientOcclusionRadius);
                            };

                            aoRadiusSlider.DataSource_SliderValueString = (_) =>
                            {
                                return (Mathf.InverseLerp(0f, 2f, DaggerfallUnity.Settings.AmbientOcclusionRadius) * 100f).ToString("N0") + "%";
                            };

                            aoRadiusSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                float val = slider.GetSliderValue();
                                DaggerfallUnity.Settings.AmbientOcclusionRadius = Mathf.Lerp(0f, 2f, val);

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.AmbientOcclusion);
                                }
                            };
                        }

                        ////// QUALITY //////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider aoQualitySlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            aoQualitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("quality"));
                            aoQualitySlider.SetSliderMinMax(0, _localizedStrings_aoQualityLevels.Length - 1, true);

                            aoQualitySlider.DataSource_GameObjectActive = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionMethod == (int)AmbientOcclusionMode.ScalableAmbientObscurance;
                            };

                            aoQualitySlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionQuality;
                            };

                            aoQualitySlider.DataSource_SliderValueString = (_) =>
                            {
                                return _localizedStrings_aoQualityLevels[DaggerfallUnity.Settings.AmbientOcclusionQuality];
                            };

                            aoQualitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.AmbientOcclusionQuality = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.AmbientOcclusion);
                                }
                            };
                        }

                        ////// THICKNESS ////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider aoThicknessSlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            aoThicknessSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("thickness"));
                            aoThicknessSlider.SetSliderMinMax(1f, 10f, false);

                            aoThicknessSlider.DataSource_GameObjectActive = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionMethod == (int)AmbientOcclusionMode.MultiScaleVolumetricObscurance;
                            };

                            aoThicknessSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionThickness;
                            };

                            aoThicknessSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.AmbientOcclusionThickness.ToString("N1");
                            };

                            aoThicknessSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.AmbientOcclusionThickness = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.AmbientOcclusion);
                                }
                            };
                        }
                        #endregion

                        #region Motion Blur
                        UIScrollListGroup motionBlurGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("motionBlur"));
                        motionBlurGroup.Collapse();

                        ////// ENABLE //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle motionBlurToggle = motionBlurGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            motionBlurToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("enable"));
                            motionBlurToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.MotionBlurEnable; };
                            motionBlurToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.MotionBlurEnable = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.MotionBlur);
                                }
                            };
                        }

                        ////// SHUTTER ANGLE ////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider intensitySlider = motionBlurGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            intensitySlider.SetTitle(UIUtilityFunctions.GetLocalizedText("shutterAngle"));
                            intensitySlider.SetSliderMinMax(0, 360, true);
                            intensitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MotionBlurShutterAngle; };

                            intensitySlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.MotionBlurShutterAngle.ToString("N0");
                            };

                            intensitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.MotionBlurShutterAngle = slider.GetSliderValueAsInt();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.MotionBlur);
                                }
                            };
                        }

                        ////// SAMPLE COUNT /////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider motionBlurSlider = motionBlurGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            motionBlurSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("sampleCount"));
                            motionBlurSlider.SetSliderMinMax(0, 3, true);

                            motionBlurSlider.DataSource_SliderValue = (_) =>
                            {
                                int idx = 0;
                                switch (DaggerfallUnity.Settings.MotionBlurSampleCount)
                                {
                                    case 4: idx = 0; break;
                                    case 8: idx = 1; break;
                                    case 16: idx = 2; break;
                                    case 32: idx = 3; break;
                                }
                                return idx;
                            };

                            motionBlurSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.MotionBlurSampleCount.ToString("N0");
                            };

                            motionBlurSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                
                                int val = slider.GetSliderValueAsInt();
                                switch (val)
                                {
                                    case 0:
                                        DaggerfallUnity.Settings.MotionBlurSampleCount = 4;
                                        break;
                                    case 1:
                                        DaggerfallUnity.Settings.MotionBlurSampleCount = 8;
                                        break;
                                    case 2:
                                        DaggerfallUnity.Settings.MotionBlurSampleCount = 16;
                                        break;
                                    case 3:
                                        DaggerfallUnity.Settings.MotionBlurSampleCount = 32;
                                        break;
                                }

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.MotionBlur);
                                }
                            };
                        }
                        #endregion

                        #region Vignette
                        UIScrollListGroup vignetteGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("vignette"));
                        vignetteGroup.Collapse();

                        ////// ENABLE //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle vignetteToggle = vignetteGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            vignetteToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("enable"));
                            vignetteToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.VignetteEnable; };
                            vignetteToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.VignetteEnable = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Vignette);
                                }
                            };
                        }

                        ////// INTENSITY ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider vignetteSlider = vignetteGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            vignetteSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("intensity"));
                            vignetteSlider.SetSliderMinMax(0f, 1f, false);

                            vignetteSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.VignetteIntensity;
                            };

                            vignetteSlider.DataSource_SliderValueString = (_) =>
                            {
                                return (DaggerfallUnity.Settings.VignetteIntensity * 100f).ToString("N0") + "%";
                            };

                            vignetteSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.VignetteIntensity = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Vignette);
                                }
                            };
                        }

                        ////// SMOOTHNESS /////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider smoothnessSlider = vignetteGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            smoothnessSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("smoothness"));
                            smoothnessSlider.SetSliderMinMax(0f, 1f, false);

                            smoothnessSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.VignetteSmoothness;
                            };

                            smoothnessSlider.DataSource_SliderValueString = (_) =>
                            {
                                return (DaggerfallUnity.Settings.VignetteSmoothness * 100f).ToString("N0") + "%";
                            };

                            smoothnessSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.VignetteSmoothness = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Vignette);
                                }
                            };
                        }

                        ////// ROUNDNESS //////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider roundnessSlider = vignetteGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            roundnessSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("roundness"));
                            roundnessSlider.SetSliderMinMax(0f, 1f, false);

                            roundnessSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.VignetteRoundness;
                            };

                            roundnessSlider.DataSource_SliderValueString = (_) =>
                            {
                                return (DaggerfallUnity.Settings.VignetteRoundness * 100f).ToString("N0") + "%";
                            };

                            roundnessSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;

                                float val = slider.GetSliderValue();
                                DaggerfallUnity.Settings.VignetteRoundness = val;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Vignette);
                                }
                            };
                        }

                        ////// ROUNDED //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle roundedToggle = vignetteGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            roundedToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("rounded"));
                            roundedToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.VignetteRounded; };
                            roundedToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.VignetteRounded = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Vignette);
                                }
                            };
                        }
                        #endregion

                        #region Color Boost
                        UIScrollListGroup colorBoostGroup = postFxGroup.GetOrAddSubScrollListGroup(UIUtilityFunctions.GetLocalizedText("colorBoost"));
                        colorBoostGroup.Collapse();

                        ////// ENABLE //////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UIToggle colorBoostToggle = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                            colorBoostToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("enable"));
                            colorBoostToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ColorBoostEnable; };
                            colorBoostToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostEnable = toggle.isToggledOn;

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }

                        ////// INTENSITY ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider colorBoostSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            colorBoostSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("intensity"));
                            colorBoostSlider.SetSliderMinMax(0f, 1f, false);

                            colorBoostSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostIntensity;
                            };

                            colorBoostSlider.DataSource_SliderValueString = (_) =>
                            {
                                return (DaggerfallUnity.Settings.ColorBoostIntensity * 100f).ToString("N0") + "%";
                            };

                            colorBoostSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostIntensity = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }

                        ////// RADIUS ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider radiusSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            radiusSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("radius"));
                            radiusSlider.SetSliderMinMax(0.1f, 50.0f, false);

                            radiusSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostRadius;
                            };

                            radiusSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostRadius.ToString("F1");
                            };

                            radiusSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostRadius = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }

                        ////// INTERIOR SCALE ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider interiorScaleSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            interiorScaleSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("interiorScale"));
                            interiorScaleSlider.SetSliderMinMax(0f, 8f, false);

                            interiorScaleSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostInteriorScale;
                            };

                            interiorScaleSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostInteriorScale.ToString("F1");
                            };

                            interiorScaleSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostInteriorScale = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }

                        ////// EXTERIOR SCALE ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider exteriorScaleSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            exteriorScaleSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("exteriorScale"));
                            exteriorScaleSlider.SetSliderMinMax(0f, 8f, false);

                            exteriorScaleSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostExteriorScale;
                            };

                            exteriorScaleSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostExteriorScale.ToString("F1");
                            };

                            exteriorScaleSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostExteriorScale = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }

                        ////// DUNGEON SCALE ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider dungeonScaleSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            dungeonScaleSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("dungeonScale"));
                            dungeonScaleSlider.SetSliderMinMax(0f, 8f, false);

                            dungeonScaleSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostDungeonScale;
                            };

                            dungeonScaleSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostDungeonScale.ToString("F1");
                            };

                            dungeonScaleSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostDungeonScale = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }

                        ////// DUNGEON FALLOFF ///////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            UISlider falloffSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                            falloffSlider.SetTitle(UIUtilityFunctions.GetLocalizedText("dungeonFalloff"));
                            falloffSlider.SetSliderMinMax(0f, 8f, false);

                            falloffSlider.DataSource_SliderValue = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostDungeonFalloff;
                            };

                            falloffSlider.DataSource_SliderValueString = (_) =>
                            {
                                return DaggerfallUnity.Settings.ColorBoostDungeonFalloff.ToString("F1");
                            };

                            falloffSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ColorBoostDungeonFalloff = slider.GetSliderValue();

                                if (GameManager.Instance.StartGameBehaviour != null)
                                {
                                    GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.ColorBoost);
                                }
                            };
                        }
                        #endregion

                        #region Dithering
                        UIToggle ditherToggle = postFxGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        ditherToggle.SetDisplayName(UIUtilityFunctions.GetLocalizedText("dither"));
                        ditherToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.DitherEnable; };
                        ditherToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.DitherEnable = toggle.isToggledOn;

                            if (GameManager.Instance.StartGameBehaviour != null)
                            {
                                GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Dither);
                            }
                        };
                        #endregion
                        #endregion

                        #region Camera Settings
                        UIScrollListGroup cameraGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup("Camera"); // TODO(Acreal): localize this string
                        cameraGroup.Collapse();

                        UISlider fovSlider = cameraGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        fovSlider.SetTitle("Field of View"); // TODO(Acreal): localize this string
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
                        shadowRes.SetTitle("Shadow Resolution"); // TODO(Acreal): localize this string
                        shadowRes.SetSliderMinMax(0, 3, true);
                        shadowRes.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.ShadowResolutionMode; };
                        shadowRes.DataSource_SliderValueString = (_) => { return _localizedStrings_shadowResolutionModes[DaggerfallUnity.Settings.ShadowResolutionMode]; };
                        shadowRes.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ShadowResolutionMode = (int)slider.GetSliderValue();
                        };

                        UISlider shadowDistExt = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowDistExt.SetTitle("Exterior Shadow Distance"); // TODO(Acreal): localize this string
                        shadowDistExt.SetSliderMinMax(0.1f, 150f, false);
                        shadowDistExt.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.ExteriorShadowDistance; };
                        shadowDistExt.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.ExteriorShadowDistance.ToString("F2"); };
                        shadowDistExt.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ExteriorShadowDistance = slider.GetSliderValue();
                        };

                        UISlider shadowDistInt = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowDistInt.SetTitle("Interior Shadow Distance"); // TODO(Acreal): localize this string
                        shadowDistInt.SetSliderMinMax(0.1f, 50f, false);
                        shadowDistInt.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.InteriorShadowDistance; };
                        shadowDistInt.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.InteriorShadowDistance.ToString("F2"); };
                        shadowDistInt.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.InteriorShadowDistance = slider.GetSliderValue();
                        };

                        UISlider shadowDistDungeon = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        shadowDistDungeon.SetTitle("Dungeon Shadow Distance"); // TODO(Acreal): localize this string
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
                        soundVolume.SetTitle("Sound"); // TODO(Acreal): localize this string
                        soundVolume.SetSliderMinMax(0f, 100f, true);
                        soundVolume.DataSource_SliderValue = (_) => { return Mathf.RoundToInt(DaggerfallUnity.Settings.SoundVolume * 100f); };
                        soundVolume.DataSource_SliderValueString = (_) =>
                        {
                            return ((int)(DaggerfallUnity.Settings.SoundVolume * 100f)).ToString("N0") + "%";
                        };
                        soundVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.SoundVolume = slider.GetSliderValue() * 0.01f;
                        };

                        UISlider musicVolume = volumeGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        musicVolume.SetTitle("Music"); // TODO(Acreal): localize this string
                        musicVolume.SetSliderMinMax(0f, 100f, true);
                        musicVolume.DataSource_SliderValue = (_) => { return Mathf.RoundToInt(DaggerfallUnity.Settings.MusicVolume * 100f); };
                        musicVolume.DataSource_SliderValueString = (_) =>
                        {
                            return ((int)(DaggerfallUnity.Settings.MusicVolume * 100f)).ToString("N0") + "%";
                        };
                        musicVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
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

                            // the InputManager only sets its 'movementAcceleration' bool in its
                            // Start function, so we need to force it to call that function again
                            // Note: this will also refresh some of its internal containers, but
                            // none of those have side effects beyond taking up cycles
                            InputManager.Instance.SendMessage("Start");
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
                            GameManager.Instance.SpeedChanger.ToggleSneak = toggle.isToggledOn;
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
                        slider_weaponSwingMode.SetTitle(TextManager.Instance.GetText("MainMenu", "weaponSwingMode"));
                        slider_weaponSwingMode.SetSliderMinMax(0, _localizedStrings_weaponSwingModes.Length - 1, true);
                        slider_weaponSwingMode.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.WeaponSwingMode; };
                        slider_weaponSwingMode.DataSource_SliderValueString = (_) => { return _localizedStrings_weaponSwingModes[DaggerfallUnity.Settings.WeaponSwingMode]; };

                        slider_weaponSwingMode.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
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
                            axisBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;

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
                        slider_lookSensitivity.SetTitle("Look Sensitivity"); // TODO(Acreal): localize this string
                        slider_lookSensitivity.SetSliderMinMax(0f, 1f, false);
                        slider_lookSensitivity.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickLookSensitivity; };
                        slider_lookSensitivity.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickLookSensitivity.ToString("F2"); };
                        slider_lookSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            DaggerfallUnity.Settings.JoystickLookSensitivity = slider.GetSliderValue();
                        };

                        UISlider slider_uiMouseSensitivity = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_uiMouseSensitivity.SetTitle("UI Mouse Sensitivity"); // TODO(Acreal): localize this string
                        slider_uiMouseSensitivity.SetSliderMinMax(0f, 1f, false);
                        slider_uiMouseSensitivity.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MouseLookSensitivity; };
                        slider_uiMouseSensitivity.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.MouseLookSensitivity.ToString("F2"); };
                        slider_uiMouseSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            DaggerfallUnity.Settings.MouseLookSensitivity = slider.GetSliderValue();
                        };

                        UISlider slider_uiMouseSmoothing = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_uiMouseSmoothing.SetTitle("Mouse Smoothing Factor"); // TODO(Acreal): localize this string
                        slider_uiMouseSmoothing.SetSliderMinMax(0f, 0.9f, false);
                        slider_uiMouseSmoothing.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MouseLookSmoothingFactor; };
                        slider_uiMouseSmoothing.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.MouseLookSmoothingFactor.ToString("F2"); };
                        slider_uiMouseSmoothing.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            DaggerfallUnity.Settings.MouseLookSmoothingFactor = slider.GetSliderValue();
                        };

                        UISlider slider_maxMovementThreshold = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_maxMovementThreshold.SetTitle("Maximum Movement Threshold"); // TODO(Acreal): localize this string
                        slider_maxMovementThreshold.SetSliderMinMax(0f, 1f, false);
                        slider_maxMovementThreshold.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickMovementThreshold; };
                        slider_maxMovementThreshold.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickMovementThreshold.ToString("F2"); };
                        slider_maxMovementThreshold.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            DaggerfallUnity.Settings.JoystickMovementThreshold = slider.GetSliderValue();
                        };

                        UISlider slider_deadzone = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        slider_deadzone.SetTitle("Deadzone"); // TODO(Acreal): localize this string
                        slider_deadzone.SetSliderMinMax(0f, 1f, false);
                        slider_deadzone.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickDeadzone; };
                        slider_deadzone.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickDeadzone.ToString("F2"); };
                        slider_deadzone.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
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
                            joystickBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
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
            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Paused, true);
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
            // TODO(Acreal): localize strings
            string msg = string.Format("Do you want to clear the binding for '{0}'?", entry.actionEnumAsString); 
            UIManager.ShowConfirmationWindow("Clear Binding", msg,
                () => 
                {
                    UIManager.HideConfirmationWindow();
                    
                    if(entry is UIKeyCodeBindingEntry) { InputManager.Instance.ClearBinding(((UIKeyCodeBindingEntry)entry).boundAction); }
                    else if(entry is UIJoystickKeyBindingEntry) { InputManager.Instance.ClearJoystickUIBinding(((UIJoystickKeyBindingEntry)entry).boundAction); }
                    else if(entry is UIAxisBindingEntry) { InputManager.Instance.ClearAxisBinding(((UIAxisBindingEntry)entry).boundAction); }

                    entry.Refresh();

                    if (primary) { _allPrimaryKeybindsDict[entry.actionEnumAsString] = string.Empty; }
                    else { _allSecondaryKeybindsDict[entry.actionEnumAsString] = string.Empty; }
                    CheckDuplicates();
                },
                () =>
                {
                    UIManager.HideConfirmationWindow();
                });
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
                Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.ExclusiveFullScreen);
            }
            else
            {
                Screen.SetResolution(resolution.width, resolution.height, DaggerfallUnity.Settings.Fullscreen);
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

        private void SetKeyBind(UIControlBindingEntry bindingEntry, KeyCode bindingAsKeyCode, string bindingAsString, bool isPrimaryBinding)
        {
            bool isAxisAction = bindingEntry is UIAxisBindingEntry;
            bool isJoystickAction = !isAxisAction && bindingEntry is UIJoystickKeyBindingEntry;

            if (isAxisAction)
            {
                //there are no secondary keybinds for axis actions
                bindingEntry.SetPrimaryBindingValue(bindingAsString);
                _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = bindingAsString;

                UIAxisBindingEntry axisEntry = bindingEntry as UIAxisBindingEntry;
                InputManager.Instance.SetAxisBinding(bindingAsString, axisEntry.boundAction);

                // TODO(Acreal): add tooltips to keybindings
                //bindingEntry.SuppressToolTip = bindingEntry.Label.Text != ControlsConfigManager.ElongatedButtonText;
                //bindingEntry.ToolTipText = ControlsConfigManager.Instance.GetButtonText(code, true);
            }
            else if (isJoystickAction)
            {
                //there are no secondary keybinds for joystick actions
                bindingEntry.SetPrimaryBindingValue(bindingAsString);
                _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = bindingAsString;

                UIJoystickKeyBindingEntry joystickEntry = bindingEntry as UIJoystickKeyBindingEntry;
                InputManager.Instance.SetJoystickUIBinding(bindingAsKeyCode, joystickEntry.boundAction);

                // TODO(Acreal): add tooltips to keybindings
                //bindingEntry.SuppressToolTip = bindingEntry.Label.Text != ControlsConfigManager.ElongatedButtonText;
                //bindingEntry.ToolTipText = ControlsConfigManager.Instance.GetButtonText(code, true);
            }
            else
            {
                UIKeyCodeBindingEntry keyCodeBindingEntry = bindingEntry as UIKeyCodeBindingEntry;
                if (keyCodeBindingEntry != null)
                {
                    if (isPrimaryBinding)
                    {
                        bindingEntry.SetPrimaryBindingValue(bindingAsString);
                        _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = bindingAsString;
                    }
                    else
                    {
                        bindingEntry.SetSecondaryBindingValue(bindingAsString);
                        _allSecondaryKeybindsDict[bindingEntry.actionEnumAsString] = bindingAsString;
                    }
                    InputManager.Instance.SetBinding(bindingAsKeyCode, keyCodeBindingEntry.boundAction, isPrimaryBinding);

                    // TODO(Acreal): add tooltips to keybindings
                    //bindingEntry.SuppressToolTip = bindingEntry.Label.Text != ControlsConfigManager.ElongatedButtonText;
                    //bindingEntry.ToolTipText = ControlsConfigManager.Instance.GetButtonText(code, true);
                }
            }

            saveSettings = true;
            InputManager.Instance.SaveKeyBinds();
        }

        private IEnumerator<float> WaitForKeyPressRoutine(UIControlBindingEntry bindingEntry, bool isPrimaryBinding)
        {
            GameManager.Instance.PlayerMouseLook.enabled = false;
            InputManager.Instance.CursorVisible = false;
            UnityEngine.Cursor.visible = false;
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;

            ControlsConfigManager.Instance.UsingPrimary = isPrimaryBinding;
            AllowCancel = false;

            bool isAxisAction = bindingEntry is UIAxisBindingEntry;
            bool isJoystickAction = !isAxisAction && bindingEntry is UIJoystickKeyBindingEntry;

            // TODO(Acreal): localize strings
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

            KeyCode code = KeyCode.None;
            while ((!isAxisAction && (code = InputManager.Instance.GetAnyKeyDownIgnoreAxisBinds(true)) == KeyCode.None)
                || (isAxisAction && (code = InputManager.Instance.GetAnyKeyDown(true)) == KeyCode.None))
            {
                yield return 0f;
            }

            UIManager.HideConfirmationWindow();

            if (code != KeyCode.Escape && InputManager.Instance.ReservedKeys.FirstOrDefault(x => x == code) == KeyCode.None)
            {
                // restore mouse function
                GameManager.Instance.PlayerMouseLook.enabled = true;
                InputManager.Instance.CursorVisible = true;
                UnityEngine.Cursor.visible = true;
                UnityEngine.Cursor.lockState = CursorLockMode.None;

                string prevBindingString = isPrimaryBinding ? _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] : _allSecondaryKeybindsDict[bindingEntry.actionEnumAsString];

                KeyCode newBindingCode = code;
                string newBindingString = null;
                if (isAxisAction) { newBindingString = InputManager.Instance.AxisKeyCodeToInputAxis((int)newBindingCode); }
                else { newBindingString = ControlsConfigManager.Instance.GetButtonText(newBindingCode); }

                if (isPrimaryBinding) { _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = newBindingString; }
                else { _allSecondaryKeybindsDict[bindingEntry.actionEnumAsString] = newBindingString; }

                IEnumerable<string> values = _allPrimaryKeybindsDict.Values.Concat(_allSecondaryKeybindsDict.Values);
                HashSet<string> duplicateSet = ControlsConfigManager.Instance.GetDuplicates(values);
                if(newBindingString != null && duplicateSet.Contains(newBindingString))
                {
                    ////// FIND DUPLICATE ACTION NAME //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    string otherActionBinding = null;

                    foreach(KeyValuePair<string, string> kvp in _allPrimaryKeybindsDict)
                    {
                        if(kvp.Value == newBindingString)
                        {
                            otherActionBinding = kvp.Key;
                            break;
                        }
                    }

                    if(otherActionBinding == null)
                    {
                        foreach (KeyValuePair<string, string> kvp in _allSecondaryKeybindsDict)
                        {
                            if (kvp.Value == newBindingString)
                            {
                                otherActionBinding = kvp.Key;
                                break;
                            }
                        }
                    }

                    if (otherActionBinding != null && otherActionBinding != bindingEntry.actionEnumAsString) //ignore if binding to self
                    {
                        ////// SHOW CONFIRMATION WINDOW FOR DUPLICATE KEYBINDS //////////////////////////////////////////////////////////////////////////////////////////////////////
                        {
                            AllowCancel = false;

                            // TODO(Acreal): localize strings
                            string msg = string.Format("'{0}' is already bound to '{1}'. Do you want to duplicate this binding?\n(Warning: This could cause unintended behaviour.)", newBindingString, otherActionBinding);
                            UIManager.ShowConfirmationWindow("Duplicate Bind", msg,
                                () =>
                                {
                                    AllowCancel = true;
                                    UIManager.HideConfirmationWindow();
                                    SetKeyBind(bindingEntry, newBindingCode, newBindingString, isPrimaryBinding);
                                    _pauseWindowInstance?.panelControlSettings?.UpdateDuplicateBindings(duplicateSet);
                                },
                                () =>
                                {
                                    AllowCancel = true;
                                    UIManager.HideConfirmationWindow();
                                    if (isPrimaryBinding) { _allPrimaryKeybindsDict[bindingEntry.actionEnumAsString] = prevBindingString; }
                                    else { _allSecondaryKeybindsDict[bindingEntry.actionEnumAsString] = prevBindingString; }
                                });

                            while (!AllowCancel)
                            {
                                yield return 0f;
                            }
                        }
                    }
                }
                else
                {
                    AllowCancel = true;
                    SetKeyBind(bindingEntry, newBindingCode, newBindingString, isPrimaryBinding);
                    _pauseWindowInstance?.panelControlSettings?.UpdateDuplicateBindings(duplicateSet);
                }
            }
        }
        #endregion
    }
}