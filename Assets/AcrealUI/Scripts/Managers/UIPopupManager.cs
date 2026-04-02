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
        private Stack<IWindowController> _windowStack = null;
        #endregion


        #region Initalization
        public void Initialize()
        {
            if (_windowStack == null)
            {
                _windowStack = new Stack<IWindowController>(4);
            }
        }
        #endregion


        #region Public API
        public void PushSliderConfirmationWindow(string title, string message,int minSliderValue, int maxSliderValue, bool useWholeNumbers, object[] dataPayload, System.Action<float, object[]> onConfirm, System.Action<object[]> onCancel)
        {
            UISliderConfirmationWindowController windowController = new UISliderConfirmationWindowController();
            if (windowController != null)
            {
                _windowStack.Push((IWindowController)windowController);

                windowController.SetText(title, message);
                windowController.SetSliderMinMax(minSliderValue, maxSliderValue, useWholeNumbers);
                windowController.SetDataPayload(dataPayload);
                windowController.RegisterEvents(onConfirm, onCancel);
                windowController.ShowWindow();
            }
        }

        public void PushConfirmationWindow(string title, string message, object[] dataPayload, System.Action<object[]> onConfirm, System.Action<object[]> onCancel)
        {
            UIConfirmationWindowController windowController = new UIConfirmationWindowController();
            if (windowController != null)
            {
                _windowStack.Push((IWindowController)windowController);

                windowController.SetText(title, message);
                windowController.SetDataPayload(dataPayload);
                windowController.RegisterEvents(onConfirm, onCancel);
                windowController.ShowWindow();
            }
        }

        public void PopWindow()
        {
            if (_windowStack != null)
            {
                IWindowController windowController = _windowStack.Pop();
                if (windowController != null)
                {
                    windowController.HideWindow();
                    windowController.DestroyWindow();
                }
            }
        }
        #endregion
    }
}
