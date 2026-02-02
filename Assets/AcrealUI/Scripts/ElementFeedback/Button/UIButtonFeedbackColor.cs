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
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIButtonFeedbackColor : UIInteractiveElementFeedback
    {
        #region Variables
        [SerializeField] private Color _defaultColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _highlightedColor = new Color(1f, 1f, 1f, 1f);
        [SerializeField] private Color _pressedColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private Color _disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

        private Graphic _graphic = null;
        #endregion


        #region Initialization
        public override void Initialize(UIInteractiveElement uiElement)
        {
            base.Initialize(uiElement);

            if (_graphic == null)
            {
                _graphic = GetComponent<Graphic>();
            }
        }
        #endregion


        #region Graphic Color
        protected override void UpdateDisplay()
        {
            if (_graphic == null) { return; }

            if (_isDisabled)
            {
                _graphic.color = _disabledColor;
            }
            else if (_isPressed)
            {
                _graphic.color = _pressedColor;
            }
            else if (_isHighlighted)
            {
                _graphic.color = _highlightedColor;
            }
            else
            {
                _graphic.color = _defaultColor;
            }
        }
        #endregion
    }
}
