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
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace AcrealUI
{
    [ImportedComponent]
    public class UIInventoryWindow : UIWindow
    {
        #region Variables
        [Header("Item Source Tabs")]
        [SerializeField] private string _gameObjName_inventoryTabToggleGroup = null;
        [SerializeField] private string _gameObjName_inventoryTabToggle_player = null;
        [SerializeField] private string _gameObjName_inventoryTabToggle_wagon = null;

        [Header("Buttons")]
        [SerializeField] private string _gameObjName_goldButton = null;
        [SerializeField] private string _gameObjName_lootPileButton = null;

        [Header("Images")]
        [SerializeField] private string _gameObjName_lootPileImage = null;

        [Header("Text")]
        [SerializeField] private string _gameObjName_weightGoldParent = null;
        [SerializeField] private string _gameObjName_text_totalGold = null;
        [SerializeField] private string _gameObjName_text_totalWeight = null;

        [Header("Tweening")]
        private float _defaultPlayerInventorySize = 400f;
        private float _defaultRemoteItemContainerSize = 325f;

        [Header("Panels")]
        [SerializeField] private string _gameObjName_panel_playerStats = null;

        [Header("Scroll Lists")]
        [SerializeField] private string _gameObjName_itemList_playerInventory = null;
        [SerializeField] private string _gameObjName_itemList_container = null;


        private UICharacterPanel _panel_playerStats = null;
        private UIInventoryItemList _itemList_localItems = null;
        private UIInventoryItemList _itemList_remoteItems = null;
        private UIToggleGroup _inventoryTabToggleGroup = null;
        private UIToggle _inventoryTabToggle_player = null;
        private UIToggle _inventoryTabToggle_wagon = null;
        private UIButton _lootPileButton = null;
        private UIButton _goldButton = null;
        private TextMeshProUGUI _text_totalGold = null;
        private TextMeshProUGUI _text_totalWeight = null;
        private Image _lootPileImage = null;
        private bool _isShowingRemoteItemPanel = false;
        #endregion


        #region Properties
        public UICharacterPanel characterPanel { get { return _panel_playerStats; } }
        public UIInventoryItemList itemList_localItems { get { return _itemList_localItems; } }
        public UIInventoryItemList itemList_remoteItems { get { return _itemList_remoteItems; } }
        #endregion


        #region Events
        public event Action Event_ToggledOn_InventoryTab_Player = null;
        public event Action Event_ToggledOn_InventoryTab_Wagon = null;
        public event Action Event_OnButtonClicked_LootPile = null;
        public event Action Event_OnButtonClicked_Gold = null;
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            Transform statsTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panel_playerStats);
            if (statsTform != null)
            {
                _panel_playerStats = statsTform.GetComponent<UICharacterPanel>();
                _panel_playerStats?.Initialize();
            }

            Transform invTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_playerInventory);
            if (invTform != null)
            {
                _itemList_localItems = invTform.GetComponent<UIInventoryItemList>();
                if (_itemList_localItems != null)
                {
                    _itemList_localItems.Initialize();

                    LayoutElement playerLayoutElem = _itemList_localItems.GetComponent<LayoutElement>();
                    if (playerLayoutElem != null)
                    {
                        _defaultPlayerInventorySize = Mathf.Max(playerLayoutElem.minWidth, playerLayoutElem.preferredWidth);
                    }
                }
                else { Debug.LogError("[AcrealUI.UIInventoryWindow] Failed to Find PlayerInventory ItemList!"); }
            }

            Transform conTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_container);
            if (conTform != null)
            {
                _itemList_remoteItems = conTform.GetComponent<UIInventoryItemList>();
                if (_itemList_remoteItems != null)
                {
                    _itemList_remoteItems.Initialize();

                    LayoutElement containerLayoutElem = _itemList_remoteItems.GetComponent<LayoutElement>();
                    if (containerLayoutElem != null)
                    {
                        _defaultPlayerInventorySize = Mathf.Max(containerLayoutElem.minWidth, containerLayoutElem.preferredWidth);
                    }
                }
                else { Debug.LogError("[AcrealUI.UIInventoryWindow] Failed to Find Container ItemList!"); }
            }


            #region Tab Toggle References
            Transform playerTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggle_player);
            _inventoryTabToggle_player = playerTform != null ? playerTform.GetComponent<UIToggle>() : null;
            if (_inventoryTabToggle_player != null)
            {
                _inventoryTabToggle_player.Initialize();
                _inventoryTabToggle_player.Event_OnToggledOn += (_) => { Event_ToggledOn_InventoryTab_Player?.Invoke(); };
            }

            Transform wagonTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggle_wagon);
            _inventoryTabToggle_wagon = wagonTform != null ? wagonTform.GetComponent<UIToggle>() : null;
            if (_inventoryTabToggle_wagon != null)
            {
                _inventoryTabToggle_wagon.Initialize();
                _inventoryTabToggle_wagon.Event_OnToggledOn += (_) => { Event_ToggledOn_InventoryTab_Wagon?.Invoke(); };
            }

            Transform toggleGroupTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_inventoryTabToggleGroup);
            _inventoryTabToggleGroup = toggleGroupTform != null ? toggleGroupTform.GetComponent<UIToggleGroup>() : null;
            if (_inventoryTabToggleGroup != null)
            {
                _inventoryTabToggleGroup.Initialize();
            }

            Transform lootTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_lootPileButton);
            _lootPileButton = lootTform != null ? lootTform.GetComponent<UIButton>() : null;
            if (_lootPileButton != null)
            {
                _lootPileButton.Initialize();
                _lootPileButton.Event_OnLeftClick += (_, _1) => { Event_OnButtonClicked_LootPile?.Invoke(); };
            }

            Transform lootIconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_lootPileImage);
            _lootPileImage = lootIconTform != null ? lootIconTform.GetComponent<Image>() : null;
            #endregion


            #region Gold Display
            Transform goldBtnParentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_goldButton);
            if (goldBtnParentTform != null) { _goldButton = goldBtnParentTform.GetComponent<UIButton>(); }
            if (_goldButton != null)
            {
                _goldButton.Event_OnAnyClick += (_, _1) =>
                {
                    Event_OnButtonClicked_Gold?.Invoke();
                };
            }
            #endregion


            #region Text References
            Transform goldTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_totalGold);
            _text_totalGold = goldTform != null ? goldTform.GetComponent<TextMeshProUGUI>() : null;

            Transform weightTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_totalWeight);
            _text_totalWeight = weightTform != null ? weightTform.GetComponent<TextMeshProUGUI>() : null;
            #endregion
        }

        public override void Cleanup()
        {
            _panel_playerStats?.Cleanup();
            _panel_playerStats = null;

            _itemList_localItems?.Cleanup();
            _itemList_localItems = null;

            _itemList_remoteItems?.Cleanup();
            _itemList_remoteItems = null;

            base.Cleanup();
        }

        public override void ResetWindow()
        {
            _panel_playerStats?.ResetPanel();
            _itemList_localItems?.ResetList();
            _itemList_remoteItems?.ResetList();
            _inventoryTabToggleGroup?.ToggleDefault();
            base.ResetWindow();
        }
        #endregion


        #region UIWindow Overrides
        protected override void ShowInternal()
        {
            base.ShowInternal();

            _panel_playerStats?.Show();
            _inventoryTabToggleGroup?.ToggleDefault();

            TweenItemListWidth(_itemList_localItems, 0f, _defaultPlayerInventorySize);

            if (_itemList_remoteItems != null)
            {
                if (_itemList_remoteItems.itemCount > 0)
                {
                    ShowRemoteItemsPanel();
                }
                else
                {
                    _isShowingRemoteItemPanel = false;

                    LayoutElement layoutElement = _itemList_remoteItems.GetComponent<LayoutElement>();
                    if (layoutElement != null)
                    {
                        layoutElement.minWidth = 0f;
                        layoutElement.preferredWidth = 0f;
                    }
                }
            }
        }

        protected override void HideInternal()
        {
            base.HideInternal();
            _panel_playerStats?.Hide();
            TweenItemListWidth(_itemList_localItems, _defaultPlayerInventorySize, 0f);
            HideRemoteItemsPanel();
        }
        #endregion


        #region Public API
        public void ShowRemoteItemsPanel()
        {
            if (!_isShowingRemoteItemPanel)
            {
                _isShowingRemoteItemPanel = true;
                TweenItemListWidth(_itemList_remoteItems, 0f, _defaultRemoteItemContainerSize);
            }
        }

        public void HideRemoteItemsPanel()
        {
            if (_isShowingRemoteItemPanel)
            {
                _isShowingRemoteItemPanel = false;
                TweenItemListWidth(_itemList_remoteItems, _defaultRemoteItemContainerSize, 0f);
            }
        }

        public void EnableOrDisableTabs(bool enablePlayerTab, bool enableWagonTab)
        {
            if (_inventoryTabToggle_player != null) { _inventoryTabToggle_player.gameObject.SetActive(enablePlayerTab); }
            if (_inventoryTabToggle_wagon != null) { _inventoryTabToggle_wagon.gameObject.SetActive(enableWagonTab); }
        }

        public void SetLootPileActive(bool active)
        {
            if (_lootPileButton != null) { _lootPileButton.gameObject.SetActive(active); }
        }

        public void SetLootPileIcon(Sprite icon)
        {
            if (_lootPileImage != null)
            {
                _lootPileImage.sprite = icon;
                _lootPileImage.gameObject.SetActive(icon != null);
            }
        }

        public void SetTotalGoldText(string goldText)
        {
            if (_text_totalGold != null)
            {
                _text_totalGold.text = goldText;
            }
        }

        public void SetTotalWeightText(string weightText)
        {
            if (_text_totalWeight != null)
            {
                _text_totalWeight.text = weightText;
            }
        }
        #endregion


        #region Tweening
        private void TweenItemListWidth(UIItemList itemList, float startWidth, float endWidth)
        {
            UIManager.Instance.StopCoroutine(gameObject.GetInstanceID(), 0);
            UIManager.Instance.RunCoroutine(gameObject.GetInstanceID(), 0, TweenLayoutElementWidthRoutine(itemList.GetComponent<LayoutElement>(), startWidth, endWidth, 0.2f));
        }

        private IEnumerator<float> TweenLayoutElementWidthRoutine(LayoutElement layoutElement, float startWidth, float endWidth, float duration)
        {
            if (layoutElement != null)
            {
                layoutElement.minWidth = startWidth;
                layoutElement.preferredWidth = startWidth;
            }

            float durationRemaining = duration;
            while (durationRemaining > 0f)
            {
                durationRemaining -= Time.unscaledDeltaTime;
                float t = 1f - Mathf.InverseLerp(0f, duration, durationRemaining);
                float x = Mathf.Lerp(startWidth, endWidth, t);
                if (layoutElement != null)
                {
                    layoutElement.minWidth = x;
                    layoutElement.preferredWidth = x;
                }
                yield return 0f;
            }

            if (layoutElement != null)
            {
                layoutElement.minWidth = endWidth;
                layoutElement.preferredWidth = endWidth;
            }
        }
        #endregion
    }
}