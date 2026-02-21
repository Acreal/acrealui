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
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UISlider : UIInteractiveElement
    {
        #region Variables
        [SerializeField] private string _gameObjName_slider = null;
        [SerializeField] private string _gameObjName_text_sliderTitle = null;
        [SerializeField] private string _gameObjName_text_sliderValue = null;

        private Slider _slider = null;
        private TextMeshProUGUI _text_sliderName = null;
        private TextMeshProUGUI _text_sliderValue = null;
        #endregion


        #region Events
        public event Action<UISlider> Event_OnSliderValueChanged = null;
        #endregion


        #region Data Sources
        public UIDelegates.DataSourceDelegate_Float DataSource_SliderValue = null;
        public UIDelegates.DataSourceDelegate_String DataSource_SliderValueString = null;
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            if (!string.IsNullOrEmpty(_gameObjName_text_sliderTitle))
            {
                Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_sliderTitle);
                _text_sliderName = nameTform != null ? nameTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (!string.IsNullOrEmpty(_gameObjName_text_sliderValue))
            {
                Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_sliderValue);
                _text_sliderValue = nameTform != null ? nameTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (!string.IsNullOrEmpty(_gameObjName_slider))
            {
                Transform sliderTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_slider);
                _slider = sliderTform != null ? sliderTform.GetComponent<Slider>() : null;
                if (_slider != null)
                {
                    _slider.onValueChanged.AddListener((float val) =>
                    {
                        if (Event_OnSliderValueChanged != null)
                        {
                            Event_OnSliderValueChanged(this);
                        }

                        if (DataSource_SliderValueString != null)
                        {
                            SetTextValue(DataSource_SliderValueString(gameObject));
                        }
                    });
                }
            }

            base.Initialize();
        }
        #endregion


        #region Updates
        public override void Refresh()
        {
            base.Refresh();

            if (_slider != null)
            {
                _slider.interactable = !isDisabled;
            }

            if (DataSource_SliderValue != null)
            {
                _slider.SetValueWithoutNotify(DataSource_SliderValue(gameObject));
            }

            if(DataSource_SliderValueString != null)
            {
                _text_sliderValue.text = DataSource_SliderValueString(gameObject);
            }
        }
        #endregion


        #region Public API
        public float GetSliderValue()
        {
            return _slider != null ? _slider.value : 0.0f;
        }

        public int GetSliderValueAsInt()
        {
            return _slider != null ? (int)_slider.value : 0;
        }

        public void SetSliderValue(float value, bool setWithoutNotify)
        {
            if(_slider != null)
            {
                if (setWithoutNotify)
                {
                    _slider.SetValueWithoutNotify(value);

                    if (DataSource_SliderValueString != null)
                    {
                        SetTextValue(DataSource_SliderValueString(gameObject));
                    }
                }
                else { _slider.value = value; }
            }
        }

        public void SetSliderMinMax(float minValue, float maxValue, bool useWholeNumbersOnly)
        {
            if (_slider != null)
            {
                _slider.minValue = minValue;
                _slider.maxValue = maxValue;
                _slider.wholeNumbers = useWholeNumbersOnly;
            }
        }

        public void SetTextTitle(string text)
        {
            if(_text_sliderName != null)
            {
                _text_sliderName.text = text;
            }
        }
        
        public void SetTextValue(string text)
        {
            if(_text_sliderValue != null)
            {
                _text_sliderValue.text = text;
            }
        }
        #endregion
    }
}
