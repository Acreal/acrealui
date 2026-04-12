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
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public abstract class UIWindow : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string _gameObjName_canvasComponent = null;
        [SerializeField] private string _gameObjName_canvasGroup = null;
        [SerializeField] private string _gameObjName_layoutElement = null;
        [SerializeField] private string _gameObjName_closeWindowButton = null;
        [SerializeField] private string _gameObjName_backButton = null;
        [SerializeField] private string _gameObjName_headerText = null;

        protected Canvas _canvasComponent = null;
        protected CanvasGroup _canvasGroup = null;
        protected LayoutElement _layoutElement = null;
        protected UIButton _closeWindowButton = null;
        protected UIButton _backButton = null;
        protected TextMeshProUGUI _headerText = null;

        private bool _isOpen = false;
        #endregion


        #region Properties
        public bool isOpen { get { return _isOpen; } }
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
            Transform canvasTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_canvasComponent);
            if(canvasTform == null) { canvasTform = transform; }
            _canvasComponent = canvasTform.GetComponent<Canvas>();
            if (_canvasComponent != null)
            {
                _canvasComponent.enabled = false;
            }
            else { Debug.LogError("Unable to find Canvas for UIWindow attached to GameObject: " + gameObject.name); }

            // FIND CANVAS GROUP
            Transform canvasGroupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_canvasGroup);
            if(canvasGroupTform == null) { canvasGroupTform = transform; }
            _canvasGroup = canvasGroupTform.GetComponent<CanvasGroup>();
            if(_canvasGroup == null) { Debug.LogWarning("Unable to find CanvasGroup for UIWindow attached to GameObject: " + gameObject.name); }

            // FIND LAYOUT ELEMENT
            Transform layoutElemTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_layoutElement);
            if (layoutElemTform == null) { layoutElemTform = transform; }
            _layoutElement = layoutElemTform.GetComponent<LayoutElement>();
            if (_layoutElement == null) { Debug.LogWarning("Unable to find LayoutElement for UIWindow attached to GameObject: " + gameObject.name); }

            // FIND CLOSE BUTTON
            Transform closeTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_closeWindowButton);
            if (closeTform != null)
            {
                _closeWindowButton = closeTform.GetComponent<UIButton>();
                if (_closeWindowButton != null)
                {
                    _closeWindowButton.Initialize();
                    _closeWindowButton.Event_OnAnyClick += (_, _1) =>
                    {
                        Event_ButtonClick_CloseWindow?.Invoke();
                    };
                }
                else { Debug.LogWarning("Unable to find UIButton for UIWindow attached to GameObject: " + gameObject.name); }
            }

            // FIND BACK BUTTON
            Transform backTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_backButton);
            if (backTform != null)
            {
                _backButton = backTform.GetComponent<UIButton>();
                if (_backButton != null)
                {
                    _backButton.Initialize();
                    _backButton.Event_OnAnyClick += (_, _1) =>
                    {
                        Event_ButtonClick_PrevWindow?.Invoke();
                    };
                }
                else { Debug.LogWarning("Unable to find UIButton for UIWindow attached to GameObject: " + gameObject.name); }
            }

            // FIND HEADER TEXT
            Transform headerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_headerText);
            if (headerTform != null)
            {
                _headerText = headerTform.GetComponent<TextMeshProUGUI>();
                if (_headerText == null) { Debug.LogWarning("Unable to find TextMeshProUGUI for UIWindow attached to GameObject: " + gameObject.name); }
            }
        }

        /// <summary>
        /// Resets window state and sets all events to null - call when finished using a window
        /// </summary>
        public virtual void ResetWindow()
        {
            SetHeaderText(null);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        /// <summary>
        /// Resets window state, sets all events AND variables to null, and releases any temporary assets/memory
        /// </summary>
        public virtual void Cleanup()
        {
            Hide();
            SetHeaderText(null);

            Event_ButtonClick_PrevWindow = null;
            Event_ButtonClick_CloseWindow = null;

            _canvasComponent = null;
            _canvasGroup = null;

            _closeWindowButton?.Cleanup();
            _closeWindowButton = null;

            _backButton?.Cleanup();
            _backButton = null;

            _headerText = null;
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

        public void SetInputEnabled(bool enabled)
        {
            if(_canvasGroup != null)
            {
                _canvasGroup.blocksRaycasts = enabled;
                _canvasGroup.interactable = enabled;
            }
        }

        public void SetHeaderText(string header)
        {
            if (_headerText != null)
            {
                _headerText.text = header;
            }
        }

        public void SetBackButtonActive(bool active)
        {
            if(_backButton != null)
            {
                _backButton.gameObject.SetActive(active);
            }
        }

        public void SetSize(Vector2 size)
        {
            if(_layoutElement != null)
            {
                _layoutElement.minWidth = size.x;
                _layoutElement.minHeight = size.y;
            }
            else
            {
                RectTransform rt = (_canvasGroup != null ? _canvasGroup.transform : transform) as RectTransform;
                if(rt != null)
                {
                    rt.sizeDelta = size;
                }
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