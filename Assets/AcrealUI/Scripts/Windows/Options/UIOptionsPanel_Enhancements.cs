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
    public class UIOptionsPanel_Enhancements : UIOptionsPanel
    {
        public override string panelTitle => "Enhancements";


        #region Editor Variables
        [SerializeField] private UIToggle toggle_mods_off = null;
        [SerializeField] private UIToggle toggle_mods_on = null;
        [SerializeField] private UIToggle toggle_assetInjection_off = null;
        [SerializeField] private UIToggle toggle_assetInjection_on = null;
        [SerializeField] private UIToggle toggle_compressModdedTextures_off = null;
        [SerializeField] private UIToggle toggle_compressModdedTextures_on = null;
        [SerializeField] private UIToggle toggle_itemBasedPlayerTorch_off = null;
        [SerializeField] private UIToggle toggle_itemBasedPlayerTorch_on = null;
        [SerializeField] private UIToggle toggle_enableConsole_off = null;
        [SerializeField] private UIToggle toggle_enableConsole_on = null;
        [SerializeField] private UIToggle toggle_nearDeathWarning_off = null;
        [SerializeField] private UIToggle toggle_nearDeathWarning_on = null;
        [SerializeField] private UIToggle toggle_alternateRandomEnemySelection_off = null;
        [SerializeField] private UIToggle toggle_alternateRandomEnemySelection_on = null;
        [SerializeField] private UIToggle toggle_advancedClimbingSystem_off = null;
        [SerializeField] private UIToggle toggle_advancedClimbingSystem_on = null;
        [SerializeField] private UIToggle toggle_combatVocalizations_off = null;
        [SerializeField] private UIToggle toggle_combatVocalizations_on = null;
        [SerializeField] private UIToggle toggle_enemyInfighting_off = null;
        [SerializeField] private UIToggle toggle_enemyInfighting_on = null;
        [SerializeField] private UIToggle toggle_enhancedCombatAI_off = null;
        [SerializeField] private UIToggle toggle_enhancedCombatAI_on = null;
        [SerializeField] private UIToggle toggle_allowMagicItemRepairs_off = null;
        [SerializeField] private UIToggle toggle_allowMagicItemRepairs_on = null;
        [SerializeField] private UIToggle toggle_instantRepairs_off = null;
        [SerializeField] private UIToggle toggle_instantRepairs_on = null;
        [SerializeField] private UIToggle toggle_guildQuestPlayerSelection_off = null;
        [SerializeField] private UIToggle toggle_guildQuestPlayerSelection_on = null;
        [SerializeField] private UIToggle toggle_equipBowsInLeftHandOnly_off = null;
        [SerializeField] private UIToggle toggle_equipBowsInLeftHandOnly_on = null;
        [SerializeField] private Slider slider_dungeonAmbientLight = null;
        [SerializeField] private TextMeshProUGUI text_dungeonAmbientLightValue = null;
        [SerializeField] private Slider slider_nightAmbientLight = null;
        [SerializeField] private TextMeshProUGUI text_nightAmbientLightValue = null;
        [SerializeField] private Slider slider_playerTorchLight = null;
        [SerializeField] private TextMeshProUGUI text_playerTorchValue = null;
        [SerializeField] private Slider slider_maximumLoiterTime = null;
        [SerializeField] private TextMeshProUGUI text_maximumLoiterTimeValue = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            toggle_mods_off.Event_OnToggledOn += OnModsToggledOff;
            toggle_mods_on.Event_OnToggledOn += OnModsToggledOn;

            toggle_assetInjection_off.Event_OnToggledOn += OnAssetInjectionToggledOff;
            toggle_assetInjection_on.Event_OnToggledOn += OnAssetInjectionToggledOn;

            toggle_compressModdedTextures_off.Event_OnToggledOn += OnCompressModdedTexturesToggledOff;
            toggle_compressModdedTextures_on.Event_OnToggledOn += OnCompressModdedTexturesToggledOn;

            toggle_itemBasedPlayerTorch_off.Event_OnToggledOn += OnItemBasedPlayerTorchToggledOff;
            toggle_itemBasedPlayerTorch_on.Event_OnToggledOn += OnItemBasedPlayerTorchToggledOn;

            toggle_enableConsole_off.Event_OnToggledOn += OnEnableConsoleToggledOff;
            toggle_enableConsole_on.Event_OnToggledOn += OnEnableConsoleToggledOn;

            toggle_nearDeathWarning_off.Event_OnToggledOn += OnNearDeathWarningToggledOff;
            toggle_nearDeathWarning_on.Event_OnToggledOn += OnNearDeathWarningToggledOn;

            toggle_alternateRandomEnemySelection_off.Event_OnToggledOn += OnAlternateRandomEnemySelectionToggledOff;
            toggle_alternateRandomEnemySelection_on.Event_OnToggledOn += OnAlternateRandomEnemySelectionToggledOn;

            toggle_advancedClimbingSystem_off.Event_OnToggledOn += OnAdvancedClimbingSystemToggledOff;
            toggle_advancedClimbingSystem_on.Event_OnToggledOn += OnAdvancedClimbingSystemToggledOn;

            toggle_combatVocalizations_off.Event_OnToggledOn += OnCombatVocalizationsToggledOff;
            toggle_combatVocalizations_on.Event_OnToggledOn += OnCombatVocalizationsToggledOn;

            toggle_enemyInfighting_off.Event_OnToggledOn += OnEnemyInfightingToggledOff;
            toggle_enemyInfighting_on.Event_OnToggledOn += OnEnemyInfightingToggledOn;

            toggle_enhancedCombatAI_off.Event_OnToggledOn += OnEnhancedCombatAIToggledOff;
            toggle_enhancedCombatAI_on.Event_OnToggledOn += OnEnhancedCombatAIToggledOn;

            toggle_allowMagicItemRepairs_off.Event_OnToggledOn += OnAllowMagicItemRepairsToggledOff;
            toggle_allowMagicItemRepairs_on.Event_OnToggledOn += OnAllowMagicItemRepairsToggledOn;

            toggle_instantRepairs_off.Event_OnToggledOn += OnInstantRepairsToggledOff;
            toggle_instantRepairs_on.Event_OnToggledOn += OnInstantRepairsToggledOn;

            toggle_guildQuestPlayerSelection_off.Event_OnToggledOn += OnGuildQuestPlayerSelectionToggledOff;
            toggle_guildQuestPlayerSelection_on.Event_OnToggledOn += OnGuildQuestPlayerSelectionToggledOn;

            toggle_equipBowsInLeftHandOnly_off.Event_OnToggledOn += OnEquipBowsInLeftHandOnlyToggledOff;
            toggle_equipBowsInLeftHandOnly_on.Event_OnToggledOn += OnEquipBowsInLeftHandOnlyToggledOn;

            slider_dungeonAmbientLight.minValue = 0f;
            slider_dungeonAmbientLight.maxValue = 1f;
            slider_maximumLoiterTime.wholeNumbers = false;
            slider_dungeonAmbientLight.onValueChanged.AddListener(OnDungeonAmbientLightValueChanged);

            slider_nightAmbientLight.minValue = 0f;
            slider_nightAmbientLight.maxValue = 1f;
            slider_maximumLoiterTime.wholeNumbers = false;
            slider_nightAmbientLight.onValueChanged.AddListener(OnNightAmbientLightValueChanged);

            slider_playerTorchLight.minValue = 0f;
            slider_playerTorchLight.maxValue = 1f;
            slider_maximumLoiterTime.wholeNumbers = false;
            slider_playerTorchLight.onValueChanged.AddListener(OnPlayerTorchLightValueChanged);

            slider_maximumLoiterTime.minValue = 3;
            slider_maximumLoiterTime.maxValue = 12;
            slider_maximumLoiterTime.wholeNumbers = true;
            slider_maximumLoiterTime.onValueChanged.AddListener(OnMaximumLoiterTimeChanged);
        }
        #endregion


        #region Show/Hide
        public override void Show()
        {
            toggle_mods_off.isToggledOn = !DaggerfallUnity.Settings.LypyL_ModSystem;
            toggle_mods_on.isToggledOn = DaggerfallUnity.Settings.LypyL_ModSystem;

            toggle_assetInjection_off.isToggledOn = !DaggerfallUnity.Settings.AssetInjection;
            toggle_assetInjection_on.isToggledOn = DaggerfallUnity.Settings.AssetInjection;

            toggle_compressModdedTextures_off.isToggledOn = !DaggerfallUnity.Settings.CompressModdedTextures;
            toggle_compressModdedTextures_on.isToggledOn = DaggerfallUnity.Settings.CompressModdedTextures;

            toggle_itemBasedPlayerTorch_off.isToggledOn = !DaggerfallUnity.Settings.PlayerTorchFromItems;
            toggle_itemBasedPlayerTorch_on.isToggledOn = DaggerfallUnity.Settings.PlayerTorchFromItems;

            toggle_enableConsole_off.isToggledOn = !DaggerfallUnity.Settings.LypyL_GameConsole;
            toggle_enableConsole_on.isToggledOn = DaggerfallUnity.Settings.LypyL_GameConsole;

            toggle_nearDeathWarning_off.isToggledOn = !DaggerfallUnity.Settings.NearDeathWarning;
            toggle_nearDeathWarning_on.isToggledOn = DaggerfallUnity.Settings.NearDeathWarning;

            toggle_alternateRandomEnemySelection_off.isToggledOn = !DaggerfallUnity.Settings.AlternateRandomEnemySelection;
            toggle_alternateRandomEnemySelection_on.isToggledOn = DaggerfallUnity.Settings.AlternateRandomEnemySelection;

            toggle_advancedClimbingSystem_off.isToggledOn = !DaggerfallUnity.Settings.AdvancedClimbing;
            toggle_advancedClimbingSystem_on.isToggledOn = DaggerfallUnity.Settings.AdvancedClimbing;

            toggle_combatVocalizations_off.isToggledOn = !DaggerfallUnity.Settings.CombatVoices;
            toggle_combatVocalizations_on.isToggledOn = DaggerfallUnity.Settings.CombatVoices;

            toggle_enemyInfighting_off.isToggledOn = !DaggerfallUnity.Settings.EnemyInfighting;
            toggle_enemyInfighting_on.isToggledOn = DaggerfallUnity.Settings.EnemyInfighting;

            toggle_enhancedCombatAI_off.isToggledOn = !DaggerfallUnity.Settings.EnhancedCombatAI;
            toggle_enhancedCombatAI_on.isToggledOn = DaggerfallUnity.Settings.EnhancedCombatAI;

            toggle_allowMagicItemRepairs_off.isToggledOn = !DaggerfallUnity.Settings.AllowMagicRepairs;
            toggle_allowMagicItemRepairs_on.isToggledOn = DaggerfallUnity.Settings.AllowMagicRepairs;

            toggle_instantRepairs_off.isToggledOn = !DaggerfallUnity.Settings.InstantRepairs;
            toggle_instantRepairs_on.isToggledOn = DaggerfallUnity.Settings.InstantRepairs;

            toggle_guildQuestPlayerSelection_off.isToggledOn = !DaggerfallUnity.Settings.GuildQuestListBox;
            toggle_guildQuestPlayerSelection_on.isToggledOn = DaggerfallUnity.Settings.GuildQuestListBox;

            toggle_equipBowsInLeftHandOnly_off.isToggledOn = !DaggerfallUnity.Settings.BowLeftHandWithSwitching;
            toggle_equipBowsInLeftHandOnly_on.isToggledOn = DaggerfallUnity.Settings.BowLeftHandWithSwitching;

            slider_dungeonAmbientLight.value = DaggerfallUnity.Settings.DungeonAmbientLightScale;
            text_dungeonAmbientLightValue.text = DaggerfallUnity.Settings.DungeonAmbientLightScale.ToString("N1");

            slider_nightAmbientLight.value = DaggerfallUnity.Settings.NightAmbientLightScale;
            text_nightAmbientLightValue.text = DaggerfallUnity.Settings.NightAmbientLightScale.ToString("N1");

            slider_playerTorchLight.value = DaggerfallUnity.Settings.PlayerTorchLightScale;
            text_playerTorchValue.text = DaggerfallUnity.Settings.PlayerTorchLightScale.ToString("N1");

            slider_maximumLoiterTime.value = DaggerfallUnity.Settings.LoiterLimitInHours;
            text_maximumLoiterTimeValue.text = DaggerfallUnity.Settings.LoiterLimitInHours.ToString("N0");

            base.Show();
        }
        #endregion


        #region Input Handling
        private void OnModsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.LypyL_ModSystem = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnModsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.LypyL_ModSystem = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAssetInjectionToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AssetInjection = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAssetInjectionToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AssetInjection = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCompressModdedTexturesToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.CompressModdedTextures = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCompressModdedTexturesToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.CompressModdedTextures = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnItemBasedPlayerTorchToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.PlayerTorchFromItems = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnItemBasedPlayerTorchToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.PlayerTorchFromItems = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnableConsoleToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.LypyL_GameConsole = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnableConsoleToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.LypyL_GameConsole = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnNearDeathWarningToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.NearDeathWarning = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnNearDeathWarningToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.NearDeathWarning = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAlternateRandomEnemySelectionToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AlternateRandomEnemySelection = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAlternateRandomEnemySelectionToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AlternateRandomEnemySelection = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAdvancedClimbingSystemToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AdvancedClimbing = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAdvancedClimbingSystemToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AdvancedClimbing = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCombatVocalizationsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.CombatVoices = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnCombatVocalizationsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.CombatVoices |= true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnemyInfightingToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnemyInfighting = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnemyInfightingToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnemyInfighting = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnhancedCombatAIToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnhancedCombatAI = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEnhancedCombatAIToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.EnhancedCombatAI = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAllowMagicItemRepairsToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AllowMagicRepairs = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnAllowMagicItemRepairsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.AllowMagicRepairs = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInstantRepairsToggledOff(UIToggle toggle )
        {
            DaggerfallUnity.Settings.InstantRepairs = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnInstantRepairsToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.InstantRepairs = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnGuildQuestPlayerSelectionToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.GuildQuestListBox = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnGuildQuestPlayerSelectionToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.GuildQuestListBox = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEquipBowsInLeftHandOnlyToggledOff(UIToggle toggle)
        {
            DaggerfallUnity.Settings.BowLeftHandWithSwitching = false;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnEquipBowsInLeftHandOnlyToggledOn(UIToggle toggle)
        {
            DaggerfallUnity.Settings.BowLeftHandWithSwitching = true;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
        }

        private void OnDungeonAmbientLightValueChanged(float val)
        {
            DaggerfallUnity.Settings.DungeonAmbientLightScale = val;
            text_dungeonAmbientLightValue.text = DaggerfallUnity.Settings.DungeonAmbientLightScale.ToString("N1");
        }

        private void OnNightAmbientLightValueChanged(float val)
        {
            DaggerfallUnity.Settings.NightAmbientLightScale = val;
            text_nightAmbientLightValue.text = DaggerfallUnity.Settings.NightAmbientLightScale.ToString("N1");
        }

        private void OnPlayerTorchLightValueChanged(float val)
        {
            DaggerfallUnity.Settings.PlayerTorchLightScale = val;
            text_playerTorchValue.text = DaggerfallUnity.Settings.PlayerTorchLightScale.ToString("N1");
        }

        private void OnMaximumLoiterTimeChanged(float val)
        {
            DaggerfallUnity.Settings.LoiterLimitInHours = (int)val;
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            text_maximumLoiterTimeValue.text = DaggerfallUnity.Settings.LoiterLimitInHours.ToString("N0");
        }
        #endregion
    }
}