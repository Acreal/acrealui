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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    public class UIOptionsPanel_Gameplay : UIOptionsPanel
    {
        public override string panelTitle => "Gameplay";


        #region Editor Variables
        [SerializeField] private TextMeshProUGUI text_soundFont = null;
        [SerializeField] private UIToggle toggle_startInDungeon_off = null;
        [SerializeField] private UIToggle toggle_startInDungeon_on = null;
        [SerializeField] private UIToggle toggle_smallerDungeons_off = null;
        [SerializeField] private UIToggle toggle_smallerDungeons_on = null;
        [SerializeField] private UIToggle toggle_movementAcceleration_off = null;
        [SerializeField] private UIToggle toggle_movementAcceleration_on = null;
        [SerializeField] private UIToggle toggle_bowsDrawRelease_off = null;
        [SerializeField] private UIToggle toggle_bowsDrawRelease_on = null;
        [SerializeField] private UIToggle toggle_sneak_off = null;
        [SerializeField] private UIToggle toggle_sneak_on = null;
        [SerializeField] private UIToggle toggle_alternateMusic_off = null;
        [SerializeField] private UIToggle toggle_alternateMusic_on = null;
        [SerializeField] private UIToggle toggle_spellLighting_off = null;
        [SerializeField] private UIToggle toggle_spellLighting_on = null;
        [SerializeField] private UIToggle toggle_spellShadows_off = null;
        [SerializeField] private UIToggle toggle_spellShadows_on = null;
        [SerializeField] private UIArrowSelection arrowSelection_dungeonTextures = null;
        [SerializeField] private UIArrowSelection arrowSelection_cameraRecoilStrength = null;
        [SerializeField] private Slider slider_mouseSensitivity = null;
        [SerializeField] private TextMeshProUGUI text_mouseSensitivityValue = null;
        [SerializeField] private Slider slider_sfxVolume = null;
        [SerializeField] private TextMeshProUGUI text_sfxVolumeValue = null;
        [SerializeField] private Slider slider_musicVolume = null;
        [SerializeField] private TextMeshProUGUI text_musicVolumeValue = null;
        #endregion


        #region Private Variables
        private string[] localizedDungeonTextureValues = null;
        private string[] localizedCameraRecoilValues = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            toggle_startInDungeon_off.Event_OnToggledOn += OnStartInDungeonToggledOff;
            toggle_startInDungeon_on.Event_OnToggledOn += OnStartInDungeonToggledOn;

            toggle_smallerDungeons_off.Event_OnToggledOn += OnSmallerDungeonsToggleOff;
            toggle_smallerDungeons_on.Event_OnToggledOn += OnSmallerDungeonsToggleOn;

            toggle_movementAcceleration_off.Event_OnToggledOn += OnMovementAccelerationToggleOff;
            toggle_movementAcceleration_on.Event_OnToggledOn += OnMovementAccelerationToggleOn;

            toggle_bowsDrawRelease_off.Event_OnToggledOn += OnBowsDrawReleaseToggleOff;
            toggle_bowsDrawRelease_on.Event_OnToggledOn += OnBowsDrawReleaseToggleOn;

            toggle_sneak_off.Event_OnToggledOn += OnSneakToggleOff;
            toggle_sneak_on.Event_OnToggledOn += OnSneakToggleOn;

            toggle_alternateMusic_off.Event_OnToggledOn += OnAlternateMusicToggleOff;
            toggle_alternateMusic_on.Event_OnToggledOn += OnAlternateMusicToggleOn;

            toggle_spellLighting_off.Event_OnToggledOn += OnSpellLightingToggleOff;
            toggle_spellLighting_on.Event_OnToggledOn += OnSpellLightingToggleOn;

            toggle_spellShadows_off.Event_OnToggledOn += OnSpellShadowsToggleOff;
            toggle_spellShadows_on.Event_OnToggledOn += OnSpellShadowsToggleOn;

            arrowSelection_dungeonTextures.onSelectionChanged += OnDungeonTexturesValueChanged;
            arrowSelection_cameraRecoilStrength.onSelectionChanged += OnCameraRecoilStrengthValueChanged;

            slider_mouseSensitivity.minValue = 0.1f;
            slider_mouseSensitivity.maxValue = 16f;
            slider_mouseSensitivity.onValueChanged.AddListener(OnMouseSensitivityChanged);

            slider_sfxVolume.minValue = 0f;
            slider_sfxVolume.maxValue = 1f;
            slider_sfxVolume.onValueChanged.AddListener(OnSfxVolumeChanged);

            slider_musicVolume.minValue = 0f;
            slider_musicVolume.maxValue = 1f;
            slider_musicVolume.onValueChanged.AddListener(OnMusicVolumeChanged);

            //DUNGEON TEXTURE SETTING
            localizedDungeonTextureValues = TextManager.Instance.GetLocalizedTextList("dungeonTextureModes", TextCollections.TextSettings);
            if (localizedDungeonTextureValues != null && localizedDungeonTextureValues.Length > 0)
            {
                foreach (string texValue in localizedDungeonTextureValues)
                {
                    if (!string.IsNullOrEmpty(texValue))
                    {
                        arrowSelection_dungeonTextures.PushTextOption(texValue);
                    }
                }
                arrowSelection_dungeonTextures.gameObject.SetActive(true);
            }
            else
            {
                arrowSelection_dungeonTextures.gameObject.SetActive(false);
            }

            //CAMERA RECOIL SETTING
            localizedCameraRecoilValues = TextManager.Instance.GetLocalizedTextList("cameraRecoilStrengths", TextCollections.TextSettings);
            if (localizedCameraRecoilValues != null && localizedCameraRecoilValues.Length > 0)
            {
                foreach (string texValue in localizedCameraRecoilValues)
                {
                    if (!string.IsNullOrEmpty(texValue))
                    {
                        arrowSelection_cameraRecoilStrength.PushTextOption(texValue);
                    }
                }
                arrowSelection_cameraRecoilStrength.gameObject.SetActive(true);
            }
            else
            {
                arrowSelection_cameraRecoilStrength.gameObject.SetActive(false);
            }
        }
        #endregion


        #region Show/Hide
        public override void Show()
        {
            toggle_startInDungeon_on.isToggledOn = (DaggerfallUnity.Settings.StartInDungeon);
            toggle_startInDungeon_off.isToggledOn = (!DaggerfallUnity.Settings.StartInDungeon);

            toggle_smallerDungeons_on.isToggledOn = (DaggerfallUnity.Settings.SmallerDungeons);
            toggle_smallerDungeons_off.isToggledOn = (!DaggerfallUnity.Settings.SmallerDungeons);

            toggle_movementAcceleration_on.isToggledOn = (DaggerfallUnity.Settings.MovementAcceleration);
            toggle_movementAcceleration_off.isToggledOn = (!DaggerfallUnity.Settings.MovementAcceleration);

            toggle_bowsDrawRelease_on.isToggledOn = (DaggerfallUnity.Settings.BowDrawback);
            toggle_bowsDrawRelease_off.isToggledOn = (!DaggerfallUnity.Settings.BowDrawback);

            toggle_sneak_on.isToggledOn = (DaggerfallUnity.Settings.EnableSpellLighting);
            toggle_sneak_off.isToggledOn = (!DaggerfallUnity.Settings.EnableSpellLighting);

            toggle_alternateMusic_on.isToggledOn = (DaggerfallUnity.Settings.AlternateMusic);
            toggle_alternateMusic_off.isToggledOn = (!DaggerfallUnity.Settings.AlternateMusic);

            toggle_spellLighting_on.isToggledOn = (DaggerfallUnity.Settings.EnableSpellLighting);
            toggle_spellLighting_off.isToggledOn = (!DaggerfallUnity.Settings.EnableSpellLighting);

            toggle_spellShadows_on.isToggledOn = (DaggerfallUnity.Settings.EnableSpellShadows);
            toggle_spellShadows_off.isToggledOn = (!DaggerfallUnity.Settings.EnableSpellShadows);

            slider_mouseSensitivity.SetValueWithoutNotify(DaggerfallUnity.Settings.MouseLookSensitivity);
            text_mouseSensitivityValue.text = DaggerfallUnity.Settings.MouseLookSensitivity.ToString("N2");
            slider_sfxVolume.SetValueWithoutNotify(DaggerfallUnity.Settings.SoundVolume);
            text_sfxVolumeValue.text = (DaggerfallUnity.Settings.SoundVolume * 100f).ToString("N0");
            slider_musicVolume.SetValueWithoutNotify(DaggerfallUnity.Settings.MusicVolume);
            text_musicVolumeValue.text = (DaggerfallUnity.Settings.MusicVolume * 100f).ToString("N0");

            text_soundFont.text = !string.IsNullOrEmpty(DaggerfallUnity.Settings.SoundFont) ? DaggerfallUnity.Settings.SoundFont : "default";

            arrowSelection_dungeonTextures.SetIndexNoCallback(DaggerfallUnity.Settings.RandomDungeonTextures);
            arrowSelection_cameraRecoilStrength.SetIndexNoCallback(DaggerfallUnity.Settings.CameraRecoilStrength);

            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
        #endregion


        #region Input Handling
        private void OnStartInDungeonToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.StartInDungeon = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnStartInDungeonToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.StartInDungeon = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSmallerDungeonsToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.SmallerDungeons = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSmallerDungeonsToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.SmallerDungeons = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnMovementAccelerationToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.MovementAcceleration = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnMovementAccelerationToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.MovementAcceleration = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnBowsDrawReleaseToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.BowDrawback = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnBowsDrawReleaseToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.BowDrawback = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSneakToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.ToggleSneak = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSneakToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.ToggleSneak = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAlternateMusicToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AlternateMusic = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAlternateMusicToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AlternateMusic = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSpellLightingToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableSpellLighting = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSpellLightingToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableSpellLighting = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSpellShadowsToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableSpellShadows = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSpellShadowsToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableSpellShadows = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnDungeonTexturesValueChanged(UIArrowSelection arrowSelection)
        {
            int idx = arrowSelection != null ? arrowSelection.index : 0;
            int max = localizedDungeonTextureValues != null ? localizedDungeonTextureValues.Length : 0;
            int val = Mathf.Clamp(idx, 0, max);
            DaggerfallUnity.Settings.RandomDungeonTextures = val;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCameraRecoilStrengthValueChanged(UIArrowSelection arrowSelection)
        {
            int idx = arrowSelection != null ? arrowSelection.index : 0;
            int max = localizedCameraRecoilValues != null ? localizedCameraRecoilValues.Length : 0;
            int val = Mathf.Clamp(idx, 0, max);
            DaggerfallUnity.Settings.CameraRecoilStrength = val;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnMouseSensitivityChanged(float val)
        {
            DaggerfallUnity.Settings.MouseLookSensitivity = val;
            text_mouseSensitivityValue.text = DaggerfallUnity.Settings.MouseLookSensitivity.ToString("N2");
        }

        private void OnSfxVolumeChanged(float val)
        {
            DaggerfallUnity.Settings.SoundVolume = val;
            text_sfxVolumeValue.text = (val * 100f).ToString("N0");
        }

        private void OnMusicVolumeChanged(float val)
        {
            DaggerfallUnity.Settings.MusicVolume = val;
            text_musicVolumeValue.text = (val * 100f).ToString("N0");
        }
        #endregion
    }
}