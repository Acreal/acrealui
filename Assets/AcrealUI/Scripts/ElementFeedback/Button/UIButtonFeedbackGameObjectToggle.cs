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
    public class UIButtonFeedbackGameObjectToggle : UIElementFeedback
    {
        #region Variables
        [SerializeField] private string _gameObjName_objectToToggle = null;
        [SerializeField] private bool _activateObjByDefault = false;
        [SerializeField] private bool _activateObjOnHighlight = false;
        [SerializeField] private bool _activateObjOnPress = false;
        [SerializeField] private bool _activateObjOnDisable = false;

        private GameObject _objectToToggle = null;
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            base.Initialize(uiElement);

            Transform tform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_objectToToggle);
            if (tform != null)
            {
                _objectToToggle = tform.gameObject;
            }

            Refresh();
        }
        #endregion


        #region Update
        public override void Refresh()
        {
            if (_objectToToggle == null) { return; }

            UIInteractiveElement elem = _uiElement as UIInteractiveElement;
            if (elem != null)
            {
                if (elem.isDisabled)
                {
                    if (_objectToToggle.activeSelf != _activateObjOnDisable)
                    {
                        _objectToToggle.SetActive(_activateObjOnDisable);
                    }
                }
                else if (elem.isPressed)
                {
                    if (_objectToToggle.activeSelf != _activateObjOnPress)
                    {
                        _objectToToggle.SetActive(_activateObjOnPress);
                    }
                }
                else if (elem.isHighlighted)
                {
                    if (_objectToToggle.activeSelf != _activateObjOnHighlight)
                    {
                        _objectToToggle.SetActive(_activateObjOnHighlight);
                    }
                }
                else
                {
                    if (_objectToToggle.activeSelf != _activateObjByDefault)
                    {
                        _objectToToggle.SetActive(_activateObjByDefault);
                    }
                }
            }
        }
        #endregion
    }
}
