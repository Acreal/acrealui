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
using IniParser;
using IniParser.Model;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

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
        private string[] _localizedStrings_weaponSwingModes = null;
        private IniData _defaultIniData = null;
        #endregion


        #region Initalization/Cleanup
        public UIPauseWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            _allPrimaryKeybindsDict = new Dictionary<string, string>(64);
            _allSecondaryKeybindsDict = new Dictionary<string, string>(64);

            _stringToActionEnumDict = new Dictionary<string, InputManager.Actions>(45);
            _stringToAxisActionEnumDict = new Dictionary<string, InputManager.AxisActions>(4);
            _stringToJoystickActionEnumDict = new Dictionary<string, InputManager.JoystickUIActions>(4);

            _localizedStrings_weaponSwingModes = TextManager.Instance.GetLocalizedTextList("weaponSwingModes", TextCollections.TextSettings);

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
                        UIUtilityFunctions.PlayButtonClick();
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
                            UIUtilityFunctions.PlayButtonClick();
                            CancelWindow();
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_SaveGame += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            uiManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { uiManager, DaggerfallUnitySaveGameWindow.Modes.SaveGame, this, false }));
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_LoadGame += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            uiManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.UnitySaveGame, new object[] { uiManager, DaggerfallUnitySaveGameWindow.Modes.LoadGame, this, false }));
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_Settings += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings);
                        };

                        _pauseWindowInstance.panelPaused.Event_OnButtonClicked_ExitGame += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Confirmation);

                            DaggerfallMessageBox confirmExitBox = new DaggerfallMessageBox(uiManager, DaggerfallMessageBox.CommonMessageBoxButtons.YesNo, strAreYouSure, this);
                            confirmExitBox.OnButtonClick += (DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton) =>
                            {
                                sender.CloseWindow();

                                if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
                                {
                                    if (saveSettings)
                                    {
                                        DaggerfallUnity.Settings.SaveSettings();
                                        saveSettings = false;
                                    }

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
                            UIUtilityFunctions.PlayButtonClick();
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_General);
                        };

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_VideoSettings += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_Video);
                        };

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_AudioSettings += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            _pauseWindowInstance.SetState(UIPauseWindow.PauseWindowState.Settings_Audio);
                        };

                        _pauseWindowInstance.panelSettings.Event_OnButtonClicked_ControlSettings += () =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
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
                        UIScrollListGroup generalGroup = _pauseWindowInstance.panelGeneralSettings.GetScrollListGroup("Gameplay"); //TODO(Acreal): localize this string

                        UIToggle headbobToggle = generalGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        headbobToggle.SetDisplayName("Head Bobbing"); //TODO(Acreal): localize this string
                        headbobToggle.isToggledOn = DaggerfallUnity.Settings.HeadBobbing;
                        headbobToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClick();
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

                        #region Camera Settings
                        UIScrollListGroup cameraGroup = _pauseWindowInstance.panelVideoSettings.GetScrollListGroup("Camera");

                        UISlider fov = cameraGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        fov.SetTextTitle("Field of View"); //TODO(Acreal): localize this string
                        fov.SetSliderMinMax(60, 120, true);
                        fov.SetSliderValue(DaggerfallUnity.Settings.FieldOfView, true);
                        fov.Event_OnSliderValueChanged += (UISlider slider) =>
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
                        UIScrollListGroup shadowGroup = _pauseWindowInstance.panelVideoSettings.GetScrollListGroup("Shadows");

                        UISlider shadowRes = shadowGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        shadowRes.SetTextTitle("Shadow Resolution"); //TODO(Acreal): localize this string
                        shadowRes.SetSliderMinMax(0, 3, true);
                        shadowRes.SetSliderValue(DaggerfallUnity.Settings.ShadowResolutionMode, true);
                        shadowRes.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ShadowResolutionMode = (int)slider.GetSliderValue();
                        };

                        UISlider shadowDistExt = shadowGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        shadowDistExt.SetTextTitle("Exterior Shadow Distance"); //TODO(Acreal): localize this string
                        shadowDistExt.SetSliderMinMax(0.1f, 150f, false);
                        shadowDistExt.SetSliderValue(DaggerfallUnity.Settings.ExteriorShadowDistance, true);
                        shadowDistExt.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.ExteriorShadowDistance = slider.GetSliderValue();
                        };

                        UISlider shadowDistInt = shadowGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        shadowDistInt.SetTextTitle("Interior Shadow Distance"); //TODO(Acreal): localize this string
                        shadowDistInt.SetSliderMinMax(0.1f, 50f, false);
                        shadowDistInt.SetSliderValue(DaggerfallUnity.Settings.InteriorShadowDistance, true);
                        shadowDistInt.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.InteriorShadowDistance = slider.GetSliderValue();
                        };

                        UISlider shadowDistDungeon = shadowGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        shadowDistDungeon.SetTextTitle("Dungeon Shadow Distance"); //TODO(Acreal): localize this string
                        shadowDistDungeon.SetSliderMinMax(0.1f, 50f, false);
                        shadowDistDungeon.SetSliderValue(DaggerfallUnity.Settings.DungeonShadowDistance, true);
                        shadowDistDungeon.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            DaggerfallUnity.Settings.DungeonShadowDistance = slider.GetSliderValue();
                        };

                        UIToggle exteriorLightShadows = shadowGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        exteriorLightShadows.SetDisplayName("Exterior Lights Cast Shadows"); //TODO(Acreal): localize this string
                        exteriorLightShadows.isToggledOn = DaggerfallUnity.Settings.ExteriorLightShadows;
                        exteriorLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (DaggerfallUnity.Settings.ExteriorLightShadows != toggle.isToggledOn)
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.ExteriorLightShadows = toggle.isToggledOn;
                            }
                        };

                        UIToggle interiorLightShadows = shadowGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        interiorLightShadows.SetDisplayName("Interior Lights Cast Shadows"); //TODO(Acreal): localize this string
                        interiorLightShadows.isToggledOn = DaggerfallUnity.Settings.InteriorLightShadows;
                        interiorLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (DaggerfallUnity.Settings.InteriorLightShadows != toggle.isToggledOn)
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.InteriorLightShadows = toggle.isToggledOn;
                            }
                        };

                        UIToggle dungeonLightShadows = shadowGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        dungeonLightShadows.SetDisplayName("Dungeon Lights Cast Shadows"); //TODO(Acreal): localize this string
                        dungeonLightShadows.isToggledOn = DaggerfallUnity.Settings.DungeonLightShadows;
                        dungeonLightShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (DaggerfallUnity.Settings.DungeonLightShadows != toggle.isToggledOn)
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.DungeonLightShadows = toggle.isToggledOn;
                            }
                        };

                        UIToggle npcShadows = shadowGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        npcShadows.SetDisplayName("Dynamic NPCs Cast Shadows"); //TODO(Acreal): localize this string
                        npcShadows.isToggledOn = DaggerfallUnity.Settings.MobileNPCShadows;
                        npcShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (DaggerfallUnity.Settings.MobileNPCShadows != toggle.isToggledOn)
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.MobileNPCShadows = toggle.isToggledOn;
                            }
                        };

                        UIToggle billboardShadows = shadowGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        billboardShadows.SetDisplayName("Object Billboards Cast Shadows"); //TODO(Acreal): localize this string
                        billboardShadows.isToggledOn = DaggerfallUnity.Settings.GeneralBillboardShadows;
                        billboardShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (DaggerfallUnity.Settings.GeneralBillboardShadows != toggle.isToggledOn)
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.GeneralBillboardShadows = toggle.isToggledOn;
                            }
                        };

                        UIToggle natureBillboardShadows = shadowGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        natureBillboardShadows.SetDisplayName("Foliage Billboards Cast Shadows"); //TODO(Acreal): localize this string
                        natureBillboardShadows.isToggledOn = DaggerfallUnity.Settings.NatureBillboardShadows;
                        natureBillboardShadows.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            if (DaggerfallUnity.Settings.NatureBillboardShadows != toggle.isToggledOn)
                            {
                                saveSettings = true;
                                DaggerfallUnity.Settings.NatureBillboardShadows = toggle.isToggledOn;
                            }
                        };
                        #endregion

                        //TODO(Acreal): finish adding video options
                        //DROPDOWN: resolution selection
                        //SLIDER: fullscreen mode
                        //SLIDER: retro rendering mode
                        //SLIDER: main filter mode
                        //SLIDER: quality level
                        //TOGGLE: random dungeon textures
                        //TOGGLE: vsync
                        //TOGGLE: ambient lit interiors
                        //TOGGLE: texture array
                    }

                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    // AUDIO SETTINGS PANEL
                    ////////////////////////////////////////////////////////////////////////////////////////////////
                    if (_pauseWindowInstance.panelAudioSettings != null)
                    {
                        _pauseWindowInstance.panelAudioSettings.gameObject.SetActive(false);

                        #region Volume Settings
                        UIScrollListGroup volumeGroup = _pauseWindowInstance.panelAudioSettings.GetScrollListGroup("Volume"); //TODO(Acreal): localize this string

                        UISlider soundVolume = volumeGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        soundVolume.SetTextTitle("Sound"); //TODO(Acreal): localize this string
                        soundVolume.SetSliderMinMax(0f, 1f, false);
                        soundVolume.SetSliderValue(DaggerfallUnity.Settings.SoundVolume, true);
                        soundVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            if(Mathf.Abs(DaggerfallUnity.Settings.SoundVolume - slider.GetSliderValue()) >= 1f) { UIUtilityFunctions.PlayButtonClick(); }
                            DaggerfallUnity.Settings.SoundVolume = slider.GetSliderValue();
                        };

                        UISlider musicVolume = volumeGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        musicVolume.SetTextTitle("Music"); //TODO(Acreal): localize this string
                        musicVolume.SetSliderMinMax(0f, 1f, false);
                        musicVolume.SetSliderValue(DaggerfallUnity.Settings.MusicVolume, true);
                        musicVolume.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            saveSettings = true;
                            if (Mathf.Abs(DaggerfallUnity.Settings.MusicVolume - slider.GetSliderValue()) >= 0.05f) { UIUtilityFunctions.PlayButtonClick(); }
                            DaggerfallUnity.Settings.MusicVolume = slider.GetSliderValue();
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
                            UIUtilityFunctions.PlayButtonClick();
                            InputManager.Instance.ResetDefaults();
                            InputManager.Instance.SaveKeyBinds();
                            ControlsConfigManager.Instance.ResetUnsavedKeybinds();

                            AllowCancel = true;

                            if (_defaultIniData == null)
                            {
                                TextAsset asset = Resources.Load<TextAsset>("defaults.ini");
                                System.IO.MemoryStream stream = new System.IO.MemoryStream(asset.bytes);
                                System.IO.StreamReader reader = new System.IO.StreamReader(stream);
                                FileIniDataParser iniParser = new FileIniDataParser();
                                _defaultIniData = iniParser.ReadData(reader);
                                reader.Close();
                            }

                            if (_defaultIniData != null)
                            {
                                DaggerfallUnity.Settings.MouseLookSmoothingFactor = float.Parse(_defaultIniData["Controls"]["MouseLookSmoothingFactor"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.MouseLookSensitivity = float.Parse(_defaultIniData["Controls"]["MouseLookSensitivity"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.JoystickLookSensitivity = float.Parse(_defaultIniData["Controls"]["JoystickLookSensitivity"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.JoystickCursorSensitivity = float.Parse(_defaultIniData["Controls"]["JoystickCursorSensitivity"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.JoystickMovementThreshold = float.Parse(_defaultIniData["Controls"]["JoystickMovementThreshold"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.JoystickDeadzone = float.Parse(_defaultIniData["Controls"]["JoystickDeadzone"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.WeaponAttackThreshold = float.Parse(_defaultIniData["Controls"]["WeaponAttackThreshold"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.SoundVolume = float.Parse(_defaultIniData["Controls"]["SoundVolume"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.MusicVolume = float.Parse(_defaultIniData["Controls"]["MusicVolume"], NumberStyles.Float, CultureInfo.InvariantCulture);
                                DaggerfallUnity.Settings.Handedness = int.Parse(_defaultIniData["Controls"]["Handedness"]);
                                DaggerfallUnity.Settings.WeaponSwingMode = int.Parse(_defaultIniData["Controls"]["WeaponSwingMode"]);
                                DaggerfallUnity.Settings.CameraRecoilStrength = int.Parse(_defaultIniData["Controls"]["CameraRecoilStrength"]);
                                DaggerfallUnity.Settings.EnableController = bool.Parse(_defaultIniData["Controls"]["EnableController"]);
                                DaggerfallUnity.Settings.InvertMouseVertical = bool.Parse(_defaultIniData["Controls"]["InvertMouseVertical"]);
                                DaggerfallUnity.Settings.HeadBobbing = bool.Parse(_defaultIniData["Controls"]["HeadBobbing"]);
                                DaggerfallUnity.Settings.MovementAcceleration = bool.Parse(_defaultIniData["Controls"]["MovementAcceleration"]);
                                DaggerfallUnity.Settings.ToggleSneak = bool.Parse(_defaultIniData["Controls"]["ToggleSneak"]);
                                DaggerfallUnity.Settings.InstantRepairs = bool.Parse(_defaultIniData["Controls"]["InstantRepairs"]);
                                DaggerfallUnity.Settings.AllowMagicRepairs = bool.Parse(_defaultIniData["Controls"]["AllowMagicRepairs"]);
                                DaggerfallUnity.Settings.BowDrawback = bool.Parse(_defaultIniData["Controls"]["BowDrawback"]);
                            }

                            ResetKeybindDict();
                            CheckDuplicates();
                        };

                        #region Movement Control Settings
                        UIScrollListGroup movementGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("Movement"); //TODO(Acreal): localize this string

                        UIToggle invertLookToggle = movementGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        invertLookToggle.SetDisplayName("Invert Look"); //TODO(Acreal): localize this string
                        invertLookToggle.isToggledOn = DaggerfallUnity.Settings.InvertMouseVertical;
                        invertLookToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClick();
                            DaggerfallUnity.Settings.InvertMouseVertical = toggle.isToggledOn;
                        };

                        UIToggle movementAccelerationToggle = movementGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        movementAccelerationToggle.SetDisplayName("Movement Acceleration"); //TODO(Acreal): localize this string
                        movementAccelerationToggle.isToggledOn = DaggerfallUnity.Settings.MovementAcceleration;
                        movementAccelerationToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClick();
                            DaggerfallUnity.Settings.MovementAcceleration = toggle.isToggledOn;
                        };

                        UIToggle bowsDrawAndRelease = movementGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        bowsDrawAndRelease.SetDisplayName("Bows Draw and Release"); //TODO(Acreal): localize this string
                        bowsDrawAndRelease.isToggledOn = DaggerfallUnity.Settings.BowDrawback;
                        bowsDrawAndRelease.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClick();
                            DaggerfallUnity.Settings.BowDrawback = toggle.isToggledOn;
                        };

                        UIToggle sneakToggle = movementGroup.AddToggle(UIManager.referenceManager.prefab_toggle);
                        sneakToggle.SetDisplayName("Toggle Sneak"); //TODO(Acreal): localize this string
                        sneakToggle.isToggledOn = DaggerfallUnity.Settings.ToggleSneak;
                        sneakToggle.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                        {
                            saveSettings = true;
                            UIUtilityFunctions.PlayButtonClick();
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
                            UIKeyCodeBindingEntry keyCodeBindingEntry = (UIKeyCodeBindingEntry)movementGroup.AddBindingEntry(UIManager.referenceManager.prefab_keyCodeBindEntry, actionStr);
                            keyCodeBindingEntry.Initialize();
                            keyCodeBindingEntry.SetActionEnum(moveActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                        }
                        #endregion

                        #region Combat Control Settings
                        UIScrollListGroup combatGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("Combat"); //TODO(Acreal): localize this string

                        UISlider slider_weaponSwingMode = combatGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        slider_weaponSwingMode.Initialize();
                        slider_weaponSwingMode.SetTextTitle(TextManager.Instance.GetText("MainMenu", "weaponSwingMode"));
                        slider_weaponSwingMode.SetSliderMinMax(0, _localizedStrings_weaponSwingModes.Length - 1, true);
                        slider_weaponSwingMode.SetSliderValue(DaggerfallUnity.Settings.WeaponSwingMode, true);
                        slider_weaponSwingMode.SetTextValue(_localizedStrings_weaponSwingModes[DaggerfallUnity.Settings.WeaponSwingMode]);

                        slider_weaponSwingMode.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
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
                            UIKeyCodeBindingEntry keyCodeBindingEntry = (UIKeyCodeBindingEntry)combatGroup.AddBindingEntry(UIManager.referenceManager.prefab_keyCodeBindEntry, actionStr);
                            keyCodeBindingEntry.Initialize();
                            keyCodeBindingEntry.SetActionEnum(combatActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                        }
                        #endregion

                        #region Interaction Control Settings
                        UIScrollListGroup interactGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("Interaction"); //TODO(Acreal): localize this string

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
                            UIKeyCodeBindingEntry keyCodeBindingEntry = (UIKeyCodeBindingEntry)interactGroup.AddBindingEntry(UIManager.referenceManager.prefab_keyCodeBindEntry, actionStr);
                            keyCodeBindingEntry.Initialize();
                            keyCodeBindingEntry.SetActionEnum(interactActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));

                            keyCodeBindingEntry.Event_OnRebind  += Controls_OnRebind;
                        }
                        #endregion

                        #region Interface Control Settings
                        UIScrollListGroup interfaceGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("Interface"); //TODO(Acreal): localize this string

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
                            UIKeyCodeBindingEntry keyCodeBindingEntry = (UIKeyCodeBindingEntry)interfaceGroup.AddBindingEntry(UIManager.referenceManager.prefab_keyCodeBindEntry, actionStr);
                            keyCodeBindingEntry.Initialize();
                            keyCodeBindingEntry.SetActionEnum(interfaceActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                        }
                        #endregion

                        #region System Control Settings
                        UIScrollListGroup systemGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("System"); //TODO(Acreal): localize this string

                        List<InputManager.Actions> systemActions = new List<InputManager.Actions>()
                        {
                            InputManager.Actions.Escape,
                            InputManager.Actions.ToggleConsole,
                            InputManager.Actions.QuickSave,
                            InputManager.Actions.QuickLoad,
                            InputManager.Actions.PrintScreen,
                        };

                        for (int i = 0; i < systemActions.Count; i++)
                        {
                            string actionStr = systemActions[i].ToString();
                            UIKeyCodeBindingEntry keyCodeBindingEntry = (UIKeyCodeBindingEntry)systemGroup.AddBindingEntry(UIManager.referenceManager.prefab_keyCodeBindEntry, actionStr);
                            keyCodeBindingEntry.Initialize();
                            keyCodeBindingEntry.SetActionEnum(systemActions[i]);
                            keyCodeBindingEntry.SetActionEnumString(actionStr);
                            keyCodeBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            keyCodeBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            keyCodeBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));

                            keyCodeBindingEntry.Event_OnRebind += Controls_OnRebind;
                        }
                        #endregion

                        #region Axis Control Settings
                        UIScrollListGroup axisGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("Axis Bindings"); //TODO(Acreal): localize this string

                        List<InputManager.AxisActions> axisActions = new List<InputManager.AxisActions>()
                        {
                            InputManager.AxisActions.MovementHorizontal,
                            InputManager.AxisActions.MovementVertical,
                            InputManager.AxisActions.CameraHorizontal,
                            InputManager.AxisActions.CameraVertical,
                        };

                        for (int i = 0; i < axisActions.Count; i++)
                        {
                            string actionStr = axisActions[i].ToString();
                            UIAxisBindingEntry axisBindingEntry = (UIAxisBindingEntry)axisGroup.AddBindingEntry(UIManager.referenceManager.prefab_axisBindEntry, actionStr);
                            axisBindingEntry.Initialize();
                            axisBindingEntry.SetActionEnum(axisActions[i]);
                            axisBindingEntry.SetActionEnumString(actionStr);
                            axisBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            axisBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            axisBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));

                            axisBindingEntry.Event_OnRebind += Controls_OnRebind;

                            axisBindingEntry.Event_OnInvertChanged += (InputManager.AxisActions axisAction, bool invert) =>
                            {
                                UIUtilityFunctions.PlayButtonClick();
                                InputManager.Instance.SetAxisActionInversion(axisAction, invert);
                            };
                        }
                        #endregion

                        #region Joystick Control Settings
                        UIScrollListGroup joystickGroup = _pauseWindowInstance.panelControlSettings.GetScrollListGroup("Joystick Controls"); //TODO(Acreal): localize this string

                        UISlider slider_lookSensitivity = joystickGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        slider_lookSensitivity.Initialize();
                        slider_lookSensitivity.SetTextTitle("Look Sensitivity"); //TODO(Acreal): localize this string
                        slider_lookSensitivity.SetSliderMinMax(0f, 1f, false);
                        slider_lookSensitivity.SetSliderValue(DaggerfallUnity.Settings.JoystickLookSensitivity, true);
                        slider_lookSensitivity.SetTextValue(DaggerfallUnity.Settings.JoystickLookSensitivity.ToString("F2"));
                        slider_lookSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            float val = slider.GetSliderValue();
                            DaggerfallUnity.Settings.JoystickLookSensitivity = val;
                            slider.SetTextValue(val.ToString("F2"));
                        };

                        UISlider slider_uiMouseSensitivity = joystickGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        slider_uiMouseSensitivity.Initialize();
                        slider_uiMouseSensitivity.SetTextTitle("UI Mouse Sensitivity"); //TODO(Acreal): localize this string
                        slider_uiMouseSensitivity.SetSliderMinMax(0f, 1f, false);
                        slider_uiMouseSensitivity.SetSliderValue(DaggerfallUnity.Settings.MouseLookSensitivity, true);
                        slider_uiMouseSensitivity.SetTextValue(DaggerfallUnity.Settings.MouseLookSensitivity.ToString("F2"));
                        slider_uiMouseSensitivity.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            float val = slider.GetSliderValue();
                            DaggerfallUnity.Settings.MouseLookSensitivity = val;
                            slider.SetTextValue(val.ToString("F2"));
                        };

                        UISlider slider_uiMouseSmoothing = joystickGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        slider_uiMouseSmoothing.Initialize();
                        slider_uiMouseSmoothing.SetTextTitle("Mouse Smoothing Factor"); //TODO(Acreal): localize this string
                        slider_uiMouseSmoothing.SetSliderMinMax(0f, 0.9f, false);
                        slider_uiMouseSmoothing.SetSliderValue(DaggerfallUnity.Settings.MouseLookSmoothingFactor, true);
                        slider_uiMouseSmoothing.SetTextValue(DaggerfallUnity.Settings.MouseLookSmoothingFactor.ToString("F2"));
                        slider_uiMouseSmoothing.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            float val = slider.GetSliderValue();
                            DaggerfallUnity.Settings.MouseLookSmoothingFactor = val;
                            slider.SetTextValue(val.ToString("F2"));
                        };

                        UISlider slider_maxMovementThreshold = joystickGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        slider_maxMovementThreshold.Initialize();
                        slider_maxMovementThreshold.SetTextTitle("Maximum Movement Threshold"); //TODO(Acreal): localize this string
                        slider_maxMovementThreshold.SetSliderMinMax(0f, 1f, false);
                        slider_maxMovementThreshold.SetSliderValue(DaggerfallUnity.Settings.JoystickMovementThreshold, true);
                        slider_maxMovementThreshold.SetTextValue(DaggerfallUnity.Settings.JoystickMovementThreshold.ToString("F2"));
                        slider_maxMovementThreshold.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            float val = slider.GetSliderValue();
                            DaggerfallUnity.Settings.JoystickMovementThreshold = val;
                            slider.SetTextValue(val.ToString("F2"));
                        };

                        UISlider slider_deadzone = joystickGroup.AddSlider(UIManager.referenceManager.prefab_slider);
                        slider_deadzone.Initialize();
                        slider_deadzone.SetTextTitle("Deadzone"); //TODO(Acreal): localize this string
                        slider_deadzone.SetSliderMinMax(0f, 1f, false);
                        slider_deadzone.SetSliderValue(DaggerfallUnity.Settings.JoystickDeadzone, true);
                        slider_deadzone.SetTextValue(DaggerfallUnity.Settings.JoystickDeadzone.ToString("F2"));
                        slider_deadzone.Event_OnSliderValueChanged += (UISlider slider) =>
                        {
                            UIUtilityFunctions.PlayButtonClick();
                            float val = slider.GetSliderValue();
                            DaggerfallUnity.Settings.JoystickDeadzone = val;
                            slider.SetTextValue(val.ToString("F2"));
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
                            UIJoystickKeyBindingEntry joystickBindingEntry = (UIJoystickKeyBindingEntry)joystickGroup.AddBindingEntry(UIManager.referenceManager.prefab_joystickBindEntry, actionStr);
                            joystickBindingEntry.Initialize();
                            joystickBindingEntry.SetActionEnum(joystickActions[i]);
                            joystickBindingEntry.SetActionEnumString(actionStr);  //TODO(Acreal): localize this string
                            joystickBindingEntry.SetDisplayName(Controls_GetControlBindName(actionStr));
                            joystickBindingEntry.SetPrimaryBindingValue(Controls_GetControlBindValue(actionStr, true));
                            joystickBindingEntry.SetSecondaryBindingValue(Controls_GetControlBindValue(actionStr, false));
                            joystickBindingEntry.Event_OnRebind += Controls_OnRebind;
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
            }

            HideWindow();
            ResetKeybindDict();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
        }

        protected override void Setup()
        {
            ParentPanel.BackgroundColor = ScreenDimColor;
        }

        public override void Update()
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

        public override void Draw()
        {
            //don't need this
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            if (_pauseWindowInstance != null)
            {
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
            UIUtilityFunctions.PlayButtonClick();

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

        private void Paused_OnDetailChanged(float detailVal)
        {
            saveSettings = true;
            UIUtilityFunctions.PlayButtonClick();
            QualitySettings.SetQualityLevel(DaggerfallUnity.Settings.QualityLevel = Mathf.FloorToInt(detailVal));
            GameManager.UpdateShadowDistance();
            GameManager.UpdateShadowResolution();
        }

        private void Paused_OnFullScreenChanged(bool enableFullscreen)
        {
            saveSettings = true;
            UIUtilityFunctions.PlayButtonClick();
            DaggerfallUnity.Settings.Fullscreen = !DaggerfallUnity.Settings.Fullscreen;
        }
        #endregion


        #region Controls Panel Callbacks/Delegates
        private void Controls_OnRebind(UIControlBindingEntry bindingEntry, bool isPrimaryBinding)
        {
            UIUtilityFunctions.PlayButtonClick();
            InputManager.Instance.StartCoroutine(WaitForKeyPress(bindingEntry, isPrimaryBinding));
        }

        private string Controls_GetControlBindName(string actionEnumAsString)
        {
            InputManager.Actions inputAction;
            InputManager.AxisActions axisInputAction;
            InputManager.JoystickUIActions joystickInputAction;
            if (_stringToActionEnumDict.TryGetValue(actionEnumAsString, out inputAction))
            {
                //TODO(Acreal): Get localized versions of these strings
                //just not sure where to find them yet...
                return actionEnumAsString;
            }
            else if (_stringToAxisActionEnumDict.TryGetValue(actionEnumAsString, out axisInputAction))
            {
                return TextManager.Instance.GetText("GameSettings", AxisActionToLocalizationKey(axisInputAction));
            }
            else if (_stringToJoystickActionEnumDict.TryGetValue(actionEnumAsString, out joystickInputAction))
            {
                return TextManager.Instance.GetText("GameSettings", JoystickActionToLocalizationKey(joystickInputAction));
            }
            else
            {
                return actionEnumAsString;
            }
        }

        private string Controls_GetControlBindValue(string actionEnumAsString, bool primaryBinding)
        {
            if(string.IsNullOrEmpty(actionEnumAsString))
            {
                return string.Empty;
            }

            InputManager.Actions inputAction;
            InputManager.AxisActions axisInputAction;
            InputManager.JoystickUIActions joystickInputAction;
            if (_stringToActionEnumDict.TryGetValue(actionEnumAsString, out inputAction))
            {
                KeyCode keyCode = InputManager.Instance.GetBinding(inputAction, primaryBinding);
                return ControlsConfigManager.Instance.GetButtonText(keyCode);
            }
            else if (_stringToAxisActionEnumDict.TryGetValue(actionEnumAsString, out axisInputAction))
            {
                return InputManager.Instance.GetAxisBinding(axisInputAction);
            }
            else if (_stringToJoystickActionEnumDict.TryGetValue(actionEnumAsString, out joystickInputAction))
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

            AllowCancel = duplicateSet.Count == 0;

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

        private IEnumerator<float> WaitForKeyPress(UIControlBindingEntry bindingEntry, bool isPrimaryBinding)
        {
            //TODO(Acreal): set this value based on the binding entry that was clicked
            //(primary and secondary bindings are displayed simultaneously in the ui)
            ControlsConfigManager.Instance.UsingPrimary = isPrimaryBinding;

            bool isAxisAction = bindingEntry is UIAxisBindingEntry;
            bool isJoystickAction = !isAxisAction && bindingEntry is UIJoystickKeyBindingEntry;

            KeyCode code = KeyCode.None;
            string currentLabel = isPrimaryBinding ? bindingEntry.primaryBindingString : bindingEntry.secondaryBindingString;

            if (isPrimaryBinding) { bindingEntry.SetPrimaryBindingValue(string.Empty); }
            else { bindingEntry.SetSecondaryBindingValue(string.Empty); }

            float t = 0.05f;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0f;
            }

            while ((!isAxisAction && (code = InputManager.Instance.GetAnyKeyDownIgnoreAxisBinds(true)) == KeyCode.None)
                || (isAxisAction && (code = InputManager.Instance.GetAnyKeyDown(true)) == KeyCode.None))
            {
                AllowCancel = false;
                yield return 0f;
            }

            AllowCancel = true;

            if (code != KeyCode.None)
            {
                if (InputManager.Instance.ReservedKeys.FirstOrDefault(x => x == code) == KeyCode.None)
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

                            //TODO(Acreal): add tooltips to keybindings
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

                            //TODO(Acreal): add tooltips to keybindings
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

                            //TODO(Acreal): add tooltips to keybindings
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
                    else { bindingEntry.SetPrimaryBindingValue(currentLabel); }
                }
            }
        }
        #endregion
    }
}