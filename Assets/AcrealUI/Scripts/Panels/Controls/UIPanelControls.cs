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
    public class UIPanelControls : UIPanel
    {
        #region Variables
        [SerializeField] private string _gameObjName_button_continue = null;
        [SerializeField] private string _gameObjName_button_default = null;

        private UIButton _button_continue = null;
        private UIButton _button_default = null;
        #endregion


        #region Events
        public event System.Action Event_OnButtonClicked_Default = null;
        public event System.Action Event_OnButtonClicked_Continue = null;
        #endregion


        #region Initialize/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            if (!string.IsNullOrEmpty(_gameObjName_button_continue))
            {
                Transform continueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_continue);
                _button_continue = continueTform != null ? continueTform.GetComponent<UIButton>() : null;
                if (_button_continue != null)
                {
                    _button_continue.Initialize();
                    _button_continue.Event_OnClicked += (_) => { Event_OnButtonClicked_Continue?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_continue + "\""); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_button_default))
            {
                Transform defaultTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_button_default);
                _button_default = defaultTform != null ? defaultTform.GetComponent<UIButton>() : null;
                if (_button_default != null)
                {
                    _button_default.Initialize();
                    _button_default.Event_OnClicked += (_) => { Event_OnButtonClicked_Default?.Invoke(); };
                }
                else { Debug.LogError("[AcrealUI] Failed to load UIButton Script on GameObject \"" + _gameObjName_button_default + "\""); }
            }
        }
        #endregion


        #region Binding Entries
        public void UpdateDuplicateBindings(HashSet<string> duplicateKeyBindings)
        {
            foreach (UIScrollListGroup scrollGroup in _scrollListGroupDict.Values)
            {
                foreach (UIElement element in scrollGroup.uiElementsRO)
                {
                    UIControlBindingEntry binding = element as UIControlBindingEntry;
                    if (binding != null)
                    {
                        if (duplicateKeyBindings.Contains(binding.primaryBindingString))
                        {
                            binding.SetBindingValueColor(Color.red, true);
                        }
                        else
                        {
                            binding.SetBindingValueColor(Color.white, true);
                        }

                        if (duplicateKeyBindings.Contains(binding.secondaryBindingString))
                        {
                            binding.SetBindingValueColor(Color.red, false);
                        }
                        else
                        {
                            binding.SetBindingValueColor(Color.white, false);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
