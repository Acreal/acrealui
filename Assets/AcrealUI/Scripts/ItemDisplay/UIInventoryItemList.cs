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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIInventoryItemList : UIItemList
    {
        #region Variables
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
        [SerializeField] private string _gameObjName_sortToggle_damage = null;
        [SerializeField] private string _gameObjName_sortToggle_armor = null;
        [SerializeField] private string _gameObjName_sortToggle_condition = null;
        [SerializeField] private string _gameObjName_sortToggle_weight = null;

        [Header("Text")]
        [SerializeField] private string _gameObjName_text_itemFilter = null;

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
        private UISortToggle _sortToggle_damage = null;
        private UISortToggle _sortToggle_armor = null;
        private UISortToggle _sortToggle_condition = null;
        private UISortToggle _sortToggle_weight = null;
        private TextMeshProUGUI _text_itemFilter = null;
        #endregion


        #region MonoBehaviour
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        #endregion


        #region Initialize/Cleanup
        public override void Initialize()
        {
            #region Item Filter
            Transform filterTextTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_itemFilter);
            _text_itemFilter = filterTextTform != null ? filterTextTform.GetComponent<TextMeshProUGUI>() : null;

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
            _sortToggle_damage = InitializeSortItemsToggle(_gameObjName_sortToggle_damage, ItemColumnFlags.Damage);
            _sortToggle_armor = InitializeSortItemsToggle(_gameObjName_sortToggle_armor, ItemColumnFlags.Armor);
            _sortToggle_condition = InitializeSortItemsToggle(_gameObjName_sortToggle_condition, ItemColumnFlags.Condition);
            _sortToggle_weight = InitializeSortItemsToggle(_gameObjName_sortToggle_weight, ItemColumnFlags.Weight);
            #endregion

            //keep this at the bottom so the sort toggle group
            //initializes correctly
            base.Initialize();
        }

        public override void Cleanup()
        {
            _filterToggleGroup?.Cleanup();
            _filterToggleGroup = null;

            _toggle_filterAll?.Cleanup();
            _toggle_filterAll = null;

            _toggle_filterFavorited?.Cleanup();
            _toggle_filterFavorited = null;

            _toggle_filterWeapons?.Cleanup();
            _toggle_filterWeapons = null;

            _toggle_filterArmor?.Cleanup();
            _toggle_filterArmor = null;

            _toggle_filterPotions?.Cleanup();
            _toggle_filterPotions = null;

            _toggle_filterMagic?.Cleanup();
            _toggle_filterMagic = null;

            _toggle_filterIngredients?.Cleanup();
            _toggle_filterIngredients = null;

            _toggle_filterBooks?.Cleanup();
            _toggle_filterBooks = null;

            _toggle_filterQuestItems?.Cleanup();
            _toggle_filterQuestItems = null;

            _toggle_filterMisc?.Cleanup();
            _toggle_filterMisc = null;

            _sortToggle_damage?.Cleanup();
            _sortToggle_damage = null;

            _sortToggle_armor?.Cleanup();
            _sortToggle_armor = null;

            _sortToggle_condition?.Cleanup();
            _sortToggle_condition = null;

            _sortToggle_weight?.Cleanup();
            _sortToggle_weight = null;

            base.Cleanup();
        }

        public override void ResetList()
        {
            _filterToggleGroup?.ToggleDefault();
            base.ResetList();
        }
        #endregion


        #region Public API
        public void ScrollToBottom()
        {
            if (gameObject.activeSelf && gameObject.activeInHierarchy)
            {
                StartCoroutine(ScrollToBottomDelayed());
            }
        }

        public override void SetItemColumnFlags(ItemColumnFlags filterFlags)
        {
            base.SetItemColumnFlags(filterFlags);
            if (_sortToggle_damage != null) { _sortToggle_damage.gameObject.SetActive((filterFlags & ItemColumnFlags.Damage) != 0); }
            if (_sortToggle_armor != null) { _sortToggle_armor.gameObject.SetActive((filterFlags & ItemColumnFlags.Armor) != 0); }
            if (_sortToggle_condition != null) { _sortToggle_condition.gameObject.SetActive((filterFlags & ItemColumnFlags.Condition) != 0); }
            if (_sortToggle_weight != null) { _sortToggle_weight.gameObject.SetActive((filterFlags & ItemColumnFlags.Weight) != 0); }
        }

        public void SetFilterToggleDisabled(ItemFilter filter, bool disabled)
        {
            UIToggle toggle = GetFilterToggle(filter);
            if (toggle != null)
            {
                toggle.isDisabled = disabled;
            }
        }

        public override void SetItemFilter(ItemFilter filter)
        {
            base.SetItemFilter(filter);

            UIToggle toggle = GetFilterToggle(filter);
            if (toggle != null)
            {
                toggle.SetToggledWithoutNotify(true);
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

                toggle.Event_OnToggledOn += (UIToggle tgl) =>
                {
                    SetItemColumnFlags(UIUtilityFunctions.ItemFilterToColumnFlags(filter));
                    SetItemFilter(filter);
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
                    CallSortItemsColumnChangedEvent();
                };

                sortToggle.Event_OnSortAscendingChanged += (bool sortAscending) =>
                {
                    CallSortAscendingChangedEvent();
                };

                return sortToggle;
            }
            return null;
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
        #endregion
    }
}