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


        #region MonoBehaviour
        protected override void OnDisable()
        {
            base.OnDisable();

            if(_objectToToggle != null)
            {
                _objectToToggle.SetActive(_activateObjOnDisable);
            }
        }
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            base.Initialize(uiElement);

            if (_objectToToggle == null)
            {
                Transform tform = string.IsNullOrEmpty(_gameObjName_objectToToggle) ? transform : UIUtilityFunctions.FindDeepChild(transform, _gameObjName_objectToToggle);
                _objectToToggle = tform != null ? tform.gameObject : null;
            }

            UpdateDisplay();
        }
        #endregion


        #region Update
        protected override void UpdateDisplay()
        {
            if (_objectToToggle == null) { return; }

            if (_isDisabled)
            {
                if (_objectToToggle.activeSelf != _activateObjOnDisable)
                {
                    _objectToToggle.SetActive(_activateObjOnDisable);
                }
            }
            else if (_isPressed)
            {
                if (_objectToToggle.activeSelf != _activateObjOnPress)
                {
                    _objectToToggle.SetActive(_activateObjOnPress);
                }
            }
            else if (_isHighlighted)
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
        #endregion
    }
}
