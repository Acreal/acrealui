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
using UnityEngine.EventSystems;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIToggle : UIButton
    {
        #region Variables
        //set by UIToggleGroup to indicate whether this
        //toggle can be clicked consecutively
        public bool canBeToggledOff = true;

        protected bool _isToggledOn = false;
        private string _displayName = null;
        #endregion


        #region Properties
        public bool isToggledOn
        {
            get { return _isToggledOn; }
            set
            {
                if (_isToggledOn != value)
                {
                    _isToggledOn = value;

                    if (Event_OnToggledOnOrOff != null)
                    {
                        Event_OnToggledOnOrOff(this);
                    }

                    if(_isToggledOn)
                    {
                        if (Event_OnToggledOn != null)
                        {
                            Event_OnToggledOn(this);
                        }
                    }
                    else if(Event_OnToggledOff != null)
                    {
                        Event_OnToggledOff(this);
                    }
                }
            }
        }

        public string displayName
        {
            get { return _displayName; }
        }
        #endregion


        #region Data Sources
        public UIDelegates.DataSourceDelegate_Bool DataSource_IsToggledOn = null;
        #endregion


        #region Events
        public event System.Action<UIToggle> Event_OnToggledOn = null;
        public event System.Action<UIToggle> Event_OnToggledOff = null;
        public event System.Action<UIToggle> Event_OnToggledOnOrOff = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            Event_OnToggledOn = null;
            Event_OnToggledOff = null;
            Event_OnToggledOnOrOff = null;

            base.Initialize();
        }
        #endregion


        #region Public API
        public void SetToggledWithoutNotify(bool toggled)
        {
            _isToggledOn = toggled;

            if (_elementFeedbackArray != null)
            {
                foreach (UIInteractiveElementFeedback element in _elementFeedbackArray)
                {
                    element.Refresh();
                }
            }
        }

        public void SetDisplayName(string toggleDisplayName)
        {
            _displayName = toggleDisplayName;

            if(_titleText != null )
            {
                _titleText.text = _displayName;
            }
        }

        public override void Refresh()
        {
            if (DataSource_IsToggledOn != null)
            {
                _isToggledOn = DataSource_IsToggledOn(gameObject);
            }

            base.Refresh();
        }
        #endregion


        #region Mouse Input
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            UIUtilityFunctions.PlayButtonClickSound();
        }

        public override void OnPointerClick(PointerEventData pointerData)
        {
            if (isDisabled || (isToggledOn && !canBeToggledOff)) { return; }
            base.OnPointerClick(pointerData);
            isToggledOn = !_isToggledOn;
        }
        #endregion
    }
}