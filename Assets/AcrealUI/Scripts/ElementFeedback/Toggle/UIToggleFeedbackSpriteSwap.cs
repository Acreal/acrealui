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
    public class UIToggleFeedbackSpriteSwap : UIToggleFeedback
    {
        #region Variables
        [SerializeField] private string _toggledOnSpriteName = null;
        [SerializeField] private string _toggledOffSpriteName = null;
        [SerializeField] private string _toggledOnAndPressedSpriteName = null;
        [SerializeField] private string _toggledOffAndPressedSpriteName = null;
        [SerializeField] private string _toggledOnAndHighlightedSpriteName = null;
        [SerializeField] private string _toggledOffAndHighlightedSpriteName = null;
        [SerializeField] private string _disabledSpriteName = null;

        private Image _image = null;
        private Sprite _toggledOnSprite = null;
        private Sprite _toggledOffSprite = null;
        private Sprite _toggledOnAndPressedSprite = null;
        private Sprite _toggledOffAndPressedSprite = null;
        private Sprite _toggledOnAndHighlightedSprite = null;
        private Sprite _toggledOffAndHighlightedSprite = null;
        private Sprite _disabledSprite = null;
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            base.Initialize(uiElement);

            if(_image == null) { _image = GetComponent<Image>(); }
            if (_toggledOnSprite == null) { _toggledOnSprite = UIManager.mod.GetAsset<Sprite>(_toggledOnSpriteName); }
            if (_toggledOffSprite == null) { _toggledOffSprite = UIManager.mod.GetAsset<Sprite>(_toggledOffSpriteName); }
            if (_toggledOnAndPressedSprite == null) { _toggledOnAndPressedSprite = UIManager.mod.GetAsset<Sprite>(_toggledOnAndPressedSpriteName); }
            if (_toggledOffAndPressedSprite == null) { _toggledOffAndPressedSprite = UIManager.mod.GetAsset<Sprite>(_toggledOffAndPressedSpriteName); }
            if (_toggledOnAndHighlightedSprite == null) { _toggledOnAndHighlightedSprite = UIManager.mod.GetAsset<Sprite>(_toggledOnAndHighlightedSpriteName); }
            if (_toggledOffAndHighlightedSprite == null) { _toggledOffAndHighlightedSprite = UIManager.mod.GetAsset<Sprite>(_toggledOffAndHighlightedSpriteName); }
            if (_disabledSprite == null) { _disabledSprite = UIManager.mod.GetAsset<Sprite>(_disabledSpriteName); }
        }
        #endregion


        #region Update/Refresh
        public override void Refresh()
        {
            base.Refresh();

            if (_image != null)
            {
                UIInteractiveElement elem = _uiElement as UIInteractiveElement;
                if (elem != null)
                {
                    Sprite sprite = null;

                    if (elem.isDisabled)
                    {
                        sprite = _disabledSprite;
                    }
                    else if (toggle != null)
                    {
                        if (toggle.isToggledOn)
                        {
                            if (elem.isPressed) { sprite = _toggledOnAndPressedSprite; }
                            else if (elem.isHighlighted) { sprite = _toggledOnAndHighlightedSprite; }
                            else { sprite = _toggledOnSprite; }
                        }
                        else
                        {
                            if (elem.isPressed) { sprite = _toggledOffAndPressedSprite; }
                            else if (elem.isHighlighted) { sprite = _toggledOffAndHighlightedSprite; }
                            else { sprite = _toggledOffSprite; }
                        }
                    }
                    else
                    {
                        sprite = _toggledOffSprite;
                    }

                    if (_image.sprite != sprite)
                    {
                        _image.sprite = sprite;
                    }
                }
            }
        }
        #endregion
    }
}
