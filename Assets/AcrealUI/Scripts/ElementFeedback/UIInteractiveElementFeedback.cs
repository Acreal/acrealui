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
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIInteractiveElementFeedback : MonoBehaviour
    {
        #region Variables
        protected bool _isDisabled = false;
        protected bool _isPressed = false;
        protected bool _isHighlighted = false;
        #endregion


        #region MonoBehaviour
        protected virtual void OnDisable()
        {
            _isDisabled = true;
            _isPressed = false;
            _isHighlighted = false;
        }

        private void OnEnable()
        {
            _isDisabled = false;
            UpdateDisplay();
        }
        #endregion


        #region Initialization
        public virtual void Initialize(UIInteractiveElement uiElement)
        {
            uiElement.Event_OnDisabledChanged += (bool disabled) =>
            {
                _isDisabled = disabled;
                UpdateDisplay();
            };
        }
        #endregion


        #region Update
        protected virtual void UpdateDisplay()
        {

        }
        #endregion


        #region IPointer
        public virtual void OnPointerEnter()
        {
            _isHighlighted = true;
            UpdateDisplay();
        }

        public virtual void OnPointerExit()
        {
            _isHighlighted = false;
            UpdateDisplay();
        }

        public virtual void OnPointerDown()
        {
            _isPressed = true;
            UpdateDisplay();
        }

        public virtual void OnPointerUp()
        {
            _isPressed = false;
            UpdateDisplay();
        }

        public virtual void OnPointerClick()
        {

        }
        #endregion
    }
}
