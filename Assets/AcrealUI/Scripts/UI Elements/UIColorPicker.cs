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

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    public class UIColorPicker : MonoBehaviour
    {
        [SerializeField] private UIButton button_pickColor = null;
        [SerializeField] private Image image_colorDisplay = null;

        private static UIOptionsPanel_ColorPicker colorPickerPrefabRef = null;
        private static UIOptionsPanel_ColorPicker colorPickerPanel = null;
        private static UIColorPicker selectedColorPicker = null;

        //public event System.Action onButtonClicked = null;
        public event System.Action<Color> onColorChanged = null;


        public Color currentColor
        {
            get { return image_colorDisplay.color; }
            set { image_colorDisplay.color = value; }
        }

        public void Initialize()
        {
            if (colorPickerPanel == null)
            {
                if (colorPickerPrefabRef == null)
                {
                    //colorPickerPrefabRef = UIManager.referenceManager.prefab_colorPicker;
                }
            }

            button_pickColor.Event_OnClicked += OnPickColorButtonClicked;
        }

        public void Show()
        {
            if (colorPickerPanel == null)
            {
                colorPickerPanel = Object.Instantiate(colorPickerPrefabRef);
            }

            if (colorPickerPanel != null)
            {
                colorPickerPanel.panelPivot.position = button_pickColor.transform.position;
                colorPickerPanel.panelPivot.localScale = new Vector3(1f, 0f, 1f);
                colorPickerPanel.gameObject.SetActive(true);

                colorPickerPanel.onColorConfirmed = OnColorConfirmed;
                colorPickerPanel.onCancel = OnCancel;

                selectedColorPicker = this;
                colorPickerPanel.currentColor = image_colorDisplay.color;

                StopAllCoroutines();
                StartCoroutine(ShowPanelRoutine());
            }
        }

        public void Hide()
        {
            if (colorPickerPanel != null && selectedColorPicker == this)
            {
                colorPickerPanel.onColorConfirmed = null;
                colorPickerPanel.onCancel = null;
                selectedColorPicker = null;
            }

            StopAllCoroutines();
            StartCoroutine(HidePanelRoutine());
        }

        private void OnPickColorButtonClicked(UIButton button)
        {
            Show();
        }

        private void OnColorConfirmed(Color color)
        {
            image_colorDisplay.color = color;

            if (onColorChanged != null)
            {
                onColorChanged(color);
            }

            Hide();
        }

        private void OnCancel()
        {
            Hide();
        }

        private IEnumerator ShowPanelRoutine()
        {
            CanvasGroup canvasGroup = colorPickerPanel.GetComponent<CanvasGroup>();
            float t = 0.1f;
            while (t > 0f)
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
                float inverseT = 1 - Mathf.InverseLerp(0f, 0.1f, t);
                colorPickerPanel.panelPivot.localScale = Vector3.Lerp(new Vector3(1f, 0.07f, 1f), Vector3.one, inverseT);
                if(canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(0f, 1f, inverseT);
                }
            }
            colorPickerPanel.panelPivot.localScale = Vector3.one;
        }

        private IEnumerator HidePanelRoutine()
        {
            CanvasGroup canvasGroup = colorPickerPanel.GetComponent<CanvasGroup>();
            float t = 0.1f;
            while (t > 0f)
            {
                yield return null;
                t -= Time.unscaledDeltaTime;
                float inverseT = 1 - Mathf.InverseLerp(0f, 0.1f, t);
                colorPickerPanel.panelPivot.localScale = Vector3.Lerp(Vector3.one, new Vector3(1f, 0.07f, 1f), inverseT);

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.Lerp(1f, 0f, inverseT);
                }
            }
            colorPickerPanel.gameObject.SetActive(false);
        }
    }
}