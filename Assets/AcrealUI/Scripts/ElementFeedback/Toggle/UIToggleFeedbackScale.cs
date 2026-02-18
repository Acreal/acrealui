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
    public class UIToggleFeedbackScale : UIToggleFeedback
    {
        #region Variables
        [SerializeField] private float _scaleWhenOn = 1.0f;
        [SerializeField] private float _scaleWhenOff = 1.0f;
        [SerializeField] private float _scaleWhenOnAndPressed = 0.9f;
        [SerializeField] private float _scaleWhenOffAndPressed = 0.9f;
        [SerializeField] private float _scaleWhenOnAndHighlighted = 1.1f;
        [SerializeField] private float _scaleWhenOffAndHighlighted = 1.1f;
        [SerializeField] private float _scaleWhenDisabled = 1.0f;
        #endregion


        #region Update
        public override void Refresh()
        {
            base.Refresh();

            if (_interactiveElement.isDisabled)
            {
                transform.localScale = Vector3.one * _scaleWhenDisabled;
            }
            else if (toggle != null)
            {
                if (toggle.isToggledOn)
                {
                    if (_interactiveElement.isPressed) { transform.localScale = Vector3.one * _scaleWhenOnAndPressed; }
                    else if (_interactiveElement.isHighlighted) { transform.localScale = Vector3.one * _scaleWhenOnAndHighlighted; }
                    else { transform.localScale = Vector3.one * _scaleWhenOn; }
                }
                else
                {
                    if (_interactiveElement.isPressed) { transform.localScale = Vector3.one * _scaleWhenOffAndPressed; }
                    else if (_interactiveElement.isHighlighted) { transform.localScale = Vector3.one * _scaleWhenOffAndHighlighted; }
                    else { transform.localScale = Vector3.one * _scaleWhenOff; }
                }
            }
            else
            {
                transform.localScale = Vector3.one * _scaleWhenOff;
            }
        }
        #endregion
    }
}
