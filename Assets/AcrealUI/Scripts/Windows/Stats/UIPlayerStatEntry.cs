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
    public class UIPlayerStatEntry : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string _gameObjName_text_statName = null;
        [SerializeField] private string _gameObjName_text_statValue = null;
        [SerializeField] private string _gameObjName_image_statIcon = null;

        private TextMeshProUGUI _text_statName = null;
        private TextMeshProUGUI _text_statValue = null;
        private Image _image_statIcon = null;
        #endregion


        #region Initialization
        public void Initialize()
        {
            Transform iconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_image_statIcon);
            _image_statIcon = iconTform != null ? iconTform.GetComponent<Image>() : null;
                
            Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_statName);
            _text_statName = nameTform != null ? nameTform.GetComponent<TextMeshProUGUI>() : null;
                
            Transform valueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_statValue);
            _text_statValue = valueTform != null ? valueTform.GetComponent<TextMeshProUGUI>() : null;
        }

        public void Cleanup()
        {
            _image_statIcon = null;
            _text_statName = null;
            _text_statValue = null;
        }
        #endregion


        #region Public API
        public void SetDisplayName(string name)
        {
            if (_text_statName != null)
            {
                _text_statName.text = name;
            }
        }

        public void SetDisplayValue(string value)
        {
            if (_text_statValue != null)
            {
                _text_statValue.text = value;
            }
        }

        public void SetIcon(Sprite icon)
        {
            if(_image_statIcon != null)
            {
                _image_statIcon.sprite = icon;
            }
        }
        #endregion
    }
}
