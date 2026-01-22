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
using UnityEngine.UI;
using TMPro;

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;

namespace AcrealUI
{
    public class UIOptionsPanel_Interface : UIOptionsPanel
    {
        public override string panelTitle => "Interface";


        #region Definitions
        //these were copied directly from DaggerfallWorkshop.Game.UserInterfaceWindows.AdvancedSettingsWindow
        private enum InteractionModeIconModes { none, minimal, large, classic, colour, monochrome, classicXhair, colourXhair };
        private enum IconsPositioningSchemes { classic, medium, small, smalldeckleft, smalldeckright, smallvertleft, smallvertright, smallhorzbottom };
        #endregion


        #region Editor Variables
        [SerializeField] private UIToggle toggle_tooltips_off = null;
        [SerializeField] private UIToggle toggle_tooltips_on = null;
        [SerializeField] private UIToggle toggle_crosshair_off = null;
        [SerializeField] private UIToggle toggle_crosshair_on = null;
        [SerializeField] private UIToggle toggle_vitalsIndicators_on = null;
        [SerializeField] private UIToggle toggle_vitalsIndicators_off = null;
        [SerializeField] private UIToggle toggle_arrowCounter_on = null;
        [SerializeField] private UIToggle toggle_arrowCounter_off = null;
        [SerializeField] private UIToggle toggle_countdownQuestJournalClocks_off = null;
        [SerializeField] private UIToggle toggle_countdownQuestJournalClocks_on = null;
        [SerializeField] private UIToggle toggle_inventoryInfoPanel_off = null;
        [SerializeField] private UIToggle toggle_inventoryInfoPanel_on = null;
        [SerializeField] private UIToggle toggle_enhancedItemLists_off = null;
        [SerializeField] private UIToggle toggle_enhancedItemLists_on = null;
        [SerializeField] private UIToggle toggle_talkWindowModernStyle_off = null;
        [SerializeField] private UIToggle toggle_talkWindowModernStyle_on = null;
        [SerializeField] private UIToggle toggle_geographicBackgrounds_off = null;
        [SerializeField] private UIToggle toggle_geographicBackgrounds_on = null;
        [SerializeField] private UIToggle toggle_dungeonWagonAccessPrompt_off = null;
        [SerializeField] private UIToggle toggle_dungeonWagonAccessPrompt_on = null;
        [SerializeField] private UIToggle toggle_outlineRegionalMapLocations_off = null;
        [SerializeField] private UIToggle toggle_outlineRegionalMapLocations_on = null;
        [SerializeField] private UIArrowSelection arrowSelection_interactionModeIcon = null;
        [SerializeField] private UIArrowSelection arrowSelection_buffIconsLayout = null;
        [SerializeField] private UIArrowSelection arrowSelection_helmAndShieldMaterialDisplay = null;
        [SerializeField] private UIColorPicker colorPicker_textColor = null;
        [SerializeField] private UIColorPicker colorPicker_backgroundColor = null;
        [SerializeField] private Slider slider_tooltipDelay = null;
        [SerializeField] private TextMeshProUGUI text_tooltipDelayValue = null;
        #endregion


        #region Private Variables
        private string[] localizedInteractionModeIconValues = null;
        private string[] localizedBuffIconsLayoutValues = null;
        private string[] localizedHelmAndShieldMaterialDisplayValues = null;
        #endregion


