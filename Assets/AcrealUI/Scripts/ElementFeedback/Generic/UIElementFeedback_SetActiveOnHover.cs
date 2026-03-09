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
    public class UIElementFeedback_SetActiveOnHover : UIElementFeedback
    {
        #region Variables
        [SerializeField] private string _gameObjName_ObjectToSet = null;

        private GameObject _objectToSet = null;
        #endregion


        #region Initialization
        public override void Initialize(UIElement uiElement)
        {
            Transform objTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_ObjectToSet);
            if (objTform != null)
            {
                _objectToSet = objTform.gameObject;
            }

            base.Initialize(uiElement);
        }
        #endregion


        #region Update/Refresh
        public override void Refresh()
        {
            base.Refresh();

            if(_objectToSet != null && _uiElement != null)
            {
                UIInteractiveElement elem = _uiElement as UIInteractiveElement;
                if (elem != null)
                {
                    if (_objectToSet.activeSelf != (elem.isHighlighted || elem.isPressed))
                    {
                        _objectToSet.SetActive(elem.isHighlighted);
                    }
                }
            }
        }
        #endregion
    }
}
