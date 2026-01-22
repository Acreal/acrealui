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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIInventoryWindow_ItemEntry : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region Variables
        [SerializeField] private string _gameObjName_itemTypeIcon = null;
        [SerializeField] private string _gameObjName_nameText = null;
        [SerializeField] private string _gameObjName_damageText = null;
        [SerializeField] private string _gameObjName_armorText = null;
        [SerializeField] private string _gameObjName_conditionSlider = null;
        [SerializeField] private string _gameObjName_noConditionText = null;
        [SerializeField] private string _gameObjName_weightText = null;
        [SerializeField] private string _gameObjName_valueText = null;
        [SerializeField] private string _gameObjName_statusParent_equipped = null;
        [SerializeField] private string _gameObjName_statusParent_broken = null;
        [SerializeField] private string _gameObjName_statusParent_prohibited = null;
        [SerializeField] private string _gameObjName_statusParent_magic = null;
        [SerializeField] private string _gameObjName_statusParent_poisoned = null;
        [SerializeField] private string _gameObjName_highlight = null;
        [SerializeField] private Color _prohibitedItemColor = Color.red;

        private UISlider _slider_condition = null;
        private Image _image_itemTypeIcon = null;
        private TextMeshProUGUI _text_name = null;
        private TextMeshProUGUI _text_damage = null;
        private TextMeshProUGUI _text_armor = null;
        private TextMeshProUGUI _text_noCondition = null;
        private TextMeshProUGUI _text_weight = null;
        private TextMeshProUGUI _text_value = null;
        private GameObject _statusParent_equipped = null;
        private GameObject _statusParent_broken = null;
        private GameObject _statusParent_prohibited = null;
        private GameObject _statusParent_magic = null;
        private GameObject _statusParent_poisoned = null;
        private Image _image_highlight = null;
        private Color _defaultHighlightColor = Color.white;
        #endregion


        #region Delegates
        public System.Action<UIInventoryWindow_ItemEntry> Delegate_OnPointerEnter = null;
        public System.Action<UIInventoryWindow_ItemEntry> Delegate_OnPointerExit = null;
        public System.Action<UIInventoryWindow_ItemEntry> Delegate_OnLeftClicked = null;
        public System.Action<UIInventoryWindow_ItemEntry> Delegate_OnRightClicked = null;
        #endregion


        #region Properties
        public ulong itemUID { get; private set; }
        #endregion


        #region Initialization
        public void Initalize()
        {
            if (_image_itemTypeIcon == null)
            {
                Transform typeIconTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_itemTypeIcon);
                _image_itemTypeIcon = typeIconTform != null ? typeIconTform.GetComponent<Image>() : null;
            }

            if (_text_name == null)
            {
                Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_nameText);
                _text_name = nameTform != null ? nameTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (_text_damage == null)
            {
                Transform dmgTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_damageText);
                _text_damage = dmgTform != null ? dmgTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (_text_armor == null)
            {
                Transform armorTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_armorText);
                _text_armor = armorTform != null ? armorTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (_slider_condition == null && !string.IsNullOrEmpty(_gameObjName_conditionSlider))
            {
                Transform conTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_conditionSlider);
                _slider_condition = conTform != null ? conTform.GetComponent<UISlider>() : null;
                if(_slider_condition != null)
                {
                    _slider_condition.Initialize();
                    _slider_condition.SetSliderMinMax(0f, 1f, false);
                }
            }

            if (_text_noCondition == null)
            {
                Transform noConTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_noConditionText);
                _text_noCondition = noConTform != null ? noConTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (_text_weight == null)
            {
                Transform weightTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_weightText);
                _text_weight = weightTform != null ? weightTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (_text_value == null)
            {
                Transform valTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_valueText);
                _text_value = valTform != null ? valTform.GetComponent<TextMeshProUGUI>() : null;
            }

            if (_statusParent_equipped == null)
            {
                Transform equippedTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_statusParent_equipped);
                _statusParent_equipped = equippedTform != null ? equippedTform.gameObject : null;
            }

            if (_statusParent_broken == null)
            {
                Transform brokenTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_statusParent_broken);
                _statusParent_broken = brokenTform != null ? brokenTform.gameObject : null;
            }

            if (_statusParent_prohibited == null)
            {
                Transform prohibTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_statusParent_prohibited);
                _statusParent_prohibited = prohibTform != null ? prohibTform.gameObject : null;
            }

            if (_statusParent_magic == null)
            {
                Transform magicTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_statusParent_magic);
                _statusParent_magic = magicTform != null ? magicTform.gameObject : null;
            }

            if (_statusParent_poisoned == null)
            {
                Transform poisonedTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_statusParent_poisoned);
                _statusParent_poisoned = poisonedTform != null ? poisonedTform.gameObject : null;
            }

            if (_image_highlight == null)
            {
                Transform highlightTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_highlight);
                _image_highlight = highlightTform != null ? highlightTform.GetComponent<Image>() : null;
                if(_image_highlight != null)
                {
                    _defaultHighlightColor = _image_highlight.color;
                    _image_highlight.gameObject.SetActive(false);
                }
            }
        }
        #endregion


        #region MonoBehaviour
        private void OnDisable()
        {
            Delegate_OnPointerEnter = null;
            Delegate_OnPointerExit = null;
            Delegate_OnLeftClicked = null;
            Delegate_OnRightClicked = null;

            if (_image_highlight != null)
            {
                _image_highlight.gameObject.SetActive(false);
            }
        }
        #endregion


        #region Public API
        public void SetItemUID(ulong uid)
        {
            itemUID = uid;
        }

        public void SetColumnValue_ItemArchetypeIcon(Sprite itemTypeIcon)
        {
            if(_image_itemTypeIcon != null)
            {
                _image_itemTypeIcon.sprite = itemTypeIcon;
                _image_itemTypeIcon.gameObject.SetActive(itemTypeIcon != null);
            }
        }

        public void SetColumnValue_Name(string itemName)
        {
            if (_text_name != null)
            {
                _text_name.text = itemName;
            }
        }

        public void SetColumnValue_Damage(string damageValue)
        {
            if (_text_damage != null)
            {
                _text_damage.text = damageValue;
                _text_damage.gameObject.SetActive(damageValue != null);
            }
        }

        public void SetColumnValue_Armor(string armorValue)
        {
            if (_text_armor != null)
            {
                _text_armor.text = armorValue;
                _text_armor.gameObject.SetActive(armorValue != null);
            }
        }

        /// <summary>
        /// Sets the value of the condition slider (NOTE: a value of < 0 will disable the slider)
        /// </summary>
        public void SetColumnValue_Condition(float conditionPercent)
        {
            bool showCondition = conditionPercent >= 0f;

            if (_slider_condition != null)
            {
                if (showCondition)
                {
                    conditionPercent = Mathf.Clamp01(conditionPercent);
                    _slider_condition.SetSliderValue(conditionPercent, true);
                }
                _slider_condition.gameObject.SetActive(showCondition);
            }
        }

        public void SetNoConditionText(string noConditionStr)
        {
            if(_text_noCondition != null)
            {
                _text_noCondition.text = noConditionStr;
                _text_noCondition.gameObject.SetActive(noConditionStr != null);
            }
        }

        public void SetColumnValue_Weight(string weightValue)
        {
            if (_text_weight != null)
            {
                _text_weight.text = weightValue;
                _text_weight.gameObject.SetActive(weightValue != null);
            }
        }

        public void SetColumnValue_GoldValue(string goldValue)
        {
            if (_text_value != null)
            {
                _text_value.text = goldValue;
                _text_value.gameObject.SetActive(goldValue != null);
            }
        }

        public void SetStatusIcons(bool prohibited, bool equipped, bool broken, bool enchanted, bool poisoned)
        {
            if(_image_highlight != null)
            {
                _image_highlight.color = prohibited ? _prohibitedItemColor : _defaultHighlightColor;
            }

            if (_statusParent_prohibited != null)
            {
                _statusParent_prohibited.SetActive(prohibited);
            }

            if (_statusParent_equipped != null)
            { 
                _statusParent_equipped.SetActive(equipped); 
            }

            if (_statusParent_broken != null)
            { 
                _statusParent_broken.SetActive(broken); 
            }

            if (_statusParent_magic != null) 
            {
                _statusParent_magic.SetActive(enchanted); 
            }

            if (_statusParent_poisoned != null) 
            { 
                _statusParent_poisoned.SetActive(poisoned);
            }
        }

        public void ClearEvents()
        {
            Delegate_OnPointerEnter = null;
            Delegate_OnPointerExit = null;
            Delegate_OnLeftClicked = null;
            Delegate_OnRightClicked = null;
        }
        #endregion


        #region Input Handling
        public void OnPointerEnter(PointerEventData pointerData)
        {
            if (_image_highlight != null)
            {
                _image_highlight.gameObject.SetActive(true);
            }

            Delegate_OnPointerEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData pointerData)
        {
            if (_image_highlight != null)
            {
                _image_highlight.gameObject.SetActive(false);
            }

            Delegate_OnPointerExit?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData pointerData)
        {
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                Delegate_OnLeftClicked?.Invoke(this);
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                Delegate_OnRightClicked?.Invoke(this);
            }
        }
        #endregion
    }
}