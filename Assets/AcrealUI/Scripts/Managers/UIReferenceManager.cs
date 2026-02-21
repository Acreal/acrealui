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

using DaggerfallConnect;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public class UIReferenceManager
    {
        #region Variables
        public UIButton prefab_button = null;
        public UISlider prefab_slider = null;
        public UIToggle prefab_toggle = null;
        public UIScrollListGroup prefab_scrollListGroup = null;
        public UIScrollListGroup prefab_subScrollListGroup = null;
        public UIScrollListRow prefab_scrollListRow = null;

        public UIPauseWindow prefab_pauseWindow = null;
        public UIWindowSaveLoad prefab_saveLoadWindow = null;

        public UIConfirmationWindow prefab_confirmationWindow = null;

        public UISaveGameEntry prefab_saveEntry = null;
       
        public UIToggle prefab_resolutionSettingEntry = null;

        public UIInventoryWindow prefab_inventoryWindow = null;
        public UIInventoryWindow_ItemEntry itemEntryPrefab = null;
        public UIPlayerStatEntry prefab_playerStatEntry = null;
        public UIPlayerSkillEntry prefab_playerSkillEntry = null;

        public Canvas prefab_tooltipCanvas = null;
        public UITextTooltip prefab_textTooltip = null;
        public UITextIconTooltip prefab_iconTextTooltip = null;
        public UIItemDetailsTooltip prefab_itemDetailsTooltip = null;
        public UIItemStatTooltipEntry prefab_tooltip_itemStatEntry = null;
        public UIItemStatSliderTooltipEntry prefab_tooltip_itemStatSliderEntry = null;
        public UIItemPowerTooltipEntry prefab_tooltip_itemPowerEntry = null;

        public UIKeyCodeBindingEntry prefab_keyCodeBindEntry = null;
        public UIAxisBindingEntry prefab_axisBindEntry = null;
        public UIJoystickKeyBindingEntry prefab_joystickBindEntry = null;

        public GameObject prefab_hud = null;

        private Dictionary<DFCareer.Stats, Sprite> _statToIconDict = null;
        private Dictionary<DFCareer.Skills, Sprite> _skillToIconDict = null;
        private Dictionary<SkillRank, Sprite> _skillRankToIconDict = null;
        private Dictionary<ItemType, Sprite> _itemTypeToIconDict = null;
        private Dictionary<ItemArchetype, Sprite> _itemArchetypeToIconDict = null;
        private Dictionary<KeyCode, Sprite> _keyCodeToIconDict = null;
        #endregion


        #region Initalization/Cleanup
        public void Initialize(Mod mod)
        {
            GameObject buttonObj = mod.GetAsset<GameObject>("Prefab_StandardElement_Button");
            prefab_button = buttonObj != null ? buttonObj.GetComponent<UIButton>() : null;

            GameObject sliderObj = mod.GetAsset<GameObject>("Prefab_StandardElement_Slider");
            prefab_slider = sliderObj != null ? sliderObj.GetComponent<UISlider>() : null;

            GameObject toggleObj = mod.GetAsset<GameObject>("Prefab_StandardElement_Toggle");
            prefab_toggle = toggleObj != null ? toggleObj.GetComponent<UIToggle>() : null;

            GameObject scrolLGroupObj = mod.GetAsset<GameObject>("Prefab_StandardElement_ScrollGroup");
            prefab_scrollListGroup = scrolLGroupObj != null ? scrolLGroupObj.GetComponent<UIScrollListGroup>() : null;

            GameObject subScrolLGroupObj = mod.GetAsset<GameObject>("Prefab_StandardElement_SubScrollGroup");
            prefab_subScrollListGroup = subScrolLGroupObj != null ? subScrolLGroupObj.GetComponent<UIScrollListGroup>() : null;

            GameObject scrollRowObj = mod.GetAsset<GameObject>("Prefab_StandardElement_ScrollListRow");
            prefab_scrollListRow = scrollRowObj != null ? scrollRowObj.GetComponent<UIScrollListRow>() : null;

            GameObject pauseWindowObj = mod.GetAsset<GameObject>("Prefab_PauseWindow");
            prefab_pauseWindow = pauseWindowObj != null ? pauseWindowObj.GetComponent<UIPauseWindow>() : null;

            GameObject saveWindowObj = mod.GetAsset<GameObject>("Prefab_SaveLoadGameWindow");
            prefab_saveLoadWindow = saveWindowObj != null ? saveWindowObj.GetComponent<UIWindowSaveLoad>() : null;

            GameObject saveEntryObj = mod.GetAsset<GameObject>("Prefab_SaveGameEntry");
            prefab_saveEntry = saveEntryObj != null ? saveEntryObj.GetComponent<UISaveGameEntry>() : null;

            GameObject keyCodeBindObj = mod.GetAsset<GameObject>("Prefab_KeyCodeControlBindingEntry");
            prefab_keyCodeBindEntry = keyCodeBindObj != null ? keyCodeBindObj.GetComponent<UIKeyCodeBindingEntry>() : null;

            GameObject axisBindObj = mod.GetAsset<GameObject>("Prefab_AxisControlBindingEntry");
            prefab_axisBindEntry = axisBindObj != null ? axisBindObj.GetComponent<UIAxisBindingEntry>() : null;

            GameObject joystickBindObj = mod.GetAsset<GameObject>("Prefab_JoystickControlBindingEntry");
            prefab_joystickBindEntry = joystickBindObj != null ? joystickBindObj.GetComponent<UIJoystickKeyBindingEntry>() : null;

            prefab_hud = mod.GetAsset<GameObject>("Prefab_HUD");

            GameObject inventoryWindowObj = mod.GetAsset<GameObject>("Prefab_InventoryWindow");
            prefab_inventoryWindow = inventoryWindowObj != null ? inventoryWindowObj.GetComponent<UIInventoryWindow>() : null;

            GameObject itemEntryObj = mod.GetAsset<GameObject>("Prefab_InventoryItemEntry");
            itemEntryPrefab = itemEntryObj != null ? itemEntryObj.GetComponent<UIInventoryWindow_ItemEntry>() : null;

            GameObject payerStatEntryObj = mod.GetAsset<GameObject>("Prefab_CharacterStatEntry");
            prefab_playerStatEntry = payerStatEntryObj != null ? payerStatEntryObj.GetComponent<UIPlayerStatEntry>() : null;

            GameObject skillEntryObj = mod.GetAsset<GameObject>("Prefab_CharacterSkillEntry");
            prefab_playerSkillEntry = skillEntryObj != null ? skillEntryObj.GetComponent<UIPlayerSkillEntry>() : null;

            GameObject tooltipCanvasObj = mod.GetAsset<GameObject>("Prefab_TooltipCanvas");
            prefab_tooltipCanvas = tooltipCanvasObj != null ? tooltipCanvasObj.GetComponent<Canvas>() : null;

            GameObject textTooltipObj = mod.GetAsset<GameObject>("Prefab_TextTooltip");
            prefab_textTooltip = textTooltipObj != null ? textTooltipObj.GetComponent<UITextTooltip>() : null;

            GameObject iconTextTooltipObj = mod.GetAsset<GameObject>("Prefab_TextIconTooltip");
            prefab_iconTextTooltip = iconTextTooltipObj != null ? iconTextTooltipObj.GetComponent<UITextIconTooltip>() : null;

            GameObject itemDetailsTooltipObj = mod.GetAsset<GameObject>("Prefab_ItemDetailsTooltip");
            prefab_itemDetailsTooltip = itemDetailsTooltipObj != null ? itemDetailsTooltipObj.GetComponent<UIItemDetailsTooltip>() : null;

            GameObject powerEntryTooltipObj = mod.GetAsset<GameObject>("Prefab_ItemPowerEntryTooltip");
            prefab_tooltip_itemPowerEntry = powerEntryTooltipObj != null ? powerEntryTooltipObj.GetComponent<UIItemPowerTooltipEntry>() : null;

            GameObject statEntryTooltipObj = mod.GetAsset<GameObject>("Prefab_ItemStatEntryTooltip");
            prefab_tooltip_itemStatEntry = statEntryTooltipObj != null ? statEntryTooltipObj.GetComponent<UIItemStatTooltipEntry>() : null;

            GameObject statSliderEntryTooltipObj = mod.GetAsset<GameObject>("Prefab_ItemStatSliderEntryTooltip");
            prefab_tooltip_itemStatSliderEntry = statSliderEntryTooltipObj != null ? statSliderEntryTooltipObj.GetComponent<UIItemStatSliderTooltipEntry>() : null;

            GameObject resolutionEntryObj = mod.GetAsset<GameObject>("Prefab_ResolutionSettingEntry");
            prefab_resolutionSettingEntry = resolutionEntryObj != null ? resolutionEntryObj.GetComponent<UIToggle>() : null;

            GameObject confirmationObj = mod.GetAsset<GameObject>("Prefab_ConfirmationWindow");
            prefab_confirmationWindow = confirmationObj != null ? confirmationObj.GetComponent<UIConfirmationWindow>() : null;

            _keyCodeToIconDict = new Dictionary<KeyCode, Sprite>()
            {
                { KeyCode.Mouse0, mod.GetAsset<Sprite>("Icon_Mouse_LeftClick") },
            };

            _statToIconDict = new Dictionary<DFCareer.Stats, Sprite>()
            {
                { DFCareer.Stats.Agility, mod.GetAsset<Sprite>("Icon_Stat_Agility") },
                { DFCareer.Stats.Endurance, mod.GetAsset<Sprite>("Icon_Stat_Endurance") },
                { DFCareer.Stats.Intelligence, mod.GetAsset<Sprite>("Icon_Stat_Intelligence") },
                { DFCareer.Stats.Luck, mod.GetAsset<Sprite>("Icon_Stat_Luck") },
                { DFCareer.Stats.Personality, mod.GetAsset<Sprite>("Icon_Stat_Personality") },
                { DFCareer.Stats.Speed, mod.GetAsset<Sprite>("Icon_Stat_Speed") },
                { DFCareer.Stats.Strength, mod.GetAsset<Sprite>("Icon_Stat_Strength") },
                { DFCareer.Stats.Willpower, mod.GetAsset<Sprite>("Icon_Stat_Willpower") },
            };

            _skillToIconDict = new Dictionary<DFCareer.Skills, Sprite>()
            {
                {DFCareer.Skills.Alteration, mod.GetAsset<Sprite>("Icon_Skill_Alteration") },
                {DFCareer.Skills.Archery, mod.GetAsset<Sprite>("Icon_Skill_Archery") },
                {DFCareer.Skills.Axe, mod.GetAsset<Sprite>("Icon_Skill_Axe") },
                {DFCareer.Skills.Backstabbing, mod.GetAsset<Sprite>("Icon_Skill_Backstabbing") },
                {DFCareer.Skills.BluntWeapon, mod.GetAsset<Sprite>("Icon_Skill_BluntWeapon") },
                {DFCareer.Skills.Centaurian, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Climbing, mod.GetAsset<Sprite>("Icon_Skill_Climbing") },
                {DFCareer.Skills.CriticalStrike, mod.GetAsset<Sprite>("Icon_Skill_CriticalStrike") },
                {DFCareer.Skills.Daedric, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Destruction, mod.GetAsset<Sprite>("Icon_Skill_Destruction") },
                {DFCareer.Skills.Dodging, mod.GetAsset<Sprite>("Icon_Skill_Dodging") },
                {DFCareer.Skills.Dragonish, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Etiquette, mod.GetAsset<Sprite>("Icon_Skill_Etiquette") },
                {DFCareer.Skills.Giantish, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.HandToHand, mod.GetAsset<Sprite>("Icon_Skill_HandToHand") },
                {DFCareer.Skills.Harpy, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Illusion, mod.GetAsset<Sprite>("Icon_Skill_Illusion") },
                {DFCareer.Skills.Impish, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Jumping, mod.GetAsset<Sprite>("Icon_Skill_Jumping") },
                {DFCareer.Skills.Lockpicking, mod.GetAsset<Sprite>("Icon_Skill_Lockpicking") },
                {DFCareer.Skills.LongBlade, mod.GetAsset<Sprite>("Icon_Skill_LongBlade") },
                {DFCareer.Skills.Medical, mod.GetAsset<Sprite>("Icon_Skill_Medical") },
                {DFCareer.Skills.Mercantile, mod.GetAsset<Sprite>("Icon_Skill_Mercantile") },
                {DFCareer.Skills.Mysticism, mod.GetAsset<Sprite>("Icon_Skill_Mysticism") },
                {DFCareer.Skills.Nymph, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Orcish, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Pickpocket, mod.GetAsset<Sprite>("Icon_Skill_Pickpocket") },
                {DFCareer.Skills.Restoration, mod.GetAsset<Sprite>("Icon_Skill_Restoration") },
                {DFCareer.Skills.Running, mod.GetAsset<Sprite>("Icon_Skill_Running") },
                {DFCareer.Skills.Spriggan, mod.GetAsset<Sprite>("Icon_Skill_Language") },
                {DFCareer.Skills.Streetwise, mod.GetAsset<Sprite>("Icon_Skill_Streetwise") },
                {DFCareer.Skills.Thaumaturgy, mod.GetAsset<Sprite>("Icon_Skill_Thaumaturgy") },
                {DFCareer.Skills.ShortBlade, mod.GetAsset<Sprite>("Icon_Skill_ShortBlade") },
                {DFCareer.Skills.Stealth, mod.GetAsset<Sprite>("Icon_Skill_Stealth") },
                {DFCareer.Skills.Swimming, mod.GetAsset<Sprite>("Icon_Skill_Swimming") },
            };

            _skillRankToIconDict = new Dictionary<SkillRank, Sprite>()
            {
                { SkillRank.Primary, mod.GetAsset<Sprite>("Icon_SkillRank_Primary") },
                { SkillRank.Major, mod.GetAsset<Sprite>("Icon_SkillRank_Major") },
                { SkillRank.Minor, mod.GetAsset<Sprite>("Icon_SkillRank_Minor") },
            };

            _itemTypeToIconDict = new Dictionary<ItemType, Sprite>()
            {
                { ItemType.Armor, mod.GetAsset<Sprite>("Icon_ItemType_Armor") },
                { ItemType.Readable, mod.GetAsset<Sprite>("Icon_ItemType_Book") },
                { ItemType.Clothing, mod.GetAsset<Sprite>("Icon_ItemType_Clothing") },
                { ItemType.Currency, mod.GetAsset<Sprite>("Icon_ItemType_Currency") },
                { ItemType.Food, mod.GetAsset<Sprite>("Icon_ItemType_Food") },
                { ItemType.Ingredient, mod.GetAsset<Sprite>("Icon_ItemType_Ingredient") },
                { ItemType.Jewellery, mod.GetAsset<Sprite>("Icon_ItemType_Jewellery") },
                { ItemType.Junk, mod.GetAsset<Sprite>("Icon_ItemType_Junk") },
                { ItemType.Potion, mod.GetAsset<Sprite>("Icon_ItemType_Potion") },
                { ItemType.QuestItem, mod.GetAsset<Sprite>("Icon_ItemType_QuestItem") },
                { ItemType.Unknown, mod.GetAsset<Sprite>("Icon_ItemType_Unknown") },
                { ItemType.Weapon, mod.GetAsset<Sprite>("Icon_ItemType_Weapon") },
            };

            _itemArchetypeToIconDict = new Dictionary<ItemArchetype, Sprite>()
            {
                { ItemArchetype.Amulet, mod.GetAsset<Sprite>("Icon_ItemArchetype_Amulet") },
                { ItemArchetype.Arrow, mod.GetAsset<Sprite>("Icon_ItemArchetype_Arrow") },
                { ItemArchetype.Axe, mod.GetAsset<Sprite>("Icon_ItemArchetype_Axe") },
                { ItemArchetype.Book, mod.GetAsset<Sprite>("Icon_ItemArchetype_Book") },
                { ItemArchetype.Boots, mod.GetAsset<Sprite>("Icon_ItemArchetype_Boots") },
                { ItemArchetype.Bow, mod.GetAsset<Sprite>("Icon_ItemArchetype_Bow") },
                { ItemArchetype.Bracelet, mod.GetAsset<Sprite>("Icon_ItemArchetype_Bracelet") },
                { ItemArchetype.Chestpiece, mod.GetAsset<Sprite>("Icon_ItemArchetype_Chestpiece") },
                { ItemArchetype.Cloak, mod.GetAsset<Sprite>("Icon_ItemArchetype_Cloak") },
                { ItemArchetype.ClothingChest, mod.GetAsset<Sprite>("Icon_ItemArchetype_ClothingChest") },
                { ItemArchetype.ClothingLegs, mod.GetAsset<Sprite>("Icon_ItemArchetype_ClothingLegs") },
                { ItemArchetype.CreatureIngredient, mod.GetAsset<Sprite>("Icon_ItemArchetype_CreatureIngredient") },
                { ItemArchetype.Flail, mod.GetAsset<Sprite>("Icon_ItemArchetype_Flail") },
                { ItemArchetype.Gauntlets, mod.GetAsset<Sprite>("Icon_ItemArchetype_Gauntlet") },
                { ItemArchetype.Gem, mod.GetAsset<Sprite>("Icon_ItemArchetype_Gem") },
                { ItemArchetype.Gold, mod.GetAsset<Sprite>("Icon_ItemArchetype_Gold") },
                { ItemArchetype.Greaves, mod.GetAsset<Sprite>("Icon_ItemArchetype_Greaves") },
                { ItemArchetype.Helm, mod.GetAsset<Sprite>("Icon_ItemArchetype_Helm") },
                { ItemArchetype.LightSource, mod.GetAsset<Sprite>("Icon_ItemArchetype_LightSource") },
                { ItemArchetype.LongBlade, mod.GetAsset<Sprite>("Icon_ItemArchetype_LongBlade") },
                { ItemArchetype.Mace, mod.GetAsset<Sprite>("Icon_ItemArchetype_Mace") },
                { ItemArchetype.Map, mod.GetAsset<Sprite>("Icon_ItemArchetype_Map") },
                { ItemArchetype.Mark, mod.GetAsset<Sprite>("Icon_ItemArchetype_Mark") },
                { ItemArchetype.MetalIngot, mod.GetAsset<Sprite>("Icon_ItemArchetype_MetalIngot") },
                { ItemArchetype.Note, mod.GetAsset<Sprite>("Icon_ItemArchetype_Note") },
                { ItemArchetype.Painting, mod.GetAsset<Sprite>("Icon_ItemArchetype_Painting") },
                { ItemArchetype.Pauldrons, mod.GetAsset<Sprite>("Icon_ItemArchetype_Pauldrons") },
                { ItemArchetype.PlantIngredient, mod.GetAsset<Sprite>("Icon_ItemArchetype_PlantIngredient") },
                { ItemArchetype.LiquidIngredient, mod.GetAsset<Sprite>("Icon_ItemArchetype_LiquidIngredient") },
                { ItemArchetype.Recipe, mod.GetAsset<Sprite>("Icon_ItemArchetype_Note") },
                { ItemArchetype.Religion, mod.GetAsset<Sprite>("Icon_ItemArchetype_Religion") },
                { ItemArchetype.Ring, mod.GetAsset<Sprite>("Icon_ItemArchetype_Ring") },
                { ItemArchetype.Shield, mod.GetAsset<Sprite>("Icon_ItemArchetype_Shield") },
                { ItemArchetype.ShortBlade, mod.GetAsset<Sprite>("Icon_ItemArchetype_ShortBlade") },
                { ItemArchetype.Spellbook, mod.GetAsset<Sprite>("Icon_ItemArchetype_Book") },
                { ItemArchetype.Staff, mod.GetAsset<Sprite>("Icon_ItemArchetype_Staff") },
                { ItemArchetype.ToothIngredient, mod.GetAsset<Sprite>("Icon_ItemArchetype_ToothIngredient") },
                { ItemArchetype.Warhammer, mod.GetAsset<Sprite>("Icon_ItemArchetype_Warhammer") },
                { ItemArchetype.Wrists, mod.GetAsset<Sprite>("Icon_ItemArchetype_Bracer") },
            };

            //we dont want to populate the actual ItemArchetype.Unknown field in
            //this dictionary because we sometimes use the ItemType as a fallback
            //if there is no ItemArchetype Sprite assigned
            for (int i = 1; i < (int)ItemArchetype._COUNT; i++)
            {
                if(!_itemArchetypeToIconDict.ContainsKey((ItemArchetype)i))
                {
                    _itemArchetypeToIconDict.Add((ItemArchetype)i, mod.GetAsset<Sprite>("Icon_ItemArchetype_Unknown"));
                }
            }
        }

        public void Shutdown()
        {
            _statToIconDict = null;
            _itemTypeToIconDict = null;
        }
        #endregion


        #region Public API
        public Sprite GetKeyCodeIcon(KeyCode keyCode)
        {
            Sprite sprite = null;
            if (_keyCodeToIconDict != null)
            {
                _keyCodeToIconDict.TryGetValue(keyCode, out sprite);
            }
            return sprite;
        }

        public Sprite GetItemTypeIcon(ItemType itemType)
        {
            Sprite sprite = null;
            if (_itemTypeToIconDict != null)
            {
                _itemTypeToIconDict.TryGetValue(itemType, out sprite);
            }
            return sprite;
        }

        public Sprite GetItemArchetypeIcon(ItemArchetype itemArchetype)
        {
            Sprite sprite = null;
            if (_itemArchetypeToIconDict != null)
            {
                _itemArchetypeToIconDict.TryGetValue(itemArchetype, out sprite);
            }
            return sprite;
        }

        public Sprite GetPrimaryStatIcon(DFCareer.Skills skill)
        {
            return GetStatIcon(DaggerfallSkills.GetPrimaryStat(skill));
        }

        public Sprite GetStatIcon(DFCareer.Stats stat)
        {
            Sprite icon = null;
            if (_statToIconDict != null)
            {
                _statToIconDict.TryGetValue(stat, out icon);
            }
            return icon;
        }

        public Sprite GetSkillIcon(DFCareer.Skills skill)
        {
            Sprite icon = null;
            if(_skillToIconDict != null)
            {
                _skillToIconDict.TryGetValue(skill, out icon);
            }
            return icon;
        }

        public Sprite GetSkillRankIcon(SkillRank skillRank)
        {
            Sprite icon = null;
            if (_skillRankToIconDict != null)
            {
                _skillRankToIconDict.TryGetValue(skillRank, out icon);
            }
            return icon;
        }
        #endregion
    }
}