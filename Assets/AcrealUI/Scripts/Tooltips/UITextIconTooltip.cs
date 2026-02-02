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
    public class UITextIconTooltip : UITextTooltip
    {
        private const float _ICON_DEFAULT_SIZE = 96f;
        private const float _ICON_MAX_SIZE = _ICON_DEFAULT_SIZE * 1.5f;

        [SerializeField] private string _gameObjName_iconImage = null;
        [SerializeField] private string _gameObjName_iconLayoutElement = null;

        private Image _iconImage = null;
        private LayoutElement _iconLayoutElement = null;


        public override void Initalize()
        {
            base.Initalize();

            if (_iconImage == null && !string.IsNullOrEmpty(_gameObjName_iconImage))
            {
                Transform iconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_iconImage);
                if (iconTform != null) { _iconImage = iconTform.GetComponent<Image>(); }
            }

            if (_iconLayoutElement == null && !string.IsNullOrEmpty(_gameObjName_iconLayoutElement))
            {
                Transform layoutTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_iconLayoutElement);
                if (layoutTform != null)
                { 
                    _iconLayoutElement = layoutTform.GetComponent<LayoutElement>();
                    _iconLayoutElement.minWidth = _ICON_DEFAULT_SIZE;
                    _iconLayoutElement.minHeight = _ICON_DEFAULT_SIZE;
                }
            }
        }

        public void SetIcon(Sprite sprite)
        {
            if(_iconImage != null)
            {
                _iconImage.sprite = sprite;
                _iconImage.gameObject.SetActive(sprite != null);

                if (_iconLayoutElement != null)
                {
                    if (sprite != null)
                    {
                        float aspect = sprite.rect.width / sprite.rect.height;
                        _iconLayoutElement.minHeight = Mathf.Clamp(_iconLayoutElement.minWidth / aspect, _ICON_DEFAULT_SIZE, _ICON_MAX_SIZE);
                    }
                    else
                    {
                        _iconLayoutElement.minHeight = _ICON_MAX_SIZE;
                    }
                }
            }
        }
    }
}