        #region MonoBehaviour
        public override void Initialize()
        {
            base.Initialize();

            toggle_tooltips_on.Event_OnToggledOn += OnTooltipsToggledOn;
            toggle_tooltips_off.Event_OnToggledOn += OnTooltipsToggledOff;

            toggle_crosshair_on.Event_OnToggledOn += OnCrosshairToggleOn;
            toggle_crosshair_off.Event_OnToggledOn += OnCrosshairToggleOff;

            toggle_vitalsIndicators_on.Event_OnToggledOn += OnVitalsIndicatorsToggledOn;
            toggle_vitalsIndicators_off.Event_OnToggledOn += OnVitalsIndicatorsToggledOff;

            toggle_arrowCounter_on.Event_OnToggledOn += OnArrowCounterToggledOn;
            toggle_arrowCounter_off.Event_OnToggledOn += OnArrowCounterToggledOff;

            toggle_countdownQuestJournalClocks_on.Event_OnToggledOn += OnCountdownQuestJournalClocksToggledOff;
            toggle_countdownQuestJournalClocks_off.Event_OnToggledOn += OnCountdownQuestJournalClocksToggledOn;

            toggle_inventoryInfoPanel_on.Event_OnToggledOn += OnInventoryInfoPanelToggledOn;
            toggle_inventoryInfoPanel_off.Event_OnToggledOn += OnInventoryInfoPanelToggledOff;

            toggle_enhancedItemLists_on.Event_OnToggledOn += OnEnhancedItemListsToggledOn;
            toggle_enhancedItemLists_off.Event_OnToggledOn += OnEnhancedItemListsToggledOff;

            toggle_talkWindowModernStyle_on.Event_OnToggledOn += OnTalkWindowModernStyleToggleOn;
            toggle_talkWindowModernStyle_off.Event_OnToggledOn += OnTalkWindowModernStyleToggleOff;

            toggle_geographicBackgrounds_on.Event_OnToggledOn += OnGeographicBackgroundsToggledOn;
            toggle_geographicBackgrounds_off.Event_OnToggledOn += OnGeographicBackgroundsToggledOff;

            toggle_dungeonWagonAccessPrompt_on.Event_OnToggledOn += OnDungeonWagonAccessPromptToggledOn;
            toggle_dungeonWagonAccessPrompt_off.Event_OnToggledOn += OnDungeonWagonAccessPromptToggledOff;

            toggle_outlineRegionalMapLocations_on.Event_OnToggledOn += OnOutlineRegionalMapLocationsToggledOn;
            toggle_outlineRegionalMapLocations_off.Event_OnToggledOn += OnOutlineRegionalMapLocationsToggledOff;

            arrowSelection_interactionModeIcon.onSelectionChanged += OnInteractionModeIconChanged;
            arrowSelection_buffIconsLayout.onSelectionChanged += OnBuffIconsLayoutChanged;
            arrowSelection_helmAndShieldMaterialDisplay.onSelectionChanged += OnHelmAndShieldMaterialDisplayChanged;

            colorPicker_textColor.Initialize();
            colorPicker_textColor.onColorChanged += OnTextColorChanged;

            colorPicker_backgroundColor.Initialize();
            colorPicker_backgroundColor.onColorChanged += OnBackGroundColorChanged;

            slider_tooltipDelay.minValue = 0f;
            slider_tooltipDelay.maxValue = 10f;
            slider_tooltipDelay.onValueChanged.AddListener(OnTooltipDelayChanged);

            //DUNGEON TEXTURE SETTING
            localizedInteractionModeIconValues = TextManager.Instance.GetLocalizedTextList("interactionModeIconModes", TextCollections.TextSettings);
            if (localizedInteractionModeIconValues != null && localizedInteractionModeIconValues.Length > 0)
            {
                foreach (string texValue in localizedInteractionModeIconValues)
                {
                    if (!string.IsNullOrEmpty(texValue))
                    {
                        arrowSelection_interactionModeIcon.PushTextOption(texValue);
                    }
                }
                arrowSelection_interactionModeIcon.gameObject.SetActive(true);
            }
            else
            {
                arrowSelection_interactionModeIcon.gameObject.SetActive(false);
            }

            //CAMERA RECOIL SETTING
            localizedBuffIconsLayoutValues = TextManager.Instance.GetLocalizedTextList("iconsPositioningSchemes", TextCollections.TextSettings);
            if (localizedBuffIconsLayoutValues != null && localizedBuffIconsLayoutValues.Length > 0)
            {
                foreach (string texValue in localizedBuffIconsLayoutValues)
                {
                    if (!string.IsNullOrEmpty(texValue))
                    {
                        arrowSelection_buffIconsLayout.PushTextOption(texValue);
                    }
                }
                arrowSelection_buffIconsLayout.gameObject.SetActive(true);
            }
            else
            {
                arrowSelection_buffIconsLayout.gameObject.SetActive(false);
            }

            //HELM AND SHIELD MATERIAL DISPLAY SETTING
            localizedHelmAndShieldMaterialDisplayValues = TextManager.Instance.GetLocalizedTextList("helmAndShieldMaterialDisplay", TextCollections.TextSettings);
            if (localizedHelmAndShieldMaterialDisplayValues != null && localizedHelmAndShieldMaterialDisplayValues.Length > 0)
            {
                foreach (string textValue in localizedHelmAndShieldMaterialDisplayValues)
                {
                    if (!string.IsNullOrEmpty(textValue))
                    {
                        arrowSelection_helmAndShieldMaterialDisplay.PushTextOption(textValue);
                    }
                }
                arrowSelection_helmAndShieldMaterialDisplay.gameObject.SetActive(true);
            }
            else
            {
                arrowSelection_helmAndShieldMaterialDisplay.gameObject.SetActive(false);
            }
        }
        #endregion


