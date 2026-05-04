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

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Text;
using UnityEngine;

namespace AcrealUI
{
    public class UITradeWindowController : DaggerfallTradeWindow, IWindowController
    {
        #region Variables
        private UITradeWindow _tradeWindowInstance = null;
        private UICharacterPanelController _characterPanelController = null;
        private UIItemListController _localItemsController = null;
        private UIItemListController _merchantItemsController = null;
        private UIItemListController _buyListController = null;
        private UIItemListController _sellListController = null;
        private ItemCollection _buyCollection = null;
        private ItemCollection _sellCollection = null;
        #endregion


        #region Initialization/Cleanup
        public UITradeWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null, WindowModes windowMode = WindowModes.Sell, IGuild guild = null) : base(uiManager, previous)
        {
            //base class needs these references, but we don't use them
            localItemListScroller = new ItemListScroller(defaultToolTip);
            remoteItemListScroller = new ItemListScroller(defaultToolTip);
            wagonButton = new Button();

            _buyCollection = new ItemCollection();
            _sellCollection = new ItemCollection();
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            usingWagon = false;
            localItems = playerEntity.Items;

            _tradeWindowInstance?.Show();
        }

        public void HideWindow()
        {
            _tradeWindowInstance?.Hide();
        }

        public override void OnPop()
        {
            base.OnPop();
            HideWindow();
            _tradeWindowInstance?.ResetWindow();
            UIManager.tooltipManager.HideActiveTooltip();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
        }
        #endregion


        #region Overriden Base Functions
        public override void OnPush()
        {
            if (!IsSetup)
            {
                Setup();
            }

            base.OnPush();

            //workaround for base class setting the remoteItems to
            //the wagon's items (we use localItems for that)
            if (usingWagon)
            {
                remoteItems = lastRemoteItems;
                usingWagon = false;
            }

            _tradeWindowInstance?.Show(true);
            _tradeWindowInstance?.ShowRemoteItemsPanel();

            _characterPanelController.UpdateAll();

            _localItemsController.SetItemCollection(localItems);
            _merchantItemsController.SetItemCollection(merchantItems);

            _buyListController.SetItemCollection(_buyCollection);
            _sellListController.SetItemCollection(_sellCollection);
        }

        protected override void Setup()
        {
            IsSetup = true;

            if (_tradeWindowInstance == null)
            {
                #region Window Instance
                UIWindow window = UIManager.Instance.GetWindowInstance(UIWindowInstanceType.Trade);
                if (window == null || !(window is UITradeWindow))
                {
                    Debug.LogError("[AcrealUI.UITradeWindowController] UIManager.GetWindowInstance(UIWindowInstanceType.Trade) returned " + (window == null ? " NULL!" : "a window of the wrong type! Expected type UITradeWindow, but got " + window.GetType().ToString() + "!"));
                    return;
                }

                _tradeWindowInstance = window as UITradeWindow;
                _tradeWindowInstance.Initialize();
                _tradeWindowInstance.Event_ToggledOn_InventoryTab_Wagon += () =>
                {
                    lastRemoteItems = remoteItems;
                    lastRemoteTargetType = remoteTargetType;
                    remoteItems = PlayerEntity.WagonItems;
                    remoteTargetType = RemoteTargetTypes.Wagon;
                };
                #endregion

                #region Character Panel
                _characterPanelController = new UICharacterPanelController(_tradeWindowInstance.characterPanel);
                _characterPanelController.Initialize();
                #endregion

                #region Local List
                _localItemsController = new UIItemListController(_tradeWindowInstance.itemList_localItems);
                _localItemsController.Initialize();

                _localItemsController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount == 2)
                    {
                        TransferItem(itemEntry, _localItemsController, _sellListController);
                    }
                };
                
