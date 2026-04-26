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
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIPanel : MonoBehaviour
    {
        #region Variables
        [Header("Dynamic Sizing")]
        [SerializeField] private string _gameObjName_layoutElement = null;
        [SerializeField] private float _panelVerticalSizeOffset = 40f;

        [Header("Dynamic Elements")]
        [SerializeField] private string _gameObjName_scrollGroupParent = null;

        protected CanvasGroup _canvasGroup = null;
        protected LayoutElement _layoutElement = null;

        protected Transform _scrollGroupParent = null;
        protected Dictionary<string, UIScrollListGroup> _scrollListGroupDict = null;
        
        private Vector2 _panelSize;
        #endregion


        #region Properties
        public Vector2 panelSize
        {
            get { return _panelSize; }
            protected set { _panelSize = value; }
        }

        public LayoutElement layoutElement { get { return _layoutElement; } }
        #endregion


        #region Initialization/Cleanup
        public virtual void Initialize()
        {
            _scrollListGroupDict = new Dictionary<string, UIScrollListGroup>();

            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
            }

            _scrollGroupParent = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_scrollGroupParent);
            
            Transform layoutTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_layoutElement);
            if (layoutTform != null)
            {
                _layoutElement = layoutTform != null ? layoutTform.GetComponent<LayoutElement>() : null;
            }

            if (_layoutElement == null)
            {
                _layoutElement = GetComponent<LayoutElement>();
            }
        }

        public virtual void Cleanup()
        {
            if (_scrollListGroupDict != null)
            {
                List<UIScrollListGroup> groupsToDestroy = new List<UIScrollListGroup>(_scrollListGroupDict.Count);
                foreach (UIScrollListGroup scrollGroup in _scrollListGroupDict.Values)
                {
                    scrollGroup.Cleanup();
                    groupsToDestroy.Add(scrollGroup);
                }

                for (int i = groupsToDestroy.Count - 1; i >= 0; i--)
                {
                    Destroy(groupsToDestroy[i].gameObject);
                }
            }
            _scrollListGroupDict = null;
            _canvasGroup = null;
            _layoutElement = null;
            _scrollGroupParent = null;
            _panelSize = Vector2.zero;
        }

        public virtual void ResetPanel()
        {
            if (_scrollListGroupDict != null)
            {
                foreach (UIScrollListGroup scrollGroup in _scrollListGroupDict.Values)
                {
                    scrollGroup.ResetGroup();
                }
            }

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }
        #endregion


        #region Show/Hide
        public virtual void Show()
        {
            gameObject.SetActive(true);

            if (_layoutElement != null && (_layoutElement.minHeight > 0 || _layoutElement.minHeight > 0 || _layoutElement.preferredWidth > 0 || _layoutElement.preferredHeight > 0))
            {
                float width = Mathf.Max(_layoutElement.minWidth, _layoutElement.preferredWidth);
                float height = Mathf.Max(_layoutElement.minHeight, _layoutElement.preferredHeight);
                panelSize = new Vector2(width, height + _panelVerticalSizeOffset);
            }
            else
            {
                RectTransform rt = transform as RectTransform;
                panelSize = rt != null ? new Vector2(rt.sizeDelta.x, rt.sizeDelta.y) : Vector2.zero;
                panelSize = new Vector2(panelSize.x, panelSize.y + _panelVerticalSizeOffset);
            }
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }
        #endregion


        #region Public API
        public void Refresh()
        {
            if (_scrollListGroupDict != null)
            {
                foreach (UIScrollListGroup scrollGroup in _scrollListGroupDict.Values)
                {
                    scrollGroup.Refresh();
                }
            }
        }

        public virtual UIScrollListGroup GetOrAddScrollListGroup(string bindingGroupName)
        {
            UIScrollListGroup scrollGroup = null;
            if (bindingGroupName != null)
            {
                if (!_scrollListGroupDict.TryGetValue(bindingGroupName, out scrollGroup))
                {
                    scrollGroup = PopScrollListGroupFromPool(UIManager.referenceManager.prefab_scrollListGroup, _scrollGroupParent);
                    if (scrollGroup != null)
                    {
                        scrollGroup.Initialize();
                        scrollGroup.SetTextTitle(bindingGroupName);
                    }

                    _scrollListGroupDict.Add(bindingGroupName, scrollGroup);
                }
            }
            return scrollGroup;
        }
        #endregion


        #region ScrollListGroup Management
        private UIScrollListGroup PopScrollListGroupFromPool(UIScrollListGroup scrollListGroupPrefab, Transform parent)
        {
            //TODO(Acreal): add pooling
            UIScrollListGroup group = Instantiate(scrollListGroupPrefab, parent);
            group.transform.localScale = Vector3.one;
            return group;
        }
        #endregion
    }
}
