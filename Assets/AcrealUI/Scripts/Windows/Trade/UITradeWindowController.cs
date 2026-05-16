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

using DaggerfallConnect;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public class UITradeWindowController : DaggerfallTradeWindow, IWindowController
    {
        #region Variables
        //this was taken from the base class - it's set to private there, but we need it here
        private static readonly Dictionary<DFLocation.BuildingTypes, List<ItemGroups>> StoreBuysItemType = new Dictionary<DFLocation.BuildingTypes, List<ItemGroups>>()
        {
            { DFLocation.BuildingTypes.Alchemist, new List<ItemGroups>()
                { ItemGroups.Gems, ItemGroups.CreatureIngredients1, ItemGroups.CreatureIngredients2, ItemGroups.CreatureIngredients3, ItemGroups.PlantIngredients1, ItemGroups.PlantIngredients2, ItemGroups.MiscellaneousIngredients1, ItemGroups.MiscellaneousIngredients2, ItemGroups.MetalIngredients } },
            { DFLocation.BuildingTypes.Armorer, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Weapons } },
            { DFLocation.BuildingTypes.Bookseller, new List<ItemGroups>()
                { ItemGroups.Books } },
            { DFLocation.BuildingTypes.ClothingStore, new List<ItemGroups>()
                { ItemGroups.MensClothing, ItemGroups.WomensClothing } },
            { DFLocation.BuildingTypes.FurnitureStore, new List<ItemGroups>()
                { ItemGroups.Furniture } },
            { DFLocation.BuildingTypes.GemStore, new List<ItemGroups>()
                { ItemGroups.Gems, ItemGroups.Jewellery } },
            { DFLocation.BuildingTypes.GeneralStore, new List<ItemGroups>()
                { ItemGroups.Books, ItemGroups.MensClothing, ItemGroups.WomensClothing, ItemGroups.Transportation, ItemGroups.Jewellery, ItemGroups.Weapons, ItemGroups.UselessItems2 } },
            { DFLocation.BuildingTypes.PawnShop, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Books, ItemGroups.MensClothing, ItemGroups.WomensClothing, ItemGroups.Gems, ItemGroups.Jewellery, ItemGroups.ReligiousItems, ItemGroups.Weapons, ItemGroups.UselessItems2, ItemGroups.Paintings } },
            { DFLocation.BuildingTypes.WeaponSmith, new List<ItemGroups>()
                { ItemGroups.Armor, ItemGroups.Weapons } },
        };

        private UITradeWindow _tradeWindowInstance = null;
        private UICharacterPanelController _characterPanelController = null;
        private UIItemListController _localItemsController = null;
        private UIItemListController _remoteItemsController = null;
        private UIItemListController _buyListController = null;
        private UIItemListController _sellListController = null;
        private readonly ItemCollection _buyCollection = null;
        private readonly ItemCollection _sellCollection = null;
        private PlayerGPS.DiscoveredBuilding _buildingDiscoveryData = new PlayerGPS.DiscoveredBuilding();
        private List<ItemGroups> _itemTypesAccepted = null;
        #endregion


        #region Initialization/Cleanup
        public UITradeWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null, WindowModes windowMode = WindowModes.Buy, IGuild guild = null) : base(uiManager, previous, windowMode, guild)
        {
            //base class needs these references, but we don't use them
            localItemListScroller = new ItemListScroller(defaultToolTip);
            remoteItemListScroller = new ItemListScroller(defaultToolTip);
            wagonButton = new Button();

            _buyCollection = new ItemCollection();
            _sellCollection = new ItemCollection();
            _itemTypesAccepted = StoreBuysItemType[DFLocation.BuildingTypes.GeneralStore];
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
            TransferAll(_buyListController, _remoteItemsController);
            TransferAll(_sellListController, _localItemsController);
            base.OnPop();
            HideWindow();
            _tradeWindowInstance?.Cleanup();
            UIManager.tooltipManager.HideActiveTooltip();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
        }
        #endregion


        #region Overridden Base Functions
        public override void OnPush()
        {
            base.OnPush();

            if (!IsSetup)
            {
                Setup();
            }

            if (!UsingIdentifySpell)
            {
                _buildingDiscoveryData = GameManager.Instance.PlayerEnterExit.BuildingDiscoveryData;
                // Get building info, message if invalid, otherwise setup accepted item list
                if (_buildingDiscoveryData.buildingKey <= 0)
                {
                    DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("oldSaveNoTrade"), true);
                }
                else if (WindowMode == WindowModes.Sell)
                {
                    StoreBuysItemType.TryGetValue(_buildingDiscoveryData.buildingType, out List<ItemGroups> buyTypes);
                    if (buyTypes == null)
                    {
                        buyTypes = StoreBuysItemType[DFLocation.BuildingTypes.GeneralStore];
                    }
                    _itemTypesAccepted = buyTypes;
                }
            }

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
            _remoteItemsController.SetItemCollection(remoteItems);

            _buyListController.SetItemCollection(_buyCollection);
            _sellListController.SetItemCollection(_sellCollection);

            UpdateBuySubtotal();
            UpdateSellSubtotal();
            UpdateTradeTotal();
        }

        protected override void Setup()
        {
            IsSetup = true;

            if (_tradeWindowInstance == null)
            {
                #region Window Instance
                UITradeWindow tradeWindow = UIManager.Instance.GetWindowInstance(UIWindowInstanceType.Trade) as UITradeWindow;
                if (tradeWindow == null)
                {
                    Debug.LogError("[AcrealUI.UITradeWindowController] UIManager.GetWindowInstance(UIWindowInstanceType.Trade) returned " + (tradeWindow == null ? " NULL!" : "a window of the wrong type! Expected type UITradeWindow, but got " + tradeWindow.GetType().ToString() + "!"));
                    return;
                }

                _tradeWindowInstance = tradeWindow;
                _tradeWindowInstance.Initialize();

                _tradeWindowInstance.Event_ToggledOn_InventoryTab_Player += () =>
                {
                    usingWagon = false;
                    localItems = playerEntity.Items;
                    _localItemsController.SetItemCollection(localItems);
                };

                _tradeWindowInstance.Event_ToggledOn_InventoryTab_Wagon += () =>
                {
                    usingWagon = true;
                    localItems = playerEntity.WagonItems;
                    _localItemsController.SetItemCollection(localItems);
                };

                _tradeWindowInstance.Event_OnConfirmTrade += () =>
                {
                    if (playerEntity == null) { return; }

                    int totalValue = GetNetTradeValue();
                    bool canAfford = totalValue <= 0 || playerEntity.GoldPieces >= totalValue;
                    if (canAfford && UIUtilityFunctions.PlayerCanCarryItems(_buyCollection, true, out List<int> playerCarryList))
                    {
                        TransferAll(_sellListController, _remoteItemsController);

                        for (int i = 0; i < _buyCollection.Count; i++)
                        {
                            DaggerfallUnityItem item = _buyCollection.GetItem(i);
                            
                            int playerStackSize = playerCarryList[i];
                            if (playerStackSize == 0)
                            {
                                playerEntity.WagonItems.AddItem(item);
                            }
                            else if (playerStackSize < item.stackCount)
                            {
                                DaggerfallUnityItem splitItem = _buyCollection.SplitStack(item, playerStackSize);
                                playerEntity.Items.AddItem(splitItem);
                                playerEntity.WagonItems.AddItem(item);
                            }
                            else
                            {
                                playerEntity.Items.AddItem(item);
                            }
                        }
                        _buyCollection.Clear();
                        _buyListController.UpdateItemList(true);

                        //handle payment to player
                        bool receivedLetterOfCredit = false;
                        if (totalValue < 0f)
                        {
                            totalValue = Mathf.Abs(totalValue);
                            float gWeight = totalValue * DaggerfallBankManager.goldPieceWeightInKg;
                            if (playerEntity.MaxEncumbrance - playerEntity.CarriedWeight >= gWeight)
                            {
                                playerEntity.GoldPieces += totalValue;
                            }
                            else
                            {
                                DaggerfallUnityItem loc = ItemBuilder.CreateItem(ItemGroups.MiscItems, (int)MiscItems.Letter_of_credit);
                                loc.value = totalValue;
                                playerEntity.Items.AddItem(loc);
                                receivedLetterOfCredit = true;
                            }
                        }
                        else //handle payment to merchant
                        {
                            playerEntity.DeductGoldAmount(totalValue);
                        }

                        //play sound effect
                        DaggerfallUI.Instance.PlayOneShot(receivedLetterOfCredit
                            ? SoundClips.ParchmentScratching
                            : SoundClips.GoldPieces);

                        //NOTE(Acreal): tallying the skill this way means that to optimize
                        //for advancing this skill, you want to confirm trades one item at
                        //a time.
                        //tally per item?
                        //tally per action? (ie buy, sell, repair)
                        playerEntity.TallySkill(DFCareer.Skills.Mercantile, 1);
                        
                        if (receivedLetterOfCredit)
                        {
                            DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("letterOfCredit"));
                        }
                        
                        _localItemsController?.UpdateItemList(true);
                    }
                };
                #endregion

                #region Character Panel
                _characterPanelController = new UICharacterPanelController(_tradeWindowInstance.characterPanel);
                _characterPanelController.Initialize();
                #endregion

                #region Local List
                _localItemsController = new UIItemListController(_tradeWindowInstance.itemList_localItems, Guild);
                _localItemsController.Initialize();
                _localItemsController.itemList.SetItemSortingFlags(ItemSortingFlags.Default);

                _localItemsController.DataSource_DisableItemEntryInput = (GameObject sender) =>
                {
                    UIItemEntry itemEntry = sender.GetComponent<UIItemEntry>();
                    DaggerfallUnityItem item = localItems.GetItem(itemEntry.itemUID);
                    return item == null || _itemTypesAccepted == null || !_itemTypesAccepted.Contains(item.ItemGroup);
                };

                _localItemsController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount == 2)
                    {
                        TransferItem(itemEntry, _localItemsController, _sellListController);
                        UpdateSellSubtotal();
                        UpdateTradeTotal();
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
                                                                                0, item.stackCount - 1, defaultValue, true, //slider params
                                                                                new object[1] { item }, //additional meta data to pass along
                                                                                (float sliderValue, object[] dataPayload) => //on confirm
                                                                                {
                                                                                    UIManager.popupManager.HideActivePopupWindow();

                                                                                    DaggerfallUnityItem itemToSplit = dataPayload[0] as DaggerfallUnityItem;

                                                                                    int splitSize = (int)sliderValue;
                                                                                    DaggerfallUnityItem splitItem = _localItemsController.itemCollection.SplitStack(itemToSplit, splitSize);
                                                                                    TransferItem(splitItem, _localItemsController, _sellListController);
                                                                                    UpdateSellSubtotal();
                                                                                    UpdateTradeTotal();
                                                                                },
                                                                                (_) => { UIManager.popupManager.HideActivePopupWindow(); });
                            }
                            else
                            {
                                TransferItem(itemEntry, _localItemsController, _sellListController);
                                UpdateSellSubtotal();
                                UpdateTradeTotal();
                            }
                        }
                    }
                };
                #endregion

                #region Remote List
                _remoteItemsController = new UIItemListController(_tradeWindowInstance.itemList_remoteItems, Guild, true);
                _remoteItemsController.Initialize();
                _remoteItemsController.itemList.SetItemSortingFlags(ItemSortingFlags.Default);

                _remoteItemsController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount == 2)
                    {
                        TransferItem(itemEntry, _remoteItemsController, _buyListController);
                        UpdateBuySubtotal();
                        UpdateTradeTotal();
                    }
                };

                _remoteItemsController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    DaggerfallUnityItem item = remoteItems?.GetItem(itemEntry.itemUID);
                    if (item != null)
                    {
                        bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                        if (splitting)
                        {
                            int maxCount = item.stackCount - 1;
                            int defaultValue = item.stackCount / 2;

                            UIManager.popupManager.ShowSliderConfirmationWindow(UITextStrings.InventoryWindow_Label_SplitStack.GetText(), string.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                            0, item.stackCount - 1, defaultValue, true, //slider params
                                                                            new object[1] { item }, //additional meta data to pass along
                                                                            (float sliderValue, object[] dataPayload) => //on confirm
                                                                            {
                                                                                UIManager.popupManager.HideActivePopupWindow();

                                                                                DaggerfallUnityItem itemToSplit = dataPayload[0] as DaggerfallUnityItem;

                                                                                int splitSize = (int)sliderValue;
                                                                                DaggerfallUnityItem splitItem = _remoteItemsController.itemCollection.SplitStack(itemToSplit, splitSize);
                                                                                TransferItem(splitItem, _remoteItemsController, _buyListController);
                                                                                UpdateBuySubtotal();
                                                                                UpdateTradeTotal();
                                                                            },
                                                                            (_) => { UIManager.popupManager.HideActivePopupWindow(); });
                        }
                        else
                        {
                            TransferItem(itemEntry, _remoteItemsController, _buyListController);
                            UpdateBuySubtotal();
                            UpdateTradeTotal();
                        }
                    }
                };
                #endregion

                #region Buy List
                _buyListController = new UIItemListController(_tradeWindowInstance.buyList, Guild, true);
                _buyListController.Initialize();
                _buyListController.itemList.SetItemSortingFlags(ItemSortingFlags.MerchantItemFlags);

                _buyListController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount == 2)
                    {
                        TransferItem(itemEntry, _buyListController, _remoteItemsController);
                        UpdateBuySubtotal();
                        UpdateTradeTotal();
                    }
                };

                _buyListController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    TransferItem(itemEntry, _buyListController, _remoteItemsController);
                    UpdateBuySubtotal();
                    UpdateTradeTotal();
                };

                UITradeItemList buyTradeList = _buyListController.itemList as UITradeItemList;
                if (buyTradeList != null)
                {
                    buyTradeList.Event_OnButtonClicked_EmptyList += () =>
                    {
                        TransferAll(_buyListController, _remoteItemsController);
                        UpdateBuySubtotal();
                        UpdateTradeTotal();
                    };
                }
                #endregion

                #region Sell List
                _sellListController = new UIItemListController(_tradeWindowInstance.sellList, Guild);
                _sellListController.Initialize();
                _sellListController.itemList.SetItemSortingFlags(ItemSortingFlags.MerchantItemFlags);

                _sellListController.Event_OnItemLeftClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    if (clickCount != 2) return;
                    TransferItem(itemEntry, _sellListController, _localItemsController);
                    UpdateSellSubtotal();
                    UpdateTradeTotal();
                };

                _sellListController.Event_OnItemRightClicked += (UIItemEntry itemEntry, int clickCount) =>
                {
                    TransferItem(itemEntry, _sellListController, _localItemsController);
                    UpdateSellSubtotal();
                    UpdateTradeTotal();
                };

                UITradeItemList sellTradeList = _sellListController.itemList as UITradeItemList;
                if (sellTradeList != null)
                {
                    sellTradeList.Event_OnButtonClicked_EmptyList += () =>
                    {
                        TransferAll(_sellListController, _localItemsController);
                        UpdateSellSubtotal();
                        UpdateTradeTotal();
                    };
                }
                #endregion
            }
        }

        //unused functions
        public override void Draw() { }
        public override void Refresh(bool refreshPaperDoll = true) { }
        protected override void UpdateLocalTargetIcon() { }
        protected override void UpdateRemoteTargetIcon() { }
        protected override void ShowTradePopup() { }
        protected override void FilterLocalItems() { }
        protected override void FilterRemoteItems() { }
        protected override void RaiseOnTradeHandler(int numItems, int value) { }
        protected override void ConfirmTrade_OnButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton) { }
        protected override void AccessoryItemsButton_OnLeftMouseClick(BaseScreenComponent sender, Vector2 position) { }
        protected override void AccessoryItemsButton_OnMouseClick(BaseScreenComponent sender, Vector2 position, ActionModes actionMode) { }
        protected override void AccessoryItemsButton_OnMouseEnter(BaseScreenComponent sender) { }
        protected override void AccessoryItemsButton_OnRightMouseClick(BaseScreenComponent sender, Vector2 position) { }
        protected override void GoldButton_OnMouseEnter(BaseScreenComponent sender) { }
        protected override void ItemListScroller_OnHover(DaggerfallUnityItem item) { }
        protected override void LocalItemListScroller_OnItemClick(DaggerfallUnityItem item, ActionModes actionMode) { }
        protected override void LocalItemListScroller_OnHover(DaggerfallUnityItem item) { }
        protected override void LocalItemListScroller_OnItemLeftClick(DaggerfallUnityItem item) { }
        protected override void LocalItemListScroller_OnItemMiddleClick(DaggerfallUnityItem item) { }
        protected override void LocalItemListScroller_OnItemRightClick(DaggerfallUnityItem item) { }
        protected override void PaperDoll_OnLeftMouseClick(BaseScreenComponent sender, Vector2 position) { }
        protected override void PaperDoll_OnMiddleMouseClick(BaseScreenComponent sender, Vector2 position) { }
        protected override void PaperDoll_OnMouseClick(BaseScreenComponent sender, Vector2 position, ActionModes actionMode) { }
        protected override void PaperDoll_OnMouseMove(int x, int y) { }
        protected override void PaperDoll_OnRightMouseClick(BaseScreenComponent sender, Vector2 position) { }
        protected override void RemoteItemListScroller_OnHover(DaggerfallUnityItem item) { }
        protected override void RemoteItemListScroller_OnItemClick(DaggerfallUnityItem item, ActionModes actionMode) { }
        protected override void RemoteItemListScroller_OnItemLeftClick(DaggerfallUnityItem item) { }
        protected override void RemoteItemListScroller_OnItemMiddleClick(DaggerfallUnityItem item) { }
        protected override void RemoteItemListScroller_OnItemRightClick(DaggerfallUnityItem item) { }
        protected override ActionModes GetActionModeRightClick() { return ActionModes.Info; }
        protected override Color ItemBackgroundColourHandler(DaggerfallUnityItem item) { return Color.clear; }
        #endregion


        #region Gold/Weight
        private void UpdateBuySubtotal()
        {
            UITradeItemList buyTradeList = _buyListController.itemList as UITradeItemList;
            buyTradeList?.SetSubtotalText(GetCollectionValueSubtotal(_buyCollection, true).ToString("N0"));
        }

        private void UpdateSellSubtotal()
        {
            UITradeItemList sellTradeList = _sellListController.itemList as UITradeItemList;
            sellTradeList?.SetSubtotalText(GetCollectionValueSubtotal(_sellCollection, false).ToString("N0"));
        }

        private void UpdateTradeTotal()
        {
            if (_tradeWindowInstance != null)
            {
                int totalValue = GetNetTradeValue();
                bool canAfford = totalValue <= 0 || UIUtilityFunctions.PlayerHasEnoughGold(totalValue);
                bool canCarry = UIUtilityFunctions.PlayerCanCarryItems(_buyCollection, true, out _);
                _tradeWindowInstance?.SetConfirmButtonEnabled(canAfford && canCarry);

                Color color = Color.white;
                char symbol = ' ';
                if (totalValue != 0)
                {
                    symbol = totalValue > 0 ? '-' : '+';
                    color = totalValue > 0 ? Color.red : Color.green;
                }
                totalValue = Mathf.Abs(totalValue);
                _tradeWindowInstance.SetTotalTradeValueText(symbol + totalValue.ToString("N0"), color);
            }
        }

        private int GetCollectionValueSubtotal(ItemCollection collection, bool buying)
        {
            int subtotal = 0;
            if (collection != null)
            {
                UIItemQueryOptions queryOptions = UIUtilityFunctions.GetItemQueryOptionsForPlayer(Guild);
                queryOptions.valueIsPurchaseCost = buying;

                for (int i = 0; i < collection.Count; i++)
                {
                    subtotal += UIUtilityFunctions.GetItemValue(collection.GetItem(i), queryOptions.shopQuality, queryOptions.holidayId, queryOptions.factionId, queryOptions.valueIsPurchaseCost);
                }
            }
            return subtotal;
        }

        /// <summary>
        /// gets the net value of the trade, summing up all items bought and sold
        /// </summary>
        private int GetNetTradeValue()
        {
            int buyTotal = GetCollectionValueSubtotal(_buyCollection, true);
            int sellTotal = GetCollectionValueSubtotal(_sellCollection, false);
            return buyTotal - sellTotal;
        }
        #endregion


        #region Item Transferring
        private void TransferAll(UIItemListController sourceController, UIItemListController destinationController)
        {
            if (sourceController != null && destinationController != null)
            {
                for (int i = sourceController.itemCollection.Count - 1; i >= 0; i--)
                {
                    TransferItem(sourceController.itemCollection.GetItem(i), sourceController, destinationController, false);
                }

                sourceController.UpdateItemList(true);
                destinationController.UpdateItemList(false);
                UIManager.tooltipManager.HideActiveTooltip();
            }
        }

        private void TransferItem(DaggerfallUnityItem item, UIItemListController sourceController, UIItemListController destinationController, bool updateLists = true)
        {
            if (item != null && sourceController != null && destinationController != null)
            {
                TransferItem(item, sourceController.itemCollection, destinationController.itemCollection);

                if (updateLists)
                {
                    sourceController.UpdateItemList(true);
                    destinationController.UpdateItemList(false);
                    UIManager.tooltipManager.HideActiveTooltip();
                }
            }
        }

        private void TransferItem(UIItemEntry itemEntry, UIItemListController sourceController, UIItemListController destinationController, bool updateLists = true)
        {
            DaggerfallUnityItem item = itemEntry != null && sourceController != null ? sourceController.itemCollection?.GetItem(itemEntry.itemUID) : null;
            TransferItem(item, sourceController, destinationController, updateLists);
        }
        #endregion
    }
}
