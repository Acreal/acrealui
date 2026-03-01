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
    public class UIItemStatSliderTooltipEntry : UIItemStatTooltipEntry
    {
        #region Variables
        [SerializeField] private string _gameObjName_slider_condition = null;

        private UISlider _slider_condition = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            if (_slider_condition == null && _gameObjName_slider_condition != null)
            {
                Transform sliderTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_slider_condition);
                if (sliderTform != null)
                {
                    _slider_condition = sliderTform.GetComponent<UISlider>();
                    if (_slider_condition != null)
                    {
                        _slider_condition.Initialize();
                    }
                }
            }
        }
        #endregion


        #region Public API
        public void SetSliderValue(float value)
        {
            if (_slider_condition != null)
            {
                _slider_condition.SetSliderValue(value, false);
            }
        }

        public void SetSliderMinMaxValue(float minValue, float maxValue, bool useWholeNumbers)
        {
            if (_slider_condition != null)
            {
                _slider_condition.SetSliderMinMax(minValue, maxValue, useWholeNumbers);
            }
        }
        #endregion
    }
}
