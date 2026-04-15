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

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AcrealUI
{
    public abstract class UIElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Variables
        [SerializeField, HideInInspector] protected bool _initializeOnAwake = false;
        [SerializeField] protected string _gameObjName_titleText = null;
        [SerializeField] protected string _gameObjName_valueText = null;

        protected TextMeshProUGUI _titleText = null;
        protected TextMeshProUGUI _valueText = null;
        #endregion


        #region Data Sources
        public UIDelegates.DataSourceDelegate_Bool DataSource_GameObjectActive = null;
        public UIDelegates.DataSourceDelegate_String DataSource_ValueDisplayString = null;
        #endregion


        #region MonoBehaviour
        protected virtual void Awake()
        {
            if (_initializeOnAwake)
            {
                //Debug.LogError(gameObject.name + "." + GetInstanceID() + " has _initializeOnAwake set to true!");
                Initialize();
            }
        }
        #endregion


        #region Initialization/Cleanup
        public virtual void Initialize()
        {
            Transform titleTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_titleText);
            if (titleTform != null)
            {
                _titleText = titleTform.GetComponent<TextMeshProUGUI>();
            }

            Transform valueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_valueText);
            if (valueTform != null)
            {
                _valueText = valueTform.GetComponent<TextMeshProUGUI>();
            }

            Refresh();
        }

        /// <summary>
        /// sets all events and references to null - use before destroying/releasing window
        /// </summary>
        public virtual void Cleanup()
        {
            DataSource_GameObjectActive = null;
            DataSource_ValueDisplayString = null;

            _titleText = null;
            _valueText = null;
        }

        /// <summary>
        /// resets any variables, and returns any
        /// interface elements to a default state
        /// </summary>
        public virtual void ResetElement()
        {
            Refresh();
        }
        #endregion


        #region Public API
        public virtual void Refresh()
        {
            if(DataSource_GameObjectActive != null)
            {
                bool active = DataSource_GameObjectActive(gameObject);
                if(gameObject.activeSelf != active)
                {
                    gameObject.SetActive(active);
                }
            }

            if(DataSource_ValueDisplayString != null)
            {
                _valueText.text = DataSource_ValueDisplayString(gameObject);
            }
        }

        public void SetTitle(string title)
        {
            if(_titleText != null)
            {
                _titleText.text = title;
                _titleText.gameObject.SetActive(!string.IsNullOrWhiteSpace(title));
            }
        }

        public void SetTitleTextSize(int textSize)
        {
            if (_titleText != null)
            {
                _titleText.fontSize = textSize;
            }
        }

        public void SetDisplayValue(string displayValue)
        {
            if (_valueText != null)
            {
                _valueText.text = displayValue;
                _valueText.gameObject.SetActive(!string.IsNullOrWhiteSpace(displayValue));
            }
        }

        public void SetDisplayValueTextSize(int textSize)
        {
            if (_valueText != null)
            {
                _valueText.fontSize = textSize;
            }
        }
        #endregion


        #region Pointer Functions
        public virtual void OnPointerClick(PointerEventData eventData)
        {

        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            //Debug.Log(gameObject.name + "." + gameObject.GetInstanceID() + ".UIElement.OnPointerEnter()");
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            //Debug.Log(gameObject.name + "." + gameObject.GetInstanceID() + ".UIElement.OnPointerExit()");
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            //Debug.Log(gameObject.name + "." + gameObject.GetInstanceID() + ".UIElement.OnPointerDown()");
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            //Debug.Log(gameObject.name + "." + gameObject.GetInstanceID() + ".UIElement.OnPointerUp()");
        }
        #endregion
    }
}