        #region Show/Hide
        public override void Show()
        {
            toggle_tooltips_on.isToggledOn = (DaggerfallUnity.Settings.EnableToolTips);
            toggle_tooltips_off.isToggledOn = (!DaggerfallUnity.Settings.EnableToolTips);

            toggle_crosshair_on.isToggledOn = (DaggerfallUnity.Settings.Crosshair);
            toggle_crosshair_off.isToggledOn = (!DaggerfallUnity.Settings.Crosshair);

            toggle_vitalsIndicators_on.isToggledOn = (DaggerfallUnity.Settings.EnableVitalsIndicators);
            toggle_vitalsIndicators_off.isToggledOn = (!DaggerfallUnity.Settings.EnableVitalsIndicators);

            toggle_arrowCounter_on.isToggledOn = (DaggerfallUnity.Settings.EnableArrowCounter);
            toggle_arrowCounter_off.isToggledOn = (!DaggerfallUnity.Settings.EnableArrowCounter);

            toggle_countdownQuestJournalClocks_on.isToggledOn = (DaggerfallUnity.Settings.ShowQuestJournalClocksAsCountdown);
            toggle_countdownQuestJournalClocks_off.isToggledOn = (!DaggerfallUnity.Settings.ShowQuestJournalClocksAsCountdown);

            toggle_inventoryInfoPanel_on.isToggledOn = (DaggerfallUnity.Settings.EnableInventoryInfoPanel);
            toggle_inventoryInfoPanel_off.isToggledOn = (!DaggerfallUnity.Settings.EnableInventoryInfoPanel);

            toggle_enhancedItemLists_on.isToggledOn = (DaggerfallUnity.Settings.EnableEnhancedItemLists);
            toggle_enhancedItemLists_off.isToggledOn = (!DaggerfallUnity.Settings.EnableEnhancedItemLists);

            toggle_talkWindowModernStyle_on.isToggledOn = (DaggerfallUnity.Settings.EnableModernConversationStyleInTalkWindow);
            toggle_talkWindowModernStyle_off.isToggledOn = (!DaggerfallUnity.Settings.EnableModernConversationStyleInTalkWindow);

            toggle_geographicBackgrounds_on.isToggledOn = (DaggerfallUnity.Settings.EnableGeographicBackgrounds);
            toggle_geographicBackgrounds_off.isToggledOn = (!DaggerfallUnity.Settings.EnableGeographicBackgrounds);

            toggle_dungeonWagonAccessPrompt_on.isToggledOn = (DaggerfallUnity.Settings.DungeonExitWagonPrompt);
            toggle_dungeonWagonAccessPrompt_off.isToggledOn = (!DaggerfallUnity.Settings.DungeonExitWagonPrompt);

            toggle_outlineRegionalMapLocations_on.isToggledOn = (DaggerfallUnity.Settings.TravelMapLocationsOutline);
            toggle_outlineRegionalMapLocations_off.isToggledOn = (!DaggerfallUnity.Settings.TravelMapLocationsOutline);

            slider_tooltipDelay.SetValueWithoutNotify(DaggerfallUnity.Settings.ToolTipDelayInSeconds);
            text_tooltipDelayValue.text = DaggerfallUnity.Settings.ToolTipDelayInSeconds.ToString("N1");

            arrowSelection_helmAndShieldMaterialDisplay.SetIndexNoCallback(DaggerfallUnity.Settings.HelmAndShieldMaterialDisplay);

            #region Interaction Mode Icon
            int interactionModeIdx = 0;
            if (localizedInteractionModeIconValues != null && localizedInteractionModeIconValues.Length > 0)
            {
                for (int i = 0; i < localizedInteractionModeIconValues.Length; i++)
                {
                    if (((InteractionModeIconModes)i).ToString() == DaggerfallUnity.Settings.InteractionModeIcon)
                    {
                        interactionModeIdx = i;
                        break;
                    }
                }
            }
            arrowSelection_interactionModeIcon.SetIndexNoCallback(interactionModeIdx);
            #endregion

            #region Buff Icons Layout
            int buffIconsLayoutIdx = 0;
            if (localizedBuffIconsLayoutValues != null && localizedBuffIconsLayoutValues.Length > 0)
            {
                for (int i = 0; i < localizedBuffIconsLayoutValues.Length; i++)
                {
                    if (((IconsPositioningSchemes)i).ToString() == DaggerfallUnity.Settings.IconsPositioningScheme)
                    {
                        buffIconsLayoutIdx = i;
                        break;
                    }
                }
            }
            arrowSelection_buffIconsLayout.SetIndexNoCallback(buffIconsLayoutIdx);
            #endregion

            base.Show();
        }

