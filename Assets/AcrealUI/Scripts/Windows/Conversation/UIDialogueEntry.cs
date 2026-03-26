/*
Copyright (c) 2025-2026 Acreal (https://github.com/acreal)

Permission is hereby granted, free of charge, to any person obtaining x copy of this software and associated 
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
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIDialogueEntry : UIInteractiveElement
    {
        #region Variables
        [SerializeField] private string _gameObjName_borderImage = null;
        [SerializeField] private string _gameObjName_portraitRawImage = null;
        [SerializeField] private string _gameObjName_copyToNotebookButton = null;
        [SerializeField] private string _gameObjName_slideTransform = null;
        [SerializeField] private float _minimumHeight = 76f;
        [SerializeField] private float _slideInDuration = 0.2f;
        [SerializeField] private float _slideInDelay = 0.05f;
        [SerializeField] private bool _slideInFromRightSide = false;

        private CanvasGroup _canvasGroup = null;
        private Image _borderImage = null;
        private RawImage _portraitRawImage = null;
        private UIButton _copyToNotebookButton = null;
        private RectTransform _slideTransform = null;
        private Coroutine _slideRoutine = null;
        #endregion


        #region Properties
        public bool isPlayerDialogue { get; set; }
        #endregion


        #region Events
        public System.Action<UIDialogueEntry> Event_OnButtonClicked_CopyToNotebook = null;
        #endregion


        #region Initialization
        public override void Initialize()
        {
            base.Initialize();

            _canvasGroup = GetComponent<CanvasGroup>();

            _slideTransform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_slideTransform) as RectTransform;

            Transform borderTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_borderImage);
            if (borderTform != null)
            {
                _borderImage = borderTform.GetComponent<Image>();
            }

            Transform portraitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_portraitRawImage);
            if(portraitTform != null )
            {
                _portraitRawImage = portraitTform.GetComponent<RawImage>();
            }

            Transform copyTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_copyToNotebookButton);
            if(copyTform != null)
            {
                _copyToNotebookButton = copyTform.GetComponent<UIButton>();
                if(_copyToNotebookButton != null)
                {
                    _copyToNotebookButton.Initialize();
                    _copyToNotebookButton.Event_OnLeftClick += (_, _1) =>
                    {
                        Event_OnButtonClicked_CopyToNotebook?.Invoke(this);
                    };
                }
            }
        }
        #endregion


        #region Public API
        public void SetBorderColor(Color color)
        {
            if(_borderImage != null)
            {
                _borderImage.color = color;
            }
        }

        public void SetPortraitTexture(Texture2D portrait)
        {
            if(_portraitRawImage != null )
            {
                _portraitRawImage.texture = portrait;
            }
        }

        public void PlaySlideInAnim()
        {
            if(_slideRoutine != null)
            {
                StopCoroutine(_slideRoutine);
            }

            if(_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            _slideRoutine = StartCoroutine(SlideInRoutine(_slideInDuration, _slideInDelay));
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> SlideInRoutine(float duration, float delay)
        {
            yield return 0f;

            RectTransform parentRT = _slideTransform.parent as RectTransform;
            HorizontalOrVerticalLayoutGroup layoutGroup = _slideTransform.GetComponent<HorizontalOrVerticalLayoutGroup>();
            HorizontalOrVerticalLayoutGroup parentLayoutGroup = parentRT.GetComponent<HorizontalOrVerticalLayoutGroup>();

            float padding = _slideInFromRightSide ? parentLayoutGroup.padding.left : parentLayoutGroup.padding.right;
            float width = Mathf.Min(layoutGroup.preferredWidth, parentRT.sizeDelta.x - padding);

            //NOTE(Acreal): we need to set these values twice, because changing the width of this transform
            //will also change the preferred height of the child text element
            //so we set once to get the correct width, rebuild it, and set again to get the correct height
            _slideTransform.sizeDelta = new Vector2(width, layoutGroup.preferredHeight);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_slideTransform);
            _slideTransform.sizeDelta = new Vector2(width, layoutGroup.preferredHeight);

            LayoutElement parentLayoutElem = parentRT.GetComponent<LayoutElement>();
            parentLayoutElem.minHeight = Mathf.CeilToInt(Mathf.Max(_slideTransform.sizeDelta.y, _minimumHeight));
            float fromPositionX = _slideInFromRightSide ? width : -width;
            float toPositionX = 0f;

            Vector2 localPos = _slideTransform.localPosition;
            localPos.x = fromPositionX;
            _slideTransform.localPosition = localPos;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
            }

            float t = delay;
            while(t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                yield return 0f;
            }

            t = duration;
            while(t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                float lerpT = 1f - Mathf.InverseLerp(0f, duration, t);
                lerpT = lerpT * lerpT * lerpT * lerpT; //quadratic easing

                localPos = _slideTransform.localPosition;
                localPos.x = Mathf.Lerp(fromPositionX, toPositionX, lerpT);
                _slideTransform.localPosition = localPos;

                if (_canvasGroup != null)
                {
                    _canvasGroup.alpha = lerpT;
                }

                yield return 0f;
            }

            localPos = _slideTransform.localPosition;
            localPos.x = toPositionX;
            _slideTransform.localPosition = localPos;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
        #endregion
    }
}
