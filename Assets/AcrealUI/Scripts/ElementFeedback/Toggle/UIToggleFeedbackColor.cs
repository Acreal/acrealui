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
    public class UIToggleFeedbackColor : UIToggleFeedback
    {
        #region Variables
        [SerializeField] private Color _colorWhenOn = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _colorWhenOff = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _colorWhenOnAndPressed = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _colorWhenOffAndPressed = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _colorWhenOnAndHighlighted = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _colorWhenOffAndHighlighted = new Color(0.9f, 0.9f, 0.9f, 1f);
        [SerializeField] private Color _colorOnDisable = new Color(0.9f, 0.9f, 0.9f, 1f);

        private Graphic _graphic = null;
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            base.Initialize(uiElement);
            
            if(_graphic == null)
            {
                _graphic = GetComponent<Graphic>();
            }
        }
        #endregion


        #region Update
        public override void Refresh()
        {
            base.Refresh();

            if (_graphic != null)
            {
                UIInteractiveElement elem = _uiElement as UIInteractiveElement;
                if (elem != null)
                {
                    if (elem.isDisabled)
                    {
                        _graphic.color = _colorOnDisable;
                    }
                    else if (toggle != null)
                    {
                        if (toggle.isToggledOn)
                        {
                            if (elem.isPressed) { _graphic.color = _colorWhenOnAndPressed; }
                            else if (elem.isHighlighted) { _graphic.color = _colorWhenOnAndHighlighted; }
                            else { _graphic.color = _colorWhenOn; }
                        }
                        else
                        {
                            if (elem.isPressed) { _graphic.color = _colorWhenOffAndPressed; }
                            else if (elem.isHighlighted) { _graphic.color = _colorWhenOffAndHighlighted; }
                            else { _graphic.color = _colorWhenOff; }
                        }
                    }
                }
            }
        }
        #endregion
    }
}
