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
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIElementFeedback : MonoBehaviour
    {
        #region Variables
        protected UIElement _uiElement = null;
        #endregion


        #region Initialization
        public virtual void Initialize(UIElement uiElement)
        {
            _uiElement = uiElement;

            UIInteractiveElement interactiveElement = _uiElement as UIInteractiveElement;
            if(interactiveElement != null)
            {
                interactiveElement.Event_OnDisabledChanged += (_) =>
                {
                    Refresh();
                };
            }
        }

        /// <summary>
        ///  sets all events and references to null
        /// </summary>
        public virtual void Cleanup()
        {
            _uiElement = null;
        }
        #endregion


        #region Update
        public virtual void Refresh()
        {

        }
        #endregion


        #region IPointer
        public virtual void OnPointerEnter()
        {
            Refresh();
        }

        public virtual void OnPointerExit()
        {
            Refresh();
        }

        public virtual void OnPointerDown()
        {
            Refresh();
        }

        public virtual void OnPointerUp()
        {
            Refresh();
        }

        public virtual void OnPointerClick()
        {

        }
        #endregion
    }
}
