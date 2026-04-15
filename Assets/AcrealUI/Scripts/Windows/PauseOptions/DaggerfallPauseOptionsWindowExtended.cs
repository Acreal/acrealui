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


/*
This is a simple extension to the default DaggerfallPauseOptionsWindow that adds a toggle
for the new pause window UI
*/

using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Utility.AssetInjection;
using UnityEngine;

namespace AcrealUI
{
    public class DaggerfallPauseOptionsWindowExtended : DaggerfallPauseOptionsWindow
    {
        protected Color continueButtonBackgroundColor = new Color(0.5f, 0.0f, 0.0f, 1.0f);
        protected Button useNewWindowButton = null;

        public DaggerfallPauseOptionsWindowExtended(IUserInterfaceManager uiManager, IUserInterfaceWindow previousWindow = null)
            : base(uiManager, previousWindow)
        {

        }

        protected override void Setup()
        {
            base.Setup();

            // Use New UI button
            useNewWindowButton = DaggerfallUI.AddButton(new Rect(optionsPanel.Size.x / 2f - 40, optionsPanel.Size.y + 2, 80, 10), optionsPanel);
            useNewWindowButton.Label.Text = "Use New Pause Menu";
            useNewWindowButton.Size = new Vector2(80, 10);

            if (TextureReplacement.TryImportTexture("advancedControlsContinueButtonBackgroundColor", true, out Texture2D tex))
            {
                useNewWindowButton.BackgroundTexture = tex;
            }
            else
            {
                useNewWindowButton.BackgroundColor = continueButtonBackgroundColor;
            }

            useNewWindowButton.OnMouseClick += (_, _1) =>
            {
                UIUtilityFunctions.PlayButtonClickSound();
                UIManager.Instance.EnableCoreWindow(UIWindowType.PauseOptions);
                CancelWindow();
                UIManager.Instance.ApplyCoreWindowChanges();
                DaggerfallUI.Instance.UserInterfaceManager.PostMessage(DaggerfallUIMessages.dfuiOpenPauseOptionsDialog);
            };
        }
    }
}
