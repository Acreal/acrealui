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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIItemPowerTooltipEntry : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string _gameObjName_image_icon = null;
        [SerializeField] private string _gameObjName_text_title = null;
        [SerializeField] private string _gameObjName_text_description = null;

        private Image _image_icon = null;
        private TextMeshProUGUI _text_title = null;
        private TextMeshProUGUI _text_description = null;
        #endregion


        #region Initialization
        public void Initialize()
        {
            if(_image_icon == null && !string.IsNullOrEmpty(_gameObjName_image_icon))
            {
                Transform iconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_image_icon);
                if(iconTform != null) { _image_icon = iconTform.GetComponent<Image>(); }
            }

            if (_text_title == null && !string.IsNullOrEmpty(_gameObjName_text_title))
            {
                Transform titleTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_title);
                if (titleTform != null) { _text_title = titleTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (_text_description == null && !string.IsNullOrEmpty(_gameObjName_text_description))
            {
                Transform descTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_description);
                if (descTform != null) { _text_description = descTform.GetComponent<TextMeshProUGUI>(); }
            }
        }
        #endregion


        #region Public API
        public void SetIcon(Sprite icon)
        {
            if(_image_icon != null)
            {
                _image_icon.sprite = icon;
                _image_icon.gameObject.SetActive(icon != null);
            }
        }

        public void SetTitle(string title)
        {
            if (_text_title != null)
            {
                _text_title.text = title;
                _text_title.gameObject.SetActive(title != null);
            }
        }

        public void SetDescription(string description)
        {
            if (_text_description != null)
            {
                _text_description.text = description;
                _text_description.gameObject.SetActive(description != null);
            }
        }
        #endregion
    }
}
