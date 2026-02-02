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
using UnityEngine;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIShowTextTooltip : UIShowTooltip
    {
        #region Variables
        [SerializeField] private string _titleText = null;
        [SerializeField] private string _messageText = null;
        [SerializeField] private bool _titleTextIsLocalizationKey = false;
        [SerializeField] private bool _messageTextIsLocalizationKey = false;
        #endregion


        #region Initialization
        public override void Initialize(UIInteractiveElement parentElement)
        {
            base.Initialize(parentElement);

            if(_titleTextIsLocalizationKey) 
            {
                _titleText = UIUtilityFunctions.GetLocalizedText(_titleText);
            }

            if(_messageTextIsLocalizationKey)
            {
                _messageText = UIUtilityFunctions.GetLocalizedText(_messageText);
            }
        }
        #endregion


        #region UIShowTooltip Overrides
        protected override void ShowInternal()
        {
            base.ShowInternal();
            UIManager.tooltipManager.ShowTextTooltip(gameObject, _titleText, _messageText);
        }
        #endregion
    }
}
