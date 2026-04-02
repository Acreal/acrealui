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
    public class UISliderConfirmationWindow : UIConfirmationWindow
    {
        #region Variables
        [SerializeField] private string _gameObjName_slider = null;
        [SerializeField] private string _gameObjName_sliderTitleText = null;
        [SerializeField] private string _gameObjName_sliderValueText = null;

        private UISlider _slider = null;
        private TextMeshProUGUI _sliderTitleText = null;
        private TextMeshProUGUI _sliderValueText = null;
        #endregion


        #region Properties
        public float sliderValue
        {
            get { return _slider != null ? _slider.GetSliderValue() : 0f; }
        }

        public int sliderValueAsInt
        {
            get { return _slider != null ? _slider.GetSliderValueAsInt() : 0; }
        }
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            Transform sliderTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_slider);
            if (sliderTform != null)
            {
                _slider = sliderTform.GetComponent<UISlider>();
                if (_slider != null)
                {
                    _slider.Initialize();
                    _slider.Event_OnSliderValueChanged += (UISlider slider) =>
                    {
                        if (slider.useWholeNumbers)
                        {
                            slider.SetDisplayValue(slider.GetSliderValueAsInt().ToString("N0"));
                        }
                        else
                        {
                            slider.SetDisplayValue(slider.GetSliderValue().ToString("F2"));
                        }
                    };
                }
            }

            Transform titleTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_sliderTitleText);
            if (titleTform != null)
            {
                _sliderTitleText = titleTform.GetComponent<TextMeshProUGUI>();
            }

            Transform valueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_sliderValueText);
            if (valueTform != null)
            {
                _sliderValueText = valueTform.GetComponent<TextMeshProUGUI>();
            }
        }
        #endregion


        #region Public API
        public void SetSliderMinMax(float min, float max, bool useWholeNumbers)
        {
            if (_slider != null)
            {
                _slider.SetSliderMinMax(min, max, useWholeNumbers);
            }
        }
        #endregion
    }
}