        public override void Hide()
        {
            base.Hide();
        }
        #endregion


        #region Input Handling
        private void OnTooltipsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableToolTips = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnTooltipsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableToolTips = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCrosshairToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.Crosshair = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCrosshairToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.Crosshair = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnVitalsIndicatorsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableVitalsIndicators = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnVitalsIndicatorsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableVitalsIndicators = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnArrowCounterToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableArrowCounter = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnArrowCounterToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableArrowCounter = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCountdownQuestJournalClocksToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.ShowQuestJournalClocksAsCountdown = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCountdownQuestJournalClocksToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.ShowQuestJournalClocksAsCountdown = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInventoryInfoPanelToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableInventoryInfoPanel = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInventoryInfoPanelToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableInventoryInfoPanel = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnhancedItemListsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableEnhancedItemLists = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnhancedItemListsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableEnhancedItemLists = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnTalkWindowModernStyleToggleOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableModernConversationStyleInTalkWindow = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnTalkWindowModernStyleToggleOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableModernConversationStyleInTalkWindow = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnGeographicBackgroundsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableGeographicBackgrounds = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnGeographicBackgroundsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnableGeographicBackgrounds = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnDungeonWagonAccessPromptToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.DungeonExitWagonPrompt = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnDungeonWagonAccessPromptToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.DungeonExitWagonPrompt = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnOutlineRegionalMapLocationsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.TravelMapLocationsOutline = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnOutlineRegionalMapLocationsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.TravelMapLocationsOutline = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInteractionModeIconChanged(UIArrowSelection arrowSelection)
        {
            int idx = arrowSelection != null ? arrowSelection.index : 0;
            int max = localizedInteractionModeIconValues != null ? localizedInteractionModeIconValues.Length : 0;
            int val = Mathf.Clamp(idx, 0, max);
            DaggerfallUnity.Settings.InteractionModeIcon = ((InteractionModeIconModes)val).ToString();
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnBuffIconsLayoutChanged(UIArrowSelection arrowSelection)
        {
            int idx = arrowSelection != null ? arrowSelection.index : 0;
            int max = localizedBuffIconsLayoutValues != null ? localizedBuffIconsLayoutValues.Length : 0;
            int val = Mathf.Clamp(idx, 0, max);
            DaggerfallUnity.Settings.IconsPositioningScheme = ((IconsPositioningSchemes)val).ToString();
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnHelmAndShieldMaterialDisplayChanged(UIArrowSelection arrowSelection)
        {
            int idx = arrowSelection != null ? arrowSelection.index : 0;
            int max = localizedHelmAndShieldMaterialDisplayValues != null ? localizedHelmAndShieldMaterialDisplayValues.Length : 0;
            int val = Mathf.Clamp(idx, 0, max);
            DaggerfallUnity.Settings.HelmAndShieldMaterialDisplay = val;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnTextColorChanged(Color color)
        {
            DaggerfallUnity.Settings.ToolTipTextColor = color;
        }

        private void OnBackGroundColorChanged(Color color)
        {
            DaggerfallUnity.Settings.ToolTipBackgroundColor = color;
        }

        private void OnTooltipDelayChanged(float val)
        {
            DaggerfallUnity.Settings.ToolTipDelayInSeconds = val;
            text_tooltipDelayValue.text = DaggerfallUnity.Settings.ToolTipDelayInSeconds.ToString("N1");
        }
        #endregion
    }
}