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

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop;

namespace AcrealUI
{
    [System.Obsolete]
    public class UIVideoOptionsPanel : UIOptionsPanel
    {
        public override string panelTitle => "Video";


        #region Editor Variables
        [SerializeField] private TextMeshProUGUI text_textureArraysValue = null;
        [SerializeField] private UIToggle toggle_fullscreen_off = null;
        [SerializeField] private UIToggle toggle_fullscreen_on = null;
        [SerializeField] private UIToggle toggle_runInBackground_off = null;
        [SerializeField] private UIToggle toggle_runInBackground_on = null;
        [SerializeField] private UIToggle toggle_shadows_dungeonLights_off = null;
        [SerializeField] private UIToggle toggle_shadows_dungeonLights_on = null;
        [SerializeField] private UIToggle toggle_shadows_interiorLights_off = null;
        [SerializeField] private UIToggle toggle_shadows_interiorLights_on = null;
        [SerializeField] private UIToggle toggle_shadows_exteriorLights_off = null;
        [SerializeField] private UIToggle toggle_shadows_exteriorLights_on = null;
        [SerializeField] private UIToggle toggle_simplifyInteriorLighting_off = null;
        [SerializeField] private UIToggle toggle_simplifyInteriorLighting_on = null;
        //[SerializeField] private UIArrowSelection arrowSelection_quality = null;
        //[SerializeField] private UIArrowSelection arrowSelection_mainFilter = null;
        //[SerializeField] private UIArrowSelection arrowSelection_guiFilter = null;
        //[SerializeField] private UIArrowSelection arrowSelection_videoFilter = null;
        //[SerializeField] private UIArrowSelection arrowSelection_shadowResolution = null;
        [SerializeField] private Slider slider_resolution = null;
        [SerializeField] private TextMeshProUGUI text_resolutionValue = null;
        [SerializeField] private Slider slider_fov = null;
        [SerializeField] private TextMeshProUGUI text_fovValue = null;
        [SerializeField] private Slider slider_terrainDist = null;
        [SerializeField] private TextMeshProUGUI text_terrainDistValue = null;
        #endregion


        #region Private Variables
        private string[] localizedQualitySettingsValues = null;
        private string[] localizedResolutionModes = null;
        private string[] localizedFilterModes = null;
        private Resolution[] availableResolutions;
        private List<string> selectableResolutionOptions = null;
        private int selectedResolutionIdx = -1;
        #endregion


