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
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace AcrealUI
{
    public class UIInventoryWindowController : DaggerfallInventoryWindow, IWindowController
    {
        #region Variables
        private const float _TOOLTIP_X_OFFSET = 20f;

        private UIInventoryWindow _inventoryWindowInstance = null;
        private List<MobileTypes> _enemyTypesForStatCalc = null;
        private MobileTypes _currentEnemyTypeForStatCalc = MobileTypes.None;
        #endregion


        #region Initialization
        public UIInventoryWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null)
                : base(uiManager, previous)
        {
            AllowCancel = false;

            //base class needs these references, but we don't use them
            localItemListScroller = new ItemListScroller(defaultToolTip);
            remoteItemListScroller = new ItemListScroller(defaultToolTip);
            wagonButton = new Button();

            if (_inventoryWindowInstance == null)
            {
                UIWindow window = UIManager.Instance.GetWindowInstance(UIWindowInstanceType.Inventory);
                if (window == null || !(window is UIInventoryWindow))
                {
                    Debug.LogError("UIManager.GetWindowInstance(UIWindowInstanceType.Inventory) returned " + (window == null ? " NULL!" : "a window of the wrong type! Expected type UIInventoryWindow, but got " + window.GetType().ToString() + "!"));
                    return;
                }

                _inventoryWindowInstance = window as UIInventoryWindow;
                _inventoryWindowInstance.Initialize();

                #region Event Listeners
                if (_inventoryWindowInstance.itemList_playerInventory != null)
                {
                    _inventoryWindowInstance.itemList_playerInventory.Event_OnItemFilterChanged += () =>
                    {
                        UpdatePlayerInventory(true);
                    };

                    _inventoryWindowInstance.itemList_playerInventory.Event_OnSortItemsColumnChanged += () =>
                    {
                        UpdatePlayerInventory();
                    };

                    _inventoryWindowInstance.itemList_playerInventory.Event_OnSortAscendingChanged += () =>
                    {
                        UpdatePlayerInventory();
                    };

                    _inventoryWindowInstance.itemList_playerInventory.Event_OnToggled_InventoryTab_Player += () =>
                    {
                        localItems = playerEntity.Items;
                        usingWagon = false;
                        UpdatePlayerInventory(true);
                        _inventoryWindowInstance.itemList_playerInventory.SetTotalWeightText(BuildPlayerWeightString());
                    };

                    _inventoryWindowInstance.itemList_playerInventory.Event_OnToggled_InventoryTab_Wagon += () =>
                    {
                        localItems = playerEntity.WagonItems;
                        usingWagon = true;
                        UpdatePlayerInventory(true);
                        _inventoryWindowInstance.itemList_playerInventory.SetTotalWeightText(BuildPlayerWeightString());
                    };

                    _inventoryWindowInstance.itemList_playerInventory.Event_OnGoldButtonClicked += () =>
                    {
                        // Show message box
                        const int goldToDropTextId = 25;
                        DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(uiManager, this);
                        mb.SetTextTokens(goldToDropTextId);
                        mb.TextPanelDistanceY = 0;
                        mb.InputDistanceX = 15;
                        mb.InputDistanceY = -6;
                        mb.TextBox.Numeric = true;
                        mb.TextBox.MaxCharacters = 8;
                        mb.TextBox.Text = "0";
                        mb.OnGotUserInput += DropGoldPopup_OnGotUserInput;
                        mb.Show();
                    };
                }

                if (_inventoryWindowInstance.itemList_container != null)
                {
                    _inventoryWindowInstance.itemList_container.Event_OnItemFilterChanged += () =>
                    {
                        UpdateContainerInventory(true);
                    };

                    _inventoryWindowInstance.itemList_container.Event_OnSortItemsColumnChanged += () =>
                    {
                        UpdateContainerInventory();
                    };

                    _inventoryWindowInstance.itemList_container.Event_OnSortAscendingChanged += () =>
                    {
                        UpdateContainerInventory();
                    };

                    _inventoryWindowInstance.itemList_container.Event_OnToggled_InventoryTab_Wagon += () =>
                    {
                        lastRemoteItems = remoteItems;
                        lastRemoteTargetType = remoteTargetType;
                        remoteItems = PlayerEntity.WagonItems;
                        remoteTargetType = RemoteTargetTypes.Wagon;
                    };
                }
                #endregion

                #region Stats
                AddStatsToStatWindow(UITextStrings.InventoryWindow_Label_PrimaryStats.GetText(),
                                    DFCareer.Stats.Agility,
                                    DFCareer.Stats.Endurance,
                                    DFCareer.Stats.Intelligence,
                                    DFCareer.Stats.Luck,
                                    DFCareer.Stats.Personality,
                                    DFCareer.Stats.Speed,
                                    DFCareer.Stats.Strength,
                                    DFCareer.Stats.Willpower);
                #endregion

                #region Equipment Stats
                _inventoryWindowInstance.panel_playerStats.Event_OnEnemyTypeSelected += (int enemyTypeIndex) =>
                {
                    if (_currentEnemyTypeForStatCalc != _enemyTypesForStatCalc[enemyTypeIndex])
                    {
                        _currentEnemyTypeForStatCalc = _enemyTypesForStatCalc[enemyTypeIndex];
                        UpdateEquipmentStats();
                    }
                };
                #endregion

                #region Skills
                playerEntity = GameManager.Instance.PlayerEntity;
                List<DFCareer.Skills> primarySkills = playerEntity.GetPrimarySkills();

                AddSkillsToStatWindow(UITextStrings.InventoryWindow_Label_CombatSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Archery,
                                        DFCareer.Skills.Axe,
                                        DFCareer.Skills.Backstabbing,
                                        DFCareer.Skills.BluntWeapon,
                                        DFCareer.Skills.CriticalStrike,
                                        DFCareer.Skills.Dodging,
                                        DFCareer.Skills.HandToHand,
                                        DFCareer.Skills.LongBlade,
                                        DFCareer.Skills.Medical,
                                        DFCareer.Skills.ShortBlade);

                AddSkillsToStatWindow(UITextStrings.InventoryWindow_Label_MagicSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Alteration,
                                        DFCareer.Skills.Destruction,
                                        DFCareer.Skills.Illusion,
                                        DFCareer.Skills.Mysticism,
                                        DFCareer.Skills.Restoration,
                                        DFCareer.Skills.Thaumaturgy);

                AddSkillsToStatWindow(UITextStrings.InventoryWindow_Label_StealthSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Lockpicking,
                                        DFCareer.Skills.Pickpocket,
                                        DFCareer.Skills.Stealth);

                AddSkillsToStatWindow(UITextStrings.InventoryWindow_Label_SocialSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Etiquette,
                                        DFCareer.Skills.Mercantile,
                                        DFCareer.Skills.Streetwise);

                AddSkillsToStatWindow(UITextStrings.InventoryWindow_Label_MovementSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Climbing,
                                        DFCareer.Skills.Jumping,
                                        DFCareer.Skills.Running,
                                        DFCareer.Skills.Swimming);

                AddSkillsToStatWindow(UITextStrings.InventoryWindow_Label_LanguageSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Orcish,
                                        DFCareer.Skills.Harpy,
                                        DFCareer.Skills.Giantish,
                                        DFCareer.Skills.Dragonish,
                                        DFCareer.Skills.Nymph,
                                        DFCareer.Skills.Daedric,
                                        DFCareer.Skills.Spriggan,
                                        DFCareer.Skills.Centaurian,
                                        DFCareer.Skills.Impish);
                #endregion
            }
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            _inventoryWindowInstance?.Show();
        }

        public void HideWindow()
        {
            _inventoryWindowInstance?.Hide();
        }

        public override void OnPop()
        {
            base.OnPop();
            HideWindow();
            _inventoryWindowInstance?.ResetWindow();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
            UIManager.tooltipManager.HideActiveTooltip();
        }
        #endregion


        #region Overridden Base Class Functions
        public override void OnPush()
        {
            base.OnPush();

            //workaround for base class setting the remoteItems to
            //the wagon's items (we use localItems for that)
            if (usingWagon)
            {
                remoteItems = lastRemoteItems;
                usingWagon = false;
            }

            if (_inventoryWindowInstance != null)
            {
                #region Stats Panel
                UpdateEnemyTypes();
                UpdatePlayerInfo();
                UpdateVitalStats();
                UpdatePaperDoll();
                UpdateEquipmentStats();
                UpdatePrimaryStatsAndSkills();
                #endregion

                #region Player Inventory
                if (_inventoryWindowInstance.itemList_playerInventory != null)
                {
                    _inventoryWindowInstance.itemList_playerInventory.SetTotalGoldText(playerEntity.GoldPieces.ToString("N0"));
                    _inventoryWindowInstance.itemList_playerInventory.SetTotalWeightText(BuildPlayerWeightString());

                    bool hasWagon = UIUtilityFunctions.PlayerHasWagonAccess();
                    _inventoryWindowInstance.itemList_playerInventory.EnableOrDisableTabs(hasWagon, hasWagon, false, false);
                    _inventoryWindowInstance.itemList_playerInventory.SetActiveFilter(ItemFilter.All);
                    UpdatePlayerFilterToggles();
                    UpdatePlayerInventory();
                }
                #endregion

                #region Container Inventory
                if (_inventoryWindowInstance.itemList_container != null)
                {
                    _inventoryWindowInstance.itemList_container.SetTotalGoldText(null);
                    _inventoryWindowInstance.itemList_container.SetTotalWeightText(null);
                    _inventoryWindowInstance.itemList_container.EnableOrDisableTabs(false, false, false, LootTarget != null || (droppedItems != null && droppedItems.Count > 0));
                    _inventoryWindowInstance.itemList_container.SetActiveFilter(ItemFilter.All);

                    if (lootTarget != null)
                    {
                        _inventoryWindowInstance.itemList_container.SetLootPileIcon(UIUtilityFunctions.GetLootContainerIcon(lootTarget));
                    }

                    UpdateContainerFilterToggles();
                    UpdateContainerInventory();
                }
                #endregion

                _inventoryWindowInstance.Show();
            }
        }

        protected override void Setup()
        {
            IsSetup = true;
            ParentPanel.BackgroundColor =  Color.clear;
        }

        //OBSOLETE FUNCTIONS:
        public override void Draw() { }
        public override void Refresh(bool refreshPaperDoll = true) { }
        protected override void SetRemoteItemsAnimation() { }
        protected override void SelectTabPage(TabPages tabPage) { }
        protected override void SelectActionMode(ActionModes mode) { }
        protected override void UpdateLocalTargetIcon() { }
        protected override void UpdateRemoteTargetIcon() { }
        protected override void FilterLocalItems() { }
        protected override void FilterRemoteItems() { }
        #endregion


        #region Player Info (Name, Race, etc...)
        protected void UpdatePlayerInfo()
        {
            if (_inventoryWindowInstance != null && _inventoryWindowInstance.panel_playerStats != null)
            {
                //player info
                _inventoryWindowInstance.panel_playerStats.SetPlayerName(playerEntity.Name);
                _inventoryWindowInstance.panel_playerStats.SetPlayerRace(playerEntity.RaceTemplate.Name);
                _inventoryWindowInstance.panel_playerStats.SetPlayerClass(playerEntity.Career.Name);
                _inventoryWindowInstance.panel_playerStats.SetPlayerLevel(playerEntity.Level.ToString("N0"));

                //exp
                _inventoryWindowInstance.panel_playerStats.SetExperiencePercent(UIUtilityFunctions.GetPlayerExperiencePercent(playerEntity));
            }
        }
        #endregion


        #region Enemy Info
        private void UpdateEnemyTypes()
        {
            _enemyTypesForStatCalc = UIUtilityFunctions.GetPossibleEnemyTypes();
            _enemyTypesForStatCalc.Sort((x, y) => { return x.ToString().CompareTo(y.ToString()); });

            List<string> enemyTypeStrings = new List<string>(_enemyTypesForStatCalc.Count);
            for (int i = 0; i < _enemyTypesForStatCalc.Count; i++)
            {
                enemyTypeStrings.Add(UIUtilityFunctions.SplitStringIntoWords(_enemyTypesForStatCalc[i].ToString()));
            }

            _enemyTypesForStatCalc.Insert(0, MobileTypes.None);
            enemyTypeStrings.Insert(0, UITextStrings.Global_Label_None.GetText());

            _currentEnemyTypeForStatCalc = MobileTypes.None;

            _inventoryWindowInstance.panel_playerStats.SetEnemyTypeOptions(enemyTypeStrings);
        }
        #endregion


        #region Stats
        protected void AddStatsToStatWindow(string scrollGroupName, params DFCareer.Stats[] stats)
        {
            List<UIStatData> statDataList = new List<UIStatData>();

            for (int i = 0; i < stats.Length; i++)
            {
                DFCareer.Stats stat = stats[i];

                UIStatData statData = new UIStatData
                {
                    statEnumAsInt = (int)stats[i],
                    name = DaggerfallUnity.Instance.TextProvider.GetStatName(stat),
                    icon = UIManager.referenceManager.GetStatIcon(stat),
                };

                statDataList.Add(statData);
            }

            _inventoryWindowInstance.panel_playerStats.AddStats(scrollGroupName, statDataList);
        }

        protected void AddSkillsToStatWindow(string scrollGroupName, List<DFCareer.Skills> primarySkills, params DFCareer.Skills[] skills)
        {
            List<UISkillData> skillDataList = new List<UISkillData>();

            for (int i = 0; i < skills.Length; i++)
            {
                DFCareer.Skills skill = skills[i];
                SkillRank skillRank = UIUtilityFunctions.SkillToSkillRank(skill);

                UISkillData skillData = new UISkillData
                {
                    skillEnumAsInt = (int)skills[i],
                    name = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill),
                    icon = UIManager.referenceManager.GetSkillIcon(skill),
                    rankIcon = UIManager.referenceManager.GetSkillRankIcon(skillRank),
                };

                skillDataList.Add(skillData);
            }

            _inventoryWindowInstance.panel_playerStats.AddSkills(scrollGroupName, skillDataList);
        }

        protected void UpdateVitalStats()
        {
            if (_inventoryWindowInstance == null || _inventoryWindowInstance.panel_playerStats == null) { return; }

            if (playerEntity != null)
            {
                //health
                _inventoryWindowInstance.panel_playerStats.SetHealthSliderPercent(playerEntity.CurrentHealthPercent);
                _inventoryWindowInstance.panel_playerStats.SetHealthValue(playerEntity.CurrentHealth.ToString("N0") + " / " + playerEntity.MaxHealth.ToString("N0"));

                //fatigue
                _inventoryWindowInstance.panel_playerStats.SetFatigueSliderPercent(playerEntity.CurrentFatigue / (float)playerEntity.MaxFatigue);
                _inventoryWindowInstance.panel_playerStats.SetFatigueValue(playerEntity.CurrentFatigue.ToString("N0") + " / " + playerEntity.MaxFatigue.ToString("N0"));

                //magicka
                _inventoryWindowInstance.panel_playerStats.SetMagickaSliderPercent(playerEntity.CurrentMagicka / (float)playerEntity.MaxMagicka);
                _inventoryWindowInstance.panel_playerStats.SetMagickaValue(playerEntity.CurrentMagicka.ToString("N0") + " / " + playerEntity.MaxMagicka.ToString("N0"));
            }
            else
            {
                _inventoryWindowInstance.panel_playerStats.SetHealthSliderPercent(0f);
                _inventoryWindowInstance.panel_playerStats.SetFatigueSliderPercent(0f);
                _inventoryWindowInstance.panel_playerStats.SetMagickaSliderPercent(0f);

                _inventoryWindowInstance.panel_playerStats.SetHealthValue(null);
                _inventoryWindowInstance.panel_playerStats.SetFatigueValue(null);
                _inventoryWindowInstance.panel_playerStats.SetMagickaValue(null);
            }
        }

        protected void UpdateEquipmentStats()
        {
            if (_inventoryWindowInstance == null || _inventoryWindowInstance.panel_playerStats == null) { return; }

            if (playerEntity != null)
            {
                int enemyTypeAsInt = (int)_currentEnemyTypeForStatCalc;
                DaggerfallUnityItem mainHandWep = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightHand);
                DaggerfallUnityItem offHandWep = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftHand);

                //damage
                _inventoryWindowInstance.panel_playerStats.SetMainHandDamageValue(UIUtilityFunctions.GetPlayerDamageString(mainHandWep, enemyTypeAsInt));
                _inventoryWindowInstance.panel_playerStats.SetOffHandDamageValue(UIUtilityFunctions.GetPlayerDamageString(offHandWep, enemyTypeAsInt));

                //hit chance
                _inventoryWindowInstance.panel_playerStats.SetMainHandHitChanceValue(UIUtilityFunctions.GetPlayerBaseHitChanceString(mainHandWep, enemyTypeAsInt));
                _inventoryWindowInstance.panel_playerStats.SetOffHandHitChanceValue(UIUtilityFunctions.GetPlayerBaseHitChanceString(offHandWep, enemyTypeAsInt));

                //total armor value from all equipped armor
                _inventoryWindowInstance.panel_playerStats.SetTotalArmorValue(UIUtilityFunctions.GetPlayerArmorAfterCalculation().ToString("N0"));
            }
            else
            {
                _inventoryWindowInstance.panel_playerStats.SetMainHandDamageValue(null);
                _inventoryWindowInstance.panel_playerStats.SetOffHandDamageValue(null);
                _inventoryWindowInstance.panel_playerStats.SetMainHandHitChanceValue(null);
                _inventoryWindowInstance.panel_playerStats.SetOffHandHitChanceValue(null);
                _inventoryWindowInstance.panel_playerStats.SetTotalArmorValue(null);
            }
        }

        protected void UpdatePrimaryStatsAndSkills()
        {
            if (_inventoryWindowInstance != null && _inventoryWindowInstance.panel_playerStats != null && playerEntity != null)
            {
                foreach (DFCareer.Stats stat in Enum.GetValues(typeof(DFCareer.Stats)))
                {
                    if (stat != DFCareer.Stats.None)
                    {
                        _inventoryWindowInstance.panel_playerStats.SetStatValue((int)stat, playerEntity.Stats.GetLiveStatValue(stat));
                    }
                }

                for (int i = 0; i < (int)DFCareer.Skills.Count; i++)
                {
                    _inventoryWindowInstance.panel_playerStats.SetSkillValue(i, playerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)i));
                }
            }
        }
        #endregion


        #region Paper Doll
        protected void UpdatePaperDoll()
        {
            if (_inventoryWindowInstance != null && _inventoryWindowInstance.panel_playerStats != null)
            {
                _inventoryWindowInstance.panel_playerStats.UpdatePaperDoll();
            }
        }
        #endregion


        #region Items
        protected void UpdateItemList(UIInventoryItemList itemList, ItemCollection itemCollection, bool clearFirst, 
                                      Action<UIItemEntry> onLeftClick, Action<UIItemEntry> onRightClick)
        {
            if(itemList == null){ return; }
            
            if (clearFirst)
            {
                itemList.ClearAllItemEntries();
            }

            if(itemCollection == null) { return; }

            DFCareer career = playerEntity != null ? playerEntity.Career : null;
            UIItemQueryOptions queryOptions = new UIItemQueryOptions();
            if(career != null)
            {
                queryOptions.forbiddenArmorsAsInt = (int)career.ForbiddenArmors;
                queryOptions.forbiddenShieldsAsInt = (int)career.ForbiddenShields;
                queryOptions.forbiddenMaterialsAsInt = (int)career.ForbiddenMaterials;
                queryOptions.forbiddenProficienciesAsInt = (int)career.ForbiddenProficiencies;
            };

            ItemFilter itemFilter = itemList.activeItemFilter;
            List<UIItemData> itemData = UIUtilityFunctions.ItemCollectionToItemDataList(itemCollection, queryOptions, itemFilter);
            if (itemData != null && itemData.Count > 0)
            {
                ItemColumnFlags sortByColumn = itemList.sortByColumn;
                bool ascending = itemList.sortByAscending;
                UIUtilityFunctions.SortItemsByColumn(itemData, sortByColumn, ascending);

                for (int i = 0; i < itemData.Count; i++)
                {
                    UIItemEntry itemEntry = itemList.AddItemEntry(itemData[i].UID);
                    InitializeItemEntryWithItemData(itemCollection, itemEntry, itemData[i], UIUtilityFunctions.ItemFilterToColumnFlags(itemFilter));
                    itemEntry.transform.SetSiblingIndex(i);
                    itemEntry.Delegate_OnPointerEnter = OnPointerEnter_ItemEntry;
                    itemEntry.Delegate_OnPointerExit = OnPointerExit_ItemEntry;
                    itemEntry.Delegate_OnLeftClicked = onLeftClick;
                    itemEntry.Delegate_OnRightClicked = onRightClick;
                }
            }
        }
        protected void UpdatePlayerInventory(bool clearFirst = false)
        {
            if (_inventoryWindowInstance == null) { return; }
            if (_inventoryWindowInstance.itemList_playerInventory == null) { return; }
            UpdateItemList(_inventoryWindowInstance.itemList_playerInventory, localItems, clearFirst, OnItemLeftClicked_Inventory, OnItemRightClicked_Inventory);
        }

        protected void UpdateContainerInventory(bool clearFirst = false)
        {
            if (_inventoryWindowInstance == null) { return; }
            if (_inventoryWindowInstance.itemList_container == null) { return; }
            UpdateItemList(_inventoryWindowInstance.itemList_container, remoteItems, clearFirst, OnItemClicked_Container, OnItemClicked_Container);
        }

        protected void InitializeItemEntryWithItemData(ItemCollection itemCollection, UIItemEntry itemEntry, UIItemData itemData, ItemColumnFlags columnFlags)
        {
            string name = null;
            if ((columnFlags & ItemColumnFlags.Name) != 0)
            {
                name = UIUtilityFunctions.GetItemName(itemData);
            }
            itemEntry.SetColumnValue_Name(name);

            itemEntry.SetColumnValue_ItemArchetypeIcon(itemData.archetypeIcon);

            DaggerfallUnityItem item = itemCollection != null ? itemCollection.GetItem(itemData.UID) : null;
            bool ignoreDamage = item != null && item.ItemGroup == ItemGroups.Weapons && item.TemplateIndex == (int)Weapons.Arrow;

            if (((columnFlags & ItemColumnFlags.Damage) == 0))
            {
                itemEntry.SetColumnValue_Damage(null);
            }
            else
            {
                itemEntry.SetColumnValue_Damage(ignoreDamage ? "-" : string.Format("{0:N0}-{1:N0}", itemData.minDamageValue, itemData.maxDamageValue));
            }

            if (((columnFlags & ItemColumnFlags.Armor) == 0))
            {
                itemEntry.SetColumnValue_Armor(null);
            }
            else
            {
                itemEntry.SetColumnValue_Armor(itemData.armorValue > 0 ? itemData.armorValue.ToString("N0") : "-");
            }

            if ((columnFlags & ItemColumnFlags.Condition) == 0)
            {
                itemEntry.SetColumnValue_Condition(-1f); //disables the slider
                itemEntry.SetNoConditionText(null);
            }
            else if(itemData.conditionPercent >= 0f)
            {
                itemEntry.SetColumnValue_Condition(itemData.conditionPercent);
                itemEntry.SetNoConditionText(null);
            }
            else
            {
                itemEntry.SetColumnValue_Condition(-1f); //disables the slider
                itemEntry.SetNoConditionText("-");
            }

            string valueStr = itemData.goldValue > 0 ? itemData.goldValue.ToString("N0") : "-";
            itemEntry.SetColumnValue_GoldValue(((columnFlags & ItemColumnFlags.GoldValue) != 0) ? valueStr : null);

            //NOTE(Acreal): add option to count total stack weight and not just unit weight?
            string weightStr = itemData.weightValue > float.Epsilon ? itemData.weightValue.ToString("F2") : "-";
            itemEntry.SetColumnValue_Weight(((columnFlags & ItemColumnFlags.Weight) != 0) ? weightStr : null);

            itemEntry.SetStatusIcons((itemData.itemStatusFlags & ItemStatusFlags.Prohibited) != 0,
                                     (itemData.itemStatusFlags & ItemStatusFlags.Equipped) != 0,
                                     (itemData.itemStatusFlags & ItemStatusFlags.Broken) != 0,
                                     (itemData.itemStatusFlags & ItemStatusFlags.Magic) != 0,
                                     (itemData.itemStatusFlags & ItemStatusFlags.Poisoned) != 0);
        }

        private void GatherItemStatInformation(UIItemData itemData, out List<ItemStatData> statData, out List<ItemStatSliderData> sliderStatData)
        {
            statData = new List<ItemStatData>();
            sliderStatData = new List<ItemStatSliderData>();

            bool isArrow = itemData.itemArchetype == ItemArchetype.Arrow;

            switch (itemData.itemType)
            {
                case ItemType.Armor:
                    statData.Add(new ItemStatData { name = UITextStrings.InventoryWindow_Label_Armor.GetText(), description = itemData.armorValue.ToString("N0") });
                    break;

                case ItemType.Weapon:
                    if (!isArrow)
                    {
                        statData.Add(new ItemStatData { name = UITextStrings.InventoryWindow_Label_Damage.GetText(), description = (itemData.minDamageValue.ToString("N0") + "-" + itemData.maxDamageValue.ToString("N0")) });
                    }
                    break;
            }

            if (itemData.weightValue > float.Epsilon)
            {
                statData.Add(new ItemStatData { name = UITextStrings.InventoryWindow_Label_Weight.GetText(), description = itemData.weightValue.ToString("F2") + " " + UITextStrings.Abbreviation_Kilograms.GetText() });
            }

            if (!isArrow)
            {
                string conStr = itemData.showCondition && itemData.conditionPercent >= 0f ? (itemData.conditionPercent * 100f).ToString("N0") + "%" : null;
                if (conStr != null)
                {
                    sliderStatData.Add(new ItemStatSliderData { name = UITextStrings.InventoryWindow_Label_Condition.GetText(), description = conStr, sliderValue = itemData.conditionPercent });
                }
            }

            if (itemData.goldValue > 0)
            {
                statData.Add(new ItemStatData { name = UITextStrings.InventoryWindow_Label_Value.GetText(), description = itemData.goldValue.ToString("N0") + UITextStrings.Abbreviation_Grams.GetText() });
            }
        }

        private void GatherItemPowerInformation(DaggerfallUnityItem item, out List<ItemPowerData> itemPowerData)
        {
            if (item.HasLegacyEnchantments || item.IsPotion)
            {
                itemPowerData = new List<ItemPowerData>();
                List<string> itemPowerStrings = UIUtilityFunctions.GetItemMagicInfo(item);
                if (itemPowerStrings != null)
                {
                    for (int i = 0; i < itemPowerStrings.Count; i++)
                    {
                        itemPowerData.Add(new ItemPowerData()
                        {
                            //Sprite_Circle_Empty
                            icon = UIManager.mod.GetAsset<Sprite>("Sprite_Circle_Empty"),
                            description = itemPowerStrings[i],
                        });
                    }
                }
            }
            else
            {
                itemPowerData = null;
            }
        }

        private void CalculatePivotAndTooltipPos(UIItemEntry entry, out Vector2 pivot, out Vector2 tooltipPos)
        {
            RectTransform rt = entry.transform as RectTransform;
            Vector3[] cornerArray = new Vector3[4];
            rt.GetWorldCorners(cornerArray);

            Vector3 center = RectTransformUtility.WorldToScreenPoint(null, (cornerArray[0] + cornerArray[2]) * 0.5f);
            float x = center.x >= (Screen.width * 0.5f) ? 1f : 0f;
            float y = 0.5f;
            pivot = new Vector2(x, y);

            if (pivot.x >= 0.5f)
            {
                Vector3 worldPos = (cornerArray[0] + cornerArray[1]) * 0.5f;
                tooltipPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
                tooltipPos.x -= _TOOLTIP_X_OFFSET;
            }
            else
            {
                Vector3 worldPos = (cornerArray[2] + cornerArray[3]) * 0.5f;
                tooltipPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
                tooltipPos.x += _TOOLTIP_X_OFFSET;
            }
        }

        private void UpdatePlayerFilterToggles()
        {
            if (_inventoryWindowInstance != null)
            {
                if (_inventoryWindowInstance.itemList_playerInventory != null)
                {
                    //start at index 1 because we never disable the 'All' toggle
                    for (int i = 1; i < (int)ItemFilter._COUNT; i++)
                    {
                        bool hasItems = UIUtilityFunctions.ItemCollectionContainsItemsWithFilter(localItems, (ItemFilter)i);
                        _inventoryWindowInstance.itemList_playerInventory.SetFilterToggleDisabled((ItemFilter)i, !hasItems);
                    }
                }
            }
        }

        private void UpdateContainerFilterToggles()
        {
            if (_inventoryWindowInstance != null)
            {
                if (_inventoryWindowInstance.itemList_container != null)
                {
                    //start at index 1 because we never disable the 'All' toggle
                    for (int i = 1; i < (int)ItemFilter._COUNT; i++)
                    {
                        bool hasItems = UIUtilityFunctions.ItemCollectionContainsItemsWithFilter(remoteItems, (ItemFilter)i);
                        _inventoryWindowInstance.itemList_container.SetFilterToggleDisabled((ItemFilter)i, !hasItems);
                    }
                }
            }
        }

        private string BuildPlayerWeightString()
        {
            float totalWeight = usingWagon ? localItems.GetWeight() : playerEntity.CarriedWeight;
            string totalWeightStr = totalWeight.ToString("N1");
            StringBuilder strBuilder = new StringBuilder(totalWeightStr);
            strBuilder.Append(" / ");

            if (usingWagon)
            {
                strBuilder.Append(ItemHelper.WagonKgLimit.ToString("F1"));
            }
            else if(playerEntity != null)
            {
                strBuilder.Append(playerEntity.MaxEncumbrance.ToString("F1"));
            }
            else
            {
                strBuilder.Append("0.0");
            }

            strBuilder.Append(" Kg");
            return strBuilder.ToString();
        }
        #endregion


        #region Input Handling
        private void OnPointerEnter_ItemEntry(UIItemEntry entry)
        {
            DaggerfallUnityItem item = localItems.GetItem(entry.itemUID);
            if (item == null) { item = remoteItems.GetItem(entry.itemUID); }
            if (item == null) { return; } //can't find item in either list, so bail out

            //item data
            DFCareer career = playerEntity.Career;
            UIItemQueryOptions queryOptions = new UIItemQueryOptions
            {
                forbiddenArmorsAsInt = (int)career.ForbiddenArmors,
                forbiddenShieldsAsInt = (int)career.ForbiddenShields,
                forbiddenMaterialsAsInt = (int)career.ForbiddenMaterials,
                forbiddenProficienciesAsInt = (int)career.ForbiddenProficiencies,
            };
            UIItemData itemData = UIUtilityFunctions.ItemToItemData(item, queryOptions);

            //stats and powers
            GatherItemStatInformation(itemData, out List<ItemStatData> itemStatDataList, out List<ItemStatSliderData> itemStatSliderDataList);
            GatherItemPowerInformation(item, out List<ItemPowerData> itemPowersDataList);

            //screen position
            CalculatePivotAndTooltipPos(entry, out Vector2 pivot, out Vector2 tooltipPos);
            
            //display the tooltip
            UIManager.tooltipManager.ShowItemDetailsTooltip(entry.gameObject, itemData.icon, itemData.longName, itemData.description, itemStatDataList, itemStatSliderDataList, itemPowersDataList, pivot, tooltipPos);
        }

        private void OnPointerExit_ItemEntry(UIItemEntry entry)
        {
            if (entry != null && UIManager.tooltipManager.hoveredObject != null && UIManager.tooltipManager.hoveredObject.GetInstanceID() == entry.gameObject.GetInstanceID())
            {
                UIManager.tooltipManager.HideActiveTooltip();
            }
        }

        private void OnItemLeftClicked_Inventory(UIItemEntry itemEntry)
        {
            if (itemEntry != null && localItems != null)
            {
                DaggerfallUnityItem item = localItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    if (usingWagon)
                    {
                        //when using the wagon, left click function
                        //changes to swapping item over to the dropped
                        //items list - at which point it can be added
                        //back into the player's inventory
                        OnItemRightClicked_Inventory(itemEntry);
                    }
                    else
                    {
                        bool clearInventoryBeforeUpdate = false;

                        //contextual behavior based on item type
                        if (UIUtilityFunctions.ItemIsEquippable(item))
                        {
                            if (item.IsEquipped)
                            {
                                UnequipItem(item);
                            }
                            else
                            {
                                LocalItemListScroller_OnItemClick(item, ActionModes.Equip);
                            }
                        }
                        else if (UIUtilityFunctions.ItemTypeIsUseable(UIUtilityFunctions.ItemToItemType(item)))
                        {
                            LocalItemListScroller_OnItemClick(item, ActionModes.Use);

                            if(localItems.GetItem(itemEntry.itemUID) == null)
                            {
                                // item was consumed or removed
                                clearInventoryBeforeUpdate = true;
                            }
                        }
                        else
                        {
                            Debug.LogError("[ACREALUI] Unhandled Item Type: " + UIUtilityFunctions.ItemToItemType(item));
                        }


                        UpdateVitalStats();
                        UpdatePaperDoll();
                        UpdateEquipmentStats();
                        UpdatePlayerInventory(clearInventoryBeforeUpdate);

                        if(clearInventoryBeforeUpdate)
                        {
                            UIManager.tooltipManager.HideActiveTooltip();
                        }
                    }

                    _inventoryWindowInstance.itemList_playerInventory.SetTotalWeightText(BuildPlayerWeightString());
                }
            }
        }

        private void OnItemRightClicked_Inventory(UIItemEntry itemEntry)
        {
            if (itemEntry != null && localItems != null)
            {
                DaggerfallUnityItem item = localItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                    if (splitting)
                    {
                        int maxCount = item.stackCount - 1;
                        int defaultValue = item.stackCount / 2;

                        // TODO(Acreal): localize "Split Stack" string
                        UIManager.popupManager.PushSliderConfirmationWindow("Split Stack", String.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                        0, item.stackCount - 1, true, //slider params
                                                                        new object[3] { localItems, remoteItems, item }, //additional meta data to pass along
                                                                        (float sliderValue, object[] dataPayload) => //on confirm
                                                                        {
                                                                            UIManager.popupManager.PopWindow();

                                                                            ItemCollection sourceCollection = dataPayload[0] as ItemCollection;
                                                                            ItemCollection destCollection = dataPayload[1] as ItemCollection;
                                                                            DaggerfallUnityItem itemToSplit = dataPayload[2] as DaggerfallUnityItem;
                                                                            if (sourceCollection != null && itemToSplit != null)
                                                                            {
                                                                                int splitSize = (int)sliderValue;
                                                                                DaggerfallUnityItem splitItem = sourceCollection.SplitStack(itemToSplit, splitSize);
                                                                                HandleLocalItemClick(splitItem);
                                                                            }
                                                                        },
                                                                        (_) => { UIManager.popupManager.PopWindow(); });
                    }
                    else
                    {
                        HandleLocalItemClick(item);
                    }
                }
            }
        }

        private void HandleLocalItemClick(DaggerfallUnityItem item)
        {
            ItemType itemType = UIUtilityFunctions.ItemToItemType(item);

            //unequip the item if we have it equipped before transferring it
            if (UIUtilityFunctions.ItemIsEquippable(item))
            {
                if (item.IsEquipped)
                {
                    UnequipItem(item);
                    UpdateVitalStats();
                    UpdatePaperDoll();
                    UpdateEquipmentStats();
                }
            }

            LocalItemListScroller_OnItemClick(item, ActionModes.Remove);

            //item removed successfully
            if (!localItems.Contains(item))
            {
                UpdatePlayerFilterToggles();
                UpdateContainerFilterToggles();
                UpdatePlayerInventory(true);
                UpdateContainerInventory();

                UIManager.tooltipManager.HideActiveTooltip();

                if (_inventoryWindowInstance != null)
                {
                    _inventoryWindowInstance.itemList_container?.EnableOrDisableTabs(false, false, false, LootTarget != null || (droppedItems != null && droppedItems.Count > 0));
                    _inventoryWindowInstance.itemList_playerInventory?.SetTotalWeightText(BuildPlayerWeightString());
                    _inventoryWindowInstance.itemList_container?.Show();
                }
            }
        }

        private void OnItemClicked_Container(UIItemEntry itemEntry)
        {
            if (itemEntry != null && remoteItems != null)
            {
                DaggerfallUnityItem item = remoteItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                    if (splitting)
                    {
                        int maxCount = item.stackCount - 1;
                        int defaultValue = item.stackCount / 2;

                        // TODO(Acreal): localize "Split Stack" string
                        UIManager.popupManager.PushSliderConfirmationWindow("Split Stack", String.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                        0, item.stackCount - 1, true, //slider params
                                                                        new object[3] { localItems, remoteItems, item }, //additional meta data to pass along
                                                                        (float sliderValue, object[] dataPayload) => //on confirm
                                                                        {
                                                                            UIManager.popupManager.PopWindow();

                                                                            ItemCollection localCollection = dataPayload[0] as ItemCollection;
                                                                            ItemCollection remoteCollection = dataPayload[1] as ItemCollection;
                                                                            DaggerfallUnityItem itemToSplit = dataPayload[2] as DaggerfallUnityItem;
                                                                            if (remoteCollection != null && localCollection != null && itemToSplit != null)
                                                                            {
                                                                                int splitSize = (int)sliderValue;
                                                                                DaggerfallUnityItem splitItem = remoteCollection.SplitStack(item, splitSize);
                                                                                HandleContainerItemClick(splitItem);
                                                                                UpdateContainerInventory(true);
                                                                            }
                                                                        },
                                                                        (_) => { UIManager.popupManager.PopWindow(); });
                    }
                    else
                    {
                        HandleContainerItemClick(item);
                    }
                }
            }
        }

        private void HandleContainerItemClick(DaggerfallUnityItem item)
        {
            if (item != null)
            {
                if (usingWagon)
                {
                    //this is a small workaround for allowing the base class's
                    //code to handle this case appropriately, despite moving
                    //the wagon display to where the player's inventory is
                    //(in base class the wagon's inventory would be kept in "remoteItems")
                    ItemCollection prevLocalItems = localItems;
                    ItemCollection prevRemoteItems = remoteItems;

                    localItems = remoteItems;
                    remoteItems = playerEntity.WagonItems;

                    LocalItemListScroller_OnItemClick(item, ActionModes.Remove);

                    localItems = prevLocalItems;
                    remoteItems = prevRemoteItems;
                }
                else
                {
                    RemoteItemListScroller_OnItemClick(item, ActionModes.Remove);
                }

                //item was successfully transferred to player
                if (!remoteItems.Contains(item))
                {
                    UpdateContainerFilterToggles();
                    UpdateContainerInventory(true);
                    UpdatePlayerFilterToggles();
                    UpdatePlayerInventory();

                    UIManager.tooltipManager.HideActiveTooltip();

                    if (UIUtilityFunctions.ItemIsGold(item))
                    {
                        _inventoryWindowInstance.itemList_playerInventory.SetTotalGoldText(playerEntity.GoldPieces.ToString("N0"));
                    }

                    _inventoryWindowInstance.itemList_playerInventory.SetTotalWeightText(BuildPlayerWeightString());

                    if (remoteItems.Count == 0)
                    {
                        if (lootTarget != null)
                        {
                            PopWindow();
                        }
                        else
                        {
                            _inventoryWindowInstance.itemList_container?.Hide();
                        }
                    }
                }
            }
        }

        private void DropGoldPopup_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            // Determine how many gold pieces to drop
            int goldToDrop = 0;
            bool result = int.TryParse(input, out goldToDrop);
            if (!result || goldToDrop < 1)
            {
                return;
            }

            if(playerEntity != null)
            {
                // Get player gold count
                int playerGold = playerEntity.GoldPieces;
                if (goldToDrop > playerGold)
                {
                    return;
                }

                // Create new item for gold pieces and add to other container
                DaggerfallUnityItem goldPieces = ItemBuilder.CreateGoldPieces(goldToDrop);
                remoteItems.AddItem(goldPieces);

                // Remove gold count from player
                GameManager.Instance.PlayerEntity.GoldPieces -= goldToDrop;

                if (_inventoryWindowInstance != null)
                {
                    //update player gold display
                    if (_inventoryWindowInstance.itemList_playerInventory != null)
                    {
                        _inventoryWindowInstance.itemList_playerInventory.SetTotalGoldText(playerEntity.GoldPieces.ToString("N0"));
                        _inventoryWindowInstance.itemList_playerInventory.SetTotalWeightText(BuildPlayerWeightString());
                    }

                    //update and show dropped item display
                    if (_inventoryWindowInstance.itemList_container != null)
                    {
                        _inventoryWindowInstance.itemList_container.EnableOrDisableTabs(false, false, false, LootTarget != null || (remoteItems != null && remoteItems.Count > 0));
                        _inventoryWindowInstance.itemList_container.SetLootPileIcon(UIUtilityFunctions.GetLootContainerIcon(lootTarget));
                        UpdateContainerFilterToggles();
                        UpdateContainerInventory();

                        UIManager.tooltipManager.HideActiveTooltip();

                        _inventoryWindowInstance.itemList_container.Show();
                    }
                }
            }
        }
        #endregion
    }
}
