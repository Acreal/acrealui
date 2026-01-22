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
using DaggerfallWorkshop.Game.Utility.ModSupport;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIControlBindingEntry : UIElement
    {
        #region Variables
        [SerializeField] protected string _gameObjName_text_bindingName = null;
        [SerializeField] protected string _gameObjName_text_bindingValue_primary = null;
        [SerializeField] protected string _gameObjName_text_bindingValue_secondary = null;
        [SerializeField] protected string _gameObjName_toggle_invert = null;
        [SerializeField] protected string _gameObjName_button_bindPrimary = null;
        [SerializeField] protected string _gameObjName_button_bindSecondary = null;

        protected TextMeshProUGUI _text_bindingName = null;
        protected TextMeshProUGUI _text_bindingValue_primary = null;
        protected TextMeshProUGUI _text_bindingValue_secondary = null;
        protected UIButton _button_bindPrimary = null;
        protected UIButton _button_bindSecondary = null;

        private string _actionEnumAsString = null;
        #endregion


        #region Events/Callbacks
        public event System.Action<UIControlBindingEntry, bool> Event_OnRebind = null;
        #endregion


        #region Properties
        public string actionEnumAsString
        {
            get { return _actionEnumAsString; }
        }

        public string primaryBindingString
        {
            get { return _text_bindingValue_primary != null ? _text_bindingValue_primary.text : string.Empty; }
        }

        public string secondaryBindingString
        {
            get { return _text_bindingValue_secondary != null ? _text_bindingValue_secondary.text : string.Empty; }
        }
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            //dont call base.Initialize() here, because secondary UIElement scripts
            //will control the feedback elements

            _text_bindingName = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_bindingName).GetComponent<TextMeshProUGUI>();
            _text_bindingValue_primary = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_bindingValue_primary).GetComponent<TextMeshProUGUI>();
            _text_bindingValue_secondary = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_bindingValue_secondary).GetComponent<TextMeshProUGUI>();

            _button_bindPrimary = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_bindPrimary).GetComponent<UIButton>();
            _button_bindPrimary.Initialize();
            _button_bindPrimary.Event_OnClicked += OnClick_PrimaryBinding;

            _button_bindSecondary = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_bindSecondary).GetComponent<UIButton>();
            _button_bindSecondary.Initialize();
            _button_bindSecondary.Event_OnClicked += OnClick_SecondaryBinding;
        }
        #endregion


        #region UI Callbacks
        private void OnClick_PrimaryBinding(UIButton button)
        {
            if(Event_OnRebind != null)
            {
                Event_OnRebind(this, true);
            }
        }

        private void OnClick_SecondaryBinding(UIButton button)
        {
            if (Event_OnRebind != null)
            {
                Event_OnRebind(this, false);
            }
        }
        #endregion


        #region Public API
        public void SetActionEnumString(string actionEnumString)
        {
            _actionEnumAsString = actionEnumString;
        }

        public void SetDisplayName(string name)
        {
            if (_text_bindingName != null)
            {
                _text_bindingName.text = name;
            }
        }

        public void SetPrimaryBindingValue(string primaryBinding)
        {
            if(_text_bindingValue_primary)
            {
                _text_bindingValue_primary.text = primaryBinding;
            }
        }

        public void SetSecondaryBindingValue(string primaryBinding)
        {
            if (_text_bindingValue_secondary)
            {
                _text_bindingValue_secondary.text = primaryBinding;
            }
        }
        #endregion


        #region Utility
        public void SetBindingValueColor(Color c, bool primary)
        {
            if (primary)
            {
                if (_text_bindingValue_primary != null)
                {
                    _text_bindingValue_primary.color = c;
                }
            }
            else
            {
                if (_text_bindingValue_secondary != null)
                {
                    _text_bindingValue_secondary.color = c;
                }
            }
        }
        #endregion
    }
}