        #region MonoBehaviour
        public override void Initialize()
        {
            base.Initialize();

            Resolution screenRes = Screen.currentResolution;
            availableResolutions = DaggerfallUI.GetDistinctResolutions();
            selectableResolutionOptions = new List<string>();
            for (int i = 0; i < availableResolutions.Length; i++)
            {
                string res = string.Format("{0}x{1}", availableResolutions[i].width, availableResolutions[i].height);
                selectableResolutionOptions.Add(res);

                if (selectedResolutionIdx < 0 && availableResolutions[i].width == screenRes.width &&
                    availableResolutions[i].height == screenRes.height)
                {
                    selectedResolutionIdx = i;
                }
            }

            slider_resolution.minValue = 0;
            slider_resolution.maxValue = availableResolutions.Length-1;
            slider_resolution.wholeNumbers = true;
            slider_resolution.onValueChanged.AddListener(OnResolutionSliderChanged);

            slider_fov.minValue = 60;
            slider_fov.maxValue = 120;
            slider_fov.wholeNumbers = true;
            slider_fov.onValueChanged.AddListener(OnFovSliderChanged);

            slider_terrainDist.minValue = 1;
            slider_terrainDist.maxValue = 4;
            slider_terrainDist.wholeNumbers = true;
            slider_terrainDist.onValueChanged.AddListener(OnTerrainDistSliderChanged);

            toggle_fullscreen_off.Event_OnToggledOn += OnFullscreenToggledOff;
            toggle_fullscreen_on.Event_OnToggledOn += OnFullscreenToggledOn;

            toggle_runInBackground_off.Event_OnToggledOn += OnRunInBackgroundToggledOff;
            toggle_runInBackground_on.Event_OnToggledOn += OnRunInBackgroundToggledOn;

            toggle_shadows_dungeonLights_off.Event_OnToggledOn += OnDungeonLightShadowsToggledOff;
            toggle_shadows_dungeonLights_on.Event_OnToggledOn += OnDungeonLightShadowsToggledOn;

            toggle_shadows_interiorLights_off.Event_OnToggledOn += OnInteriorLightsToggledOff;
            toggle_shadows_interiorLights_on.Event_OnToggledOn += OnInteriorLightsToggledOn;

            toggle_shadows_exteriorLights_off.Event_OnToggledOn += OnExteriorLightShadowsToggledOff;
            toggle_shadows_exteriorLights_on.Event_OnToggledOn += OnExteriorLightShadowsToggledOn;

            toggle_simplifyInteriorLighting_off.Event_OnToggledOn += OnSimplifyInteriorLightingToggledOff;
            toggle_simplifyInteriorLighting_on.Event_OnToggledOn += OnSimplifyInteriorLightingToggledOn;

            //QUALITY SETTING
            //localizedQualitySettingsValues = TextManager.Instance.GetLocalizedTextList("qualitySettings", TextCollections.TextSettings);
            //if (localizedQualitySettingsValues != null && localizedQualitySettingsValues.Length > 0)
            //{
            //    foreach (string texValue in localizedQualitySettingsValues)
            //    {
            //        if (!string.IsNullOrEmpty(texValue))
            //        {
            //            arrowSelection_quality.PushTextOption(texValue);
            //        }
            //    }
            //    arrowSelection_quality.onSelectionChanged += OnQualityLevelChanged;
            //    arrowSelection_quality.gameObject.SetActive(true);
            //}
            //else
            //{
            //    arrowSelection_quality.gameObject.SetActive(false);
            //}

            //MAIN, GUI, AND VIDEO TEXTURE FILTERS
            //localizedFilterModes = TextManager.Instance.GetLocalizedTextList("filterModes", TextCollections.TextSettings);
            //if (localizedFilterModes != null && localizedFilterModes.Length > 0)
            //{
            //    foreach (string texValue in localizedFilterModes)
            //    {
            //        if (!string.IsNullOrEmpty(texValue))
            //        {
            //            arrowSelection_mainFilter.PushTextOption(texValue);
            //            arrowSelection_guiFilter.PushTextOption(texValue);
            //            arrowSelection_videoFilter.PushTextOption(texValue);
            //        }
            //    }

            //    arrowSelection_mainFilter.onSelectionChanged += OnMainFilterChanged;
            //    arrowSelection_guiFilter.onSelectionChanged += OnGUIFilterChanged;
            //    arrowSelection_videoFilter.onSelectionChanged += OnVideoFilterChanged;

            //    arrowSelection_mainFilter.gameObject.SetActive(true);
            //    arrowSelection_guiFilter.gameObject.SetActive(true);
            //    arrowSelection_videoFilter.gameObject.SetActive(true);
            //}
            //else
            //{
            //    arrowSelection_mainFilter.gameObject.SetActive(false);
            //    arrowSelection_guiFilter.gameObject.SetActive(false);
            //    arrowSelection_videoFilter.gameObject.SetActive(false);
            //}

            //SHADOW RESOLUTION SETTINGs
            //localizedResolutionModes = TextManager.Instance.GetLocalizedTextList("shadowResolutionModes", TextCollections.TextSettings);
            //if (localizedResolutionModes != null && localizedResolutionModes.Length > 0)
            //{
            //    foreach (string texValue in localizedResolutionModes)
            //    {
            //        if (!string.IsNullOrEmpty(texValue))
            //        {
            //            arrowSelection_shadowResolution.PushTextOption(texValue);
            //        }
            //    }

            //    arrowSelection_shadowResolution.onSelectionChanged += OnShadowResolutionChanged;
            //    arrowSelection_shadowResolution.gameObject.SetActive(true);
            //}
            //else
            //{
            //    arrowSelection_shadowResolution.gameObject.SetActive(false);
            //}

            //TEXTURE ARRAY SETTING
            string textureArrayLabel = TextManager.Instance.GetLocalizedText("textureArrayLabel", TextCollections.TextSettings);
            if (!SystemInfo.supports2DArrayTextures)
            {
                textureArrayLabel += TextManager.Instance.GetLocalizedText("unsupported", TextCollections.TextSettings);
            }
            else
            {
                textureArrayLabel += DaggerfallUnity.Settings.EnableTextureArrays ? TextManager.Instance.GetLocalizedText("enabled", TextCollections.TextSettings) : TextManager.Instance.GetLocalizedText("disabled", TextCollections.TextSettings);
            }
            text_textureArraysValue.text = textureArrayLabel;
        }
        #endregion


