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

using UnityEngine;
using TMPro;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;


namespace AcrealUI
{
    /// <summary>
    /// a toggle that includes a text component that is changed when toggled
    /// </summary>
    [ImportedComponent]
    public class UITextToggle : UIToggle
    {
        #region Variables
        [SerializeField] private string _gameObjName_valueText = null;
        [SerializeField] private string _toggledOnText = "On";
        [SerializeField] private string _toggledOffText = "Off";
        [SerializeField] private bool _messagesAreLocalizationKeys = false;
        [SerializeField] private string localizationTableID = null;

        private TextMeshProUGUI text_value = null;
        #endregion


        #region Properties
        public string toggledOnText
        {
            get { return _toggledOnText; }
            set
            {
                _toggledOnText = value;
                UpdateTextValue();
            }
        }

        public string toggledOffText
        {
            get { return _toggledOffText; }
            set
            {
                _toggledOffText = value;
                UpdateTextValue();
            }
        }

        public bool messagesAreLocalizationKeys
        {
            get { return _messagesAreLocalizationKeys; }
            set
            {
                if (_messagesAreLocalizationKeys != value)
                {
                    _messagesAreLocalizationKeys = value;
                    UpdateTextValue();
                }
            }
        }
        #endregion


        #region MonoBehaviour
        public override void Initialize()
        {
            base.Initialize();

            Transform textTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_valueText);
            text_value = textTform != null ? textTform.GetComponent<TextMeshProUGUI>() : null;

            Event_OnToggledOnOrOff += (UIToggle toggle) => { UpdateTextValue(); };
            Event_OnDisabledChanged += (bool disabled) => 
            {
                UpdateTextValue();
            };
        }

        private void Start()
        {
            UpdateTextValue();
        }
        #endregion


        #region Update Text
        private void UpdateTextValue()
        {
            if(text_value != null)
            {
                if (messagesAreLocalizationKeys)
                {
                    text_value.text = !isDisabled && isToggledOn ? TextManager.Instance.GetText(localizationTableID, _toggledOnText) : TextManager.Instance.GetText(localizationTableID, _toggledOffText);
                }
                else
                {
                    text_value.text = !isDisabled && isToggledOn ? _toggledOnText : _toggledOffText;
                }
            }
        }
        #endregion
    }
}
