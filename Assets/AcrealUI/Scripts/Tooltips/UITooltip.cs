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

using TMPro;
using UnityEngine;

namespace AcrealUI
{
    public abstract class UITooltip : MonoBehaviour
    {
        [SerializeField] private string _gameObjName_text_title = null;

        public bool anchorToMouse = false;

        private TextMeshProUGUI _text_title = null;


        public virtual void Initalize()
        {
            if(_text_title == null && !string.IsNullOrEmpty(_gameObjName_text_title))
            {
                Transform titleTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_title);
                if (titleTform != null) { _text_title = titleTform.GetComponent<TextMeshProUGUI>(); }
            }
        }

        public void SetTitle(string title)
        {
            if(_text_title != null)
            {
                _text_title.text = title;
                _text_title.gameObject.SetActive(!string.IsNullOrEmpty(title));
            }
        }
    }
}
