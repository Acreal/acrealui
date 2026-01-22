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
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIToggleGroup : MonoBehaviour
    {
        #region Variables
        [SerializeField] private List<string> _gameObjNameList_toggles = null;
        [SerializeField] private string _gameObjName_defaultToggle = null;

        public bool canBeToggledOff = false;

        private bool initialized = false;
        private List<UIToggle> _toggles = null;
        private UIToggle _defaultToggle = null;
        #endregion


        #region Initialization
        public void Initialize()
        {
            if (!initialized)
            {
                initialized = true;

                if (!string.IsNullOrEmpty(_gameObjName_defaultToggle))
                {
                    Transform defaultTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_defaultToggle);
                    _defaultToggle = defaultTform != null ? defaultTform.GetComponent<UIToggle>() : null;
                }

                if (_toggles == null && _gameObjNameList_toggles != null)
                {
                    _toggles = new List<UIToggle>();

                    foreach (string s in _gameObjNameList_toggles)
                    {
                        if (!string.IsNullOrEmpty(s))
                        {
                            Transform toggleTform = UIUtilityFunctions.FindDeepChild(transform, s);
                            UIToggle toggle = toggleTform != null ? toggleTform.GetComponent<UIToggle>() : null;
                            if (toggle != null)
                            {
                                _toggles.Add(toggle);
                            }
                        }
                    }
                }

                if (_toggles != null)
                {
                    for (int i = 0; i < _toggles.Count; i++)
                    {
                        _toggles[i].canBeToggledOff = canBeToggledOff;
                        _toggles[i].Event_OnToggledOn += OnToggleSwitchedOn;
                    }
                }
            }
        }
        #endregion


        #region MonoBehaviour
        private void Awake()
        {
            Initialize();
        }

        private void Start()
        {
            bool toggleIsOn = false;
            if (_toggles != null && _toggles.Count > 0)
            {
                for (int i = 0; i < _toggles.Count; i++)
                {
                    if (_toggles[i] != null && _toggles[i].isToggledOn)
                    {
                        toggleIsOn = true;
                        break;
                    }
                }
            }

            if (!toggleIsOn)
            {
                ToggleDefault();
            }
        }
        #endregion


        #region Public API
        public void ToggleDefault()
        {
            if(_defaultToggle != null && !_defaultToggle.isToggledOn)
            {
                _defaultToggle.isToggledOn = true;
            }
        }

        public void SetActiveToggle(UIToggle toggle)
        {
            if(toggle != null && !toggle.isToggledOn)
            {
                toggle.isToggledOn = true;
            }
            else if(!canBeToggledOff)
            {
                if(_defaultToggle == null && _toggles != null && _toggles.Count > 0)
                { 
                    _defaultToggle = _toggles[0];
                }
                ToggleDefault();
            }
            else
            {
                foreach (UIToggle tgl in _toggles)
                {
                    if (tgl.isToggledOn)
                    {
                        tgl.isToggledOn = false;
                    }
                }
            }
        }

        public void AddToggle(UIToggle toggle)
        {
            if(toggle != null)
            {
                if (_toggles == null || !_toggles.Contains(toggle))
                {
                    if (_toggles == null)
                    {
                        _toggles = new List<UIToggle>();
                    }
                    _toggles.Add(toggle);

                    toggle.canBeToggledOff = canBeToggledOff;
                    toggle.Event_OnToggledOn += OnToggleSwitchedOn;
                }
            }
        }

        public void RemoveToggle(UIToggle toggle)
        {
            if (toggle != null && _toggles != null)
            {
                _toggles.Remove(toggle);
                toggle.canBeToggledOff = true;
                toggle.Event_OnToggledOn -= OnToggleSwitchedOn;
            }
        }
        #endregion


        #region Input Callbacks
        private void OnToggleSwitchedOn(UIToggle toggle)
        {
            if (_toggles != null)
            {
                for (int i = 0; i < _toggles.Count; i++)
                {
                    if(_toggles[i] == toggle) { continue; }
                    _toggles[i].isToggledOn = false;
                }
            }
        }
        #endregion
    }
}