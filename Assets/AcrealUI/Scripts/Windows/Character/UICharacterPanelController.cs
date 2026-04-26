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

using DaggerfallWorkshop;
using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using System.Collections.Generic;

namespace AcrealUI
{
    public class UICharacterPanelController
    {
        #region Variables
        private UICharacterPanel _characterPanel = null;
        private List<MobileTypes> _enemyTypesForStatCalc = null;
        private MobileTypes _currentEnemyTypeForStatCalc = MobileTypes.None;
        #endregion


        #region Initialization/Cleanup
        public UICharacterPanelController(UICharacterPanel charPanel)
        {
            _characterPanel = charPanel;
        }

        public void Initialize()
        {
            if (_characterPanel != null)
            {
                #region Stats
                AddStatsToCharacterPanel(_characterPanel, UITextStrings.InventoryWindow_Label_PrimaryStats.GetText(),
                                    DFCareer.Stats.Agility,
                                    DFCareer.Stats.Endurance,
                                    DFCareer.Stats.Intelligence,
                                    DFCareer.Stats.Luck,
                                    DFCareer.Stats.Personality,
                                    DFCareer.Stats.Speed,
                                    DFCareer.Stats.Strength,
                                    DFCareer.Stats.Willpower);
                #endregion

                #region Skills
                GameManager gm = GameManager.Instance;
                PlayerEntity playerEntity = gm != null ? gm.PlayerEntity : null;
                List<DFCareer.Skills> primarySkills = playerEntity != null ? playerEntity.GetPrimarySkills() : null;
                if (primarySkills == null) { return; }

                AddSkillsToCharacterWindow(_characterPanel, UITextStrings.InventoryWindow_Label_CombatSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Archery,
                                        DFCareer.Skills.Axe,
                                        DFCareer.Skills.Backstabbing,
                                        DFCareer.Skills.BluntWeapon,
                                        DFCareer.Skills.CriticalStrike,
                                        DFCareer.Skills.Dodging,
                                        DFCareer.Skills.HandToHand,
                                        DFCareer.Skills.LongBlade,
                                        DFCareer.Skills.Medical,
                                        DFCareer.Skills.ShortBlade);

                AddSkillsToCharacterWindow(_characterPanel, UITextStrings.InventoryWindow_Label_MagicSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Alteration,
                                        DFCareer.Skills.Destruction,
                                        DFCareer.Skills.Illusion,
                                        DFCareer.Skills.Mysticism,
                                        DFCareer.Skills.Restoration,
                                        DFCareer.Skills.Thaumaturgy);

                AddSkillsToCharacterWindow(_characterPanel, UITextStrings.InventoryWindow_Label_StealthSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Lockpicking,
                                        DFCareer.Skills.Pickpocket,
                                        DFCareer.Skills.Stealth);

                AddSkillsToCharacterWindow(_characterPanel, UITextStrings.InventoryWindow_Label_SocialSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Etiquette,
                                        DFCareer.Skills.Mercantile,
                                        DFCareer.Skills.Streetwise);

                AddSkillsToCharacterWindow(_characterPanel, UITextStrings.InventoryWindow_Label_MovementSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Climbing,
                                        DFCareer.Skills.Jumping,
                                        DFCareer.Skills.Running,
                                        DFCareer.Skills.Swimming);

                AddSkillsToCharacterWindow(_characterPanel, UITextStrings.InventoryWindow_Label_LanguageSkills.GetText(), primarySkills,
                                        DFCareer.Skills.Orcish,
                                        DFCareer.Skills.Harpy,
                                        DFCareer.Skills.Giantish,
                                        DFCareer.Skills.Dragonish,
                                        DFCareer.Skills.Nymph,
                                        DFCareer.Skills.Daedric,
                                        DFCareer.Skills.Spriggan,
                                        DFCareer.Skills.Centaurian,
                                        DFCareer.Skills.Impish);
                #endregion

                #region Equipment Stats
                _characterPanel.Event_OnEnemyTypeSelected += (int enemyTypeIndex) =>
                {
                    if (_enemyTypesForStatCalc != null && enemyTypeIndex > 0 && enemyTypeIndex < _enemyTypesForStatCalc.Count)
                    {
                        if (_currentEnemyTypeForStatCalc != _enemyTypesForStatCalc[enemyTypeIndex])
                        {
                            _currentEnemyTypeForStatCalc = _enemyTypesForStatCalc[enemyTypeIndex];
                            UpdateEquipmentStatValues();
                        }
                    }
                };
                #endregion
            }
        }

