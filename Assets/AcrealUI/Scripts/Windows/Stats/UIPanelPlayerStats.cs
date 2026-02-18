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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AcrealUI
{
    [ImportedComponent]
    public class UIPanelPlayerStats : UIPanel
    {
        #region Variables
        [Header("Show/Hide Panel")]
        [SerializeField] private string _gameObjName_showPanelButton = null;
        [SerializeField] private string _gameObjName_hidePanelButton = null;

        [Header("Player Info")]
        [SerializeField] private string _gameObjName_playerNameText = null;
        [SerializeField] private string _gameObjName_playerRaceText = null;
        [SerializeField] private string _gameObjName_playerClassText = null;
        [SerializeField] private string _gameObjName_playerLevelText = null;

        [Header("Experience")]
        [SerializeField] private string _gameObjName_experiencePercentSlider = null;

        [Header("Vital Statistics")]
        [SerializeField] private string _gameObjName_healthSlider = null;
        [SerializeField] private string _gameObjName_fatigueSlider = null;
        [SerializeField] private string _gameObjName_magickaSlider = null;

        [Header("Equipment Statistics")]
        [SerializeField] private string _gameObjName_enemyTypeDropdown = null;
        [SerializeField] private string _gameObjName_totalArmorText = null;
        [SerializeField] private string _gameObjName_mainHandDamageText = null;
        [SerializeField] private string _gameObjName_offHandDamageText = null;
        [SerializeField] private string _gameObjName_mainHandHitChanceText = null;
        [SerializeField] private string _gameObjName_offHandHitChanceText = null;

        [Header("Paper Doll")]
        [SerializeField] private string _gameObjName_paperDollRawImage = null;

        [Header("Tweening")]
        [SerializeField] private Vector2 _hiddenPos = new Vector2(275f, 0f);
        [SerializeField] private Vector2 _shownPos = Vector2.zero;

        private TMP_Dropdown _enemyTypeDropdown = null;
        private UISlider _experiencePercentSlider = null;
        private UISlider _healthSlider = null;
        private UISlider _fatigueSlider = null;
        private UISlider _magickaSlider = null;
        private UIButton _showPanelButton = null;
        private UIButton _hidePanelButton = null;
        private TextMeshProUGUI _playerNameText = null;
        private TextMeshProUGUI _playerRaceText = null;
        private TextMeshProUGUI _playerClassText = null;
        private TextMeshProUGUI _playerLevelText = null;
        private TextMeshProUGUI _mainHandDamageText = null;
        private TextMeshProUGUI _offHandDamageText = null;
        private TextMeshProUGUI _mainHandHitChanceText = null;
        private TextMeshProUGUI _offHandHitChanceText = null;
        private TextMeshProUGUI _totalArmorText = null;
        private Dictionary<int, UIPlayerStatEntry> _statEnumAsIntToStatEntryDict = null;
        private Dictionary<int, UIPlayerSkillEntry> _skillEnumAsIntToSkillEntryDict = null;
        private bool _panelIsShown = false;
        private RawImage _paperDollRawImage = null;
        #endregion


        #region Events
        public event System.Action<int> Event_OnEnemyTypeSelected = null;
        #endregion


        #region MonoBehaviour
        private void OnDisable()
        {
            StopAllCoroutines();
        }
        #endregion


        #region Initalization
        public override void Initialize()
        {
            base.Initialize();

            _statEnumAsIntToStatEntryDict = new Dictionary<int, UIPlayerStatEntry>();
            _skillEnumAsIntToSkillEntryDict = new Dictionary<int, UIPlayerSkillEntry>();

            if (!string.IsNullOrWhiteSpace(_gameObjName_enemyTypeDropdown))
            {
                Transform dropdownTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_enemyTypeDropdown);
                _enemyTypeDropdown = dropdownTform != null ? dropdownTform.GetComponent<TMP_Dropdown>() : null;
                if (_enemyTypeDropdown != null)
                {
                    _enemyTypeDropdown.ClearOptions();
                    _enemyTypeDropdown.onValueChanged.AddListener((_) => { Event_OnEnemyTypeSelected?.Invoke(_enemyTypeDropdown.value); });
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_experiencePercentSlider))
            {
                Transform expTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_experiencePercentSlider);
                _experiencePercentSlider = expTform != null ? expTform.GetComponent<UISlider>() : null;
                if (_experiencePercentSlider != null)
                {
                    _experiencePercentSlider.Initialize();
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_healthSlider))
            {
                Transform healthTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_healthSlider);
                _healthSlider = healthTform != null ? healthTform.GetComponent<UISlider>() : null;
                if (_healthSlider != null)
                {
                    _healthSlider.Initialize();
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_fatigueSlider))
            {
                Transform fatigueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_fatigueSlider);
                _fatigueSlider = fatigueTform != null ? fatigueTform.GetComponent<UISlider>() : null;
                if (_fatigueSlider != null)
                {
                    _fatigueSlider.Initialize();
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_magickaSlider))
            {
                Transform magickaTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_magickaSlider);
                _magickaSlider = magickaTform != null ? magickaTform.GetComponent<UISlider>() : null;
                if (_magickaSlider != null)
                {
                    _magickaSlider.Initialize();
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_showPanelButton))
            {
                Transform showTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_showPanelButton);
                _showPanelButton = showTform != null ? showTform.GetComponent<UIButton>() : null;
                if (_showPanelButton != null)
                {
                    _showPanelButton.Initialize();
                    _showPanelButton.Event_OnClicked += (_, _1) =>
                    {
                        Show();
                    };
                }
            }

            if (!string.IsNullOrWhiteSpace(_gameObjName_hidePanelButton))
            {
                Transform hideTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_hidePanelButton);
                _hidePanelButton = hideTform != null ? hideTform.GetComponent<UIButton>() : null;
                if (_hidePanelButton != null)
                {
                    _hidePanelButton.Initialize();
                    _hidePanelButton.Event_OnClicked += (_, _1) =>
                    {
                        Hide();
                    };
                }
            }

            if (!string.IsNullOrEmpty(_gameObjName_playerLevelText))
            {
                Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerNameText);
                if (nameTform != null) { _playerNameText = nameTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_playerLevelText))
            {
                Transform raceTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerRaceText);
                if (raceTform != null) { _playerRaceText = raceTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_playerLevelText))
            {
                Transform classTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerClassText);
                if (classTform != null) { _playerClassText = classTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_playerLevelText))
            {
                Transform levelTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerLevelText);
                if (levelTform != null) { _playerLevelText = levelTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_mainHandDamageText))
            {
                Transform mainTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_mainHandDamageText);
                if (mainTform != null) { _mainHandDamageText = mainTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_offHandDamageText))
            {
                Transform offTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_offHandDamageText);
                if (offTform != null) { _offHandDamageText = offTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_mainHandHitChanceText))
            {
                Transform mainHitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_mainHandHitChanceText);
                if (mainHitTform != null) { _mainHandHitChanceText = mainHitTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_offHandHitChanceText))
            {
                Transform offHitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_offHandHitChanceText);
                if (offHitTform != null) { _offHandHitChanceText = offHitTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_totalArmorText))
            {
                Transform armorTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_totalArmorText);
                if (armorTform != null) { _totalArmorText = armorTform.GetComponent<TextMeshProUGUI>(); }
            }

            if (!string.IsNullOrEmpty(_gameObjName_paperDollRawImage))
            {
                Transform dollTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_paperDollRawImage);
                if(dollTform != null) { _paperDollRawImage = dollTform.GetComponent<RawImage>(); }
            }

            if (layoutElement != null)
            {
                RectTransform rt = layoutElement.transform as RectTransform;
                rt.anchoredPosition = _hiddenPos;
            }
        }
        #endregion


        #region Public API
        public void ShowImmediate()
        {
            if (!_panelIsShown)
            {
                _panelIsShown = true;

                if (_showPanelButton) { _showPanelButton.gameObject.SetActive(false); }
                if (_hidePanelButton) { _hidePanelButton.gameObject.SetActive(true); }

                if (layoutElement != null)
                {
                    RectTransform rt = layoutElement.transform as RectTransform;
                    rt.anchoredPosition = _shownPos;
                }
            }
        }

        public override void Show()
        {
            if (!_panelIsShown)
            {
                _panelIsShown = true;

                if (_showPanelButton) { _showPanelButton.gameObject.SetActive(false); }
                if (_hidePanelButton) { _hidePanelButton.gameObject.SetActive(true); }

                if (layoutElement != null)
                {
                    StopAllCoroutines();

                    RectTransform rt = layoutElement.transform as RectTransform;
                    StartCoroutine(MovePanelRoutine(rt.anchoredPosition, _shownPos, 0.2f));
                }
            }
        }

        public void HideImmediate()
        {
            if (_panelIsShown)
            {
                _panelIsShown = false;

                if (_showPanelButton) { _showPanelButton.gameObject.SetActive(true); }
                if (_hidePanelButton) { _hidePanelButton.gameObject.SetActive(false); }

                if (layoutElement != null)
                {
                    RectTransform rt = layoutElement.transform as RectTransform;
                    rt.anchoredPosition = _hiddenPos;
                }
            }
        }

        public override void Hide()
        {
            if (_panelIsShown)
            {
                _panelIsShown = false;

                if (_showPanelButton) { _showPanelButton.gameObject.SetActive(true); }
                if (_hidePanelButton) { _hidePanelButton.gameObject.SetActive(false); }

                if (layoutElement != null)
                {
                    StopAllCoroutines();

                    RectTransform rt = layoutElement.transform as RectTransform;
                    StartCoroutine(MovePanelRoutine(rt.anchoredPosition, _hiddenPos, 0.2f));
                }
            }
        }

        public void SetPlayerName(string playerName)
        {
            if(_playerNameText != null)
            {
                _playerNameText.text = playerName;
            }
        }

        public void SetPlayerRace(string playerRace)
        {
            if (_playerRaceText != null)
            {
                _playerRaceText.text = playerRace;
            }
        }

        public void SetPlayerClass(string playerClass)
        {
            if (_playerClassText != null)
            {
                _playerClassText.text = playerClass;
            }
        }

        public void SetPlayerLevel(string playerLevel)
        {
            if (_playerLevelText != null)
            {
                _playerLevelText.text = playerLevel;
            }
        }

        public void SetExperiencePercent(float expPct)
        {
            expPct = Mathf.Clamp01(expPct);

            if (_experiencePercentSlider != null)
            {
                _experiencePercentSlider.SetSliderValue(expPct, false);
                _experiencePercentSlider.SetTextValue((expPct * 100f).ToString("N0") + "%");
            }
        }
        
        public void SetHealthSliderPercent(float healthPct)
        {
            if(_healthSlider != null)
            {
                healthPct = Mathf.Clamp01(healthPct);
                _healthSlider.SetSliderValue(healthPct, false);
            }
        }

        public void SetFatigueSliderPercent(float fatiguePct)
        {
            if (_fatigueSlider != null)
            {
                fatiguePct = Mathf.Clamp01(fatiguePct);
                _fatigueSlider.SetSliderValue(fatiguePct, false);
            }
        }

        public void SetMagickaSliderPercent(float magickaPct)
        {
            if (_magickaSlider != null)
            {
                magickaPct = Mathf.Clamp01(magickaPct);
                _magickaSlider.SetSliderValue(magickaPct, false);
            }
        }

        public void SetHealthValue(string healthStr)
        {
            if (_healthSlider != null)
            {
                _healthSlider.SetTextValue(healthStr);
            }
        }

        public void SetFatigueValue(string fatigueStr)
        {
            if (_fatigueSlider != null)
            {
                _fatigueSlider.SetTextValue(fatigueStr);
            }
        }

        public void SetMagickaValue(string magickaStr)
        {
            if (_magickaSlider != null)
            {
                _magickaSlider.SetTextValue(magickaStr);
            }
        }

        public void SetMainHandDamageValue(string damageStr)
        {
            if (_mainHandDamageText != null)
            {
                _mainHandDamageText.text = damageStr;
            }
        }

        public void SetOffHandDamageValue(string damageStr)
        {
            if (_offHandDamageText != null)
            {
                _offHandDamageText.text = damageStr;
            }
        }

        public void SetMainHandHitChanceValue(string hitChanceStr)
        {
            if (_mainHandHitChanceText != null)
            {
                _mainHandHitChanceText.text = hitChanceStr;
            }
        }

        public void SetOffHandHitChanceValue(string hitChanceStr)
        {
            if (_offHandHitChanceText != null)
            {
                _offHandHitChanceText.text = hitChanceStr;
            }
        }

        public void SetTotalArmorValue(string armorStr)
        {
            if(_totalArmorText != null)
            {
                _totalArmorText.text = armorStr;
            }
        }

        public void UpdatePaperDoll()
        {
            if (_paperDollRawImage != null)
            {
                _paperDollRawImage.texture = UIUtilityFunctions.GetPaperDollTexture();

                LayoutElement layoutElem = _paperDollRawImage.GetComponent<LayoutElement>();
                if (layoutElem != null)
                {
                    int paperDollWidth = 110;
                    int paperDollHeight = 184;
                    float aspect = paperDollWidth / (float)paperDollHeight;
                    layoutElem.minHeight = layoutElem.minWidth / aspect;
                }
            }
        }

        public void SetStatValue(int statEnumAsInt, int statValue)
        {
            UIPlayerStatEntry entry;
            _statEnumAsIntToStatEntryDict.TryGetValue(statEnumAsInt, out entry);
            if (entry != null)
            {
                entry.SetDisplayValue(statValue.ToString("N0"));
            }
        }

        public void SetSkillValue(int skillEnumAsInt, short skillValue)
        {
            UIPlayerSkillEntry entry;
            _skillEnumAsIntToSkillEntryDict.TryGetValue(skillEnumAsInt, out entry);
            if (entry != null)
            {
                entry.SetDisplayValue(skillValue.ToString("N0"));
            }
        }

        public void AddStats(string scrollGroupName, List<UIStatData> statDataList)
        {
            UIScrollListGroup scrollGroup = GetOrAddScrollListGroup(scrollGroupName);

            for (int i = 0; i < statDataList.Count; i++)
            {
                AddStat(scrollGroup, statDataList[i]);
            }
        }

        private void AddStat(UIScrollListGroup scrollListGroup, UIStatData statData)
        {
            UIPlayerStatEntry statEntry = Instantiate(UIManager.referenceManager.prefab_playerStatEntry, scrollListGroup.groupParent);
            if (statEntry != null)
            {
                statEntry.Initialize();
                statEntry.SetDisplayName(statData.name);
                statEntry.SetIcon(statData.icon);

                _statEnumAsIntToStatEntryDict[statData.statEnumAsInt] = statEntry;
            }
        }

        public void AddSkills(string scrollGroupName, List<UISkillData> skillDataList)
        {
            UIScrollListGroup scrollGroup = GetOrAddScrollListGroup(scrollGroupName);

            for (int i = 0; i < skillDataList.Count; i++)
            {
                AddSkill(scrollGroup, skillDataList[i]);
            }
        }

        public void AddSkill(string scrollGroupName, UISkillData skillData)
        {
            UIScrollListGroup scrollGroup = GetOrAddScrollListGroup(scrollGroupName);
            AddSkill(scrollGroup, skillData);
        }

        private void AddSkill(UIScrollListGroup scrollGroup, UISkillData skillData)
        {
            if(scrollGroup == null)
            {
                Debug.LogError("[AcrealUI] Unable to add a skill without a valid UIScrollListGroup to attach it to!");
                return;
            }

            UIPlayerSkillEntry skillEntry = Instantiate(UIManager.referenceManager.prefab_playerSkillEntry, scrollGroup.groupParent);
            if (skillEntry != null)
            {
                skillEntry.Initialize();
                skillEntry.SetDisplayName(skillData.name);
                skillEntry.SetIcon(skillData.icon);
                skillEntry.SetSkillRankIcon(skillData.rankIcon);
                _skillEnumAsIntToSkillEntryDict[skillData.skillEnumAsInt] = skillEntry;
            }
        }

        public void SetEnemyTypeOptions(List<string> enemyTypes)
        {
            if(_enemyTypeDropdown != null)
            {
                _enemyTypeDropdown.ClearOptions();
                _enemyTypeDropdown.AddOptions(enemyTypes);
            }
        }
        #endregion


        #region Coroutines
        private IEnumerator<float> MovePanelRoutine(Vector2 from, Vector2 to, float duration)
        {
            RectTransform rt = layoutElement.transform as RectTransform;
            rt.anchoredPosition = from;

            float t = duration;
            while(t > 0f)
            {
                t -= Time.unscaledDeltaTime;
                float lerpT = 1f - Mathf.InverseLerp(0f, duration, t);
                rt.anchoredPosition = Vector2.Lerp(from, to, lerpT);
                yield return 0f;
            }

            rt.anchoredPosition = to;
        }
        #endregion
    }
}
