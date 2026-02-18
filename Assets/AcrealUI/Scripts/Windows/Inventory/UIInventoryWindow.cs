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
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace AcrealUI
{
    [ImportedComponent]
    public class UIInventoryWindow : UIWindow
    {
        #region Variables
        private const float _PLAYER_INVENTORY_SIZE = 400f;
        private const float _CONTAINER_INVENTORY_SIZE = 350f;

        [Header("Panels")]
        [SerializeField] private string _gameObjName_panel_playerStats = null;

        [Header("Scroll Lists")]
        [SerializeField] private string _gameObjName_itemList_playerInventory = null;
        [SerializeField] private string _gameObjName_itemList_container = null;

        private UIPanelPlayerStats _panel_playerStats = null;
        private UIInventoryWindow_ItemList _itemList_playerInventory = null;
        private UIInventoryWindow_ItemList _itemList_container = null;
        private bool _isShowingContainerPanel = false;
        #endregion


        #region Properties
        public UIPanelPlayerStats panel_playerStats { get { return _panel_playerStats; } }
        public UIInventoryWindow_ItemList itemList_playerInventory { get { return _itemList_playerInventory; } }
        public UIInventoryWindow_ItemList itemList_container { get { return _itemList_container; } }
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            if (!string.IsNullOrEmpty(_gameObjName_panel_playerStats))
            {
                Transform statsTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panel_playerStats);
                _panel_playerStats = statsTform != null ? statsTform.GetComponent<UIPanelPlayerStats>() : null;
                if (_panel_playerStats != null)
                {
                    _panel_playerStats.Initialize();
                }
            }

            if (!string.IsNullOrEmpty(_gameObjName_itemList_playerInventory))
            {
                Transform invTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_playerInventory);
                _itemList_playerInventory = invTform != null ? invTform.GetComponent<UIInventoryWindow_ItemList>() : null;
                if (_itemList_playerInventory != null)
                {
                    _itemList_playerInventory.Initialize();
                }
                else { Debug.LogError("[AcrealUI] Failed to Find PlayerInventory ItemList!"); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_itemList_container))
            {
                Transform conTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_container);
                _itemList_container = conTform != null ? conTform.GetComponent<UIInventoryWindow_ItemList>() : null;
                if (_itemList_container != null)
                {
                    _itemList_container.Initialize();
                }
                else { Debug.LogError("[AcrealUI] Failed to Find Container ItemList!"); }
            }
        }
        #endregion


        #region Open/Close
        protected override void ShowInternal()
        {
            base.ShowInternal();

            bool containerHasItems = _itemList_container != null && _itemList_container.itemCount > 0;

            if (_panel_playerStats != null)
            {
                if (containerHasItems)
                {
                    _panel_playerStats.HideImmediate();
                }
                else
                {
                    _panel_playerStats.HideImmediate();
                    _panel_playerStats.Show();
                }
            }

            LayoutElement inventoryLayoutElem = _itemList_playerInventory != null ? _itemList_playerInventory.GetComponent<LayoutElement>() : null;
            if (inventoryLayoutElem != null)
            {
                StartCoroutine(TweenPanelWidthRoutine(inventoryLayoutElem, 0f, _PLAYER_INVENTORY_SIZE, 0.2f));
            }

            LayoutElement containerLayoutElem = _itemList_container != null ? _itemList_container.GetComponent<LayoutElement>() : null;
            if (containerLayoutElem != null)
            {
                _isShowingContainerPanel = containerHasItems;
                if (containerHasItems)
                {
                    StartCoroutine(TweenPanelWidthRoutine(containerLayoutElem, 0f, _CONTAINER_INVENTORY_SIZE, 0.2f));
                }
                else
                {
                    containerLayoutElem.minWidth = 0f;
                    containerLayoutElem.preferredWidth = 0f;
                }
            }

            if (panel_playerStats != null)
            {
                panel_playerStats.Show();
            }

            if (_itemList_playerInventory != null && _itemList_playerInventory.tabToggleGroup != null)
            {
                _itemList_playerInventory.tabToggleGroup.ToggleDefault();
            }

            if (_itemList_container != null && _itemList_container.tabToggleGroup != null)
            {
                _itemList_container.tabToggleGroup.ToggleDefault();
            }
        }

        protected override void HideInternal()
        {
            base.HideInternal();

            LayoutElement inventoryLayoutElem = _itemList_playerInventory != null ? _itemList_playerInventory.GetComponent<LayoutElement>() : null;
            if (inventoryLayoutElem != null)
            {
                StartCoroutine(TweenPanelWidthRoutine(inventoryLayoutElem, _PLAYER_INVENTORY_SIZE, 0f, 0.2f));
            }

            HideContainerPanel();

            if(panel_playerStats != null)
            {
                panel_playerStats.Hide();
            }

            if (_itemList_playerInventory != null)
            {
                _itemList_playerInventory.Clear();
            }

            if (_itemList_container != null)
            {
                _itemList_container.Clear();
            }
        }
        #endregion


        #region Container Panel
        public void ShowContainerPanel()
        {
            if (!_isShowingContainerPanel)
            {
                _itemList_container.SetActiveFilter(ItemFilter.All);
                _itemList_container.SetItemFilterText(UIUtilityFunctions.ItemFilterToString(ItemFilter.All));

                _isShowingContainerPanel = true;
                LayoutElement containerLayoutElem = _itemList_container != null ? _itemList_container.GetComponent<LayoutElement>() : null;
                StartCoroutine(TweenPanelWidthRoutine(containerLayoutElem, 0f, _CONTAINER_INVENTORY_SIZE, 0.2f));
            }
        }

        public void HideContainerPanel()
        {
            if (_isShowingContainerPanel)
            {
                _isShowingContainerPanel = false;
                LayoutElement containerLayoutElem = _itemList_container != null ? _itemList_container.GetComponent<LayoutElement>() : null;
                StartCoroutine(TweenPanelWidthRoutine(containerLayoutElem, _CONTAINER_INVENTORY_SIZE, 0f, 0.2f));
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator TweenPanelWidthRoutine(LayoutElement panelLayoutElement, float startWidth, float endWidth, float duration, float delay = 0f)
        {
            if (panelLayoutElement != null)
            {
                panelLayoutElement.minWidth = startWidth;
                panelLayoutElement.preferredWidth = startWidth;

                float durationRemaining = delay;
                while (durationRemaining > 0f)
                {
                    durationRemaining -= Time.unscaledDeltaTime;
                    yield return null;
                }

                durationRemaining = duration;
                while (durationRemaining > 0f)
                {
                    durationRemaining -= Time.unscaledDeltaTime;
                    float x = Mathf.Lerp(startWidth, endWidth, 1f-Mathf.InverseLerp(0f, duration, durationRemaining));
                    panelLayoutElement.minWidth = x;
                    panelLayoutElement.preferredWidth = x;
                    yield return null;
                }

                panelLayoutElement.minWidth = endWidth;
                panelLayoutElement.preferredWidth = endWidth;
            }
        }
        #endregion
    }
}