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
    public class UIButtonFeedbackScale : UIElementFeedback
    {
        #region Variables
        [SerializeField] private float _defaultScale = 1.0f;
        [SerializeField] private float _highlightedScale = 1.1f;
        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _disabledScale = 1.0f;
        #endregion


        #region Graphic Color
        public override void Refresh()
        {
            UIInteractiveElement elem = _uiElement as UIInteractiveElement;
            if (elem != null)
            {
                if (elem.isDisabled)
                {
                    transform.localScale = Vector3.one * _disabledScale;
                }
                else if (elem.isPressed)
                {
                    transform.localScale = Vector3.one * _pressedScale;
                }
                else if (elem.isHighlighted)
                {
                    transform.localScale = Vector3.one * _highlightedScale;
                }
                else
                {
                    transform.localScale = Vector3.one * _defaultScale;
                }
            }
        }
        #endregion
    }
}
