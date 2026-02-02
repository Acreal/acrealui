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

/*
UISortToggle.cs

Represents a toggle with an additional bool for sorting by ascending or descending order.

Expected use case is sorting buttons on a scroll list: 
When clicked, it can begin displaying an up or down arrow, which is toggled back and forth with further clicks.
*/

using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AcrealUI
{
    [ImportedComponent]
    public class UISortToggle : UIToggle
    {
        #region Variables
        [SerializeField] private bool _sortAscendingByDefault = false;

        private bool _sortAscending = false;
        #endregion


        #region Events
        public event System.Action<bool> Event_OnSortAscendingChanged = null;
        #endregion


        #region Properties
        public bool sortByAscending
        {
            get { return _sortAscending; }
        }
        #endregion


        #region Input
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (canBeToggledOff)
            {
                //if this can be toggled off, then we have to handle it a little differently
                //because a second click in base.OnPointerClick will toggle it off instead of
                //toggling _sortAscending back and forth
                if (!_isToggledOn)
                {
                    _sortAscending = _sortAscendingByDefault;
                    base.OnPointerClick(eventData);
                }
                else
                {
                    _sortAscending = !_sortAscending;

                    foreach (UIInteractiveElementFeedback element in _elementFeedbackArray)
                    {
                        element?.OnPointerClick();
                    }
                }
            }
            else
            {
                if (!_isToggledOn)
                {
                    _sortAscending = _sortAscendingByDefault;
                }
                else
                {
                    _sortAscending = !_sortAscending;
                }
            }

            base.OnPointerClick(eventData);

            Event_OnSortAscendingChanged?.Invoke(_sortAscending);
        }
        #endregion
    }
}
