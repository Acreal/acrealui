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
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System;
using System.Text;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace AcrealUI
{
    public class UIInventoryWindowController : DaggerfallInventoryWindow, IWindowController
    {
        #region Variables
        private UIInventoryWindow _inventoryWindowInstance = null;
        private UICharacterPanelController _characterPanelController = null;
        private UIItemListController _localItemsController = null;
        private UIItemListController _remoteItemsController = null;
        #endregion


        #region Initialization
        public UIInventoryWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            //base class needs these references, but we don't use them
            localItemListScroller = new ItemListScroller(defaultToolTip);
            remoteItemListScroller = new ItemListScroller(defaultToolTip);
            wagonButton = new Button();
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            _inventoryWindowInstance?.Show(true);
        }

        public void HideWindow()
        {
            _inventoryWindowInstance?.Hide();
        }

        public override void OnPop()
        {
            base.OnPop();
            HideWindow();
            _inventoryWindowInstance?.ResetWindow();
            UIManager.tooltipManager.HideActiveTooltip();
            GameManager.Instance.PlayerMouseLook.cursorActive = false;
        }
        #endregion


        #region Overridden Base Class Functions
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

            if (_inventoryWindowInstance != null)
            {
                _inventoryWindowInstance.Show();

                _inventoryWindowInstance.SetTotalGoldText(playerEntity.GoldPieces.ToString("N0"));
                _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());

                bool hasWagon = UIUtilityFunctions.PlayerHasWagonAccess();
                _inventoryWindowInstance.EnableOrDisableTabs(hasWagon, hasWagon);

                _characterPanelController.UpdateAll();
                _localItemsController.SetItemCollection(localItems);
                _remoteItemsController.SetItemCollection(remoteItems);

                _inventoryWindowInstance.SetLootPileIcon(UIUtilityFunctions.GetLootContainerIcon(lootTarget));
                _inventoryWindowInstance.SetLootPileActive(lootTarget != null);

                if (remoteItems != null && remoteItems.Count > 0)
                {
                    _inventoryWindowInstance.ShowRemoteItemsPanel();
                }
            }
        }

        protected override void Setup()
        {
            IsSetup = true;

            playerEntity = UIUtilityFunctions.GetPlayerEntity();

            if (_inventoryWindowInstance == null)
            {
                UIWindow window = UIManager.Instance.GetWindowInstance(UIWindowInstanceType.Inventory);
                if (window == null || !(window is UIInventoryWindow))
                {
                    Debug.LogError("[AcrealUI.UIInventoryWindowController] UIManager.GetWindowInstance(UIWindowInstanceType.Inventory) returned " + (window == null ? " NULL!" : "a window of the wrong type! Expected type UIInventoryWindow, but got " + window.GetType().ToString() + "!"));
                    return;
                }

                _inventoryWindowInstance = window as UIInventoryWindow;
                _inventoryWindowInstance.Initialize();

                _characterPanelController = new UICharacterPanelController(_inventoryWindowInstance.characterPanel);
                _characterPanelController.Initialize();

                _localItemsController = new UIItemListController(_inventoryWindowInstance.itemList_localItems);
                _localItemsController.Initialize();

                _remoteItemsController = new UIItemListController(_inventoryWindowInstance.itemList_remoteItems);
                _remoteItemsController.Initialize();


                #region Events
                _localItemsController.Event_OnItemLeftClicked += OnLocalItemLeftClicked;
                _localItemsController.Event_OnItemRightClicked += OnLocalItemRightClicked;

                _remoteItemsController.Event_OnItemLeftClicked += OnRemoteItemClicked;
                _remoteItemsController.Event_OnItemRightClicked += OnRemoteItemClicked;

                _inventoryWindowInstance.Event_ToggledOn_InventoryTab_Player += () =>
                {
                    usingWagon = false;
                    localItems = playerEntity.Items;
                    _localItemsController.SetItemCollection(localItems);
                    _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());
                };

                _inventoryWindowInstance.Event_ToggledOn_InventoryTab_Wagon += () =>
                {
                    usingWagon = true;
                    localItems = playerEntity.WagonItems;
                    _localItemsController.SetItemCollection(localItems);
                    _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());
                };

                _inventoryWindowInstance.Event_OnButtonClicked_Gold += () =>
                {
                    // Show message box
                    const int goldToDropTextId = 25;
                    DaggerfallInputMessageBox mb = new DaggerfallInputMessageBox(uiManager, this);
                    mb.SetTextTokens(goldToDropTextId);
                    mb.TextPanelDistanceY = 0;
                    mb.InputDistanceX = 15;
                    mb.InputDistanceY = -6;
                    mb.TextBox.Numeric = true;
                    mb.TextBox.MaxCharacters = 8;
                    mb.TextBox.Text = "0";
                    mb.OnGotUserInput += DropGoldPopup_OnGotUserInput;
                    mb.Show();
                };
                #endregion
            }
        }

        //OBSOLETE FUNCTIONS:
        public override void Draw() { }
        public override void Refresh(bool refreshPaperDoll = true) { }
        protected override void SetRemoteItemsAnimation() { }
        protected override void SelectTabPage(TabPages tabPage) { }
        protected override void SelectActionMode(ActionModes mode) { }
        protected override void UpdateLocalTargetIcon() { }
        protected override void UpdateRemoteTargetIcon() { }
        protected override void FilterLocalItems() { }
        protected override void FilterRemoteItems() { }
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
            else if(playerEntity != null)
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


        #region Input Handling
        private void OnLocalItemLeftClicked(UIItemEntry itemEntry, int clickCount)
        {
            if (itemEntry != null && localItems != null)
            {
                DaggerfallUnityItem item = localItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    if (usingWagon)
                    {
                        //when using the wagon, left click function
                        //changes to swapping item over to the dropped
                        //items list - at which point it can be added
                        //back into the player's inventory
                        OnLocalItemRightClicked(itemEntry, clickCount);
                    }
                    else
                    {
                        bool clearInventoryBeforeUpdate = false;

                        //contextual behavior based on item type
                        if (UIUtilityFunctions.ItemIsEquippable(item))
                        {
                            if (item.IsEquipped)
                            {
                                UnequipItem(item);
                            }
                            else
                            {
                                LocalItemListScroller_OnItemClick(item, ActionModes.Equip);
                            }
                        }
                        else if (UIUtilityFunctions.ItemTypeIsUseable(UIUtilityFunctions.ItemToItemType(item)))
                        {
                            LocalItemListScroller_OnItemClick(item, ActionModes.Use);

                            if(localItems.GetItem(itemEntry.itemUID) == null)
                            {
                                // item was consumed or removed
                                clearInventoryBeforeUpdate = true;
                            }
                        }
                        else
                        {
                            Debug.LogError("[AcrealUI.UIInventoryWindowController] Unhandled Item Type: " + UIUtilityFunctions.ItemToItemType(item));
                        }

                        _characterPanelController?.UpdateAll();
                        _localItemsController.UpdateItemList(clearInventoryBeforeUpdate);

                        if(clearInventoryBeforeUpdate)
                        {
                            UIManager.tooltipManager.HideActiveTooltip();
                        }
                    }

                    _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());
                }
            }
        }

        private void OnLocalItemRightClicked(UIItemEntry itemEntry, int clickCount)
        {
            if (itemEntry != null && localItems != null)
            {
                DaggerfallUnityItem item = localItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                    if (splitting)
                    {
                        int maxCount = item.stackCount - 1;
                        int defaultValue = item.stackCount / 2;

                        // TODO(Acreal): localize "Split Stack" string
                        UIManager.popupManager.ShowSliderConfirmationWindow("Split Stack", String.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                        0, item.stackCount - 1, true, //slider params
                                                                        new object[3] { localItems, remoteItems, item }, //additional meta data to pass along
                                                                        (float sliderValue, object[] dataPayload) => //on confirm
                                                                        {
                                                                            UIManager.popupManager.HideActivePopupWindow();

                                                                            ItemCollection sourceCollection = dataPayload[0] as ItemCollection;
                                                                            ItemCollection destCollection = dataPayload[1] as ItemCollection;
                                                                            DaggerfallUnityItem itemToSplit = dataPayload[2] as DaggerfallUnityItem;
                                                                            if (sourceCollection != null && itemToSplit != null)
                                                                            {
                                                                                int splitSize = (int)sliderValue;
                                                                                DaggerfallUnityItem splitItem = sourceCollection.SplitStack(itemToSplit, splitSize);
                                                                                HandleLocalItemRightClick(splitItem);
                                                                            }
                                                                        },
                                                                        (_) => { UIManager.popupManager.HideActivePopupWindow(); });
                    }
                    else
                    {
                        HandleLocalItemRightClick(item);
                    }
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

            LocalItemListScroller_OnItemClick(item, ActionModes.Remove);

            //item removed successfully
            if (!localItems.Contains(item))
            {
                _localItemsController.UpdateItemFilterToggles();
                _localItemsController.UpdateItemList(true);

                _remoteItemsController.UpdateItemFilterToggles();
                _remoteItemsController.UpdateItemList(false);

                UIManager.tooltipManager.HideActiveTooltip();

                if (_inventoryWindowInstance != null)
                {
                    _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());
                    _inventoryWindowInstance.SetLootPileActive(LootTarget != null || (droppedItems != null && droppedItems.Count > 0));
                    _inventoryWindowInstance.ShowRemoteItemsPanel();
                }
            }
        }

        private void OnRemoteItemClicked(UIItemEntry itemEntry, int clickCount)
        {
            if (itemEntry != null && remoteItems != null)
            {
                DaggerfallUnityItem item = remoteItems.GetItem(itemEntry.itemUID);
                if (item != null)
                {
                    bool splitting = item.IsAStack() && item.stackCount >= 2 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
                    if (splitting)
                    {
                        int maxCount = item.stackCount - 1;
                        int defaultValue = item.stackCount / 2;

                        // TODO(Acreal): localize "Split Stack" string
                        UIManager.popupManager.ShowSliderConfirmationWindow("Split Stack", string.Format(TextManager.Instance.GetLocalizedText("howManyItems"), maxCount),
                                                                        0, item.stackCount - 1, true, //slider params
                                                                        new object[3] { localItems, remoteItems, item }, //additional meta data to pass along
                                                                        (float sliderValue, object[] dataPayload) => //on confirm
                                                                        {
                                                                            UIManager.popupManager.HideActivePopupWindow();

                                                                            ItemCollection localCollection = dataPayload[0] as ItemCollection;
                                                                            ItemCollection remoteCollection = dataPayload[1] as ItemCollection;
                                                                            DaggerfallUnityItem itemToSplit = dataPayload[2] as DaggerfallUnityItem;
                                                                            if (remoteCollection != null && localCollection != null && itemToSplit != null)
                                                                            {
                                                                                int splitSize = (int)sliderValue;
                                                                                DaggerfallUnityItem splitItem = remoteCollection.SplitStack(item, splitSize);
                                                                                HandleContainerItemClick(splitItem);
                                                                                _remoteItemsController?.UpdateItemList(false);
                                                                            }
                                                                        },
                                                                        (_) => { UIManager.popupManager.HideActivePopupWindow(); });
                    }
                    else
                    {
                        HandleContainerItemClick(item);
                    }
                }
            }
        }

        private void HandleContainerItemClick(DaggerfallUnityItem item)
        {
            if (item != null)
            {
                if (usingWagon)
                {
                    //this is a small workaround for allowing the base class's
                    //code to handle this case appropriately, despite moving
                    //the wagon display to where the player's inventory is
                    //(in base class the wagon's inventory would be kept in "remoteItems")
                    ItemCollection prevLocalItems = localItems;
                    ItemCollection prevRemoteItems = remoteItems;

                    localItems = remoteItems;
                    remoteItems = playerEntity.WagonItems;

                    LocalItemListScroller_OnItemClick(item, ActionModes.Remove);

                    localItems = prevLocalItems;
                    remoteItems = prevRemoteItems;
                }
                else
                {
                    RemoteItemListScroller_OnItemClick(item, ActionModes.Remove);
                }

                //item was successfully transferred to player
                if (!remoteItems.Contains(item))
                {
                    _remoteItemsController.UpdateItemFilterToggles();
                    _remoteItemsController.UpdateItemList(true);

                    _localItemsController.UpdateItemFilterToggles();
                    _localItemsController.UpdateItemList(true);

                    UIManager.tooltipManager.HideActiveTooltip();

                    if (UIUtilityFunctions.ItemIsGold(item))
                    {
                        _inventoryWindowInstance.SetTotalGoldText(playerEntity.GoldPieces.ToString("N0"));
                    }

                    _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());

                    if (remoteItems.Count == 0)
                    {
                        if (lootTarget != null)
                        {
                            PopWindow();
                        }
                        else
                        {
                            _inventoryWindowInstance.HideRemoteItemsPanel();
                        }
                    }
                }
            }
        }

        private void DropGoldPopup_OnGotUserInput(DaggerfallInputMessageBox sender, string input)
        {
            // Determine how many gold pieces to drop
            int goldToDrop = 0;
            bool result = int.TryParse(input, out goldToDrop);
            if (!result || goldToDrop < 1)
            {
                return;
            }

            if(playerEntity != null)
            {
                // Get player gold count
                int playerGold = playerEntity.GoldPieces;
                if (goldToDrop > playerGold)
                {
                    return;
                }

                // Create new item for gold pieces and add to other container
                DaggerfallUnityItem goldPieces = ItemBuilder.CreateGoldPieces(goldToDrop);
                remoteItems.AddItem(goldPieces);

                // Remove gold count from player
                GameManager.Instance.PlayerEntity.GoldPieces -= goldToDrop;

                if (_inventoryWindowInstance != null)
                {
                    //update player gold display
                    if (_inventoryWindowInstance.itemList_localItems != null)
                    {
                        _inventoryWindowInstance.SetTotalGoldText(playerEntity.GoldPieces.ToString("N0"));
                        _inventoryWindowInstance.SetTotalWeightText(BuildPlayerWeightString());
                    }

                    //update and show dropped item display
                    if (_inventoryWindowInstance.itemList_remoteItems != null)
                    {
                        _inventoryWindowInstance.SetLootPileActive(LootTarget != null || (remoteItems != null && remoteItems.Count > 0));
                        _inventoryWindowInstance.SetLootPileIcon(UIUtilityFunctions.GetLootContainerIcon(lootTarget));
                        _remoteItemsController.UpdateItemFilterToggles();
                        _remoteItemsController.UpdateItemList(false);

                        UIManager.tooltipManager.HideActiveTooltip();

                        _inventoryWindowInstance.ShowRemoteItemsPanel();
                    }
                }
            }
        }
        #endregion
    }
}