                _localItemsController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (itemEntry != null)
                    {
                        DaggerfallUnityItem item = localItems?.GetItem(itemEntry.itemUID);
                        if (item != null)
                        {
                            bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                            if (splitting)
                            {
                                int maxCount = item.stackCount - 1;
                                int defaultValue = item.stackCount / 2;

                                UIManager.popupManager.ShowSliderConfirmationWindow(UITextStrings.InventoryWindow_Label_SplitStack.GetText(), string.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                                0, item.stackCount - 1, true, //slider params
                                                                                new object[1] { item }, //additional meta data to pass along
                                                                                (float sliderValue, object[] dataPayload) => //on confirm
                                                                                {
                                                                                    UIManager.popupManager.HideActivePopupWindow();

                                                                                    DaggerfallUnityItem itemToSplit = dataPayload[0] as DaggerfallUnityItem;

                                                                                    int splitSize = (int)sliderValue;
                                                                                    DaggerfallUnityItem splitItem = _localItemsController.itemCollection.SplitStack(itemToSplit, splitSize);
                                                                                    TransferItem(splitItem, _localItemsController, _sellListController);
                                                                                },
                                                                                (_) => { UIManager.popupManager.HideActivePopupWindow(); });
                            }
                            else
                            {
                                TransferItem(itemEntry, _localItemsController, _sellListController);
                            }
                        }
                    }
                };
                #endregion

                #region Remote List
                _merchantItemsController = new UIItemListController(_tradeWindowInstance.itemList_remoteItems);
                _merchantItemsController.Initialize();
                
                _merchantItemsController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount == 2)
                    {
                        TransferItem(itemEntry, _merchantItemsController, _buyListController);
                    }
                };

                _merchantItemsController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    DaggerfallUnityItem item = localItems?.GetItem(itemEntry.itemUID);
                    if (item != null)
                    {
                        bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                        if (splitting)
                        {
                            int maxCount = item.stackCount - 1;
                            int defaultValue = item.stackCount / 2;

                            UIManager.popupManager.ShowSliderConfirmationWindow(UITextStrings.InventoryWindow_Label_SplitStack.GetText(), string.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                            0, item.stackCount - 1, true, //slider params
                                                                            new object[1] { item }, //additional meta data to pass along
                                                                            (float sliderValue, object[] dataPayload) => //on confirm
                                                                            {
                                                                                UIManager.popupManager.HideActivePopupWindow();

                                                                                DaggerfallUnityItem itemToSplit = dataPayload[0] as DaggerfallUnityItem;

                                                                                int splitSize = (int)sliderValue;
                                                                                DaggerfallUnityItem splitItem = _localItemsController.itemCollection.SplitStack(itemToSplit, splitSize);
                                                                                TransferItem(splitItem, _localItemsController, _sellListController);
                                                                            },
                                                                            (_) => { UIManager.popupManager.HideActivePopupWindow(); });
                        }
                        else
                        {
                            TransferItem(itemEntry, _localItemsController, _sellListController);
                        }
                    }
                    TransferItem(itemEntry, _merchantItemsController, _buyListController);
                };
                #endregion

                #region Buy List
                _buyListController = new UIItemListController(_tradeWindowInstance.buyList);
                _buyListController.Initialize();
                
                _buyListController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount == 2)
                    {
                        TransferItem(itemEntry, _buyListController, _merchantItemsController);
                    }
                };

                _buyListController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    TransferItem(itemEntry, _buyListController, _merchantItemsController);
                };

                UITradeItemList buyTradeList = _buyListController.itemList as UITradeItemList;
                if (buyTradeList != null)
                {
                    buyTradeList.Event_OnButtonClicked_EmptyList += () =>
                    {
                        int numItems = _buyCollection.GetNumItems();
                        for (int i = numItems - 1; i >= 0; i--)
                        {
                            DaggerfallUnityItem item = _buyCollection.GetItem(i);
                            if (item != null)
                            {
                                TransferItem(item, _buyListController, _merchantItemsController);
                            }
                        }
                    };
                }
                #endregion

                #region Sell List
                _sellListController = new UIItemListController(_tradeWindowInstance.sellList);
                _sellListController.Initialize();
                
                _sellListController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                { 
                    if (clickCount == 2) 
                    {
                        TransferItem(itemEntry, _sellListController, _localItemsController);
                    }
                };

                _sellListController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) => 
                {
                    TransferItem(itemEntry, _sellListController, _localItemsController);
                };

                UITradeItemList sellTradeList = _sellListController.itemList as UITradeItemList;
                if (sellTradeList != null)
                {
                    sellTradeList.Event_OnButtonClicked_EmptyList += () =>
                    {
                        int numItems = _sellCollection.GetNumItems();
                        for (int i = numItems - 1; i >= 0; i--)
                        {
                            DaggerfallUnityItem item = _sellCollection.GetItem(i);
                            if (item != null)
                            {
                                TransferItem(item, _sellListController, _localItemsController);
                            }
                        }
                    };
                }
                #endregion
            }
        }

        public override void Draw() { }
        public override void Refresh(bool refreshPaperDoll = true) { }
        protected override void UpdateLocalTargetIcon() { }
        protected override void UpdateRemoteTargetIcon() { }
        #endregion


        #region Weight
        private string BuildPlayerWeightString()
        {
            float totalWeight = usingWagon ? localItems.GetWeight() : playerEntity.CarriedWeight;
            string totalWeightStr = totalWeight.ToString("N1");
            StringBuilder strBuilder = new StringBuilder(totalWeightStr);
            strBuilder.Append(" / ");

            if (usingWagon)
            {
                strBuilder.Append(ItemHelper.WagonKgLimit.ToString("F1"));
            }
            else if (playerEntity != null)
            {
                strBuilder.Append(playerEntity.MaxEncumbrance.ToString("F1"));
            }
            else
            {
                strBuilder.Append("0.0");
            }

            strBuilder.Append(" Kg");
            return strBuilder.ToString();
        }
        #endregion


        #region Input Events
        private void OnLocalItemLeftClicked(UIItemEntry itemEntry, int clickCount)
        {
            if (clickCount >= 2 && itemEntry != null && localItems != null)
            {
                DaggerfallUnityItem item = localItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    HandleLocalItemRightClick(item);
                }
            }
        }

        private void HandleLocalItemRightClick(DaggerfallUnityItem item)
        {
            ItemType itemType = UIUtilityFunctions.ItemToItemType(item);

            //unequip the item if we have it equipped before transferring it
            if (UIUtilityFunctions.ItemIsEquippable(item))
            {
                if (item.IsEquipped)
                {
                    UnequipItem(item);
                    _characterPanelController?.UpdateAll();
                }
            }

            TransferItem(item, localItems, _sellCollection);

            //item removed successfully
            if (!localItems.Contains(item))
            {
                _localItemsController.UpdateItemFilterToggles();
                _localItemsController.UpdateItemList(true);

                _sellListController.UpdateItemList(false);

                UIManager.tooltipManager.HideActiveTooltip();

                _tradeWindowInstance?.SetTotalWeightText(BuildPlayerWeightString());
            }
        }

        private void TransferItem(DaggerfallUnityItem item, UIItemListController sourceController, UIItemListController destinationController)
        {
            if (item != null)
            {
                TransferItem(item, sourceController.itemCollection, destinationController.itemCollection);
                sourceController.UpdateItemList(true);
                destinationController.UpdateItemList(false);
                UIManager.tooltipManager.HideActiveTooltip();
            }
        }

        private void TransferItem(UIItemEntry itemEntry, UIItemListController sourceController, UIItemListController destinationController)
        {
            DaggerfallUnityItem item = itemEntry != null && sourceController != null ? sourceController.itemCollection?.GetItem(itemEntry.itemUID) : null;
            TransferItem(item, sourceController, destinationController);
        }
        #endregion
    }
}
