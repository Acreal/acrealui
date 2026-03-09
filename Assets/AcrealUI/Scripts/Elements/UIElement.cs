using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AcrealUI
{
    public abstract class UIElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Variables
        [SerializeField] protected bool _initializeOnAwake = false;
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
                Initialize();
            }
        }
        #endregion


        #region Initialization
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
            else if(!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
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

        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {

        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {

        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {

        }
        #endregion
    }
}
