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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIPlayerSkillEntry : MonoBehaviour
    {
        #region Variables
        [SerializeField] private string _gameObjName_text_skillName = null;
        [SerializeField] private string _gameObjName_text_skillValue = null;
        [SerializeField] private string _gameObjName_image_skillIcon = null;
        [SerializeField] private string _gameObjName_image_skillRankIcon = null;
        [SerializeField] private string _gameObjName_skillRankParent = null;

        private TextMeshProUGUI _text_skillName = null;
        private TextMeshProUGUI _text_skillValue = null;
        private Image _image_skillIcon = null;
        private Image _image_skillRankIcon = null;
        private GameObject _skillRankParent = null;
        #endregion


        #region Initialization
        public void Initialize()
        {
            Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_skillName);
            _text_skillName = nameTform != null ? nameTform.GetComponent<TextMeshProUGUI>() : null;

            Transform valTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_text_skillValue);
            _text_skillValue = valTform != null ? valTform.GetComponent<TextMeshProUGUI>() : null;

            Transform iconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_image_skillIcon);
            _image_skillIcon = iconTform != null ? iconTform.GetComponent<Image>() : null;

            Transform primaryTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_image_skillRankIcon);
            _image_skillRankIcon = primaryTform != null ? primaryTform.GetComponent<Image>() : null;

            Transform parentTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_skillRankParent);
            _skillRankParent = parentTform != null ? parentTform.gameObject : null;
        }

        public void Cleanup()
        {
            _skillRankParent = null;
            _text_skillName = null;
            _text_skillValue = null;
            _image_skillIcon = null;
            _image_skillRankIcon = null;
        }
        #endregion


        #region Public API
        public void SetDisplayName(string displayName)
        {
            if (_text_skillName != null)
            {
                _text_skillName.text = displayName;
            }
        }

        public void SetDisplayValue(string displayValue)
        {
            if(_text_skillValue != null)
            {
                _text_skillValue.text = displayValue;
            }
        }

        public void SetIcon(Sprite icon)
        {
            if(_image_skillIcon != null)
            {
                _image_skillIcon.sprite = icon;
            }
        }

        public void SetSkillRankIcon(Sprite icon)
        {
            if(_image_skillRankIcon != null)
            {
                _image_skillRankIcon.sprite = icon;
            }

            if(_skillRankParent != null)
            {
                _skillRankParent.SetActive(_image_skillRankIcon != null && _image_skillRankIcon.sprite != null);
            }
        }
        #endregion
    }
}
