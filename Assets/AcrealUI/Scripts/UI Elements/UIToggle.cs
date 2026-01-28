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
        [SerializeField] private string _gameObjectName_text_displayName = null;

        //set by UIToggleGroup to indicate whether this
        //toggle can be clicked consecutively
        #if UNITY_EDITOR
        public bool canBeToggledOff = true;
        #else
        [HideInInspector] public bool canBeToggledOff = true;
        #endif

        protected bool _isToggledOn = false;
        private string _displayName = null;
        private TextMeshProUGUI _displayNameText = null;
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

                    if(Event_OnToggledOnOrOff != null)
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

            if (!string.IsNullOrEmpty(_gameObjectName_text_displayName))
            {
                Transform textTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjectName_text_displayName);
                _displayNameText = textTform != null ? textTform.GetComponent<TextMeshProUGUI>() : null;
                SetDisplayName(_displayName);
            }
        }
        #endregion


        #region Public API
        public void SetDisplayName(string toggleDisplayName)
        {
            _displayName = toggleDisplayName;

            if(_displayNameText != null )
            {
                _displayNameText.text = _displayName;
            }
        }
        #endregion


        #region Mouse Input
        public override void OnPointerClick(PointerEventData pointerData)
        {
            if (isDisabled || (isToggledOn && !canBeToggledOff)) { return; }

            base.OnPointerClick(pointerData);

            #if LOG_UI_INPUT
            Debug.Log(gameObject.name + ".Toggle.OnPointerClick()");
            #endif

            isToggledOn = !_isToggledOn;
        }
        #endregion
    }
}