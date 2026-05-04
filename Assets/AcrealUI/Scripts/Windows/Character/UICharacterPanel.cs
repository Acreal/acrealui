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
    public class UICharacterPanel : UIPanel
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
        private RawImage _paperDollRawImage = null;
        private bool _panelIsShown = false;
        #endregion


        #region Events
        public event System.Action<int> Event_OnEnemyTypeSelected = null;
        #endregion


        #region MonoBehaviour
        private void OnDisable()
        {
            UIManager.Instance.StopCoroutine(gameObject.GetInstanceID(), 0);
        }
        #endregion


        #region Initalization/Cleanup
        public override void Initialize()
        {
            base.Initialize();

            _statEnumAsIntToStatEntryDict = new Dictionary<int, UIPlayerStatEntry>();
            _skillEnumAsIntToSkillEntryDict = new Dictionary<int, UIPlayerSkillEntry>();

            Transform dropdownTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_enemyTypeDropdown);
            if (dropdownTform != null)
            {
                _enemyTypeDropdown = dropdownTform.GetComponent<TMP_Dropdown>();
                if (_enemyTypeDropdown != null)
                {
                    _enemyTypeDropdown.ClearOptions();
                    _enemyTypeDropdown.onValueChanged.AddListener((_) => { Event_OnEnemyTypeSelected?.Invoke(_enemyTypeDropdown.value); });
                }
            }

            Transform expTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_experiencePercentSlider);
            if (expTform != null)
            {
                _experiencePercentSlider = expTform.GetComponent<UISlider>();
                if (_experiencePercentSlider != null)
                {
                    _experiencePercentSlider.Initialize();
                }
            }

            Transform healthTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_healthSlider);
            if (healthTform != null)
            {
                _healthSlider = healthTform.GetComponent<UISlider>();
                if (_healthSlider != null)
                {
                    _healthSlider.Initialize();
                }
            }

            Transform fatigueTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_fatigueSlider);
            if (fatigueTform != null)
            {
                _fatigueSlider = fatigueTform.GetComponent<UISlider>();
                if (_fatigueSlider != null)
                {
                    _fatigueSlider.Initialize();
                }
            }

            Transform magickaTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_magickaSlider);
            if (magickaTform != null)
            {
                _magickaSlider = magickaTform.GetComponent<UISlider>();
                if (_magickaSlider != null)
                {
                    _magickaSlider.Initialize();
                }
            }

            Transform showTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_showPanelButton);
            if (showTform != null)
            {
                _showPanelButton = showTform.GetComponent<UIButton>();
                if (_showPanelButton != null)
                {
                    _showPanelButton.Initialize();
                    _showPanelButton.Event_OnAnyClick += (_, _1) =>
                    {
                        Show();
                    };
                }
            }

            Transform hideTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_hidePanelButton);
            if (hideTform != null)
            {
                _hidePanelButton = hideTform.GetComponent<UIButton>();
                if (_hidePanelButton != null)
                {
                    _hidePanelButton.Initialize();
                    _hidePanelButton.Event_OnAnyClick += (_, _1) =>
                    {
                        Hide();
                    };
                }
            }

            Transform nameTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerNameText);
            if (nameTform != null) { _playerNameText = nameTform.GetComponent<TextMeshProUGUI>(); }

            Transform raceTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerRaceText);
            if (raceTform != null) { _playerRaceText = raceTform.GetComponent<TextMeshProUGUI>(); }

            Transform classTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerClassText);
            if (classTform != null) { _playerClassText = classTform.GetComponent<TextMeshProUGUI>(); }

            Transform levelTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_playerLevelText);
            if (levelTform != null) { _playerLevelText = levelTform.GetComponent<TextMeshProUGUI>(); }

            Transform mainTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_mainHandDamageText);
            if (mainTform != null) { _mainHandDamageText = mainTform.GetComponent<TextMeshProUGUI>(); }

            Transform offTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_offHandDamageText);
            if (offTform != null) { _offHandDamageText = offTform.GetComponent<TextMeshProUGUI>(); }

            Transform mainHitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_mainHandHitChanceText);
            if (mainHitTform != null) { _mainHandHitChanceText = mainHitTform.GetComponent<TextMeshProUGUI>(); }

            Transform offHitTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_offHandHitChanceText);
            if (offHitTform != null) { _offHandHitChanceText = offHitTform.GetComponent<TextMeshProUGUI>(); }

            Transform armorTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_totalArmorText);
            if (armorTform != null) { _totalArmorText = armorTform.GetComponent<TextMeshProUGUI>(); }

            Transform dollTform = UIUtilityFunctions.FindDeepChild(transform, _gameObjName_paperDollRawImage);
            if(dollTform != null) { _paperDollRawImage = dollTform.GetComponent<RawImage>(); }

            if (layoutElement != null)
            {
                RectTransform rt = layoutElement.transform as RectTransform;
                rt.anchoredPosition = _hiddenPos;
            }
        }

        public override void Cleanup()
        {
            Event_OnEnemyTypeSelected = null;

            _enemyTypeDropdown = null;

            _experiencePercentSlider?.Cleanup();
            _experiencePercentSlider = null;

            _healthSlider?.Cleanup();
            _healthSlider = null;

            _fatigueSlider?.Cleanup();
            _fatigueSlider = null;

            _magickaSlider?.Cleanup();
            _magickaSlider = null;

            _showPanelButton?.Cleanup();
            _showPanelButton = null;

            _hidePanelButton?.Cleanup();
            _hidePanelButton = null;

            _playerNameText = null;
            _playerRaceText = null;
            _playerClassText = null;
            _playerLevelText = null;

            _mainHandDamageText = null;
            _mainHandHitChanceText = null;
            _offHandDamageText = null;
            _offHandHitChanceText = null;
            _totalArmorText = null;

            List<GameObject> objsToDelete = new List<GameObject>();
            if (_statEnumAsIntToStatEntryDict != null)
            {
                foreach (UIPlayerStatEntry statEntry in _statEnumAsIntToStatEntryDict.Values)
                {
                    if (statEntry != null)
                    {
                        statEntry.Cleanup();
                        objsToDelete.Add(statEntry.gameObject);
                    }
                }
            }

            if (_skillEnumAsIntToSkillEntryDict != null)
            {
                foreach (UIPlayerSkillEntry skillEntry in _skillEnumAsIntToSkillEntryDict.Values)
                {
                    if (skillEntry != null)
                    {
                        skillEntry.Cleanup();
                        objsToDelete.Add(skillEntry.gameObject);
                    }
                }
            }

            for (int i = 0; i < objsToDelete.Count; i++)
            {
                Destroy(objsToDelete[i]);
            }

            base.Cleanup();
        }

        public override void ResetPanel()
        {
            SetPlayerName(null);
            SetPlayerClass(null);
            SetPlayerLevel(null);
            SetPlayerRace(null);

            SetHealthValue(null);
            SetFatigueValue(null);
            SetMagickaValue(null);

            SetEnemyTypeOptions(null);
            SetMainHandDamageValue(null);
            SetOffHandDamageValue(null);
            SetMainHandHitChanceValue(null);
            SetOffHandHitChanceValue(null);
            SetTotalArmorValue(null);

            SetExperiencePercent(0f);
            SetHealthSliderPercent(0f);
            SetFatigueSliderPercent(0f);
            SetMagickaSliderPercent(0f);

            foreach (UIPlayerStatEntry statEntry in _statEnumAsIntToStatEntryDict.Values)
            {
                statEntry.SetDisplayValue(null);
            }

            foreach (UIPlayerSkillEntry skillEntry in _skillEnumAsIntToSkillEntryDict.Values)
            {
                skillEntry.SetDisplayValue(null);
            }

            base.ResetPanel();
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
                    UIManager.Instance.StopCoroutine(gameObject.GetInstanceID(), 0);

                    RectTransform rt = layoutElement.transform as RectTransform;
                    UIManager.Instance.RunCoroutine(gameObject.GetInstanceID(), 0, MovePanelRoutine(rt.anchoredPosition, _shownPos, 0.2f));
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
                    UIManager.Instance.StopCoroutine(gameObject.GetInstanceID(), 0);

                    RectTransform rt = layoutElement.transform as RectTransform;
                    UIManager.Instance.RunCoroutine(gameObject.GetInstanceID(), 0, MovePanelRoutine(rt.anchoredPosition, _hiddenPos, 0.2f));
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
                _experiencePercentSlider.SetDisplayValue((expPct * 100f).ToString("N0") + "%");
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
                _healthSlider.SetDisplayValue(healthStr);
            }
        }

        public void SetFatigueValue(string fatigueStr)
        {
            if (_fatigueSlider != null)
            {
                _fatigueSlider.SetDisplayValue(fatigueStr);
            }
        }

        public void SetMagickaValue(string magickaStr)
        {
            if (_magickaSlider != null)
            {
                _magickaSlider.SetDisplayValue(magickaStr);
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

                if (enemyTypes != null)
                {
                    _enemyTypeDropdown.AddOptions(enemyTypes);
                }
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
