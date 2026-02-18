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
    public abstract class UIInteractiveElement : UIElement, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Variables
        protected bool _isDisabled = false;
        protected UIInteractiveElementFeedback[] _elementFeedbackArray = null;

        private bool _isHighlighted = false;
        private bool _isPressed = false;
        #endregion


        #region Properties
        public bool isDisabled
        {
            get { return _isDisabled; }
            set
            {
                if (_isDisabled != value)
                {
                    _isDisabled = value;

                    if(_isDisabled)
                    {
                        _isHighlighted = false;
                        _isPressed = false;
                    }

                    Event_OnDisabledChanged?.Invoke(_isDisabled);
                }
            }
        }

        public bool isHighlighted
        {
            get { return  _isHighlighted; }
        }

        public bool isPressed
        {
            get { return _isPressed; }
        }
        #endregion


        #region Events
        public event System.Action<bool> Event_OnDisabledChanged = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            if(_elementFeedbackArray == null)
            {
                _elementFeedbackArray = GetComponentsInChildren<UIInteractiveElementFeedback>(true);
                if(_elementFeedbackArray != null )
                {
                    for(int i = 0; i <  _elementFeedbackArray.Length; i++)
                    {
                        _elementFeedbackArray[i].Initialize(this);
                    }
                }
            }

            base.Initialize();
        }
        #endregion


        #region Public API
        public override void Refresh()
        {
            base.Refresh();

            if (_elementFeedbackArray != null)
            {
                foreach (UIInteractiveElementFeedback feedbackElement in _elementFeedbackArray)
                {
                    feedbackElement.Refresh();
                }
            }
        }
        #endregion


        #region Pointer Functions
        public override void OnPointerClick(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerClick();
                }
            }
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            _isHighlighted = true;

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerEnter();
                }
            }
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            _isHighlighted = false;

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerExit();
                }
            }
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            _isPressed = true;

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerDown();
                }
            }
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            _isPressed = false;

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerUp();
                }
            }
        }
        #endregion
    }
}
