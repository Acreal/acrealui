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
    public class UISortToggleFeedbackGameObjectToggle : UIToggleFeedbackGameObjectToggle
    {
        #region Variables
        [SerializeField] private bool _activateObjWhenSortAscending = false;
        [SerializeField] private bool _activateObjWhenAscendingAndPressed = false;
        [SerializeField] private bool _activateObjWhenAscendingAndHighlighted = false;

        [SerializeField] private bool _activateObjWhenSortDescending = false;
        [SerializeField] private bool _activateObjWhenDescendingAndPressed = false;
        [SerializeField] private bool _activateObjWhenDescendingAndHighlighted = false;
        #endregion


        #region Properties
        private UISortToggle sortToggle { get { return toggle as  UISortToggle; } }
        #endregion


        #region Initialization
        public override void Initialize(UIInteractiveElement uiElement)
        {
            base.Initialize(uiElement);

            if(sortToggle != null)
            {
                sortToggle.Event_OnSortAscendingChanged += (_) => { Refresh(); };
            }
        }
        #endregion


        #region Updates
        public override void Refresh()
        {
            if(_gameObjToToggle == null)
            {
                return;
            }

            if (_interactiveElement.isDisabled)
            {
                _gameObjToToggle.SetActive(_activateObjOnDisable);
            }
            else if (sortToggle != null)
            {
                if (sortToggle.isToggledOn)
                {
                    if (sortToggle.sortByAscending)
                    {
                        if (_interactiveElement.isPressed) { _gameObjToToggle.SetActive(_activateObjWhenAscendingAndPressed); }
                        else if (_interactiveElement.isHighlighted) { _gameObjToToggle.SetActive(_activateObjWhenAscendingAndHighlighted); }
                        else { _gameObjToToggle.SetActive(_activateObjWhenSortAscending); }
                    }
                    else
                    {
                        if (_interactiveElement.isPressed) { _gameObjToToggle.SetActive(_activateObjWhenDescendingAndPressed); }
                        else if (_interactiveElement.isHighlighted) { _gameObjToToggle.SetActive(_activateObjWhenDescendingAndHighlighted); }
                        else { _gameObjToToggle.SetActive(_activateObjWhenSortDescending); }
                    }
                }
                else
                {
                    if (_interactiveElement.isPressed) { _gameObjToToggle.SetActive(_activateObjWhenOffAndPressed); }
                    else if (_interactiveElement.isHighlighted) { _gameObjToToggle.SetActive(_activateObjWhenOffAndHighlighted); }
                    else { _gameObjToToggle.SetActive(_activateObjWhenOff); }
                }
            }
        }
        #endregion
    }
}
