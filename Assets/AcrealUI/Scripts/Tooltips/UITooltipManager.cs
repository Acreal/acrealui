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
    public class UITooltipManager
    {
        #region Variables
        private Dictionary<TooltipType, UITooltip> _tooltipInstanceDictionary = null;
        private Canvas _tooltipCanvas = null;
        private UITooltip _activeTooltip = null;
        private GameObject _hoveredObject = null;
        #endregion


        #region Properties
        public GameObject hoveredObject
        {
            get {  return _hoveredObject; }
        }
        #endregion


        #region Initialization/Cleanup
        public void Initialize()
        {
            _tooltipInstanceDictionary = new Dictionary<TooltipType, UITooltip>();

            _tooltipCanvas = Object.Instantiate(UIManager.referenceManager.prefab_tooltipCanvas);

            UITooltip textTooltip = Object.Instantiate(UIManager.referenceManager.prefab_textTooltip, _tooltipCanvas.transform);
            textTooltip.gameObject.SetActive(false);
            textTooltip.Initalize();
            _tooltipInstanceDictionary[TooltipType.Text] = textTooltip;

            UITooltip iconTextTooltip = Object.Instantiate(UIManager.referenceManager.prefab_iconTextTooltip, _tooltipCanvas.transform);
            iconTextTooltip.gameObject.SetActive(false);
            iconTextTooltip.Initalize();
            _tooltipInstanceDictionary[TooltipType.IconText] = iconTextTooltip;

            UITooltip itemTooltip = Object.Instantiate(UIManager.referenceManager.prefab_itemDetailsTooltip, _tooltipCanvas.transform);
            itemTooltip.gameObject.SetActive(false);
            itemTooltip.Initalize();
            _tooltipInstanceDictionary[TooltipType.ItemDetails] = itemTooltip;
        }

        public void Shutdown()
        {
            if(_tooltipInstanceDictionary != null &&  _tooltipInstanceDictionary.Count > 0)
            {
                List<GameObject> objsToDestroy = new List<GameObject>();

                foreach(UITooltip tooltip in _tooltipInstanceDictionary.Values)
                {
                    if (tooltip != null && tooltip.gameObject != null)
                    {
                        objsToDestroy.Add(tooltip.gameObject);
                    }
                }
                _tooltipInstanceDictionary.Clear();

                for(int i = objsToDestroy.Count -1; i >= 0; i--)
                {
                    Object.Destroy(objsToDestroy[i]);
                }
            }
        }
        #endregion


        #region Update
        public void Update()
        {
            if (_activeTooltip != null)
            {
                if (_activeTooltip.anchorToMouse)
                {
                    Vector3 screenPos = Input.mousePosition;
                    screenPos.x += 10f;
                    screenPos.y += 10f;
                    screenPos.x -= Screen.width * 0.5f;
                    screenPos.y -= Screen.height * 0.5f;
                    _activeTooltip.transform.localPosition = screenPos;
                }
                else
                {
                    RectTransform rt = _activeTooltip.transform as RectTransform;
                    Vector2 pos = rt.anchoredPosition;
                    pos.x = Mathf.Clamp(pos.x, 10f, Screen.width - 10f);
                    pos.y = Mathf.Clamp(pos.y, 10f, Screen.height - 10f);
                    if (Vector2.SqrMagnitude(pos - rt.anchoredPosition) >= 1f)
                    {
                        rt.anchoredPosition = pos;
                    }
                }
            }
        }

        public void LateUpdate()
        {
            if (_activeTooltip != null)
            {
                UIUtilityFunctions.ClampToScreen(_activeTooltip.transform as RectTransform);
            }
        }
        #endregion


        #region Public API
        public void ShowTextTooltip(GameObject hoveredObject, string title, string message, bool anchorToMouse = true)
        {
            HideActiveTooltip();

            UITooltip tooltip = null;
            UITextTooltip textTooltip = null;
            if(_tooltipInstanceDictionary.TryGetValue(TooltipType.Text, out tooltip))
            {
                textTooltip = tooltip as UITextTooltip;
            }

            if(textTooltip != null)
            {
                _hoveredObject = hoveredObject;
                _activeTooltip = textTooltip;

                Vector3 screenPos = Input.mousePosition;
                screenPos.x += 10f;
                screenPos.y += 10f;
                screenPos.x -= Screen.width * 0.5f;
                screenPos.y -= Screen.height * 0.5f;
                textTooltip.transform.localPosition = screenPos;

                textTooltip.SetTitle(title);
                textTooltip.SetMessage(message);
                textTooltip.anchorToMouse = anchorToMouse;
                textTooltip.gameObject.SetActive(true);
                UIUtilityFunctions.ClampToScreen(textTooltip.transform as RectTransform);
            }
        }

        public void ShowIconTextTooltip(GameObject hoveredObject, Sprite icon, string title, string message, bool anchorToMouse = true)
        {
            HideActiveTooltip();

            UITooltip tooltip = null;
            UITooltip_IconText iconTextTooltip = null;
            if (_tooltipInstanceDictionary.TryGetValue(TooltipType.IconText, out tooltip))
            {
                iconTextTooltip = tooltip as UITooltip_IconText;
            }

            if (iconTextTooltip != null)
            {
                _hoveredObject = hoveredObject;
                _activeTooltip = iconTextTooltip;

                Vector3 screenPos = Input.mousePosition;
                screenPos.x += 10f;
                screenPos.y += 10f;
                screenPos.x -= Screen.width * 0.5f;
                screenPos.y -= Screen.height * 0.5f;
                iconTextTooltip.transform.localPosition = screenPos;

                iconTextTooltip.SetIcon(icon);
                iconTextTooltip.SetTitle(title);
                iconTextTooltip.SetMessage(message);
                iconTextTooltip.anchorToMouse = anchorToMouse;
                iconTextTooltip.gameObject.SetActive(true);
                UIUtilityFunctions.ClampToScreen(iconTextTooltip.transform as RectTransform);
            }
        }

        public void ShowItemDetailsTooltip(GameObject hoveredObject, Sprite icon, string itemName, string itemDescription, 
            List<ItemStatData> itemStatData, List<ItemStatSliderData> itemStatSliderData, List<ItemPowerData> itemPowersData,
            Vector2 pivot, Vector2? staticScreenPosition = null)
        {
            HideActiveTooltip();

            UITooltip tooltip = null;
            UITooltip_ItemDetails itemDetailsTooltip = null;
            if (_tooltipInstanceDictionary.TryGetValue(TooltipType.ItemDetails, out tooltip))
            {
                itemDetailsTooltip = tooltip as UITooltip_ItemDetails;
            }

            if (itemDetailsTooltip != null)
            {
                _hoveredObject = hoveredObject;
                _activeTooltip = itemDetailsTooltip;

                RectTransform rt = itemDetailsTooltip.transform as RectTransform;
                itemDetailsTooltip.anchorToMouse = staticScreenPosition == null;
                rt.pivot = pivot;
                if (staticScreenPosition != null)
                {
                    Vector3 screenPos = staticScreenPosition.GetValueOrDefault(Vector3.zero);
                    rt.anchoredPosition = screenPos;
                }

                itemDetailsTooltip.SetTitle(itemName);
                itemDetailsTooltip.SetMessage(itemDescription);
                itemDetailsTooltip.SetIcon(icon);

                itemDetailsTooltip.ClearItemStats();
                itemDetailsTooltip.ClearItemPowers();

                if (itemStatData != null)
                {
                    for (int i = 0; i < itemStatData.Count; i++)
                    {
                        UITooltip_ItemStatEntry statEntry = itemDetailsTooltip.AddItemStat();
                        statEntry.SetIcon(itemStatData[i].icon);
                        statEntry.SetTitle(itemStatData[i].name);
                        statEntry.SetDescription(itemStatData[i].description);
                    }
                }

                if (itemStatSliderData != null)
                {
                    for (int i = 0; i < itemStatSliderData.Count; i++)
                    {
                        UITooltip_ItemStatSliderEntry statEntry = itemDetailsTooltip.AddItemStatSlider();
                        statEntry.SetIcon(itemStatSliderData[i].icon);
                        statEntry.SetTitle(itemStatSliderData[i].name);
                        statEntry.SetDescription(itemStatSliderData[i].description);
                        statEntry.SetSliderValue(itemStatSliderData[i].sliderValue);
                    }
                }

                if (itemPowersData != null)
                {
                    for (int i = 0; i < itemPowersData.Count; i++)
                    {
                        UITooltip_ItemPowerEntry statEntry = itemDetailsTooltip.AddItemPower();
                        statEntry.SetIcon(itemPowersData[i].icon);
                        statEntry.SetTitle(itemPowersData[i].name);
                        statEntry.SetDescription(itemPowersData[i].description);
                    }
                }
                itemDetailsTooltip.EnableOrDisableItemPowerDisplay(itemPowersData != null && itemPowersData.Count > 0);

                itemDetailsTooltip.gameObject.SetActive(true);
                UIUtilityFunctions.ClampToScreen(itemDetailsTooltip.transform as RectTransform);
            }
        }

        public void HideActiveTooltip()
        {
            if(_activeTooltip != null)
            {
                _activeTooltip.gameObject.SetActive(false);
            }
            _activeTooltip = null;
            _hoveredObject = null;
        }
        #endregion
    }
}
