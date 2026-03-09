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
    public class UIToggleFeedbackGameObjectToggle : UIToggleFeedback
    {
        #region Variables
        [SerializeField] private string _gameObjName_objectToToggle = null;
        [SerializeField] protected bool _activateObjWhenOn = false;
        [SerializeField] protected bool _activateObjWhenOff = false;
        [SerializeField] protected bool _activateObjWhenOnAndPressed = false;
        [SerializeField] protected bool _activateObjWhenOffAndPressed = false;
        [SerializeField] protected bool _activateObjWhenOnAndHighlighted = false;
        [SerializeField] protected bool _activateObjWhenOffAndHighlighted = false;
        [SerializeField] protected bool _activateObjOnDisable = false;

        protected GameObject _gameObjToToggle = null;
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            base.Initialize(uiElement);

            if(_gameObjToToggle == null)
            {
                if(!string.IsNullOrEmpty(_gameObjName_objectToToggle))
                {
                    Transform objTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_objectToToggle);
                    if (objTform != null)
                    {
                        _gameObjToToggle = objTform != null ? objTform.gameObject : null;
                    }
                }
                else
                {
                    _gameObjToToggle = gameObject;
                }
            }
        }
        #endregion


        #region Update
        public override void Refresh()
        {
            if (_gameObjToToggle != null)
            {
                UIInteractiveElement elem = _uiElement as UIInteractiveElement;
                if (elem != null)
                {
                    if (elem.isDisabled)
                    {
                        _gameObjToToggle.SetActive(_activateObjOnDisable);
                    }
                    else if (toggle != null)
                    {
                        if (toggle.isToggledOn)
                        {
                            if (elem.isPressed) { _gameObjToToggle.SetActive(_activateObjWhenOnAndPressed); }
                            else if (elem.isHighlighted) { _gameObjToToggle.SetActive(_activateObjWhenOnAndHighlighted); }
                            else { _gameObjToToggle.SetActive(_activateObjWhenOn); }
                        }
                        else
                        {
                            if (elem.isPressed) { _gameObjToToggle.SetActive(_activateObjWhenOffAndPressed); }
                            else if (elem.isHighlighted) { _gameObjToToggle.SetActive(_activateObjWhenOffAndHighlighted); }
                            else { _gameObjToToggle.SetActive(_activateObjWhenOff); }
                        }
                    }
                    else
                    {
                        _gameObjToToggle.SetActive(false);
                    }
                }
            }
        }
        #endregion
    }
}
