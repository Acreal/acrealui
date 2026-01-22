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

using DaggerfallWorkshop.Game;
using TMPro;
using UnityEngine;

namespace AcrealUI
{
    public class UILocalizedText : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textComp = null;
        [SerializeField] private string _localizationTableID = null;
        [SerializeField] private string _localizationKey = null;


        #region Properties
        public string localizationKey
        {
            get
            {
                return _localizationKey;
            }
            set
            {
                _localizationKey = value;
                UpdateText();
            }
        }

        public string localizationTableID
        {
            get
            {
                return _localizationTableID;
            }
            set
            {
                _localizationTableID = value;
                UpdateText();
            }
        }
        #endregion


        #region Monobehaviour
        void Start()
        {
            UpdateText();
        }
        #endregion


        #region Text Localization
        public void UpdateText()
        {
            if (_textComp != null && !string.IsNullOrEmpty(_localizationTableID) && !string.IsNullOrEmpty(_localizationKey))
            {
                _textComp.text = TextManager.Instance.GetText(_localizationTableID, _localizationKey);
            }
        }
        #endregion
    }
}