        public void Cleanup()
        {
            if (_characterPanel != null)
            {
                _characterPanel.ResetPanel();
            }
        }
        #endregion


        #region Public API
        public void AddStatsToCharacterPanel(UICharacterPanel characterPanel, string scrollGroupName, params DFCareer.Stats[] stats)
        {
            List<UIStatData> statDataList = new List<UIStatData>();
            for (int i = 0; i < stats.Length; i++)
            {
                DFCareer.Stats stat = stats[i];
                UIStatData statData = new UIStatData
                {
                    statEnumAsInt = (int)stats[i],
                    name = DaggerfallUnity.Instance.TextProvider.GetStatName(stat),
                    icon = UIManager.referenceManager.GetStatIcon(stat),
                };
                statDataList.Add(statData);
            }
            characterPanel.AddStats(scrollGroupName, statDataList);
        }

        public void AddSkillsToCharacterWindow(UICharacterPanel characterPanel, string scrollGroupName, List<DFCareer.Skills> primarySkills, params DFCareer.Skills[] skills)
        {
            List<UISkillData> skillDataList = new List<UISkillData>();
            for (int i = 0; i < skills.Length; i++)
            {
                DFCareer.Skills skill = skills[i];
                SkillRank skillRank = UIUtilityFunctions.SkillToSkillRank(skill);
                UISkillData skillData = new UISkillData
                {
                    skillEnumAsInt = (int)skills[i],
                    name = DaggerfallUnity.Instance.TextProvider.GetSkillName(skill),
                    icon = UIManager.referenceManager.GetSkillIcon(skill),
                    rankIcon = UIManager.referenceManager.GetSkillRankIcon(skillRank),
                };
                skillDataList.Add(skillData);
            }
            characterPanel.AddSkills(scrollGroupName, skillDataList);
        }

        public void UpdateAll()
        {
            UpdatePlayerInfo();
            UpdatePaperDoll();
            UpdateVitalStatValues();
            UpdatePrimaryStatValues();
            UpdateSkillValues();
            UpdateEquipmentStatValues();
            UpdateEnemyTypes();
        }

        protected void UpdatePlayerInfo()
        {
            if (_characterPanel != null)
            {
                GameManager gm = GameManager.Instance;
                PlayerEntity playerEntity = gm != null ? gm.PlayerEntity : null;
                if (playerEntity != null)
                {
                    //player info
                    _characterPanel.SetPlayerName(playerEntity.Name);
                    _characterPanel.SetPlayerRace(playerEntity.RaceTemplate.Name);
                    _characterPanel.SetPlayerClass(playerEntity.Career.Name);
                    _characterPanel.SetPlayerLevel(playerEntity.Level.ToString("N0"));

                    //exp
                    _characterPanel.SetExperiencePercent(UIUtilityFunctions.GetPlayerExperiencePercent(playerEntity));
                }
            }
        }

        public void UpdateVitalStatValues()
        {
            if (_characterPanel == null) { return; }

            GameManager gm = GameManager.Instance;
            PlayerEntity playerEntity = gm != null ? gm.PlayerEntity : null;
            if (playerEntity != null)
            {
                //health
                _characterPanel.SetHealthSliderPercent(playerEntity.CurrentHealthPercent);
                _characterPanel.SetHealthValue(playerEntity.CurrentHealth.ToString("N0") + " / " + playerEntity.MaxHealth.ToString("N0"));

                //fatigue
                _characterPanel.SetFatigueSliderPercent(playerEntity.CurrentFatigue / (float)playerEntity.MaxFatigue);
                _characterPanel.SetFatigueValue(playerEntity.CurrentFatigue.ToString("N0") + " / " + playerEntity.MaxFatigue.ToString("N0"));

                //magicka
                _characterPanel.SetMagickaSliderPercent(playerEntity.CurrentMagicka / (float)playerEntity.MaxMagicka);
                _characterPanel.SetMagickaValue(playerEntity.CurrentMagicka.ToString("N0") + " / " + playerEntity.MaxMagicka.ToString("N0"));
            }
            else
            {
                _characterPanel.SetHealthSliderPercent(0f);
                _characterPanel.SetFatigueSliderPercent(0f);
                _characterPanel.SetMagickaSliderPercent(0f);

                _characterPanel.SetHealthValue(null);
                _characterPanel.SetFatigueValue(null);
                _characterPanel.SetMagickaValue(null);
            }
        }

