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
using TMPro;
using UnityEngine;

namespace AcrealUI
{
    public class UIConfirmationWindow : UIWindow
    {
        #region Variables
        [SerializeField] private string _gameObjName_messageText = null;
        [SerializeField] private string _gameObjName_confirmButton = null;
        [SerializeField] private string _gameObjName_cancelButton = null;

        private TextMeshProUGUI _messageText = null;
        private UIButton _confirmButton = null;
        private UIButton _cancelButton = null;
        #endregion


        #region Events
        public event System.Action Event_OnConfirm = null;
        public event System.Action Event_OnCancel = null;
        #endregion


        #region Initialize
        public override void Initialize()
        {
            base.Initialize();

            if(!string.IsNullOrWhiteSpace(_gameObjName_messageText))
            {
                Transform msgTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_messageText);
                if(msgTform != null) { _messageText = msgTform.GetComponent<TextMeshProUGUI>(); };
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_confirmButton))
            {
                Transform confirmTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_confirmButton);
                if (confirmTform != null) { _confirmButton = confirmTform.GetComponent<UIButton>(); };
                if(_confirmButton != null)
                {
                    _confirmButton.Initialize();
                    _confirmButton.Event_OnClicked += (_, _1) =>
                    {
                        Event_OnConfirm?.Invoke();
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_cancelButton))
            {
                Transform cancelTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_cancelButton);
                if (cancelTform != null) { _cancelButton = cancelTform.GetComponent<UIButton>(); }
                if (_cancelButton != null)
                {
                    _cancelButton.Initialize();
                    _cancelButton.Event_OnClicked += (_, _1) =>
                    {
                        Event_OnCancel?.Invoke();
                    };
                }
            }
        }
        #endregion


        #region Update
        protected override void Update()
        {
            if (DaggerfallUI.Instance.HotkeySequenceProcessed == HotkeySequence.HotkeySequenceProcessStatus.NotFound)
            {
                KeyCode escCode = InputManager.Instance.GetBinding(InputManager.Actions.Escape);
                if (InputManager.Instance.GetKeyUp(escCode) || InputManager.Instance.GetBackButtonUp())
                {
                    Event_OnCancel?.Invoke();
                }
            }
        }
        #endregion


        #region Public API
        public void SetMessageText(string text)
        {
            if (_messageText != null)
            {
                _messageText.text = text;
            }
        }

        public void SetConfirmButtonActive(bool active)
        {
            if (_confirmButton != null)
            {
                _confirmButton.gameObject.SetActive(active);
            }
        }

        public void SetCancelButtonActive(bool active)
        {
            if (_cancelButton != null)
            {
                _cancelButton.gameObject.SetActive(active);
            }
        }
        #endregion
    }
}
