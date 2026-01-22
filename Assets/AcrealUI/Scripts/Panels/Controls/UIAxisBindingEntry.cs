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

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIAxisBindingEntry : UIControlBindingEntry
    {
        #region Variables
        protected UIToggle _toggle_invert = null;
        #endregion


        #region Properties
        public InputManager.AxisActions boundAction { get; private set; }
        #endregion


        #region Events
        public event System.Action<InputManager.AxisActions, bool> Event_OnInvertChanged = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            if (_toggle_invert == null)
            {
                Transform invertTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_toggle_invert);
                _toggle_invert = invertTform != null ? invertTform.GetComponent<UIToggle>() : null;
                if (_toggle_invert != null) { _toggle_invert.Initialize(); }
            }
        }
        #endregion


        #region Public API
        public void SetActionEnum(InputManager.AxisActions action)
        {
            boundAction = action;
        }

        public void SetInvert(bool invert)
        {
            if (_toggle_invert != null)
            {
                _toggle_invert.isToggledOn = invert;
                Event_OnInvertChanged?.Invoke(boundAction, invert);
            }
        }
        #endregion
    }
}