        public void UpdateEquipmentStatValues()
        {
            if (_characterPanel == null) { return; }

            GameManager gm = GameManager.Instance;
            PlayerEntity playerEntity = gm != null ? gm.PlayerEntity : null;
            if (playerEntity != null)
            {
                int enemyTypeAsInt = (int)_currentEnemyTypeForStatCalc;
                DaggerfallUnityItem mainHandWep = playerEntity.ItemEquipTable.GetItem(EquipSlots.RightHand);
                DaggerfallUnityItem offHandWep = playerEntity.ItemEquipTable.GetItem(EquipSlots.LeftHand);

                //damage
                _characterPanel.SetMainHandDamageValue(UIUtilityFunctions.GetPlayerDamageString(mainHandWep, enemyTypeAsInt));
                _characterPanel.SetOffHandDamageValue(UIUtilityFunctions.GetPlayerDamageString(offHandWep, enemyTypeAsInt));

                //hit chance
                _characterPanel.SetMainHandHitChanceValue(UIUtilityFunctions.GetPlayerBaseHitChanceString(mainHandWep, enemyTypeAsInt));
                _characterPanel.SetOffHandHitChanceValue(UIUtilityFunctions.GetPlayerBaseHitChanceString(offHandWep, enemyTypeAsInt));

                //total armor value from all equipped armor
                _characterPanel.SetTotalArmorValue(UIUtilityFunctions.GetPlayerArmorAfterCalculation().ToString("N0"));
            }
            else
            {
                _characterPanel.SetMainHandDamageValue(null);
                _characterPanel.SetOffHandDamageValue(null);
                _characterPanel.SetMainHandHitChanceValue(null);
                _characterPanel.SetOffHandHitChanceValue(null);
                _characterPanel.SetTotalArmorValue(null);
            }
        }

        public void UpdatePrimaryStatValues()
        {
            if (_characterPanel != null)
            {
                GameManager gm = GameManager.Instance;
                PlayerEntity playerEntity = gm != null ? gm.PlayerEntity : null;
                if (playerEntity != null)
                {
                    foreach (DFCareer.Stats stat in System.Enum.GetValues(typeof(DFCareer.Stats)))
                    {
                        if (stat != DFCareer.Stats.None)
                        {
                            _characterPanel.SetStatValue((int)stat, playerEntity.Stats.GetLiveStatValue(stat));
                        }
                    }
                }
            }
        }

        public void UpdateSkillValues()
        {
            if (_characterPanel != null)
            {
                GameManager gm = GameManager.Instance;
                PlayerEntity playerEntity = gm != null ? gm.PlayerEntity : null;
                if (playerEntity != null)
                {
                    for (int i = 0; i < (int)DFCareer.Skills.Count; i++)
                    {
                        _characterPanel.SetSkillValue(i, playerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)i));
                    }
                }
            }
        }

        public void UpdatePaperDoll()
        {
            if (_characterPanel != null)
            {
                _characterPanel.UpdatePaperDoll();
            }
        }

        public void UpdateEnemyTypes()
        {
            if (_characterPanel != null)
            {
                _enemyTypesForStatCalc = UIUtilityFunctions.GetPossibleEnemyTypes();
                _enemyTypesForStatCalc.Sort((x, y) => { return x.ToString().CompareTo(y.ToString()); });

                List<string> enemyTypeStrings = new List<string>(_enemyTypesForStatCalc.Count);
                for (int i = 0; i < _enemyTypesForStatCalc.Count; i++)
                {
                    enemyTypeStrings.Add(UIUtilityFunctions.SplitStringIntoWords(_enemyTypesForStatCalc[i].ToString()));
                }

                _enemyTypesForStatCalc.Insert(0, MobileTypes.None);
                enemyTypeStrings.Insert(0, UITextStrings.Global_Label_None.GetText());

                _currentEnemyTypeForStatCalc = MobileTypes.None;

                _characterPanel.SetEnemyTypeOptions(enemyTypeStrings);
            }
        }
        #endregion
    }
}
