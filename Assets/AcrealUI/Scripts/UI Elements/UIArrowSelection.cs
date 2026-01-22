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

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIArrowSelection : MonoBehaviour
{
    #region Editor Variables
    [SerializeField] private bool circularSelection = true;
    [SerializeField] private Button leftButton = null;
    [SerializeField] private Button rightButton = null;
    [SerializeField] private TextMeshProUGUI selectionText = null;
    [SerializeField] private List<string> textOptions = null;
    #endregion


    #region Public Variables
    public event System.Action<UIArrowSelection> onSelectionChanged = null;
    public int index { get { return selectedIdx; } }
    #endregion


    #region Private Variables
    private int selectedIdx = -1;
    private int maxCount { get { return textOptions != null ? textOptions.Count : 0; } }
    #endregion


    #region MonoBehavior
    private void Awake()
    {
        leftButton.onClick.AddListener(OnLeftButtonClicked);
        rightButton.onClick.AddListener(OnRightButtonClicked);
    }

    private void Start()
    {
        if (selectedIdx < 0)
        {
            OnRightButtonClicked();
        }
    }
    #endregion


    #region Input Callbacks
    protected virtual void OnLeftButtonClicked()
    {
        selectedIdx--;
        if (selectedIdx < 0)
        {
            if (circularSelection)
            {
                selectedIdx = maxCount-1;

                UpdateSelectedText(selectedIdx);

                if (onSelectionChanged != null)
                {
                    onSelectionChanged(this);
                }
            }
            else
            {
                selectedIdx = 0;
            }
        }
        else
        {
            UpdateSelectedText(selectedIdx);

            if (onSelectionChanged != null)
            {
                onSelectionChanged(this);
            }
        }

        Debug.Log(gameObject.name + ".SelectedIndex = " + selectedIdx);
    }

    protected virtual void OnRightButtonClicked()
    {
        selectedIdx++;
        if (selectedIdx > maxCount - 1)
        {
            if (circularSelection)
            {
                selectedIdx = 0;

                UpdateSelectedText(selectedIdx);

                if (onSelectionChanged != null)
                {
                    onSelectionChanged(this);
                }
            }
            else
            {
                selectedIdx = maxCount - 1;
            }
        }
        else
        {
            UpdateSelectedText(selectedIdx);

            if (onSelectionChanged != null)
            {
                onSelectionChanged(this);
            }
        }

        Debug.Log(gameObject.name + ".SelectedIndex = " + selectedIdx);
    }
    #endregion


    #region Text Options
    public void SetIndexNoCallback(int _index)
    {
        if (_index >= 0 && _index < textOptions.Count)
        {
            selectedIdx = _index;
            UpdateSelectedText(selectedIdx);
        }
    }

    public void PushTextOption(string text)
    {
        if (!string.IsNullOrEmpty(text))
        {
            if (textOptions == null)
            {
                textOptions = new List<string>();
            }
            textOptions.Add(text);
        }
    }

    public void PopTextOption(int index)
    {
        if(textOptions != null && index >= 0 && index < textOptions.Count)
        {
            textOptions.RemoveAt(index);
        }
    }

    private void UpdateSelectedText(int index)
    {
        if(textOptions == null) { return; }

        if(index >= 0 && index < textOptions.Count)
        {
            selectionText.text = textOptions[index];
        }
    }
    #endregion
}