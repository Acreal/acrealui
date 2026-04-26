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

using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using UnityEngine;

namespace AcrealUI
{
    public class UITradeWindowController : DaggerfallTradeWindow, IWindowController
    {
        private UITradeWindow _tradeWindowInstance = null;
        private UICharacterPanelController _characterPanelController = null;


        #region Initialization/Cleanup
        public UITradeWindowController(IUserInterfaceManager uiManager) : base(uiManager)
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
            usingWagon = false;
            localItems = playerEntity.Items;
        }

        public void HideWindow()
        {

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
        }

        protected override void Setup()
        {
            IsSetup = true;

            if (_tradeWindowInstance == null)
            {
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

                _characterPanelController = new UICharacterPanelController(_tradeWindowInstance.characterPanel);
                _characterPanelController.Initialize();
            }
        }

        public override void Draw() { }
        #endregion
    }
}
