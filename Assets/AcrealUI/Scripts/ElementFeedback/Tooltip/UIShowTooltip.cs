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

using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public abstract class UIShowTooltip : UIInteractiveElementFeedback
    {
        #region Variables
        [SerializeField] private float _showDelay = 0f;

        private float _currentDelay = 0f;
        private bool _isShown = false;
        #endregion


        #region Show/Hide
        public void Show()
        {
            if (!_isShown)
            {
                _isShown = true;

                if (_showDelay > 0f)
                {
                    StartCoroutine(ShowRoutine());
                }
                else
                {
                    ShowInternal();
                }
            }
        }

        public void Hide()
        {
            if (_isShown)
            {
                _isShown = false;
                HideInternal();
            }
        }

        protected virtual void ShowInternal()
        {

        }

        protected virtual void HideInternal()
        {
            StopAllCoroutines();

            if (UIManager.tooltipManager.hoveredObject == gameObject)
            {
                UIManager.tooltipManager.HideActiveTooltip();
            }
        }
        #endregion


        #region Mouse Input
        public override void OnPointerEnter()
        {
            base.OnPointerEnter();
            Show();
        }

        public override void OnPointerExit()
        {
            base.OnPointerExit();
            StopAllCoroutines();
            Hide();
        }

        public override void OnPointerClick()
        {
            base.OnPointerClick();
            _currentDelay = _showDelay;
        }

        private IEnumerator<float> ShowRoutine()
        {
            _currentDelay = _showDelay;
            while (_currentDelay > 0f)
            {
                _currentDelay -= Time.unscaledDeltaTime;
                yield return 0f;
            }
            ShowInternal();
        }
        #endregion
    }
}
