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
    public class UIElementFeedback_FadeInOnHover : UIElementFeedback
    {
        #region Variables
        [SerializeField] private string _gameObjName_canvasGroup = null;
        [SerializeField] private float _fadeDuration = 0.2f;
        [SerializeField] private float _fadeInDelay = 0f;
        [SerializeField] private float _fadeOutDelay = 0f;
        [SerializeField] private bool _controlActiveState = false;

        private CanvasGroup _canvasGroup = null;
        private bool _isFadedIn = false;
        private bool _isRunningFadeRoutine = true;
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            Transform canvasGroupTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_canvasGroup);
            if(canvasGroupTForm == null) { canvasGroupTForm = transform; }
            _canvasGroup = canvasGroupTForm.GetComponent<CanvasGroup>();

            if (_controlActiveState)
            {
                _canvasGroup?.gameObject.SetActive(false);
            }

            base.Initialize(uiElement);
        }
        #endregion


        #region Update/Refresh
        public override void Refresh()
        {
            base.Refresh();

            if (_canvasGroup != null)
            {
                UIInteractiveElement elem = _uiElement as UIInteractiveElement;
                if (elem != null)
                {
                    bool fadeIn = elem.isHighlighted || elem.isPressed;
                    if (_isFadedIn != fadeIn)
                    {
                        if (_isRunningFadeRoutine)
                        {
                            UIManager.Instance.StopCoroutine(gameObject.GetInstanceID(), 0);
                        }
                        _isRunningFadeRoutine = false;

                        _isFadedIn = fadeIn;
                        float targetFade = _isFadedIn ? 1f : 0f;
                        if (Mathf.Abs(_canvasGroup.alpha - targetFade) >= float.Epsilon)
                        {
                            float delay = _isFadedIn ? _fadeInDelay : _fadeOutDelay;
                            if (delay + _fadeDuration < float.Epsilon)
                            {
                                if (_canvasGroup != null)
                                {
                                    _canvasGroup.alpha = _isFadedIn ? 1f : 0f;

                                    if (_controlActiveState)
                                    {
                                        _canvasGroup.gameObject.SetActive(_isFadedIn);
                                    }
                                }
                            }
                            else
                            {
                                UIManager.Instance.RunCoroutine(gameObject.GetInstanceID(), 0, FadeRoutine(_canvasGroup.alpha, targetFade, _fadeDuration, delay));
                            }
                        }
                    }
                }
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> FadeRoutine(float from, float to, float duration, float delay)
        {
            _isRunningFadeRoutine = true;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = from;

                if(_controlActiveState && to > float.Epsilon)
                {
                    _canvasGroup.gameObject.SetActive(true);
                }
            }

            float t = delay;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0f;
            }

            t = duration;
            while (t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = Mathf.Lerp(from, to, 1f - Mathf.InverseLerp(0f, duration, t));
                }
                yield return 0f;
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = to;

                if (_controlActiveState && to <= float.Epsilon)
                {
                    _canvasGroup.gameObject.SetActive(false);
                }
            }

            _isRunningFadeRoutine = false;
        }
        #endregion
    }
}
