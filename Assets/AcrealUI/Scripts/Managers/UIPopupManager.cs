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

using System.Collections.Generic;

namespace AcrealUI
{
    public class UIPopupManager
    {
        #region Variables
        private IWindowController _activePopupController = null;
        
        private readonly UIConfirmationWindowController _textConfirmationController = null;
        private readonly UISliderConfirmationWindowController _sliderConfirmationController = null;
        #endregion


        #region Initialization/Cleanup

        public UIPopupManager()
        {
            _textConfirmationController = new UIConfirmationWindowController();
            _sliderConfirmationController = new UISliderConfirmationWindowController();
        }
        #endregion


        #region Public API
        public void ShowTextConfirmationWindow(string title, string message, object[] dataPayload, System.Action<object[]> onConfirm, System.Action<object[]> onCancel)
        {
            HideActivePopupWindow();

            if (_textConfirmationController != null)
            {
                _textConfirmationController.SetText(title, message);
                _textConfirmationController.SetDataPayload(dataPayload);
                _textConfirmationController.RegisterEvents(onConfirm, onCancel);
                _textConfirmationController.ShowWindow();
            }
            _activePopupController = _textConfirmationController;
        }

        public void ShowSliderConfirmationWindow(string title, string message, float minSliderValue, float maxSliderValue, float currentSliderValue, bool useWholeNumbers, object[] dataPayload, System.Action<float, object[]> onConfirm, System.Action<object[]> onCancel)
        {
            HideActivePopupWindow();

            if (_sliderConfirmationController != null)
            {
                _sliderConfirmationController.SetText(title, message);
                _sliderConfirmationController.SetSliderMinMax(minSliderValue, maxSliderValue, useWholeNumbers);
                _sliderConfirmationController.SetSliderValue(currentSliderValue, false);
                _sliderConfirmationController.SetDataPayload(dataPayload);
                _sliderConfirmationController.RegisterEvents(onConfirm, onCancel);
                _sliderConfirmationController.ShowWindow();
            }
            _activePopupController = _sliderConfirmationController;
        }

        public void HideActivePopupWindow()
        {
            if (_activePopupController != null)
            {
                _activePopupController.HideWindow();
                _activePopupController.OnPop();
            }
        }
        #endregion
    }
}
