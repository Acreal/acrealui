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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public abstract class UIItemList : MonoBehaviour
    {
        #region Variables
        private const int ITEM_ENTRY_BLOCK_SIZE = 5;

        [Header("Item Entries")]
        [SerializeField] private string _gameObjName_itemEntryParent = null;

        [Header("Item Info Columns")]
        [SerializeField] private string _gameObjName_sortToggleGroup = null;
        [SerializeField] private string _gameObjName_sortToggle_type = null;
        [SerializeField] private string _gameObjName_sortToggle_name = null;
        [SerializeField] private string _gameObjName_sortToggle_value = null;

        protected RectTransform _itemEntryParent = null;
        protected UISortToggle _activeSortToggle = null;
        protected ItemColumnFlags _sortItemsColumn = ItemColumnFlags.ItemType;

        private UIToggleGroup _sortToggleGroup = null;
        private UISortToggle _sortToggle_type = null;
        private UISortToggle _sortToggle_name = null;
        private UISortToggle _sortToggle_value = null;
        private Dictionary<ulong, UIItemEntry> _uidToItemEntryDict = null;
        private Stack<UIItemEntry> _itemEntryStack = null;
        private ItemFilter _activeItemFilter = ItemFilter.All;
        #endregion


        #region Events
        public event Action<ItemFilter> Event_OnItemFilterChanged = null;
        public event Action Event_OnSortItemsColumnChanged = null;
        public event Action Event_OnSortAscendingChanged = null;
        #endregion


        #region Properties
        public int itemCount { get { return _uidToItemEntryDict != null ? _uidToItemEntryDict.Count : 0; } }
        public ItemColumnFlags sortByColumn { get { return _sortItemsColumn; } }
        public bool sortByAscending { get { return _activeSortToggle == null || _activeSortToggle.sortByAscending; } }
        public ItemFilter activeItemFilter
        {
            get
            {
                return _activeItemFilter;
            }
        }
        #endregion


        #region MonoBehaviour
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        #endregion


        #region Initialize/Cleanup
        public virtual void Initialize()
        {
            _uidToItemEntryDict = new Dictionary<ulong, UIItemEntry>();
            _itemEntryStack = new Stack<UIItemEntry>();

            Transform entryParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemEntryParent);
            _itemEntryParent = entryParentTform != null ? entryParentTform as RectTransform : null;

            #region Sort Toggle References
            _sortToggle_type = InitializeSortItemsToggle(_gameObjName_sortToggle_type, ItemColumnFlags.ItemType);
            _sortToggle_name = InitializeSortItemsToggle(_gameObjName_sortToggle_name, ItemColumnFlags.Name);
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
        }

        public virtual void Cleanup()
        {
            StopAllCoroutines();

            Event_OnSortItemsColumnChanged = null;
            Event_OnSortAscendingChanged = null;

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

        public virtual void ResetList()
        {
            StopAllCoroutines();
            ClearAllItemEntries();
            SetItemFilter(ItemFilter.All);
        }
        #endregion


        #region Public API
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

            if (_itemEntryStack.Count > 0)
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
            if (itemUID == 0 || _uidToItemEntryDict == null) { return false; }

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

        public virtual void SetItemFilter(ItemFilter filter)
        {
            _activeItemFilter = filter;
            Event_OnItemFilterChanged?.Invoke(_activeItemFilter);
        }

        public virtual void SetItemColumnFlags(ItemColumnFlags filterFlags)
        {
            if (_sortToggle_type != null) { _sortToggle_type.gameObject.SetActive((filterFlags & ItemColumnFlags.ItemType) != 0); }
            if (_sortToggle_name != null) { _sortToggle_name.gameObject.SetActive((filterFlags & ItemColumnFlags.Name) != 0); }
            if (_sortToggle_value != null) { _sortToggle_value.gameObject.SetActive((filterFlags & ItemColumnFlags.GoldValue) != 0); }
        }
        #endregion


        #region Event Calls
        protected void CallSortItemsColumnChangedEvent()
        {
            Event_OnSortItemsColumnChanged?.Invoke();
        }

        protected void CallSortAscendingChangedEvent()
        {
            Event_OnSortAscendingChanged?.Invoke();
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


        #region Sorting Toggles
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
    }
}
