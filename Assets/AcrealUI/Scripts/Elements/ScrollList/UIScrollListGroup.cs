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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using TMPro;
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIScrollListGroup : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string _parent_groupEntriesGameObjName = null;
        [SerializeField] private string _text_groupTitleGameObjName = null;
        [SerializeField] private string _toggle_expandCollapseGameObjName = null;

        private Dictionary<string, UIScrollListGroup> _subScrollListGroupDict = null;
        private List<UIScrollListRow> _scrollRows = null;
        private List<UIElement> _uiElements = null;
        private Transform _parent_groupEntries = null;
        private TextMeshProUGUI _text_groupTitle = null;
        private UIToggle _toggle_expandCollapse = null;
        #endregion


        #region Properties
        public Transform groupParent { get { return _parent_groupEntries; } }
        public ReadOnlyCollection<UIScrollListRow> scrollRowsRO { get; private set; }
        public ReadOnlyCollection<UIElement> uiElementsRO { get; private set; }
        #endregion


        #region Initialize
        public virtual void Initialize()
        {
            _subScrollListGroupDict = new Dictionary<string, UIScrollListGroup>();

            _scrollRows = new List<UIScrollListRow>();
            scrollRowsRO = _scrollRows.AsReadOnly();

            _uiElements = new List<UIElement>();
            uiElementsRO = _uiElements.AsReadOnly();

            _parent_groupEntries = UIUtilityFunctions.FindDeepChild(transform, _parent_groupEntriesGameObjName);
            if (_parent_groupEntries != null)
            {
                _parent_groupEntries.gameObject.SetActive(true);
            }

            Transform titleTform = UIUtilityFunctions.FindDeepChild(transform, _text_groupTitleGameObjName);
            if (titleTform != null)
            {
                _text_groupTitle = titleTform.GetComponent<TextMeshProUGUI>();
            }

            Transform expColTform = UIUtilityFunctions.FindDeepChild(transform, _toggle_expandCollapseGameObjName);
            if (expColTform != null)
            {
                _toggle_expandCollapse = expColTform.GetComponent<UIToggle>();
                if (_toggle_expandCollapse != null)
                {
                    _toggle_expandCollapse.Initialize();
                    _toggle_expandCollapse.Event_OnToggledOnOrOff += (UIToggle toggle) =>
                    {
                        if (_parent_groupEntries != null && toggle != null)
                        {
                            _parent_groupEntries.gameObject.SetActive(toggle.isToggledOn);
                        }
                    };
                    _toggle_expandCollapse.isToggledOn = true;
                }
            }
        }
        #endregion


        #region Public API
        public void Expand()
        {
            _toggle_expandCollapse.isToggledOn = true;
        }

        public void Collapse()
        {
            _toggle_expandCollapse.isToggledOn = false;
        }

        public void Refresh()
        {
            if (_subScrollListGroupDict != null)
            {
                foreach (UIScrollListGroup group in _subScrollListGroupDict.Values)
                {
                    group.Refresh();
                }
            }

            if (_scrollRows != null)
            {
                foreach (UIScrollListRow row in _scrollRows)
                {
                    row.Refresh();
                }
            }

            if (_uiElements != null)
            {
                foreach (UIElement element in _uiElements)
                {
                    element.Refresh();
                }
            }
        }

        public void SetTextTitle(string text)
        {
            if(_text_groupTitle != null)
            {
                _text_groupTitle.text = text;
                _text_groupTitle.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }

        public virtual UIScrollListGroup GetOrAddSubScrollListGroup(string bindingGroupName)
        {
            UIScrollListGroup scrollGroup = null;
            if (bindingGroupName != null)
            {
                if (!_subScrollListGroupDict.TryGetValue(bindingGroupName, out scrollGroup))
                {
                    scrollGroup = PopScrollListGroupFromPool(UIManager.referenceManager.prefab_subScrollListGroup, groupParent);
                    if (scrollGroup != null)
                    {
                        scrollGroup.Initialize();
                        scrollGroup.SetTextTitle(bindingGroupName);
                    }

                    _subScrollListGroupDict.Add(bindingGroupName, scrollGroup);
                }
            }
            return scrollGroup;
        }

        public UIScrollListRow AddRow()
        {
            if(UIManager.referenceManager.prefab_scrollListRow != null)
            {
                UIScrollListRow scrollRow = Instantiate(UIManager.referenceManager.prefab_scrollListRow, _parent_groupEntries);
                if(scrollRow != null)
                {
                    scrollRow.Initialize();
                    _scrollRows.Add(scrollRow);
                    return scrollRow;
                }
            }
            return null;
        }

        public UIElement AddElement(UIElement elementPrefab)
        {
            if (elementPrefab == null) { return null; }

            UIElement elem = Instantiate(elementPrefab, _parent_groupEntries);
            elem.Initialize();
            elem.transform.localScale = Vector3.one;
            _uiElements.Add(elem);
            return elem;
        }
        #endregion


        #region ScrollListGroup Management
        private UIScrollListGroup PopScrollListGroupFromPool(UIScrollListGroup scrollListGroupPrefab, Transform parent)
        {
            //TODO(Acreal): add (global?) pooling
            UIScrollListGroup group = Instantiate(scrollListGroupPrefab, parent);
            group.transform.localScale = Vector3.one;
            return group;
        }
        #endregion
    }
}
