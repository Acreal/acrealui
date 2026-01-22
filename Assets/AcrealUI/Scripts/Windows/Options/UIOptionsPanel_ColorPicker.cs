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

using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.UI.Extensions.ColorPicker;

namespace AcrealUI
{
    public class UIOptionsPanel_ColorPicker : MonoBehaviour
    {
        [SerializeField] private Transform _panelPivot = null;
        //[SerializeField] private ColorPickerControl colorPickerControl = null;
        [SerializeField] private UIButton confirmButton = null;
        [SerializeField] private UIButton cancelButton = null;
        [SerializeField] private Image image_colorDisplay = null;


        public System.Action<Color> onColorConfirmed = null;
        public System.Action onCancel = null;


        public Transform panelPivot
        {
            get { return _panelPivot; }
        }

        public Color currentColor
        {
            get { return Color.white; }// colorPickerControl != null ? colorPickerControl.CurrentColor : Color.white; }
            set
            {
                //if(colorPickerControl != null)
                //{
                //    colorPickerControl.CurrentColor = value;
                //}
            }
        }


        private void Awake()
        {
            //if(colorPickerControl != null)
            //{
            //    colorPickerControl.onValueChanged.AddListener(OnColorChanged);
            //}

            if (confirmButton != null)
            {
                confirmButton.Event_OnClicked += OnConfirmButtonClicked;
            }

            if (cancelButton != null)
            {
                cancelButton.Event_OnClicked += OnCancelButtonClicked;
            }
        }

        private void OnColorChanged(Color color)
        {
            image_colorDisplay.color = color;
        }

        private void OnConfirmButtonClicked(UIButton button)
        {
            if (onColorConfirmed != null)
            {
                onColorConfirmed(currentColor);
            }
        }

        private void OnCancelButtonClicked(UIButton button)
        {
            if (onCancel != null)
            {
                onCancel();
            }
        }
    }
}