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
        private Dictionary<int, UIWindowType> _toggleIdToCoreWindowType = null;
        private List<UIWindowType> _enableableCoreWindowTypes = null;
        private List<UIWindowType> _toggledCoreWindowTypes = null;
        private int _maxFramerate = 30;
        #endregion


        #region Initalization
        public UIPauseWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            _allPrimaryKeybindsDict = new Dictionary<string, string>(64);
            _allSecondaryKeybindsDict = new Dictionary<string, string>(64);

            _stringToActionEnumDict = new Dictionary<string, InputManager.Actions>(45);
            _stringToAxisActionEnumDict = new Dictionary<string, InputManager.AxisActions>(4);
            _stringToJoystickActionEnumDict = new Dictionary<string, InputManager.JoystickUIActions>(4);

            _instanceIdToResolutionDict = new Dictionary<int, Resolution>();
            _toggleIdToActionStringDict = new Dictionary<int, string>();
            _toggleIdToCoreWindowType = new Dictionary<int, UIWindowType>();
            _toggledCoreWindowTypes = new List<UIWindowType>();
            _enableableCoreWindowTypes = new List<UIWindowType>
            {
                UIWindowType.PauseOptions,
                UIWindowType.Inventory,
                UIWindowType.Talk,
                UIWindowType.UnitySaveGame,
            };

            ResetKeybindDict();
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

                _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Default);
                _pauseWindowInstance.Show();
            }
        }

        public void HideWindow()
        {
            _pauseWindowInstance?.Hide();
        }

        public override void OnPop()
        {
            HideWindow();
            _pauseWindowInstance?.ResetWindow();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;

            if (saveSettings)
            {
                DaggerfallUnity.Settings.SaveSettings();
                saveSettings = false;
            }

            if (_toggledCoreWindowTypes != null && _toggledCoreWindowTypes.Count > 0)
            {
                for (int i = 0; i < _toggledCoreWindowTypes.Count; i++)
                {
                    if (UIManager.Instance.IsCoreWindowEnabled(_toggledCoreWindowTypes[i]))
                    {
                        UIManager.Instance.DisableCoreWindow(_toggledCoreWindowTypes[i]);
                    }
                    else
                    {
                        UIManager.Instance.EnableCoreWindow(_toggledCoreWindowTypes[i]);
                    }
                }
                _toggledCoreWindowTypes.Clear();

                //post message calls needed before and after ApplyCoreWindowChanges call
                //due to DaggerFallUI only checking to refresh its ui windows if there
                //are messages in the queue to process (a single call can/will get eaten,
                //but two calls produces consistent behavior)
                DaggerfallUI.UIManager.PostMessage(string.Empty);
                UIManager.Instance.ApplyCoreWindowChanges();
                DaggerfallUI.UIManager.PostMessage(string.Empty);
            }
        }
        #endregion


        #region Base Class Overrides
        public override void OnPush()
        {
            if (!IsSetup)
            {
                Setup();
            }

            base.OnPush();
            GameManager.Instance.SaveLoadManager.EnumerateSaves();
            _pauseWindowInstance.panelInterfaceSettings.Refresh();
            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Default, true);
            ShowWindow();
        }

        protected override void Setup()
        {
            IsSetup = true;

            if (_pauseWindowInstance == null)
            {
                UIWindow window = UIManager.Instance.GetWindowInstance(UIWindowInstanceType.PauseOptions);
                if (window == null || !(window is UIPauseWindow))
                {
                    Debug.LogError("[AcrealUI.UIPauseWindowController] UIManager.GetWindowInstance(UIWindowInstanceType.PauseOptions) returned " + (window == null ? " NULL!" : "a window of the wrong type! Expected type UIPauseWindow, but got " + window.GetType().ToString() + "!"));
                    return;
                }

                _pauseWindowInstance = window as UIPauseWindow;
                _pauseWindowInstance.Initialize();

                _pauseWindowInstance.Event_ButtonClick_PrevWindow += OnBackButtonClicked;

                //for some reason there's a compiler error if we try a straight
                // '+=' on this one
                _pauseWindowInstance.Event_ButtonClick_CloseWindow += () =>
                {
                    CancelWindow();
                };

                #region Paused Panel
                if (_pauseWindowInstance.panelPaused != null)
                {
                    _pauseWindowInstance.panelPaused.gameObject.SetActive(true);

                    _pauseWindowInstance.panelPaused.Event_OnButtonClicked_Continue += () =>
                    {
                        CancelWindow();
                    };

                    _pauseWindowInstance.panelPaused.Event_OnButtonClicked_SaveGame += () =>
                    {
                        DaggerfallUI.UIManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { DaggerfallUI.UIManager, DaggerfallUnitySaveGameWindow.Modes.SaveGame, this, false }));
                    };

                    _pauseWindowInstance.panelPaused.Event_OnButtonClicked_LoadGame += () =>
                    {
                        DaggerfallUI.UIManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { DaggerfallUI.UIManager, DaggerfallUnitySaveGameWindow.Modes.LoadGame, this, false }));
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

                        UIManager.popupManager.ShowTextConfirmationWindow(null, msg, null,
                            (_) =>
                            {
                                DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiExitGame);
                            },
                            (_) =>
                            {
                                AllowCancel = true;
                                UIManager.popupManager.HideActivePopupWindow();
                            });
                    };
                }
                #endregion

                #region Settings Panel
                if (_pauseWindowInstance.panelSettings != null)
                {
                    _pauseWindowInstance.panelSettings.gameObject.SetActive(false);

                    _pauseWindowInstance.panelSettings.Event_OnButtonClicked_GeneralSettings += () =>
                    {
                        _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_General);
                    };

                    _pauseWindowInstance.panelSettings.Event_OnButtonClicked_InterfaceSettings += () =>
                    {
                        _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_Interface);
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
                #endregion

                #region General Settings Panel
                if (_pauseWindowInstance.panelGeneralSettings != null)
                {
                    _pauseWindowInstance.panelGeneralSettings.gameObject.SetActive(false);

                    #region Gameplay Settings
                    UIScrollListGroup generalGroup = _pauseWindowInstance.panelGeneralSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Gameplay.GetText());

                    UIToggle headbobToggle = generalGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    headbobToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_HeadBobbing.GetText());
                    headbobToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.HeadBobbing; };
                    headbobToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.HeadBobbing = toggle.isToggledOn;
                    };
                    #endregion
                }
                #endregion

                #region Interface Settings Panel
                if (_pauseWindowInstance.panelInterfaceSettings != null)
                {
                    _pauseWindowInstance.panelInterfaceSettings.gameObject.SetActive(false);

                    #region Gameplay Settings
                    UIScrollListGroup interfaceGroup = _pauseWindowInstance.panelInterfaceSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_NewWindows.GetText());

                    for (int i = 0; i < _enableableCoreWindowTypes.Count; i++)
                    {
                        UIToggle windowToggle = interfaceGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        _toggleIdToCoreWindowType[windowToggle.GetInstanceID()] = _enableableCoreWindowTypes[i];

                        string windowTypeText = UITextStrings.GetWindowTypeText(_enableableCoreWindowTypes[i]);
                        windowToggle.SetDisplayName(windowTypeText);

                        windowToggle.DataSource_IsToggledOn = (GameObject sender) =>
                        {
                            UIToggle toggle = sender.GetComponent<UIToggle>();
                            if (_toggleIdToCoreWindowType.TryGetValue(toggle.GetInstanceID(), out UIWindowType wndwType))
                            {
                                bool enabled = UIManager.Instance.IsCoreWindowEnabled(wndwType);
                                if (_toggledCoreWindowTypes.Contains(wndwType)) { enabled = !enabled; }
                                return enabled;
                            }
                            return false;
                        };

                        windowToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (_toggleIdToCoreWindowType.TryGetValue(toggle.GetInstanceID(), out UIWindowType wndwType))
                            {
                                if (_toggledCoreWindowTypes.Contains(wndwType))
                                {
                                    _toggledCoreWindowTypes.Remove(wndwType);
                                }
                                else
                                {
                                    _toggledCoreWindowTypes.Add(wndwType);
                                }
                            }
                        };

                    }
                    #endregion
                }
                #endregion

                #region Video Settings Panel
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
                        if (allResolutions[i].refreshRate < 30)
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
                    UIScrollListGroup windowGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Window.GetText());
                    windowGroup.Collapse();

                    #region Resolution Settings
                    UIScrollListGroup resolutionGroup = windowGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Label_Resolution.GetText());
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
                    #endregion

                    #region Fullscreen
                    UIToggle fullScreenToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    fullScreenToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Fullscreen.GetText());
                    fullScreenToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.Fullscreen; };
                    fullScreenToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.Fullscreen = toggle.isToggledOn;
                        ApplyResolution(Screen.currentResolution);
                    };
                    #endregion

                    #region Exclusive
                    UIToggle exclusiveToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    exclusiveToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_ExclusiveFullscreen.GetText());
                    exclusiveToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ExclusiveFullscreen; };
                    exclusiveToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.ExclusiveFullscreen = toggle.isToggledOn;
                        ApplyResolution(Screen.currentResolution);
                    };
                    #endregion

                    #region Vsync
                    UIToggle vsyncToggle = windowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    vsyncToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Vsync.GetText());
                    vsyncToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.VSync; };
                    vsyncToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.VSync = toggle.isToggledOn;
                        QualitySettings.vSyncCount = toggle.isToggledOn ? 1 : 0;
                    };
                    #endregion

                    #region Framerate
                    UISlider framerateSlider = windowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    framerateSlider.SetTitle(UITextStrings.OptionsWindow_Label_MaxFramerate.GetText());
                    framerateSlider.SetSliderMinMax(30f, _maxFramerate + 1, true);
                    framerateSlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.TargetFrameRate == 0 ? _maxFramerate + 1 : DaggerfallUnity.Settings.TargetFrameRate; };
                    framerateSlider.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.TargetFrameRate == 0 ? UITextStrings.OptionsWindow_Label_Unlimited.GetText() : DaggerfallUnity.Settings.TargetFrameRate.ToString("N0"); };
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

                    #endregion

                    #region Rendering Settings
                    UIScrollListGroup renderGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Rendering.GetText());
                    renderGroup.Collapse();

                    #region Quality
                    UISlider qualitySlider = renderGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    qualitySlider.SetTitle(UITextStrings.OptionsWindow_Label_RenderQuality.GetText());
                    qualitySlider.SetSliderMinMax(0, QualitySettings.names.Length - 1, true);
                    qualitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.QualityLevel; };
                    qualitySlider.DataSource_SliderValueString = (_) => { return QualitySettings.names[DaggerfallUnity.Settings.QualityLevel]; }; // TODO(Acreal): localize these values
                    qualitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        int val = slider.GetSliderValueAsInt();
                        DaggerfallUnity.Settings.QualityLevel = val;
                        QualitySettings.SetQualityLevel(val);
                        GameManager.UpdateShadowDistance();
                        GameManager.UpdateShadowResolution();
                    };
                    #endregion

                    #region Texture Filter
                    UIScrollListGroup textureGroup = renderGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_Textures.GetText());
                    textureGroup.Collapse();

                    UISlider mainFilterSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    mainFilterSlider.SetTitle(UITextStrings.OptionsWindow_Label_MainFilter.GetText());
                    mainFilterSlider.SetSliderMinMax(0, UITextStrings.OptionsWindow_TextArray_FilterModes.arrayLength - 1, true);

                    mainFilterSlider.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.MainFilterMode;
                    };

                    mainFilterSlider.DataSource_SliderValueString = (_) =>
                    {
                        return UITextStrings.OptionsWindow_TextArray_FilterModes.GetText(DaggerfallUnity.Settings.MainFilterMode);
                    };

                    mainFilterSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.MainFilterMode = slider.GetSliderValueAsInt();
                    };
                    #endregion

                    #region GUI Filter
                    UISlider guiFilterSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    guiFilterSlider.SetTitle(UITextStrings.OptionsWindow_Label_GuiFilter.GetText());
                    guiFilterSlider.SetSliderMinMax(0, UITextStrings.OptionsWindow_TextArray_FilterModes.arrayLength - 1, true);

                    guiFilterSlider.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.GUIFilterMode;
                    };

                    guiFilterSlider.DataSource_SliderValueString = (_) =>
                    {
                        return UITextStrings.OptionsWindow_TextArray_FilterModes.GetText(DaggerfallUnity.Settings.GUIFilterMode);
                    };

                    guiFilterSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.GUIFilterMode = slider.GetSliderValueAsInt();
                    };
                    #endregion

                    #region Video Filter
                    UISlider videoFilterSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    videoFilterSlider.SetTitle(UITextStrings.OptionsWindow_Label_VideoFilter.GetText());
                    videoFilterSlider.SetSliderMinMax(0, UITextStrings.OptionsWindow_TextArray_FilterModes.arrayLength - 1, true);

                    videoFilterSlider.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.VideoFilterMode;
                    };

                    videoFilterSlider.DataSource_SliderValueString = (_) =>
                    {
                        return UITextStrings.OptionsWindow_TextArray_FilterModes.GetText(DaggerfallUnity.Settings.VideoFilterMode);
                    };

                    videoFilterSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.VideoFilterMode = slider.GetSliderValueAsInt();
                    };
                    #endregion

                    #region Dungeon Texture Mode
                    UISlider dungeonTexSlider = textureGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    dungeonTexSlider.SetTitle(UITextStrings.OptionsWindow_Label_DungeonTextureMode.GetText());
                    dungeonTexSlider.SetSliderMinMax(0, UITextStrings.OptionsWindow_TextArray_FilterModes.arrayLength - 1, true);

                    dungeonTexSlider.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.RandomDungeonTextures;
                    };

                    dungeonTexSlider.DataSource_SliderValueString = (_) =>
                    {
                        return UITextStrings.OptionsWindow_TextArray_FilterModes.GetText(DaggerfallUnity.Settings.RandomDungeonTextures);
                    };

                    dungeonTexSlider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.RandomDungeonTextures = slider.GetSliderValueAsInt();
                    };
                    #endregion

                    #region Texture Arrays
                    UIToggle texArrayToggle = textureGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    texArrayToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_EnableTextureArrays.GetText());
                    texArrayToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.EnableTextureArrays; };
                    texArrayToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.EnableTextureArrays = toggle.isToggledOn;
                    };
                    #endregion

                    #region Anti-Aliasing
                    UIScrollListGroup aaGroup = renderGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_AntiAliasing.GetText());
                    aaGroup.Collapse();

                    #region Mode
                    UISlider aaModeSlider = aaGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    aaModeSlider.SetTitle(UITextStrings.OptionsWindow_Label_Method.GetText());
                    aaModeSlider.SetSliderMinMax(0, 3, true);

                    aaModeSlider.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.AntialiasingMethod;
                    };

                    aaModeSlider.DataSource_SliderValueString = (_) =>
                    {
                        switch (DaggerfallUnity.Settings.AntialiasingMethod)
                        {
                            case 1: return UITextStrings.OptionsWindow_Label_Fxaa.GetText();
                            case 2: return UITextStrings.OptionsWindow_Label_Smaa.GetText();
                            case 3: return UITextStrings.OptionsWindow_Label_Taa.GetText();
                            default: return UITextStrings.Global_Label_None.GetText();
                        }
                    };

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
                    #endregion

                    #region TAA Sharpness
                    UISlider taaSharpnessSlider = aaGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    taaSharpnessSlider.SetTitle(UITextStrings.OptionsWindow_Label_TaaSharpness.GetText());
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
                    #endregion

                    #region SMAA Quality
                    UISlider smaaQualitySlider = aaGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    smaaQualitySlider.SetTitle(UITextStrings.OptionsWindow_Label_SmaaQuality.GetText());
                    smaaQualitySlider.SetSliderMinMax(0, 2, true);
                    smaaQualitySlider.DataSource_GameObjectActive = (_) => { return DaggerfallUnity.Settings.AntialiasingMethod == (int)AntiAliasingMethods.SMAA; };
                    smaaQualitySlider.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.AntialiasingSMAAQuality; };
                    smaaQualitySlider.DataSource_SliderValueString = (_) =>
                    {
                        switch (DaggerfallUnity.Settings.AntialiasingSMAAQuality)
                        {
                            case 1: return UITextStrings.OptionsWindow_Label_Medium.GetText();
                            case 2: return UITextStrings.OptionsWindow_Label_High.GetText();
                            default: return UITextStrings.OptionsWindow_Label_Low.GetText();
                        }
                    };
                    smaaQualitySlider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.AntialiasingSMAAQuality = slider.GetSliderValueAsInt();

                        if (GameManager.Instance.StartGameBehaviour != null)
                        {
                            GameManager.Instance.StartGameBehaviour.DeployCoreGameEffectSettings(CoreGameEffectSettingsGroups.Antialiasing);
                        }
                    };
                    #endregion

                    #region FXAA
                    UIToggle fastFxaaToggle = aaGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    fastFxaaToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_FxaaFastMode.GetText());
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
                    #endregion

                    #region Ambient Lit Interiors
                    UIToggle ambientLitToggle = renderGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    ambientLitToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_AmbientLitInteriors.GetText());
                    ambientLitToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.AmbientLitInteriors; };
                    ambientLitToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.AmbientLitInteriors = toggle.isToggledOn;
                    };
                    #endregion
                    #endregion

                    #region PostFX Settings
                    UIScrollListGroup postFxGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Label_PostProcess.GetText());
                    postFxGroup.Collapse();

                    #region Retro Mode
                    UIScrollListGroup retroGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_RetroMode.GetText());
                    retroGroup.Collapse();

                    #region Rendering Mode
                    {
                        UISlider retroSlider = retroGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        retroSlider.SetTitle(UITextStrings.OptionsWindow_Label_Mode.GetText());
                        retroSlider.SetSliderMinMax(0, 2, true);

                        retroSlider.DataSource_SliderValue = (_) =>
                        {
                            return DaggerfallUnity.Settings.RetroRenderingMode;
                        };

                        retroSlider.DataSource_SliderValueString = (_) =>
                        {
                            switch (DaggerfallUnity.Settings.RetroRenderingMode)
                            {
                                case 1: return UITextStrings.OptionsWindow_Label_RetroMode320.GetText();
                                case 2: return UITextStrings.OptionsWindow_Label_RetroMode640.GetText();
                                default: return UITextStrings.OptionsWindow_Label_RetroModeOff.GetText();
                            }
                        };

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
                    #endregion

                    #region Post-Process Mode
                    {
                        UISlider retroSlider = retroGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        retroSlider.SetTitle(UITextStrings.OptionsWindow_Label_PostProcess.GetText());
                        retroSlider.SetSliderMinMax(0, 4, true);

                        retroSlider.DataSource_SliderValue = (_) =>
                        {
                            return DaggerfallUnity.Settings.PostProcessingInRetroMode;
                        };

                        retroSlider.DataSource_SliderValueString = (_) =>
                        {
                            switch (DaggerfallUnity.Settings.PostProcessingInRetroMode)
                            {
                                case 1: return UITextStrings.OptionsWindow_Label_Posterization_Full.GetText();
                                case 2: return UITextStrings.OptionsWindow_Label_Posterization_MinusSky.GetText();
                                case 3: return UITextStrings.OptionsWindow_Label_Palettization_Full.GetText();
                                case 4: return UITextStrings.OptionsWindow_Label_Palettization_MinusSky.GetText();
                                default: return UITextStrings.OptionsWindow_Label_Off.GetText();
                            }
                        };

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
                    #endregion

                    #region Aspect Correction
                    {
                        UISlider retroSlider = retroGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        retroSlider.SetTitle(UITextStrings.OptionsWindow_Label_RetroModeAspectCorrection.GetText());
                        retroSlider.SetSliderMinMax(0, 2, true);

                        retroSlider.DataSource_SliderValue = (_) =>
                        {
                            return DaggerfallUnity.Settings.RetroModeAspectCorrection;
                        };

                        retroSlider.DataSource_SliderValueString = (_) =>
                        {
                            switch (DaggerfallUnity.Settings.RetroModeAspectCorrection)
                            {
                                case 1: return UITextStrings.OptionsWindow_Label_AspectCorrection_FourThree.GetText();
                                case 2: return UITextStrings.OptionsWindow_Label_AspectCorrection_SixteenTen.GetText();
                                default: return UITextStrings.OptionsWindow_Label_Off.GetText();
                            }
                        };

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
                    #endregion

                    #region Depth of Field
                    UIScrollListGroup dofGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_DepthOfField.GetText());
                    dofGroup.Collapse();

                    #region Enable
                    {
                        UIToggle dofToggle = dofGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        dofToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Focal Distance
                    {
                        UISlider focusDistSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        focusDistSlider.SetTitle(UITextStrings.OptionsWindow_Label_FocusDistance.GetText());
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
                    #endregion

                    #region Aperture
                    {
                        UISlider apertureSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        apertureSlider.SetTitle(UITextStrings.OptionsWindow_Label_Aperture.GetText());
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
                    #endregion

                    #region Focal Length
                    {
                        UISlider focalLengthSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        focalLengthSlider.SetTitle(UITextStrings.OptionsWindow_Label_FocalLength.GetText());
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
                    #endregion

                    #region Max Blur Size
                    {
                        UISlider focalLengthSlider = dofGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        focalLengthSlider.SetTitle(UITextStrings.OptionsWindow_Label_MaxBlurSize.GetText());
                        focalLengthSlider.SetSliderMinMax(0, 3, true);

                        focalLengthSlider.DataSource_SliderValue = (_) =>
                        {
                            return DaggerfallUnity.Settings.DepthOfFieldMaxBlurSize;
                        };

                        focalLengthSlider.DataSource_SliderValueString = (_) =>
                        {
                            switch (DaggerfallUnity.Settings.DepthOfFieldMaxBlurSize)
                            {
                                case 0: return UITextStrings.OptionsWindow_Label_Small.GetText();
                                case 1: return UITextStrings.OptionsWindow_Label_Medium.GetText();
                                case 2: return UITextStrings.OptionsWindow_Label_Large.GetText();
                                case 3: return UITextStrings.OptionsWindow_Label_VeryLarge.GetText();
                                default: return null;
                            }
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
                    #endregion

                    #region Bloom
                    UIScrollListGroup bloomGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_Bloom.GetText());
                    bloomGroup.Collapse();

                    #region Enable
                    {
                        UIToggle bloomToggle = bloomGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        bloomToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Intensity
                    {
                        UISlider intensitySlider = bloomGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        intensitySlider.SetTitle(UITextStrings.OptionsWindow_Label_Intensity.GetText());
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
                    #endregion

                    #region Threshold
                    {
                        UISlider intensitySlider = bloomGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        intensitySlider.SetTitle(UITextStrings.OptionsWindow_Label_Threshold.GetText());
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
                    #endregion

                    #region Diffusion
                    {
                        UISlider intensitySlider = bloomGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        intensitySlider.SetTitle(UITextStrings.OptionsWindow_Label_Diffusion.GetText());
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
                    #endregion

                    #region Fast Mode
                    {
                        UIToggle bloomToggle = bloomGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        bloomToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_BloomFastMode.GetText());
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
                    #endregion

                    #region Ambient Occlusion
                    UIScrollListGroup aoGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_AmbientOcclusion.GetText());
                    aoGroup.Collapse();

                    #region Enable
                    {
                        UIToggle aoToggle = aoGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        aoToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Method
                    {
                        UISlider aoMethodSlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        aoMethodSlider.SetTitle(UITextStrings.OptionsWindow_Label_Method.GetText());
                        bool shaderLevelHighEnough = SystemInfo.graphicsShaderLevel >= 45;
                        int maxLen = shaderLevelHighEnough ? 1 : 0;
                        aoMethodSlider.SetSliderMinMax(0, maxLen, true);

                        aoMethodSlider.DataSource_SliderValue = (_) =>
                        {
                            return DaggerfallUnity.Settings.AmbientOcclusionMethod;
                        };

                        aoMethodSlider.DataSource_SliderValueString = (_) =>
                        {
                            if (DaggerfallUnity.Settings.AmbientOcclusionMethod == 1)
                            {
                                return UITextStrings.OptionsWindow_Label_AO_MultiScaleVolumentric.GetText();
                            }
                            else
                            {
                                return UITextStrings.OptionsWindow_Label_AO_ScalableAmbient.GetText();
                            }
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
                    #endregion

                    #region Intensity
                    {
                        UISlider aoIntensitySlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        aoIntensitySlider.SetTitle(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Radius
                    {
                        UISlider aoRadiusSlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        aoRadiusSlider.SetTitle(UITextStrings.OptionsWindow_Label_Radius.GetText());
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
                    #endregion

                    #region Quality
                    {
                        UISlider aoQualitySlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        aoQualitySlider.SetTitle(UITextStrings.OptionsWindow_Label_Quality.GetText());
                        aoQualitySlider.SetSliderMinMax(0, 4, true);

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
                            switch (DaggerfallUnity.Settings.AmbientOcclusionQuality)
                            {
                                case 0: return UITextStrings.OptionsWindow_Label_Lowest.GetText();
                                case 1: return UITextStrings.OptionsWindow_Label_Low.GetText();
                                case 2: return UITextStrings.OptionsWindow_Label_Medium.GetText();
                                case 3: return UITextStrings.OptionsWindow_Label_High.GetText();
                                case 4: return UITextStrings.OptionsWindow_Label_Ultra.GetText();
                                default: return null;
                            }
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
                    #endregion

                    #region Thickness
                    {
                        UISlider aoThicknessSlider = aoGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        aoThicknessSlider.SetTitle(UITextStrings.OptionsWindow_Label_Thickness.GetText());
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
                    #endregion

                    #region Motion Blur
                    UIScrollListGroup motionBlurGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_MotionBlur.GetText());
                    motionBlurGroup.Collapse();

                    #region Enable
                    {
                        UIToggle motionBlurToggle = motionBlurGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        motionBlurToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Shutter Angle
                    {
                        UISlider intensitySlider = motionBlurGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        intensitySlider.SetTitle(UITextStrings.OptionsWindow_Label_ShutterAngle.GetText());
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
                    #endregion

                    #region Sample Count
                    {
                        UISlider motionBlurSlider = motionBlurGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        motionBlurSlider.SetTitle(UITextStrings.OptionsWindow_Label_SampleCount.GetText());
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
                    #endregion

                    #region Vignette
                    UIScrollListGroup vignetteGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_Vignette.GetText());
                    vignetteGroup.Collapse();

                    #region Enable
                    {
                        UIToggle vignetteToggle = vignetteGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        vignetteToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Intensity
                    {
                        UISlider vignetteSlider = vignetteGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        vignetteSlider.SetTitle(UITextStrings.OptionsWindow_Label_Intensity.GetText());
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
                    #endregion

                    #region Smoothness
                    {
                        UISlider smoothnessSlider = vignetteGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        smoothnessSlider.SetTitle(UITextStrings.OptionsWindow_Label_Smoothness.GetText());
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
                    #endregion

                    #region Rounded
                    {
                        UIToggle roundedToggle = vignetteGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        roundedToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Rounded.GetText());
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

                    #region Roundness
                    {
                        UISlider roundnessSlider = vignetteGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        roundnessSlider.SetTitle(UITextStrings.OptionsWindow_Label_Roundness.GetText());
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
                    #endregion
                    #endregion

                    #region Color Boost
                    UIScrollListGroup colorBoostGroup = postFxGroup.GetOrAddSubScrollListGroup(UITextStrings.OptionsWindow_Title_ColorBoost.GetText());
                    colorBoostGroup.Collapse();

                    #region Enable
                    {
                        UIToggle colorBoostToggle = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                        colorBoostToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Enable.GetText());
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
                    #endregion

                    #region Intensity
                    {
                        UISlider colorBoostSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        colorBoostSlider.SetTitle(UITextStrings.OptionsWindow_Label_Intensity.GetText());
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
                    #endregion

                    #region Radius
                    {
                        UISlider radiusSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        radiusSlider.SetTitle(UITextStrings.OptionsWindow_Label_Radius.GetText());
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
                    #endregion

                    #region Interior Scale
                    {
                        UISlider interiorScaleSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        interiorScaleSlider.SetTitle(UITextStrings.OptionsWindow_Label_InteriorScale.GetText());
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
                    #endregion

                    #region Exterior Scale
                    {
                        UISlider exteriorScaleSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        exteriorScaleSlider.SetTitle(UITextStrings.OptionsWindow_Label_ExteriorScale.GetText());
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
                    #endregion

                    #region Dungeon Scale
                    {
                        UISlider dungeonScaleSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        dungeonScaleSlider.SetTitle(UITextStrings.OptionsWindow_Label_DungeonScale.GetText());
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
                    #endregion

                    #region Dungeon Falloff
                    {
                        UISlider falloffSlider = colorBoostGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                        falloffSlider.SetTitle(UITextStrings.OptionsWindow_Label_DungeonFalloff.GetText());
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
                    #endregion

                    #region Dithering
                    UIToggle ditherToggle = postFxGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    ditherToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Dither.GetText());
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
                    UIScrollListGroup cameraGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Camera.GetText());
                    cameraGroup.Collapse();

                    UISlider fovSlider = cameraGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    fovSlider.SetTitle(UITextStrings.OptionsWindow_Label_FieldOfView.GetText());
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
                    UIScrollListGroup shadowGroup = _pauseWindowInstance.panelVideoSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Shadows.GetText());
                    shadowGroup.Collapse();

                    #region Resolution
                    UISlider shadowRes = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    shadowRes.SetTitle(UITextStrings.OptionsWindow_Label_ShadowResolution.GetText());
                    shadowRes.SetSliderMinMax(0, UITextStrings.OptionsWindow_TextArray_ShadowResolutionModes.arrayLength - 1, true);

                    shadowRes.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.ShadowResolutionMode;
                    };

                    shadowRes.DataSource_SliderValueString = (_) =>
                    {
                        return UITextStrings.OptionsWindow_TextArray_ShadowResolutionModes.GetText(DaggerfallUnity.Settings.ShadowResolutionMode);
                    };

                    shadowRes.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.ShadowResolutionMode = (int)slider.GetSliderValue();
                    };
                    #endregion

                    #region Exterior Shadow Distance
                    UISlider shadowDistExt = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    shadowDistExt.SetTitle(UITextStrings.OptionsWindow_Label_ExteriorShadowDistance.GetText());
                    shadowDistExt.SetSliderMinMax(0.1f, 150f, false);
                    shadowDistExt.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.ExteriorShadowDistance; };
                    shadowDistExt.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.ExteriorShadowDistance.ToString("F2"); };
                    shadowDistExt.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.ExteriorShadowDistance = slider.GetSliderValue();
                    };
                    #endregion

                    #region Interior Shadow Distance
                    UISlider shadowDistInt = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    shadowDistInt.SetTitle(UITextStrings.OptionsWindow_Label_InteriorShadowDistance.GetText());
                    shadowDistInt.SetSliderMinMax(0.1f, 50f, false);
                    shadowDistInt.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.InteriorShadowDistance; };
                    shadowDistInt.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.InteriorShadowDistance.ToString("F2"); };
                    shadowDistInt.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.InteriorShadowDistance = slider.GetSliderValue();
                    };
                    #endregion

                    #region Dungeon Shadow Distance
                    UISlider shadowDistDungeon = shadowGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    shadowDistDungeon.SetTitle(UITextStrings.OptionsWindow_Label_DungeonShadowDistance.GetText());
                    shadowDistDungeon.SetSliderMinMax(0.1f, 50f, false);
                    shadowDistDungeon.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.DungeonShadowDistance; };
                    shadowDistDungeon.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.DungeonShadowDistance.ToString("F2"); };
                    shadowDistDungeon.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.DungeonShadowDistance = slider.GetSliderValue();
                    };
                    #endregion

                    #region Exterior Light Shadows
                    UIToggle exteriorLightShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    exteriorLightShadows.SetDisplayName(UITextStrings.OptionsWindow_Label_ExteriorLightsCastShadows.GetText());
                    exteriorLightShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.ExteriorLightShadows; };
                    exteriorLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.ExteriorLightShadows = toggle.isToggledOn;
                    };
                    #endregion

                    #region Interior Light Shadows
                    UIToggle interiorLightShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    interiorLightShadows.SetDisplayName(UITextStrings.OptionsWindow_Label_InteriorLightsCastShadows.GetText());
                    interiorLightShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.InteriorLightShadows; };
                    interiorLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.InteriorLightShadows = toggle.isToggledOn;
                    };
                    #endregion

                    #region Dungeon Light Shadows
                    UIToggle dungeonLightShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    dungeonLightShadows.SetDisplayName(UITextStrings.OptionsWindow_Label_DungeonLightsCastShadows.GetText());
                    dungeonLightShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.DungeonLightShadows; };
                    dungeonLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.DungeonLightShadows = toggle.isToggledOn;
                    };
                    #endregion

                    #region NPC Shadows
                    UIToggle npcShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    npcShadows.SetDisplayName(UITextStrings.OptionsWindow_Label_NpcsCastShadows.GetText());
                    npcShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.MobileNPCShadows; };
                    npcShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.MobileNPCShadows = toggle.isToggledOn;
                    };
                    #endregion

                    #region Billboard Shadows
                    UIToggle billboardShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    billboardShadows.SetDisplayName(UITextStrings.OptionsWindow_Label_ObjectBillboardsCastShadows.GetText());
                    billboardShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.GeneralBillboardShadows; };
                    billboardShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.GeneralBillboardShadows = toggle.isToggledOn;
                    };
                    #endregion

                    #region Foliage Billboard Shadows
                    UIToggle natureBillboardShadows = shadowGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    natureBillboardShadows.SetDisplayName(UITextStrings.OptionsWindow_Label_FoliageBillboardsCastShadows.GetText());
                    natureBillboardShadows.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.NatureBillboardShadows; };
                    natureBillboardShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.NatureBillboardShadows = toggle.isToggledOn;
                    };
                    #endregion
                    #endregion
                    #endregion
                }
                #endregion

                #region Audio Settings Panel
                if (_pauseWindowInstance.panelAudioSettings != null)
                {
                    _pauseWindowInstance.panelAudioSettings.gameObject.SetActive(false);

                    #region Volume Settings
                    UIScrollListGroup volumeGroup = _pauseWindowInstance.panelAudioSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Volume.GetText());

                    #region SoundFX
                    UISlider soundVolume = volumeGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    soundVolume.SetTitle(UITextStrings.OptionsWindow_Label_SoundEffects.GetText());
                    soundVolume.SetSliderMinMax(0f, 100f, true);

                    soundVolume.DataSource_SliderValue = (_) =>
                    {
                        return Mathf.RoundToInt(DaggerfallUnity.Settings.SoundVolume * 100f);
                    };

                    soundVolume.DataSource_SliderValueString = (_) =>
                    {
                        return ((int)(DaggerfallUnity.Settings.SoundVolume * 100f)).ToString("N0") + "%";
                    };

                    soundVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.SoundVolume = slider.GetSliderValue() * 0.01f;
                    };
                    #endregion

                    #region Music
                    UISlider musicVolume = volumeGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    musicVolume.SetTitle(UITextStrings.OptionsWindow_Label_Music.GetText());
                    musicVolume.SetSliderMinMax(0f, 100f, true);

                    musicVolume.DataSource_SliderValue = (_) =>
                    {
                        return Mathf.RoundToInt(DaggerfallUnity.Settings.MusicVolume * 100f);
                    };

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
                    #endregion
                }
                #endregion

                #region Control Settings Panel
                if (_pauseWindowInstance.panelControlSettings != null)
                {
                    _pauseWindowInstance.panelControlSettings.gameObject.SetActive(false);

                    #region Defaults
                    _pauseWindowInstance.panelControlSettings.Event_OnButtonClicked_Default += () =>
                    {
                        AllowCancel = false;
                        UIManager.popupManager.ShowTextConfirmationWindow(UITextStrings.ConfirmationWindow_Title_SetDefaults.GetText(),
                                                            UITextStrings.ConfirmationWindow_Body_SetDefaults.GetText(),
                                                            null,

                                                            (_) => // Confirm
                                                            {
                                                                UIUtilityFunctions.SetDefaultKeybinds();
                                                                ResetKeybindDict();
                                                                CheckDuplicates();
                                                                _pauseWindowInstance.panelControlSettings.Refresh();
                                                                UIManager.popupManager.HideActivePopupWindow();
                                                                UIManager.Instance.ExecuteDelayed(GetHashCode(), 0, 0.2f, () => { AllowCancel = true; });
                                                            },

                                                            (_) => // Cancel
                                                            {
                                                                UIManager.popupManager.HideActivePopupWindow();
                                                                UIManager.Instance.ExecuteDelayed(GetHashCode(), 0, 0.2f, () => { AllowCancel = true; });
                                                            });
                    };
                    #endregion

                    #region Movement Control Settings
                    UIScrollListGroup movementGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Movement.GetText());
                    movementGroup.Collapse();

                    UIToggle invertLookToggle = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    invertLookToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_InvertLook.GetText());
                    invertLookToggle.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.InvertMouseVertical; };
                    invertLookToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.InvertMouseVertical = toggle.isToggledOn;
                    };

                    UIToggle movementAccelerationToggle = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    movementAccelerationToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_MovementAcceleration.GetText());
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
                    bowsDrawAndRelease.SetDisplayName(UITextStrings.OptionsWindow_Label_BowsDrawAndRelease.GetText());
                    bowsDrawAndRelease.DataSource_IsToggledOn = (_) => { return DaggerfallUnity.Settings.BowDrawback; };
                    bowsDrawAndRelease.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        saveSettings = true;
                        DaggerfallUnity.Settings.BowDrawback = toggle.isToggledOn;
                    };

                    UIToggle sneakToggle = movementGroup.AddElement(UIManager.referenceManager.prefab_toggle) as UIToggle;
                    sneakToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_ToggleSneak.GetText());
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

                    for (int i = 0; i < moveActions.Count; i++)
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
                    UIScrollListGroup combatGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Combat.GetText());
                    combatGroup.Collapse();

                    UISlider slider_weaponSwingMode = combatGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    slider_weaponSwingMode.SetTitle(UITextStrings.OptionsWindow_Label_WeaponSwingMode.GetText());
                    slider_weaponSwingMode.SetSliderMinMax(0, UITextStrings.OptionsWindow_TextArray_WeaponSwingModes.arrayLength - 1, true);

                    slider_weaponSwingMode.DataSource_SliderValue = (_) =>
                    {
                        return DaggerfallUnity.Settings.WeaponSwingMode;
                    };

                    slider_weaponSwingMode.DataSource_SliderValueString = (_) =>
                    {
                        return UITextStrings.OptionsWindow_TextArray_WeaponSwingModes.GetText(DaggerfallUnity.Settings.WeaponSwingMode);
                    };

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
                    UIScrollListGroup interactGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Interaction.GetText());
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

                        keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                        keyCodeBindingEntry.Event_OnClearBinding += Controls_OnClearBinding;
                    }
                    #endregion

                    #region Interface Control Settings
                    UIScrollListGroup interfaceGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_Interface.GetText());
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
                    UIScrollListGroup systemGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_System.GetText());
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
                    UIScrollListGroup axisGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_AxisBindings.GetText());
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
                        invertToggle.SetDisplayName(UITextStrings.OptionsWindow_Label_Invert.GetText());

                        LayoutElement layoutElem = invertToggle.GetComponent<LayoutElement>();
                        if (layoutElem != null) { layoutElem.flexibleWidth = 0f; }

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
                    UIScrollListGroup joystickGroup = _pauseWindowInstance.panelControlSettings.GetOrAddScrollListGroup(UITextStrings.OptionsWindow_Title_JoystickControls.GetText());
                    joystickGroup.Collapse();

                    UISlider slider_lookSensitivity = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    slider_lookSensitivity.SetTitle(UITextStrings.OptionsWindow_Label_LookSensitivity.GetText());
                    slider_lookSensitivity.SetSliderMinMax(0f, 1f, false);
                    slider_lookSensitivity.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickLookSensitivity; };
                    slider_lookSensitivity.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickLookSensitivity.ToString("F2"); };
                    slider_lookSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        DaggerfallUnity.Settings.JoystickLookSensitivity = slider.GetSliderValue();
                    };

                    UISlider slider_uiMouseSensitivity = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    slider_uiMouseSensitivity.SetTitle(UITextStrings.OptionsWindow_Label_UIMouseSensitivity.GetText());
                    slider_uiMouseSensitivity.SetSliderMinMax(0f, 1f, false);
                    slider_uiMouseSensitivity.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MouseLookSensitivity; };
                    slider_uiMouseSensitivity.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.MouseLookSensitivity.ToString("F2"); };
                    slider_uiMouseSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        DaggerfallUnity.Settings.MouseLookSensitivity = slider.GetSliderValue();
                    };

                    UISlider slider_uiMouseSmoothing = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    slider_uiMouseSmoothing.SetTitle(UITextStrings.OptionsWindow_Label_MouseSmoothingFactor.GetText());
                    slider_uiMouseSmoothing.SetSliderMinMax(0f, 0.9f, false);
                    slider_uiMouseSmoothing.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.MouseLookSmoothingFactor; };
                    slider_uiMouseSmoothing.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.MouseLookSmoothingFactor.ToString("F2"); };
                    slider_uiMouseSmoothing.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        DaggerfallUnity.Settings.MouseLookSmoothingFactor = slider.GetSliderValue();
                    };

                    UISlider slider_maxMovementThreshold = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    slider_maxMovementThreshold.SetTitle(UITextStrings.OptionsWindow_Label_MaximumMovementThreshold.GetText());
                    slider_maxMovementThreshold.SetSliderMinMax(0f, 1f, false);
                    slider_maxMovementThreshold.DataSource_SliderValue = (_) => { return DaggerfallUnity.Settings.JoystickMovementThreshold; };
                    slider_maxMovementThreshold.DataSource_SliderValueString = (_) => { return DaggerfallUnity.Settings.JoystickMovementThreshold.ToString("F2"); };
                    slider_maxMovementThreshold.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        DaggerfallUnity.Settings.JoystickMovementThreshold = slider.GetSliderValue();
                    };

                    UISlider slider_deadzone = joystickGroup.AddElement(UIManager.referenceManager.prefab_slider) as UISlider;
                    slider_deadzone.SetTitle(UITextStrings.OptionsWindow_Label_Deadzone.GetText());
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
                        joystickBindingEntry.SetActionEnumString(actionStr);
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
                #endregion
            }
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


        #region UI Callbacks/Delegates
        private void OnBackButtonClicked()
        {
            switch (_pauseWindowInstance.currentState)
            {
                case UIPauseWindow.PauseWindowState.Settings:
                    _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Default);
                    break;

                case UIPauseWindow.PauseWindowState.Settings_Audio:
                case UIPauseWindow.PauseWindowState.Settings_Controls:
                case UIPauseWindow.PauseWindowState.Settings_General:
                case UIPauseWindow.PauseWindowState.Settings_Interface:
                case UIPauseWindow.PauseWindowState.Settings_Video:
                    _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings);
                    break;

                case UIPauseWindow.PauseWindowState.Default:
                default:
                    CancelWindow();
                    break;
            }
        }

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
            string msg = string.Format(UITextStrings.ConfirmationWindow_Body_ClearBinding.GetText(), entry.actionEnumAsString); 
            UIManager.popupManager.ShowTextConfirmationWindow(UITextStrings.ConfirmationWindow_Title_ClearBinding.GetText(), msg, null,
                (_) => 
                {
                    UIManager.popupManager.HideActivePopupWindow();
                    
                    if(entry is UIKeyCodeBindingEntry) { InputManager.Instance.ClearBinding(((UIKeyCodeBindingEntry)entry).boundAction); }
                    else if(entry is UIJoystickKeyBindingEntry) { InputManager.Instance.ClearJoystickUIBinding(((UIJoystickKeyBindingEntry)entry).boundAction); }
                    else if(entry is UIAxisBindingEntry) { InputManager.Instance.ClearAxisBinding(((UIAxisBindingEntry)entry).boundAction); }

                    entry.Refresh();

                    if (primary) { _allPrimaryKeybindsDict[entry.actionEnumAsString] = string.Empty; }
                    else { _allSecondaryKeybindsDict[entry.actionEnumAsString] = string.Empty; }
                    CheckDuplicates();
                },
                (_) =>
                {
                    UIManager.popupManager.HideActivePopupWindow();
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

            string title = UITextStrings.ConfirmationWindow_Title_WaitingForInput.GetText();
            string primaryOrSecondary = isPrimaryBinding ? UITextStrings.Global_Label_Primary.GetText() : UITextStrings.Global_Label_Secondary.GetText();
            string rebindMsg = string.Format(UITextStrings.ConfirmationWindow_Body_RebindAction.GetText(), primaryOrSecondary, bindingEntry.actionEnumAsString);
            string pressAnyMsg = UITextStrings.ConfirmationWindow_Body_PressAnyKey.GetText();
            UIManager.popupManager.ShowTextConfirmationWindow(title, rebindMsg + "\n" + pressAnyMsg, null, null, null);

            float t = 0.2f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0f;
            }

            KeyCode code = KeyCode.None;
            bool isAxisAction = bindingEntry is UIAxisBindingEntry;
            while ((!isAxisAction && (code = InputManager.Instance.GetAnyKeyDownIgnoreAxisBinds(true)) == KeyCode.None)
                || (isAxisAction && (code = InputManager.Instance.GetAnyKeyDown(true)) == KeyCode.None))
            {
                yield return 0f;
            }

            UIManager.popupManager.HideActivePopupWindow();

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

                            string msg = string.Format(UITextStrings.ConfirmationWindow_Body_DuplicateBinding.GetText(), newBindingString, otherActionBinding);
                            UIManager.popupManager.ShowTextConfirmationWindow(UITextStrings.ConfirmationWindow_Title_DuplicateBinding.GetText(), msg, null,
                                (_) =>
                                {
                                    AllowCancel = true;
                                    UIManager.popupManager.HideActivePopupWindow();
                                    SetKeyBind(bindingEntry, newBindingCode, newBindingString, isPrimaryBinding);
                                    _pauseWindowInstance?.panelControlSettings?.UpdateDuplicateBindings(duplicateSet);
                                },
                                (_) =>
                                {
                                    AllowCancel = true;
                                    UIManager.popupManager.HideActivePopupWindow();
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