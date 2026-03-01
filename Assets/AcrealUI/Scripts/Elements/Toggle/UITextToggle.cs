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
        [SerializeField] private string _toggledOnText = "On";
        [SerializeField] private string _toggledOffText = "Off";
        [SerializeField] private bool _messagesAreLocalizationKeys = false;
        [SerializeField] private string localizationTableID = null;
        #endregion


        #region Properties
        public string toggledOnText
        {
            get { return _toggledOnText; }
            set
            {
                _toggledOnText = value;
                Refresh();
            }
        }

        public string toggledOffText
        {
            get { return _toggledOffText; }
            set
            {
                _toggledOffText = value;
                Refresh();
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
                    Refresh();
                }
            }
        }
        #endregion


        #region Update Text
        public override void Refresh()
        {
            base.Refresh();

            if(_valueText != null)
            {
                if (messagesAreLocalizationKeys)
                {
                    _valueText.text = !isDisabled && isToggledOn ? TextManager.Instance.GetText(localizationTableID, _toggledOnText) : TextManager.Instance.GetText(localizationTableID, _toggledOffText);
                }
                else
                {
                    _valueText.text = !isDisabled && isToggledOn ? _toggledOnText : _toggledOffText;
                }
            }
        }
        #endregion
    }
}
