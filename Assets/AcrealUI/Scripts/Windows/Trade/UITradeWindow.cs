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
using TMPro;
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UITradeWindow : UIInventoryWindow
    {
        #region Variables
        [SerializeField] private string _gameObjName_itemList_buy = null;
        [SerializeField] private string _gameObjName_itemList_sell = null;
        [SerializeField] private string _gameObjName_tradeTotalText = null;
        [SerializeField] private string _gameObjName_confirmButton = null;

        private UIItemList _buyList = null;
        private UIItemList _sellList = null;
        private TextMeshProUGUI _tradeTotalText = null;
        private UIButton _confirmButton = null;
        #endregion


        #region Events
        public event System.Action Event_OnConfirmTrade = null;
        #endregion


        #region Properties
        public UIItemList buyList { get { return _buyList; } }
        public UIItemList sellList { get { return _sellList; } }
        #endregion


        #region Initialization/Cleanup
        public override void Initialize()
        {
            if (_buyList == null)
            {
                Transform buyTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_buy);
                if (buyTForm != null)
                {
                    _buyList = buyTForm.GetComponent<UIItemList>();
                    if (_buyList != null)
                    {
                        _buyList.Initialize();
                    }
                    else { Debug.LogError("[AcrealUI.UITradeWindow] Failed to Find Buy List!"); }
                }
            }

            if (_sellList == null)
            {
                Transform sellTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemList_sell);
                if (sellTForm != null)
                {
                    _sellList = sellTForm.GetComponent<UIItemList>();
                    if (_sellList != null)
                    {
                        _sellList.Initialize();
                    }
                    else { Debug.LogError("[AcrealUI.UITradeWindow] Failed to Find Sell List!"); }
                }
            }

            if (_tradeTotalText == null)
            {
                Transform totalTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_tradeTotalText);
                if (totalTForm != null)
                {
                    _tradeTotalText = totalTForm.GetComponent<TextMeshProUGUI>();
                    if (_tradeTotalText == null) { Debug.LogError("[AcrealUI.UITradeWindow] Failed to Find TradeTotalText!"); }
                }
            }

            if (_confirmButton == null)
            {
                Transform confirmTForm = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_confirmButton);
                if (confirmTForm != null)
                {
                    _confirmButton = confirmTForm.GetComponent<UIButton>();
                    if (_confirmButton != null)
                    {
                        _confirmButton.Initialize();
                        _confirmButton.Event_OnLeftClick += (_, _1) => { Event_OnConfirmTrade?.Invoke(); };
                    }
                    else { Debug.LogError("[AcrealUI.UITradeWindow] Failed to Find Trade ConfirmButton!"); }
                }
            }

            base.Initialize();
        }

        public override void Cleanup()
        {
            Event_OnConfirmTrade = null;

            _buyList?.Cleanup();
            _buyList = null;

            _sellList?.Cleanup();
            _sellList = null;

            _confirmButton?.Cleanup();
            _confirmButton = null;

            _tradeTotalText = null;

            base.Cleanup();
        }

        public override void ResetWindow()
        {
            _buyList?.ResetList();
            _sellList?.ResetList();
            _confirmButton?.ResetElement();
            base.ResetWindow();
        }
        #endregion


        #region Public API
        public void SetTotalTradeValueText(string totalValueText, Color textColor)
        {
            if (_tradeTotalText != null)
            {
                _tradeTotalText.text = totalValueText;
                _tradeTotalText.color = textColor;
            }
        }

        public void SetConfirmButtonEnabled(bool enabled)
        {
            if (_confirmButton != null)
            {
                _confirmButton.isDisabled = !enabled;
            }
        }
        #endregion
    }
}
