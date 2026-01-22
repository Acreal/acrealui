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
    public abstract class UIWindow : MonoBehaviour
    {
        #region Variables
        [SerializeField] string _gameObjName_canvasComponent = null;
        [SerializeField] string _gameObjName_canvasGroup = null;

        protected Canvas _canvasComponent = null;
        protected CanvasGroup _canvasGroup = null;

        private bool _isOpen = false;
        #endregion


        #region MonoBehaviour
        protected virtual void Awake()
        {

        }

        protected virtual void Start()
        {

        }

        protected virtual void Update()
        {

        }

        protected virtual void OnDisable()
        {

        }
        #endregion


        #region Initialize/Cleanup
        public virtual void Initialize()
        {
            //since getting references relies on finding transforms
            //by name, we need to make sure we strip out Unity's
            //"(Clone)" tag in some cases
            gameObject.name = gameObject.name.Replace("(Clone)", "");

            Transform canvasTform = null;
            if (!string.IsNullOrEmpty(_gameObjName_canvasComponent))
            {
                canvasTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_canvasComponent);
            }
            if(canvasTform == null) { canvasTform = transform; }
            _canvasComponent = canvasTform != null ? canvasTform.GetComponent<Canvas>() : null;
            if (_canvasComponent != null)
            {
                _canvasComponent.enabled = false;
            }

            Transform canvasGroupTform = null;
            if (!string.IsNullOrEmpty(_gameObjName_canvasGroup)) { canvasGroupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_canvasGroup); }
            if(canvasGroupTform == null) { canvasGroupTform = transform; }
            _canvasGroup = canvasGroupTform != null ? canvasGroupTform.GetComponent<CanvasGroup>() : null;
        }

        public virtual void Cleanup()
        {

        }
        #endregion


        #region Open/Close
        public void Show()
        {
            if (!_isOpen)
            {
                ShowInternal();
            }
        }

        protected virtual void ShowInternal()
        {
            _isOpen = true;

            StopAllCoroutines();

            if (_canvasComponent != null)
            {
                _canvasComponent.enabled = true;
            }

            if (_canvasGroup != null)
            {
                StartCoroutine(TweenCanvasGroupAlphaRoutine(_canvasGroup, 0f, 0.2f, 0f, 1f));
            }
        }

        public void Hide()
        {
            if (_isOpen)
            {
                HideInternal();
            }
        }

        protected virtual void HideInternal()
        {
            _isOpen = false;

            StopAllCoroutines();

            if (_canvasGroup != null)
            {
                StartCoroutine(TweenCanvasGroupAlphaRoutine(_canvasGroup, 0f, 0.2f, 1f, 0f, OnFinishedClosing));
            }
        }

        protected virtual void OnFinishedClosing()
        {
            if (_canvasComponent != null)
            {
                _canvasComponent.enabled = false;
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> TweenCanvasGroupAlphaRoutine(CanvasGroup canvasGroupToTween, float delay, float duration, float from, float to, System.Action onFinished = null)
        {
            canvasGroupToTween.alpha = from;

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
                canvasGroupToTween.alpha = Mathf.Lerp(from, to, 1f - Mathf.InverseLerp(0f, duration, t));
                yield return 0f;
            }

            canvasGroupToTween.alpha = to;

            onFinished?.Invoke();
        }
        #endregion
    }
}