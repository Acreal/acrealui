using UnityEngine;
using UnityEngine.EventSystems;

namespace AcrealUI
{
    public class UIElement : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        #region Variables
        [SerializeField] protected bool _initializeOnAwake = false;
        #endregion


        #region Data Sources
        public UIDelegates.DataSourceDelegate_Bool DataSource_GameObjectActive = null;
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
            Refresh();
        }
        #endregion


        #region Updating
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
