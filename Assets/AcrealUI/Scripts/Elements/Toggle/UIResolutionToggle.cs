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

namespace AcrealUI
{
    [ImportedComponent]
    public class UIResolutionToggle : UIToggle
    {
        #region Variables
        [SerializeField] private string _gameObjName_resolutionText = null;
        [SerializeField] private string _gameObjName_refreshRateText = null;

        private TextMeshProUGUI _resolutionText = null;
        private TextMeshProUGUI _refreshRateText = null;
        #endregion


        #region Properties
        public Resolution resolution { get; set; }
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            if (!string.IsNullOrEmpty(_gameObjName_resolutionText))
            {
                Transform resolutionTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_resolutionText);
                _resolutionText = resolutionTform != null ? resolutionTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (!string.IsNullOrEmpty(_gameObjName_refreshRateText))
            {
                Transform refreshTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_refreshRateText);
                _refreshRateText = refreshTform != null ? refreshTform.GetComponent<TextMeshProUGUI>() : null;
            }
        }
        #endregion


        #region Public API
        public void SetResolutionText(string resolution)
        {
            if(_resolutionText != null)
            {
                _resolutionText.text = resolution;
            }
        }

        public void SetRefreshRateText(string refreshRate)
        {
            if (_refreshRateText != null)
            {
                _refreshRateText.text = refreshRate;
            }
        }
        #endregion
    }
}
