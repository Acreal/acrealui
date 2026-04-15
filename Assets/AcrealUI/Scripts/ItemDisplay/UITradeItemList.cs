using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace AcrealUI
{
    public class UITradeItemList : MonoBehaviour
    {
        #region Variables
        private const int ITEM_ENTRY_BLOCK_SIZE = 5;

        [Header("Item Entries")]
        [SerializeField] private string _gameObjName_itemEntryParent = null;

        [Header("Item Info Columns")]
        [SerializeField] private string _gameObjName_sortToggleGroup = null;
        [SerializeField] private string _gameObjName_sortToggle_name = null;
        [SerializeField] private string _gameObjName_sortToggle_value = null;

        [Header("Text")]
        [SerializeField] private string _gameObjName_weightGoldParent = null;
        [SerializeField] private string _gameObjName_text_totalGold = null;
        [SerializeField] private string _gameObjName_text_totalWeight = null;

        [Header("Buttons")]
        [SerializeField] private string _gameObjName_goldButton = null;

        private RectTransform _itemEntryParent = null;
        private UIToggleGroup _sortToggleGroup = null;
        private UISortToggle _sortToggle_name = null;
        private UISortToggle _sortToggle_value = null;
        private TextMeshProUGUI _text_totalGold = null;
        private TextMeshProUGUI _text_totalWeight = null;

        private Dictionary<ulong, UIItemEntry> _uidToItemEntryDict = null;
        private Stack<UIItemEntry> _itemEntryStack = null;
        private ItemColumnFlags _sortItemsColumn = ItemColumnFlags.ItemType;
        private UISortToggle _activeSortToggle = null;
        private bool? _isShown = null;
        #endregion


        #region Events
        public event Action Event_OnItemFilterChanged = null;
        public event Action Event_OnSortItemsColumnChanged = null;
        public event Action Event_OnSortAscendingChanged = null;
        #endregion


        #region Properties
        public int itemCount { get { return _uidToItemEntryDict != null ? _uidToItemEntryDict.Count : 0; } }
        public ItemFilter activeItemFilter { get { return ItemFilter.All; } }
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

            #region Sort Toggle References
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

            #region Text References
            Transform goldTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_totalGold);
            _text_totalGold = goldTform != null ? goldTform.GetComponent<TextMeshProUGUI>() : null;

            Transform weightTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_totalWeight);
            _text_totalWeight = weightTform != null ? weightTform.GetComponent<TextMeshProUGUI>() : null;
            #endregion
        }

        public void Cleanup()
        {
            StopAllCoroutines();

            Event_OnItemFilterChanged = null;
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

        public void ResetList()
        {
            StopAllCoroutines();
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
                gameObject.SetActive(true);
            }
        }

        public void Hide()
        {
            if (_isShown == null || _isShown.Value)
            {
                _isShown = false;
                gameObject.SetActive(false);
            }
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

        public void SetTotalGoldText(string totalGoldStr)
        {
            if (_text_totalGold != null)
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
        }

        public void SetItemColumnFlags(ItemColumnFlags filterFlags)
        {
            if (_sortToggle_name != null) { _sortToggle_name.gameObject.SetActive((filterFlags & ItemColumnFlags.Name) != 0); }
            if (_sortToggle_value != null) { _sortToggle_value.gameObject.SetActive((filterFlags & ItemColumnFlags.GoldValue) != 0); }
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
