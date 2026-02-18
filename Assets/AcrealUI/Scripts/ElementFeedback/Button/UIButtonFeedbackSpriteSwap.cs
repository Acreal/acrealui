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
    public class UIButtonFeedbackSpriteSwap : UIInteractiveElementFeedback
    {
        #region Variables
        [SerializeField] private string _defaultSpriteName = null;
        [SerializeField] private string _highlightedSpriteName = null;
        [SerializeField] private string _pressedSpriteName = null;
        [SerializeField] private string _disabledSpriteName = null;

        private Sprite _defaultSprite = null;
        private Sprite _highlightedSprite = null;
        private Sprite _pressedSprite = null;
        private Sprite _disabledSprite = null;

        private Image _image = null;
        #endregion


        #region Initialization
        public override void Initialize(UIInteractiveElement uiElement)
        {
            base.Initialize(uiElement);

            if (_image == null)
            {
                _image = GetComponent<Image>();
            }

            if (_defaultSprite == null) { _defaultSprite = UIManager.mod.GetAsset<Sprite>(_defaultSpriteName); }
            if (_highlightedSprite == null) { _highlightedSprite = UIManager.mod.GetAsset<Sprite>(_highlightedSpriteName); }
            if (_pressedSprite == null) { _pressedSprite = UIManager.mod.GetAsset<Sprite>(_pressedSpriteName); }
            if (_disabledSprite == null) { _disabledSprite = UIManager.mod.GetAsset<Sprite>(_disabledSpriteName); }
        }
        #endregion


        #region Update
        public override void Refresh()
        {
            if (_image == null) { return; }

            Sprite sprite = null;

            if (_interactiveElement.isDisabled)
            {
                sprite = _disabledSprite;
            }
            else if (_interactiveElement.isPressed)
            {
                sprite = _pressedSprite;
            }
            else if (_interactiveElement.isHighlighted)
            {
                sprite = _highlightedSprite;
            }

            if (sprite == null) { sprite = _defaultSprite; }

            if (sprite != null && _image.sprite != sprite)
            {
                _image.sprite = sprite;
            }
        }
        #endregion
    }
}
