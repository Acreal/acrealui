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

using UnityEngine;

namespace AcrealUI
{
    public class UISliderConfirmationWindowController : IWindowController
    {
        #region Variables
        private UISliderConfirmationWindow _sliderConfirmationWindow = null;
        private object[] _dataPayload = null;
        #endregion


        #region Events
        private event System.Action<float, object[]> Event_ButtonClick_OnConfirm = null;
        private event System.Action<object[]> Event_ButtonClick_OnCancel = null;
        #endregion


        #region Initialization/Cleanup
        public UISliderConfirmationWindowController()
        {
            CreateWindow();
        }

        public void CreateWindow()
        {
            if (_sliderConfirmationWindow == null && UIManager.referenceManager.prefab_confirmationWindow != null)
            {
                _sliderConfirmationWindow = Object.Instantiate(UIManager.referenceManager.prefab_sliderConfirmationWindow);
                _sliderConfirmationWindow.Initialize();
                _sliderConfirmationWindow.Hide();
                _sliderConfirmationWindow.SetBackButtonActive(false);
                _sliderConfirmationWindow.Event_ButtonClick_CloseWindow += () => { Event_ButtonClick_OnCancel?.Invoke(_dataPayload); };
                _sliderConfirmationWindow.Event_ButtonClick_PrevWindow += () => { Event_ButtonClick_OnCancel?.Invoke(_dataPayload); };
                _sliderConfirmationWindow.Event_OnConfirm += () => { Event_ButtonClick_OnConfirm?.Invoke(_sliderConfirmationWindow.sliderValue, _dataPayload); };
                _sliderConfirmationWindow.Event_OnCancel += () => { Event_ButtonClick_OnCancel?.Invoke(_dataPayload); };
            }
        }

        public void DestroyWindow()
        {
            if (_sliderConfirmationWindow != null)
            {
                Object.Destroy(_sliderConfirmationWindow.gameObject);
            }
            _sliderConfirmationWindow = null;
        }
        #endregion


        #region Public API
        public void SetText(string title, string message)
        {
            if (_sliderConfirmationWindow != null)
            {
                _sliderConfirmationWindow.SetHeaderText(title);
                _sliderConfirmationWindow.SetMessageText(message);
            }
        }

        public void SetSliderMinMax(float min, float max, bool useWholeNumbers = false)
        {
            if (_sliderConfirmationWindow != null)
            {
                _sliderConfirmationWindow.SetSliderMinMax(min, max, useWholeNumbers);
            }
        }

        public void SetDataPayload(object[] data)
        {
            _dataPayload = data;
        }

        public void RegisterEvents(System.Action<float, object[]> onConfirm, System.Action<object[]> onCancel)
        {
            Event_ButtonClick_OnConfirm = onConfirm;
            Event_ButtonClick_OnCancel = onCancel;

            if (_sliderConfirmationWindow != null)
            {
                _sliderConfirmationWindow.SetConfirmButtonActive(Event_ButtonClick_OnConfirm != null);
                _sliderConfirmationWindow.SetCancelButtonActive(Event_ButtonClick_OnCancel != null);
            }
        }
        #endregion


        #region IWindowController
        public void ShowWindow()
        {
            if (_sliderConfirmationWindow != null)
            {
                _sliderConfirmationWindow.Show();
            }
        }

        public void HideWindow()
        {
            Event_ButtonClick_OnConfirm = null;
            Event_ButtonClick_OnCancel = null;

            if (_sliderConfirmationWindow != null)
            {
                _sliderConfirmationWindow.Hide();
            }
        }
        #endregion
    }
}
