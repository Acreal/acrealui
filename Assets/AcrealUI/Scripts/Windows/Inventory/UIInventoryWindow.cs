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
        [Header("Panels")]
        [SerializeField] private string _gameObjName_panel_playerStats = null;

        [Header("Scroll Lists")]
        [SerializeField] private string _gameObjName_itemList_playerInventory = null;
        [SerializeField] private string _gameObjName_itemList_container = null;

        private UIPanelPlayerStats _panel_playerStats = null;
        private UIInventoryWindow_ItemList _itemList_playerInventory = null;
        private UIInventoryWindow_ItemList _itemList_container = null;
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

            Transform statsTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_panel_playerStats);
            if (statsTform != null)
            {
                _panel_playerStats = statsTform.GetComponent<UIPanelPlayerStats>();
                if (_panel_playerStats != null)
                {
                    _panel_playerStats.Initialize();
                }
            }

            Transform invTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_playerInventory);
            if (invTform != null)
            {
                _itemList_playerInventory = invTform.GetComponent<UIInventoryWindow_ItemList>();
                if (_itemList_playerInventory != null)
                {
                    _itemList_playerInventory.Initialize();
                }
                else { Debug.LogError("[AcrealUI] Failed to Find PlayerInventory ItemList!"); }
            }

            Transform conTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_container);
            if (conTform != null)
            {
                _itemList_container = conTform.GetComponent<UIInventoryWindow_ItemList>();
                if (_itemList_container != null)
                {
                    _itemList_container.Initialize();
                }
                else { Debug.LogError("[AcrealUI] Failed to Find Container ItemList!"); }
            }
        }

        public override void Cleanup()
        {
            _panel_playerStats?.Cleanup();
            _panel_playerStats = null;

            _itemList_playerInventory?.Cleanup();
            _itemList_playerInventory = null;

            _itemList_container?.Cleanup();
            _itemList_container = null;

            base.Cleanup();
        }

        public override void ResetWindow()
        {
            _panel_playerStats?.ResetPanel();
            _itemList_playerInventory?.ResetList();
            _itemList_container?.ResetList();
            base.ResetWindow();
        }
        #endregion


        #region UIWindow Overrides
        protected override void ShowInternal()
        {
            base.ShowInternal();

            _panel_playerStats?.Show();
            _itemList_playerInventory?.Show();

            if (_itemList_container != null)
            {
                if (_itemList_container.itemCount > 0)
                {
                    _itemList_container.Show();
                }
                else
                {
                    _itemList_container.Hide(true);
                }
            }
        }

        protected override void HideInternal()
        {
            base.HideInternal();

            _panel_playerStats?.Hide();
            _itemList_playerInventory?.Hide();
            _itemList_container?.Hide();
        }
        #endregion
    }
}