        #region Show/Hide
        public override void Show()
        {
            toggle_fullscreen_on.isToggledOn = (DaggerfallUnity.Settings.EnableToolTips);
            toggle_fullscreen_off.isToggledOn = (!DaggerfallUnity.Settings.EnableToolTips);

            toggle_runInBackground_on.isToggledOn = (DaggerfallUnity.Settings.RunInBackground);
            toggle_runInBackground_off.isToggledOn = (!DaggerfallUnity.Settings.RunInBackground);

            toggle_shadows_dungeonLights_on.isToggledOn = (DaggerfallUnity.Settings.DungeonLightShadows);
            toggle_shadows_dungeonLights_off.isToggledOn = (!DaggerfallUnity.Settings.DungeonLightShadows);

            toggle_shadows_interiorLights_on.isToggledOn = (DaggerfallUnity.Settings.InteriorLightShadows);
            toggle_shadows_interiorLights_off.isToggledOn = (!DaggerfallUnity.Settings.InteriorLightShadows);

            toggle_shadows_exteriorLights_on.isToggledOn = (DaggerfallUnity.Settings.ExteriorLightShadows);
            toggle_shadows_exteriorLights_off.isToggledOn = (!DaggerfallUnity.Settings.ExteriorLightShadows);

            toggle_simplifyInteriorLighting_on.isToggledOn = (DaggerfallUnity.Settings.AmbientLitInteriors);
            toggle_simplifyInteriorLighting_off.isToggledOn = (!DaggerfallUnity.Settings.AmbientLitInteriors);

            //arrowSelection_quality.SetIndexNoCallback(DaggerfallUnity.Settings.QualityLevel);
            //arrowSelection_mainFilter.SetIndexNoCallback(DaggerfallUnity.Settings.MainFilterMode);
            //arrowSelection_guiFilter.SetIndexNoCallback(DaggerfallUnity.Settings.GUIFilterMode);
            //arrowSelection_videoFilter.SetIndexNoCallback(DaggerfallUnity.Settings.VideoFilterMode);
            //arrowSelection_shadowResolution.SetIndexNoCallback(DaggerfallUnity.Settings.ShadowResolutionMode);

            if (selectedResolutionIdx < 0) { selectedResolutionIdx = availableResolutions.Length - 1; }
            slider_resolution.SetValueWithoutNotify(selectedResolutionIdx);
            text_resolutionValue.text = DaggerfallUnity.Settings.ResolutionWidth.ToString() + "x" + DaggerfallUnity.Settings.ResolutionHeight.ToString();

            slider_fov.SetValueWithoutNotify(DaggerfallUnity.Settings.FieldOfView);
            text_fovValue.text = DaggerfallUnity.Settings.FieldOfView.ToString("N0");

            slider_terrainDist.SetValueWithoutNotify(DaggerfallUnity.Settings.TerrainDistance);
            text_terrainDistValue.text = DaggerfallUnity.Settings.TerrainDistance.ToString("N0");

            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
        #endregion


        #region Input Handling
        private void OnFullscreenToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.Fullscreen = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnFullscreenToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.Fullscreen = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnRunInBackgroundToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.RunInBackground = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnRunInBackgroundToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.RunInBackground = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnDungeonLightShadowsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.DungeonLightShadows = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnDungeonLightShadowsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.DungeonLightShadows = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInteriorLightsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.InteriorLightShadows = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInteriorLightsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.InteriorLightShadows = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnExteriorLightShadowsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.ExteriorLightShadows = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnExteriorLightShadowsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.ExteriorLightShadows = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSimplifyInteriorLightingToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AmbientLitInteriors = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnSimplifyInteriorLightingToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AmbientLitInteriors = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        //private void OnQualityLevelChanged(UIArrowSelection arrowSelection)
        //{
        //    DaggerfallUnity.Settings.QualityLevel = arrowSelection.index;
        //    DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        //}

        //private void OnMainFilterChanged(UIArrowSelection arrowSelection)
        //{
        //    DaggerfallUnity.Settings.MainFilterMode = arrowSelection.index;
        //    DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        //}

        //private void OnGUIFilterChanged(UIArrowSelection arrowSelection)
        //{
        //    DaggerfallUnity.Settings.GUIFilterMode = arrowSelection.index;
        //    DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        //}

        //private void OnVideoFilterChanged(UIArrowSelection arrowSelection)
        //{
        //    DaggerfallUnity.Settings.VideoFilterMode = arrowSelection.index;
        //    DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        //}

        //private void OnShadowResolutionChanged(UIArrowSelection arrowSelection)
        //{
        //    DaggerfallUnity.Settings.ShadowResolutionMode = arrowSelection.index;
        //    DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        //}

        private void OnResolutionSliderChanged(float val)
        {
            selectedResolutionIdx = (int)val;
            DaggerfallUnity.Settings.ResolutionWidth = availableResolutions[selectedResolutionIdx].width;
            DaggerfallUnity.Settings.ResolutionHeight = availableResolutions[selectedResolutionIdx].height;
            text_resolutionValue.text = DaggerfallUnity.Settings.ResolutionWidth.ToString() + "x" + DaggerfallUnity.Settings.ResolutionHeight.ToString();
        }

        private void OnFovSliderChanged(float val)
        {
            int fov = (int)val;
            DaggerfallUnity.Settings.FieldOfView = fov;
            text_fovValue.text = fov.ToString("N0");
        }

        private void OnTerrainDistSliderChanged(float val)
        {
            int dist = (int)val;
            DaggerfallUnity.Settings.TerrainDistance = dist;
            text_terrainDistValue.text = dist.ToString("N0");
        }
        #endregion
    }
}