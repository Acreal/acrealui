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
using UnityEngine.EventSystems;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIButton : UIInteractiveElement
    {
        #region Events
        public event System.Action<UIButton, PointerEventData> Event_OnClicked = null;
        public event System.Action<UIButton, int> Event_OnLeftClicked = null;
        public event System.Action<UIButton, int> Event_OnRightClicked = null;
        public event System.Action<UIButton, int> Event_OnMiddleClicked = null;
        #endregion


        #region Mouse Input
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            UIUtilityFunctions.PlayButtonClickSound();
        }

        public override void OnPointerClick(PointerEventData pointerData)
        {
            if (isDisabled) { return; }

            base.OnPointerClick(pointerData);

            Event_OnClicked?.Invoke(this, pointerData);
            switch(pointerData.button)
            {
                case PointerEventData.InputButton.Left: Event_OnLeftClicked?.Invoke(this, pointerData.clickCount); break;
                case PointerEventData.InputButton.Right: Event_OnRightClicked?.Invoke(this, pointerData.clickCount); break;
                case PointerEventData.InputButton.Middle: Event_OnMiddleClicked?.Invoke(this, pointerData.clickCount); break;
            }
        }
        #endregion
    }
}