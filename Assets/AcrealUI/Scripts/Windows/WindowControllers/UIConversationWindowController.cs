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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AcrealUI
{
    public class UIConversationWindowController : DaggerfallTalkWindow, IWindowController
    {
        private UIConversationWindow _conversationWindowInstance = null;


        public UIConversationWindowController(IUserInterfaceManager uiManager, DaggerfallBaseWindow previous = null) : base(uiManager, previous)
        {
            _conversationWindowInstance = Object.Instantiate(UIManager.referenceManager.prefab_conversationWindow);
            if(_conversationWindowInstance != null)
            {
                _conversationWindowInstance.Initialize();
                _conversationWindowInstance.Hide();
            }
        }

        public void ShowWindow()
        {
            if(_conversationWindowInstance != null)
            {
                _conversationWindowInstance.Show();
            }
        }

        public void HideWindow()
        {
            if(_conversationWindowInstance != null)
            {
                _conversationWindowInstance.Hide();
            }
        }


        public override void OnPush()
        {
            base.OnPush();
            ShowWindow();
        }

        public override void OnPop()
        {
            base.OnPop();
            HideWindow();
        }

        public override void Draw() { }
    }
}
