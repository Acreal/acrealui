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
using TMPro;

namespace AcrealUI
{
    [ImportedComponent]
    public abstract class UIWindow : MonoBehaviour
    {
        #region Variables
        [SerializeField] string _gameObjName_canvasComponent = null;
        [SerializeField] string _gameObjName_canvasGroup = null;
        [SerializeField] string _gameObjName_closeWindowButton = null;
        [SerializeField] string _gameObjName_backButton = null;
        [SerializeField] string _gameObjName_headerText = null;

        protected Canvas _canvasComponent = null;
        protected CanvasGroup _canvasGroup = null;
        protected UIButton _closeWindowButton = null;
        protected UIButton _backButton = null;
        protected TextMeshProUGUI _headerText = null;

        private bool _isOpen = false;
        #endregion


        #region Events
        public event System.Action Event_ButtonClick_PrevWindow = null;
        public event System.Action Event_ButtonClick_CloseWindow = null;
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

            // FIND CANVAS COMPONENT
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

            // FIND CANVAS GROUP
            Transform canvasGroupTform = null;
            if (!string.IsNullOrEmpty(_gameObjName_canvasGroup)) { canvasGroupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_canvasGroup); }
            if(canvasGroupTform == null) { canvasGroupTform = transform; }
            _canvasGroup = canvasGroupTform != null ? canvasGroupTform.GetComponent<CanvasGroup>() : null;

            // FIND CLOSE BUTTON
            if (!string.IsNullOrEmpty(_gameObjName_closeWindowButton))
            {
                Transform closeTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_closeWindowButton);
                _closeWindowButton = closeTform != null ? closeTform.GetComponent<UIButton>() : null;
                if(_closeWindowButton != null)
                {
                    _closeWindowButton.Initialize();
                    _closeWindowButton.Event_OnClicked += (_) =>
                    {
                        Event_ButtonClick_CloseWindow?.Invoke();
                    };
                }
            }

            // FIND BACK BUTTON
            if (!string.IsNullOrEmpty(_gameObjName_backButton))
            {
                Transform backTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_backButton);
                _backButton = backTform != null ? backTform.GetComponent<UIButton>() : null;
                if (_backButton != null)
                {
                    _backButton.Initialize();
                    _backButton.Event_OnClicked += (_) =>
                    {
                        Event_ButtonClick_PrevWindow?.Invoke();
                    };
                }
            }

            if (!string.IsNullOrEmpty(_gameObjName_headerText))
            {
                Transform headerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_headerText);
                _headerText = headerTform != null ? headerTform.GetComponent<TextMeshProUGUI>() : null;
            }
        }

        public virtual void Cleanup()
        {

        }
        #endregion


        #region Public API
        public void Show()
        {
            if (!_isOpen)
            {
                ShowInternal();
            }
        }

        public void Hide()
        {
            if (_isOpen)
            {
                HideInternal();
            }
        }

        public void SetHeaderText(string header)
        {
            if (_headerText != null)
            {
                _headerText.text = header;
            }
        }
        #endregion


        #region Open/Close
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