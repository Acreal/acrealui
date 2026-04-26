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

using DaggerfallWorkshop.Game.Items;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public class UIItemListController
    {
        #region Variables
        private UIItemList _itemList = null;
        private ItemCollection _itemCollection = null;
        #endregion


        #region Events
        public event Action<UIItemEntry> Event_OnItemLeftClicked = null;
        public event Action<UIItemEntry> Event_OnItemRightClicked = null;
        #endregion


        #region Properties
        public ItemFilter activeItemFilter { get { return _itemList != null ? _itemList.activeItemFilter : ItemFilter.All; } }
        #endregion


        #region Initialization/Cleanup
        public UIItemListController(UIItemList itemList)
        {
            _itemList = itemList;
        }

        public void Initialize()
        {
            if (_itemList != null)
            {
                _itemList.Event_OnItemFilterChanged += (_) =>
                {
                    UpdateItemList(true);

                    UIInventoryItemList invItemList = _itemList as UIInventoryItemList;
                    invItemList?.SetItemFilterText(UIUtilityFunctions.ItemFilterToString(activeItemFilter));
                };

                _itemList.Event_OnSortItemsColumnChanged += () =>
                {
                    UpdateItemList(false);
                };

                _itemList.Event_OnSortAscendingChanged += () =>
                {
                    UpdateItemList(false);
                };
            }
        }

        public void Cleanup()
        {
            _itemList?.Cleanup();
            _itemList = null;
            _itemCollection = null;
        }

        public void ResetList()
        {
            _itemList?.ResetList();
        }
        #endregion


        #region Public API
        public void SetItemCollection(ItemCollection itemCollection)
        {
            _itemCollection = itemCollection;
            SetItemFilter(ItemFilter.All);
            UpdateItemFilterToggles();
        }

        public void SetItemFilter(ItemFilter filter)
        {
            _itemList?.SetItemFilter(filter);
        }

        public void UpdateItemFilterToggles()
        {
            UIInventoryItemList inventoryItemList = _itemList as UIInventoryItemList;
            if (inventoryItemList == null) { return; }

            //start at index 1 because we never disable the 'All' toggle
            for (int i = 1; i < (int)ItemFilter._COUNT; i++)
            {
                bool hasItems = UIUtilityFunctions.ItemCollectionContainsItemsWithFilter(_itemCollection, (ItemFilter)i);
                inventoryItemList.SetFilterToggleDisabled((ItemFilter)i, !hasItems);
            }
        }

        public void UpdateItemList(bool clearFirst)
        {
            if (_itemList == null) { return; }

            if (clearFirst)
            {
                _itemList.ClearAllItemEntries();
            }

            if (_itemCollection == null) { return; }

            ItemFilter itemFilter = _itemList.activeItemFilter;
            UIItemQueryOptions queryOptions = UIUtilityFunctions.GetItemQueryOptionsForPlayer();
            List<UIItemData> itemData = UIUtilityFunctions.ItemCollectionToItemDataList(_itemCollection, queryOptions, itemFilter);
            if (itemData != null && itemData.Count > 0)
            {
                ItemColumnFlags sortByColumn = _itemList.sortByColumn;
                bool ascending = _itemList.sortByAscending;
                UIUtilityFunctions.SortItemsByColumn(itemData, sortByColumn, ascending);

                for (int i = 0; i < itemData.Count; i++)
                {
                    UIItemEntry itemEntry = _itemList.AddItemEntry(itemData[i].UID);
                    InitializeItemEntryWithItemData(_itemCollection, itemEntry, itemData[i], UIUtilityFunctions.ItemFilterToColumnFlags(itemFilter));
                    itemEntry.transform.SetSiblingIndex(i);
                    itemEntry.Delegate_OnPointerEnter = OnItemHoverBegin;
                    itemEntry.Delegate_OnPointerExit = OnItemHoverEnd;
                    itemEntry.Delegate_OnLeftClicked = OnItemLeftClicked;
                    itemEntry.Delegate_OnRightClicked = OnItemRightClicked;
                }
            }
        }
        #endregion


        #region Item Entries
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
            else if (itemData.conditionPercent >= 0f)
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

        
        #endregion


        #region Item Tooltips
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
                tooltipPos.x -= UIConstants.TOOLTIP_OFFSET_ITEM;
            }
            else
            {
                Vector3 worldPos = (cornerArray[2] + cornerArray[3]) * 0.5f;
                tooltipPos = RectTransformUtility.WorldToScreenPoint(null, worldPos);
                tooltipPos.x += UIConstants.TOOLTIP_OFFSET_ITEM;
            }
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
                statData.Add(new ItemStatData { name = UITextStrings.InventoryWindow_Label_Value.GetText(), description = itemData.goldValue.ToString("N0") + UITextStrings.Abbreviation_Gold.GetText() });
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
        #endregion


        #region Input Handling
        private void OnItemLeftClicked(UIItemEntry itemEntry)
        {
            Event_OnItemLeftClicked?.Invoke(itemEntry);
        }

        private void OnItemRightClicked(UIItemEntry itemEntry)
        {
            Event_OnItemRightClicked?.Invoke(itemEntry);
        }

        private void OnItemHoverBegin(UIItemEntry itemEntry)
        {
            DaggerfallUnityItem item = _itemCollection.GetItem(itemEntry.itemUID);
            if (item == null) { return; }

            //item data
            UIItemQueryOptions queryOptions = UIUtilityFunctions.GetItemQueryOptionsForPlayer();
            UIItemData itemData = UIUtilityFunctions.ItemToItemData(item, queryOptions);

            //stats and powers
            GatherItemStatInformation(itemData, out List<ItemStatData> itemStatDataList, out List<ItemStatSliderData> itemStatSliderDataList);
            GatherItemPowerInformation(item, out List<ItemPowerData> itemPowersDataList);

            //screen position
            CalculatePivotAndTooltipPos(itemEntry, out Vector2 pivot, out Vector2 tooltipPos);

            //display the tooltip
            UIManager.tooltipManager.ShowItemDetailsTooltip(itemEntry.gameObject, itemData.icon, itemData.longName, itemData.description, itemStatDataList, itemStatSliderDataList, itemPowersDataList, pivot, tooltipPos);
        }

        private void OnItemHoverEnd(UIItemEntry itemEntry)
        {
            if (itemEntry != null && UIManager.tooltipManager.hoveredObject != null && UIManager.tooltipManager.hoveredObject.GetInstanceID() == itemEntry.gameObject.GetInstanceID())
            {
                UIManager.tooltipManager.HideActiveTooltip();
            }
        }
        #endregion
    }
}
