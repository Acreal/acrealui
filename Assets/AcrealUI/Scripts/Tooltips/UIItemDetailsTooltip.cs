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
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIItemDetailsTooltip : UITextIconTooltip
    {
        #region Variables
        [SerializeField] private string _gameObjName_text_statEntries_parent = null;
        [SerializeField] private string _gameObjName_itemPowersParent = null;
        [SerializeField] private string _gameObjName_itemPowerEntriesParent = null;

        private Transform _itemStatEntriesParent = null;
        private List<UIItemStatTooltipEntry> _statEntries = null;

        private Transform _itemPowersParent = null;
        private Transform _itemPowerEntriesParent = null;
        private List<UIItemPowerTooltipEntry> _powerEntries = null;
        #endregion


        #region Initalization
        public override void Initalize()
        {
            base.Initalize();

            _statEntries = new List<UIItemStatTooltipEntry>(3);
            _powerEntries = new List<UIItemPowerTooltipEntry>(4);

            if (_itemStatEntriesParent == null && !string.IsNullOrEmpty(_gameObjName_text_statEntries_parent))
            {
                _itemStatEntriesParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_statEntries_parent);
            }

            if (_itemPowersParent == null && !string.IsNullOrEmpty(_gameObjName_itemPowersParent))
            {
                _itemPowersParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemPowersParent);
            }

            if (_itemPowerEntriesParent == null && !string.IsNullOrEmpty(_gameObjName_itemPowerEntriesParent))
            {
                _itemPowerEntriesParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemPowerEntriesParent);
            }
        }
        #endregion


        #region Public API
        public UIItemStatTooltipEntry AddItemStat()
        {
            //TODO(Acreal): add object pooling
            if (_itemStatEntriesParent != null && UIManager.referenceManager.prefab_tooltip_itemStatEntry != null)
            {
                UIItemStatTooltipEntry entry = Object.Instantiate(UIManager.referenceManager.prefab_tooltip_itemStatEntry, _itemStatEntriesParent);
                entry.transform.localScale = Vector3.one;
                entry.Initialize();
                _statEntries.Add(entry);
                return entry;
            }
            return null;
        }

        public UIItemStatSliderTooltipEntry AddItemStatSlider()
        {
            //TODO(Acreal): add object pooling
            if (_itemStatEntriesParent != null && UIManager.referenceManager.prefab_tooltip_itemStatSliderEntry != null)
            {
                UIItemStatSliderTooltipEntry entry = Object.Instantiate(UIManager.referenceManager.prefab_tooltip_itemStatSliderEntry, _itemStatEntriesParent);
                entry.transform.localScale = Vector3.one;
                entry.Initialize();
                _statEntries.Add(entry);
                return entry;
            }
            return null;
        }

        /// <summary>
        /// removes all active item stat entries and clears the display
        /// </summary>
        public void ClearItemStats()
        {
            //TODO(Acreal): add object pooling
            if (_statEntries != null && _statEntries.Count > 0)
            {
                for (int i = _statEntries.Count - 1; i > -1; i--)
                {
                    Object.Destroy(_statEntries[i].gameObject);
                }
                _statEntries.Clear();
            }
        }

        public UIItemPowerTooltipEntry AddItemPower()
        {
            //TODO(Acreal): add object pooling
            if (_itemPowerEntriesParent != null && UIManager.referenceManager.prefab_tooltip_itemPowerEntry != null)
            {
                UIItemPowerTooltipEntry entry = Object.Instantiate(UIManager.referenceManager.prefab_tooltip_itemPowerEntry, _itemPowerEntriesParent);
                entry.transform.localScale = Vector3.one;
                entry.Initialize();
                _powerEntries.Add(entry);
                return entry;
            }
            return null;
        }

        public void EnableOrDisableItemPowerDisplay(bool enable)
        {
            if(_itemPowersParent != null)
            {
                _itemPowersParent.gameObject.SetActive(enable);
            }
        }

        /// <summary>
        /// removes all active item power entries and clears the display
        /// </summary>
        public void ClearItemPowers()
        {
            //TODO(Acreal): add object pooling
            if(_powerEntries != null && _powerEntries.Count > 0)
            {
                for (int i = _powerEntries.Count - 1; i > -1; i--)
                {
                    Object.Destroy(_powerEntries[i].gameObject);
                }
                _powerEntries.Clear();
            }
        }
        #endregion
    }
}
