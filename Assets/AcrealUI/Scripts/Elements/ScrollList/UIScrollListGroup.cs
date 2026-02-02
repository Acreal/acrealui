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

        public ReadOnlyCollection<UIInteractiveElement> uiElementsRO = null;

        protected List<UIInteractiveElement> _uiElements = null;
        protected Transform _parent_groupEntries = null;
        private TextMeshProUGUI _text_groupTitle = null;
        private UIToggle _toggle_expandCollapse = null;
        #endregion


        #region Properties
        public Transform groupParent
        {
            get { return _parent_groupEntries; }
        }
        #endregion


        #region Initialize
        public virtual void Initialize()
        {
            _uiElements = new List<UIInteractiveElement>();
            uiElementsRO = _uiElements.AsReadOnly();

            _toggle_expandCollapse = UIUtilityFunctions.FindDeepChild(transform, _toggle_expandCollapseGameObjName).GetComponent<UIToggle>();
            _toggle_expandCollapse.Initialize();
            _toggle_expandCollapse.Event_OnToggledOnOrOff += OnButtonClick_ExpandCollapse;
            _toggle_expandCollapse.isToggledOn = true;

            _parent_groupEntries = UIUtilityFunctions.FindDeepChild(transform, _parent_groupEntriesGameObjName);
            _parent_groupEntries.gameObject.SetActive(true);

            _text_groupTitle = UIUtilityFunctions.FindDeepChild(transform, _text_groupTitleGameObjName).GetComponent<TextMeshProUGUI>();
        }
        #endregion


        #region Standard UI Elements
        public UISlider AddSlider(UISlider sliderPrefab)
        {
            if (sliderPrefab == null) { return null; }

            UISlider slider = Instantiate(sliderPrefab, _parent_groupEntries);
            slider.Initialize();
            slider.transform.localScale = Vector3.one;
            _uiElements.Add(slider);
            return slider;
        }

        public UIToggle AddToggle(UIToggle togglePrefab)
        {
            if (togglePrefab == null) { return null; }

            UIToggle toggle = Instantiate(togglePrefab, _parent_groupEntries);
            toggle.Initialize();
            toggle.transform.localScale = Vector3.one;
            _uiElements.Add(toggle);
            return toggle;
        }
        #endregion


        #region UI Callbacks
        private void OnButtonClick_ExpandCollapse(UIToggle toggle)
        {
            UIUtilityFunctions.PlayButtonClick();

            if (_parent_groupEntries != null)
            {
                _parent_groupEntries.gameObject.SetActive(toggle.isToggledOn);
            }
        }
        #endregion


        #region Public API
        public void SetTextTitle(string text)
        {
            if(_text_groupTitle != null)
            {
                _text_groupTitle.text = text;
                _text_groupTitle.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }

        public UIControlBindingEntry AddBindingEntry(UIControlBindingEntry bindingEntryPrefab, string actionAsString)
        {
            if (bindingEntryPrefab == null) { return null; }

            UIControlBindingEntry bindingEntry = PopBindingEntryFromPool(bindingEntryPrefab);
            if (bindingEntry != null)
            {
                _uiElements.Add(bindingEntry);

                bindingEntry.transform.SetParent(_parent_groupEntries);
                bindingEntry.transform.localScale = Vector3.one;
            }
            return bindingEntry;
        }

        private UIControlBindingEntry PopBindingEntryFromPool(UIControlBindingEntry bindingEntryPrefab)
        {
            if (bindingEntryPrefab == null) { return null; }

            //TODO(Acreal): add pooling
            UIControlBindingEntry entry = Object.Instantiate(bindingEntryPrefab, _parent_groupEntries);
            entry.transform.localScale = Vector3.one;
            return entry;
        }
        #endregion
    }
}
