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
using System.Collections.ObjectModel;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIScrollListRow : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string _parent_groupEntriesGameObjName = null;

        private List<UIElement> _uiElements = null;
        private Transform _parent_groupEntries = null;
        #endregion


        #region Properties
        public ReadOnlyCollection<UIElement> uiElementsRO { get; private set; }
        #endregion


        #region Initialization
        public void Initialize()
        {
            _uiElements = new List<UIElement>();
            uiElementsRO = _uiElements.AsReadOnly();

            _parent_groupEntries = UIUtilityFunctions.FindDeepChild(transform, _parent_groupEntriesGameObjName);
            if(_parent_groupEntries == null) { _parent_groupEntries = transform; }
        }
        #endregion


        #region Public API
        public UIElement AddElement(UIElement elementPrefab)
        {
            if (elementPrefab == null) { return null; }

            UIElement elem = Instantiate(elementPrefab, _parent_groupEntries);
            elem.Initialize();
            elem.transform.localScale = Vector3.one;
            _uiElements.Add(elem);
            return elem;
        }

        public void Refresh()
        {
            if (_uiElements != null)
            {
                foreach (UIElement element in _uiElements)
                {
                    if (element != null)
                    {
                        element.Refresh();
                    }
                }
            }
        }
        #endregion
    }
}
