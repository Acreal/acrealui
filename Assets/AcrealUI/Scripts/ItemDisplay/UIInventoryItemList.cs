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

using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIInventoryItemList : MonoBehaviour
    {
        #region Variables
        private const int ITEM_ENTRY_BLOCK_SIZE = 5;

        [Header("Item Entries")]
        [SerializeField] private string _gameObjName_itemEntryParent = null;

        [Header("Filter Toggles")]
        [SerializeField] private string _gameObjName_toggleGroup = null;
        [SerializeField] private string _gameObjName_toggle_filterAll = null;
        [SerializeField] private string _gameObjName_toggle_filterFavorited = null;
        [SerializeField] private string _gameObjName_toggle_filterWeapons = null;
        [SerializeField] private string _gameObjName_toggle_filterArmor = null;
        [SerializeField] private string _gameObjName_toggle_filterPotions = null;
        [SerializeField] private string _gameObjName_toggle_filterMagic = null;
        [SerializeField] private string _gameObjName_toggle_filterIngredients = null;
        [SerializeField] private string _gameObjName_toggle_filterBooks = null;
        [SerializeField] private string _gameObjName_toggle_filterQuestItems = null;
        [SerializeField] private string _gameObjName_toggle_filterMisc = null;

        [Header("Item Info Columns")]
        [SerializeField] private string _gameObjName_sortToggleGroup = null;
        [SerializeField] private string _gameObjName_sortToggle_name = null;
        [SerializeField] private string _gameObjName_sortToggle_type = null;
        [SerializeField] private string _gameObjName_sortToggle_damage = null;
        [SerializeField] private string _gameObjName_sortToggle_armor = null;
        [SerializeField] private string _gameObjName_sortToggle_condition = null;
        [SerializeField] private string _gameObjName_sortToggle_weight = null;
        [SerializeField] private string _gameObjName_sortToggle_value = null;

        [Header("Text")]
        [SerializeField] private string _gameObjName_text_itemFilter = null;
        [SerializeField] private string _gameObjName_weightGoldParent = null;
        [SerializeField] private string _gameObjName_text_totalGold = null;
        [SerializeField] private string _gameObjName_text_totalWeight = null;

        [Header("Item Source Tabs")]
        [SerializeField] private string _gameObjName_inventoryTabToggleGroup = null;
        [SerializeField] private string _gameObjName_inventoryTabToggle_player = null;
        [SerializeField] private string _gameObjName_inventoryTabToggle_wagon = null;
        [SerializeField] private string _gameObjName_inventoryTabToggle_lootPile = null;
        [SerializeField] private string _gameObjName_lootPileIcon = null;

        [Header("Buttons")]
        [SerializeField] private string _gameObjName_goldButton = null;

        [Header("Tweening")]
        [SerializeField] private float _defaultSize = 350f;

        private RectTransform _itemEntryParent = null;
        private UIToggleGroup _filterToggleGroup = null;
        private UIToggle _toggle_filterAll = null;
        private UIToggle _toggle_filterFavorited = null;
        private UIToggle _toggle_filterWeapons = null;
        private UIToggle _toggle_filterArmor = null;
        private UIToggle _toggle_filterPotions = null;
        private UIToggle _toggle_filterMagic = null;
        private UIToggle _toggle_filterIngredients = null;
        private UIToggle _toggle_filterBooks = null;
        private UIToggle _toggle_filterQuestItems = null;
        private UIToggle _toggle_filterMisc = null;
        private UIToggleGroup _sortToggleGroup = null;
        private UISortToggle _sortToggle_name = null;
        private UISortToggle _sortToggle_type = null;
        private UISortToggle _sortToggle_damage = null;
        private UISortToggle _sortToggle_armor = null;
        private UISortToggle _sortToggle_condition = null;
        private UISortToggle _sortToggle_weight = null;
        private UISortToggle _sortToggle_value = null;
        private TextMeshProUGUI _text_itemFilter = null;
        private TextMeshProUGUI _text_totalGold = null;
        private TextMeshProUGUI _text_totalWeight = null;
        private GameObject _weightGoldParent = null;
        private UIButton _goldButton = null;
        private UIToggleGroup _inventoryTabToggleGroup = null;
        private UIToggle _inventoryTabToggle_player = null;
        private UIToggle _inventoryTabToggle_wagon = null;
        private UIToggle _inventoryTabToggle_lootPile = null;
        private Image _lootPileIcon = null;

        private Dictionary<ulong, UIItemEntry> _uidToItemEntryDict = null;
        private Stack<UIItemEntry> _itemEntryStack = null;
        private ItemFilter _currentItemFilter = ItemFilter.All;
        private ItemColumnFlags _sortItemsColumn = ItemColumnFlags.ItemType;
        private UISortToggle _activeSortToggle = null;
        private bool? _isShown = null;
        #endregion


        #region Events
        public event Action Event_OnItemFilterChanged = null;
        public event Action Event_OnSortItemsColumnChanged = null;
        public event Action Event_OnSortAscendingChanged = null;
        public event Action Event_OnToggled_InventoryTab_Player = null;
        public event Action Event_OnToggled_InventoryTab_Wagon = null;
        public event Action Event_OnToggled_InventoryTab_LootPile = null;
        public event Action Event_OnGoldButtonClicked = null;
        #endregion


        #region Properties
        public int itemCount { get { return _uidToItemEntryDict != null ? _uidToItemEntryDict.Count : 0; } }
        public ItemFilter activeItemFilter {  get { return _currentItemFilter; } }
        public ItemColumnFlags sortByColumn { get { return _sortItemsColumn; } }
        public bool sortByAscending { get { return _activeSortToggle == null || _activeSortToggle.sortByAscending; } }
        #endregion


        #region MonoBehaviour
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        #endregion


        #region Initialize/Cleanup
        public void Initialize()
        {
            _uidToItemEntryDict = new Dictionary<ulong, UIItemEntry>();
            _itemEntryStack = new Stack<UIItemEntry>();

            Transform entryParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemEntryParent);
            _itemEntryParent = entryParentTform != null ? entryParentTform as RectTransform : null;

            #region Tab Toggle References
            Transform playerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggle_player);
            _inventoryTabToggle_player = playerTform != null ? playerTform.GetComponent<UIToggle>() : null;
            if (_inventoryTabToggle_player != null)
            {
                _inventoryTabToggle_player.Initialize();
                _inventoryTabToggle_player.Event_OnToggledOn += (_) => { Event_OnToggled_InventoryTab_Player?.Invoke(); };
            }

            Transform wagonTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggle_wagon);
            _inventoryTabToggle_wagon = wagonTform != null ? wagonTform.GetComponent<UIToggle>() : null;
            if (_inventoryTabToggle_wagon != null)
            {
                _inventoryTabToggle_wagon.Initialize();
                _inventoryTabToggle_wagon.Event_OnToggledOn += (_) => { Event_OnToggled_InventoryTab_Wagon?.Invoke(); };
            }

            Transform toggleGroupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggleGroup);
            _inventoryTabToggleGroup = toggleGroupTform != null ? toggleGroupTform.GetComponent<UIToggleGroup>() : null;
            if (_inventoryTabToggleGroup != null)
            {
                _inventoryTabToggleGroup.Initialize();
            }

            Transform lootTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggle_lootPile);
            _inventoryTabToggle_lootPile = lootTform != null ? lootTform.GetComponent<UIToggle>() : null;
            if (_inventoryTabToggle_lootPile != null)
            {
                _inventoryTabToggle_lootPile.Initialize();
                _inventoryTabToggle_lootPile.Event_OnToggledOn += (_) => { Event_OnToggled_InventoryTab_LootPile?.Invoke(); };
            }

            Transform lootIconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_lootPileIcon);
            _lootPileIcon = lootIconTform != null ? lootIconTform.GetComponent<Image>() : null;
            #endregion

            #region ItemFilter Toggle References
            _toggle_filterAll = InitializeItemFilterToggle(_gameObjName_toggle_filterAll, ItemFilter.All);
            _toggle_filterFavorited = InitializeItemFilterToggle(_gameObjName_toggle_filterFavorited, ItemFilter.Favorite);
            _toggle_filterWeapons = InitializeItemFilterToggle(_gameObjName_toggle_filterWeapons, ItemFilter.Weapons);
            _toggle_filterArmor = InitializeItemFilterToggle(_gameObjName_toggle_filterArmor, ItemFilter.Armor);
            _toggle_filterPotions = InitializeItemFilterToggle(_gameObjName_toggle_filterPotions, ItemFilter.Potions);
            _toggle_filterMagic = InitializeItemFilterToggle(_gameObjName_toggle_filterMagic, ItemFilter.MagicItems);
            _toggle_filterIngredients = InitializeItemFilterToggle(_gameObjName_toggle_filterIngredients, ItemFilter.Ingredients);
            _toggle_filterBooks = InitializeItemFilterToggle(_gameObjName_toggle_filterBooks, ItemFilter.Books);
            _toggle_filterQuestItems = InitializeItemFilterToggle(_gameObjName_toggle_filterQuestItems, ItemFilter.QuestItems);
            _toggle_filterMisc = InitializeItemFilterToggle(_gameObjName_toggle_filterMisc, ItemFilter.MiscItems);

            if (!string.IsNullOrEmpty(_gameObjName_toggleGroup))
            {
                Transform togGrpTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_toggleGroup);
                _filterToggleGroup = togGrpTform != null ? togGrpTform.GetComponent<UIToggleGroup>() : null;
                if (_filterToggleGroup != null)
                {
                    _filterToggleGroup.Initialize();
                }
            }
            #endregion

            #region Sort Toggle References
            _sortToggle_type = InitializeSortItemsToggle(_gameObjName_sortToggle_type, ItemColumnFlags.ItemType);
            _sortToggle_name = InitializeSortItemsToggle(_gameObjName_sortToggle_name, ItemColumnFlags.Name);
            _sortToggle_damage = InitializeSortItemsToggle(_gameObjName_sortToggle_damage, ItemColumnFlags.Damage);
            _sortToggle_armor = InitializeSortItemsToggle(_gameObjName_sortToggle_armor, ItemColumnFlags.Armor);
            _sortToggle_condition = InitializeSortItemsToggle(_gameObjName_sortToggle_condition, ItemColumnFlags.Condition);
            _sortToggle_weight = InitializeSortItemsToggle(_gameObjName_sortToggle_weight, ItemColumnFlags.Weight);
            _sortToggle_value = InitializeSortItemsToggle(_gameObjName_sortToggle_value, ItemColumnFlags.GoldValue);

            Transform groupTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_sortToggleGroup);
            if (groupTForm != null)
            { 
                _sortToggleGroup = groupTForm.GetComponent<UIToggleGroup>();

                if (_sortToggleGroup != null)
                {
                    _sortToggleGroup.Initialize();
                }
            }
            #endregion

            #region Text References
            Transform filterTextTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_itemFilter);
            _text_itemFilter = filterTextTform != null ? filterTextTform.GetComponent<TextMeshProUGUI>() : null;

            Transform goldTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_totalGold);
            _text_totalGold = goldTform != null ? goldTform.GetComponent<TextMeshProUGUI>() : null;

            Transform weightTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_totalWeight);
            _text_totalWeight = weightTform != null ? weightTform.GetComponent<TextMeshProUGUI>() : null;

            Transform weightGoldParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_weightGoldParent);
            if(weightGoldParentTform != null) { _weightGoldParent = weightGoldParentTform.gameObject; }

            Transform goldBtnParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_goldButton);
            if (goldBtnParentTform != null) { _goldButton = goldBtnParentTform.GetComponent<UIButton>(); }
            if(_goldButton != null)
            {
                _goldButton.Event_OnAnyClick += (_, _1) =>
                {
                    Event_OnGoldButtonClicked?.Invoke();
                };
            }
            #endregion
        }

        public void Cleanup()
        {
            StopAllCoroutines();

            Event_OnItemFilterChanged = null;
            Event_OnSortItemsColumnChanged = null;
            Event_OnSortAscendingChanged = null;
            Event_OnToggled_InventoryTab_Player = null;
            Event_OnToggled_InventoryTab_Wagon = null;
            Event_OnToggled_InventoryTab_LootPile = null;
            Event_OnGoldButtonClicked = null;

            foreach (UIItemEntry entry in _uidToItemEntryDict.Values)
            {
                if (entry != null)
                {
                    _itemEntryStack.Push(entry);
                }
            }
            _uidToItemEntryDict = null;

            while (_itemEntryStack.Count > 0)
            {
                UIItemEntry entry = _itemEntryStack.Pop();
                if (entry != null)
                {
                    Destroy(entry.gameObject);
                }
            }
            _itemEntryStack = null;
        }

        public void ResetList()
        {
            StopAllCoroutines();

            _filterToggleGroup?.ToggleDefault();
            _inventoryTabToggleGroup?.ToggleDefault();

            SetTotalGoldText(null);
            SetTotalWeightText(null);
            ClearAllItemEntries();
        }
        #endregion


        #region Show/Hide
        public void Show()
        {
            if (_isShown == null || !_isShown.Value)
            {
                _isShown = true;
                _filterToggleGroup?.ToggleDefault();
                _inventoryTabToggleGroup?.ToggleDefault();

                LayoutElement layoutElem = GetComponent<LayoutElement>();
                if (layoutElem != null)
                {
                    StartCoroutine(TweenPanelWidthRoutine(layoutElem, 0f, _defaultSize, 0.2f));
                }
            }
        }

        public void Hide(bool hideImmediately = false)
        {
            if (_isShown == null || _isShown.Value)
            {
                _isShown = false;

                LayoutElement layoutElem = GetComponent<LayoutElement>();
                if (layoutElem != null)
                {
                    if (hideImmediately)
                    {
                        layoutElem.minWidth = 0f;
                        layoutElem.preferredWidth = 0f;
                    }
                    else
                    {
                        StartCoroutine(TweenPanelWidthRoutine(layoutElem, _defaultSize, 0f, 0.2f));
                    }
                }
            }
        }
        #endregion


        #region Public API
        public void EnableOrDisableTabs(bool enablePlayerTab, bool enableWagonTab, bool enableMerchantTab, bool enableLootPileTab)
        {
            if (_inventoryTabToggle_player != null) { _inventoryTabToggle_player.gameObject.SetActive(enablePlayerTab); }
            if (_inventoryTabToggle_wagon != null) { _inventoryTabToggle_wagon.gameObject.SetActive(enableWagonTab); }
            if (_inventoryTabToggle_lootPile != null) { _inventoryTabToggle_lootPile.gameObject.SetActive(enableLootPileTab); }
        }

        public void ScrollToBottom()
        {
            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(ScrollToBottomDelayed());
            }
        }

        public UIItemEntry AddItemEntry(ulong itemUID)
        {
            if (_uidToItemEntryDict != null)
            {
                UIItemEntry entry;
                _uidToItemEntryDict.TryGetValue(itemUID, out entry);
                if (entry != null)
                { 
                    return entry;
                }
            }

            if (_itemEntryStack == null) { return null; }

            if (_itemEntryStack.Count == 0)
            {
                SpawnItemEntryBlock();
            }

            if(_itemEntryStack.Count > 0)
            {
                UIItemEntry entry = _itemEntryStack.Pop();
                entry.SetItemUID(itemUID);
                entry.ClearEvents();
                entry.transform.SetAsLastSibling();
                entry.gameObject.SetActive(true);
                _uidToItemEntryDict[itemUID] = entry;
                return entry;
            }

            return null;
        }

        public bool RemoveItemEntry(ulong itemUID)
        {
            if(itemUID == 0 || _uidToItemEntryDict == null) { return false; }

            UIItemEntry itemEntry;
            if (_uidToItemEntryDict.TryGetValue(itemUID, out itemEntry))
            {
                bool success = _uidToItemEntryDict.Remove(itemUID);

                if (itemEntry != null && success)
                {
                    itemEntry.SetItemUID(0);
                    itemEntry.gameObject.SetActive(false);
                    _itemEntryStack.Push(itemEntry);
                }

                return success;
            }
            return false;
        }

        public void ClearAllItemEntries()
        {
            if (_uidToItemEntryDict != null)
            {
                foreach (UIItemEntry entry in _uidToItemEntryDict.Values)
                {
                    entry.SetItemUID(0);
                    entry.ClearEvents();
                    entry.gameObject.SetActive(false);
                    _itemEntryStack.Push(entry);
                }
                _uidToItemEntryDict.Clear();
            }
        }

        public void SetTotalGoldText(string totalGoldStr)
        {
            if(_text_totalGold != null)
            {
                _text_totalGold.text = totalGoldStr;
                _text_totalGold.gameObject.SetActive(totalGoldStr != null);
            }
        }

        public void SetTotalWeightText(string totalWeightStr)
        {
            if (_text_totalWeight != null)
            {
                _text_totalWeight.text = totalWeightStr;
                _text_totalWeight.gameObject.SetActive(totalWeightStr != null);
            }

            if(_weightGoldParent != null)
            {
                bool weightActive = _text_totalWeight != null && _text_totalWeight.gameObject.activeSelf;
                bool goldActive = _text_totalGold != null && _text_totalGold.gameObject.activeSelf;
                _weightGoldParent.SetActive(weightActive || goldActive);
            }
        }

        public void SetItemFilterText(string filterText)
        {
            if (_text_itemFilter != null)
            {
                _text_itemFilter.text = filterText;
                _text_itemFilter.gameObject.SetActive(filterText != null);
            }
        }

        public void SetLootPileIcon(Sprite icon)
        {
            if(_lootPileIcon != null)
            {
                _lootPileIcon.sprite = icon;
                _lootPileIcon.gameObject.SetActive(icon != null);
            }
        }

        public void SetItemColumnFlags(ItemColumnFlags filterFlags)
        {
            if (_sortToggle_name != null) { _sortToggle_name.gameObject.SetActive((filterFlags & ItemColumnFlags.Name) != 0); }
            if (_sortToggle_type != null) { _sortToggle_type.gameObject.SetActive((filterFlags & ItemColumnFlags.ItemType) != 0); }
            if (_sortToggle_damage != null) { _sortToggle_damage.gameObject.SetActive((filterFlags & ItemColumnFlags.Damage) != 0); }
            if (_sortToggle_armor != null) { _sortToggle_armor.gameObject.SetActive((filterFlags & ItemColumnFlags.Armor) != 0); }
            if (_sortToggle_condition != null) { _sortToggle_condition.gameObject.SetActive((filterFlags & ItemColumnFlags.Condition) != 0); }
            if (_sortToggle_weight != null) { _sortToggle_weight.gameObject.SetActive((filterFlags & ItemColumnFlags.Weight) != 0); }
            if (_sortToggle_value != null) { _sortToggle_value.gameObject.SetActive((filterFlags & ItemColumnFlags.GoldValue) != 0); }
        }

        public void SetFilterToggleDisabled(ItemFilter filter, bool disabled)
        {
            UIToggle toggle = GetFilterToggle(filter);
            if (toggle != null)
            {
                toggle.isDisabled = disabled;
            }
        }

        public void SetActiveFilter(ItemFilter filter)
        {
            UIToggle toggle = GetFilterToggle(filter);
            if (toggle != null)
            {
                toggle.isToggledOn = true;
            }
        }
        #endregion


        #region Filter/Sorting Toggles
        private UIToggle GetFilterToggle(ItemFilter filter)
        {
            switch (filter)
            {
                case ItemFilter.All: return _toggle_filterAll;
                case ItemFilter.Favorite: return _toggle_filterFavorited;
                case ItemFilter.Weapons: return _toggle_filterWeapons;
                case ItemFilter.Armor: return _toggle_filterArmor;
                case ItemFilter.Potions: return _toggle_filterPotions;
                case ItemFilter.Ingredients: return _toggle_filterIngredients;
                case ItemFilter.MagicItems: return _toggle_filterMagic;
                case ItemFilter.Books: return _toggle_filterBooks;
                case ItemFilter.QuestItems: return _toggle_filterQuestItems;
                case ItemFilter.MiscItems: return _toggle_filterMisc;
            }
            return null;
        }

        private UIToggle InitializeItemFilterToggle(string transformName, ItemFilter filter)
        {
            Transform toggleTform = UIUtilityFunctions.FindDeepChild(transform, transformName);
            UIToggle toggle = toggleTform != null ? toggleTform.GetComponent<UIToggle>() : null;
            if (toggle != null)
            {
                toggle.Initialize();

                toggle.Event_OnToggledOn += (_) =>
                {
                    _currentItemFilter = filter;
                    SetItemFilterText(UIUtilityFunctions.ItemFilterToString(filter));
                    SetItemColumnFlags(UIUtilityFunctions.ItemFilterToColumnFlags(filter));
                    Event_OnItemFilterChanged?.Invoke();
                };

                return toggle;
            }
            return null;
        }

        private UISortToggle InitializeSortItemsToggle(string transformName, ItemColumnFlags sortColumn)
        {
            Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, transformName);
            UISortToggle sortToggle = nameTform != null ? nameTform.GetComponent<UISortToggle>() : null;
            if (sortToggle != null)
            {
                sortToggle.Initialize();

                sortToggle.Event_OnToggledOn += (UIToggle toggle) =>
                {
                    _sortItemsColumn = sortColumn;
                    _activeSortToggle = toggle as UISortToggle;
                    Event_OnSortItemsColumnChanged?.Invoke();
                };

                sortToggle.Event_OnSortAscendingChanged += (bool sortAscending) =>
                {
                    Event_OnSortAscendingChanged?.Invoke();
                };

                return sortToggle;
            }
            return null;
        }
        #endregion


        #region Item Entries
        private void SpawnItemEntryBlock()
        {
            if (UIManager.referenceManager.prefab_itemEntry == null || _itemEntryParent == null) { return; }

            for (int i = 0; i < ITEM_ENTRY_BLOCK_SIZE; i++)
            {
                UIItemEntry itemEntry = Instantiate(UIManager.referenceManager.prefab_itemEntry, _itemEntryParent);
                itemEntry.transform.localScale = Vector3.one;
                itemEntry.Initalize();
                itemEntry.gameObject.SetActive(false);
                _itemEntryStack.Push(itemEntry);
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> ScrollToBottomDelayed()
        {
            yield return 0f;
            ScrollRect scrollRect = _itemEntryParent.GetComponentInParent<ScrollRect>();
            if(scrollRect != null)
            {
                scrollRect.normalizedPosition = Vector2.zero;
            }
        }

        private IEnumerator<float> TweenPanelWidthRoutine(LayoutElement panelLayoutElement, float startWidth, float endWidth, float duration, float delay = 0f)
        {
            if (panelLayoutElement != null)
            {
                panelLayoutElement.minWidth = startWidth;
                panelLayoutElement.preferredWidth = startWidth;

                float durationRemaining = delay;
                while (durationRemaining > 0f)
                {
                    durationRemaining -= Time.unscaledDeltaTime;
                    yield return 0f;
                }

                durationRemaining = duration;
                while (durationRemaining > 0f)
                {
                    durationRemaining -= Time.unscaledDeltaTime;
                    float x = Mathf.Lerp(startWidth, endWidth, 1f - Mathf.InverseLerp(0f, duration, durationRemaining));
                    panelLayoutElement.minWidth = x;
                    panelLayoutElement.preferredWidth = x;
                    yield return 0f;
                }

                panelLayoutElement.minWidth = endWidth;
                panelLayoutElement.preferredWidth = endWidth;
            }
        }
        #endregion
    }
}