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
using UnityEngine.EventSystems;

namespace AcrealUI
{
    [ImportedComponent]
    public abstract class UIInteractiveElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Variables
        [SerializeField] protected bool _initializeOnAwake = false;

        protected bool _isDisabled = false;
        protected UIInteractiveElementFeedback[] _elementFeedbackArray = null;
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
                    Event_OnDisabledChanged?.Invoke(_isDisabled);
                }
            }
        }
        #endregion


        #region Events
        public event System.Action<bool> Event_OnDisabledChanged = null;
        #endregion


        #region MonoBehaviour
        protected virtual void Awake()
        {
            if(_initializeOnAwake)
            {
                Initialize();
            }
        }
        #endregion


        #region Initialization
        public virtual void Initialize()
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
        }
        #endregion


        #region Pointer Functions
        public virtual void OnPointerClick(PointerEventData eventData)
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

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerEnter();
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerExit();
                }
            }
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

            if (_elementFeedbackArray != null)
            {
                for (int i = 0; i < _elementFeedbackArray.Length; i++)
                {
                    _elementFeedbackArray[i].OnPointerDown();
                }
            }
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (_isDisabled) { return; }

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
