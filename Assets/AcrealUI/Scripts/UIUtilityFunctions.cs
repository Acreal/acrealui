/*
Copyright (c) 2025-2026 Acreal (https://github.com/acreal)

Permission is hereby granted, free of charge, to any person obtaining x copy of this software and associated 
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

using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.FallExe;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Banking;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

using static DaggerfallWorkshop.Game.Formulas.FormulaHelper;


namespace AcrealUI
{
    public static class UIUtilityFunctions
    {
        private static Vector3[] _worldCornerScratchArray = new Vector3[4];
        private static float _prevButtonClickSfxTime = -1f;

        //TODO(Acreal): put this in an external text file
        private static Dictionary<string, string> _potionRecipeKeyToPowerFormatDict = new Dictionary<string, string>()
        {
            { "restorePower", "Restores {0} Magicka" },
        };


        #region Transforms
        public static Transform FindDeepChild(Transform aParent, string aName)
        {
            if (!string.IsNullOrWhiteSpace(aName))
            {
                Queue<Transform> queue = new Queue<Transform>();
                queue.Enqueue(aParent);
                while (queue.Count > 0)
                {
                    var c = queue.Dequeue();

                    if (c.name == aName)
                    {
                        return c;
                    }

                    foreach (Transform t in c)
                    {
                        queue.Enqueue(t);
                    }
                }
                Debug.LogError("[AcrealUI] Failed to Find Child Attached to GameObject \"" + (aParent != null ? aParent.gameObject.name : "NULL") + "\" by Name: " + aName);
            }
            //else
            //{
            //    Debug.LogError("[AcrealUI] No Name Provided to Find Child Attached to GameObject \"" + (aParent != null ? aParent.gameObject.name : "NULL"));
            //}
            return null;
        }
        #endregion


        #region Player
        public static string GetPlayerName()
        {
            DaggerfallEntity playerEntity = GameManager.Instance != null ? GameManager.Instance.PlayerEntity : null;
            return playerEntity != null ? playerEntity.Name : null;
        }

        public static float GetPlayerExperiencePercent(PlayerEntity playerEntity)
        {
            if (playerEntity == null) { return 0f; }
            float currentLevel = (playerEntity.CurrentLevelUpSkillSum - playerEntity.StartingLevelUpSkillSum + 28f) / 15f;
            return currentLevel - Mathf.FloorToInt(currentLevel);
        }

        public static bool PlayerHasWagonAccess()
        {
            bool playerHasWagon = GameManager.Instance.PlayerEntity.Items.Contains(ItemGroups.Transportation, (int)Transportation.Small_cart);
            if (GameManager.Instance.PlayerEnterExit.IsPlayerInsideDungeon)
            {
                return playerHasWagon && DungeonWagonAccessProximityCheck();
            }
            else
            {
                return playerHasWagon;
            }
        }

        private static bool DungeonWagonAccessProximityCheck()
        {
            const float proximityWagonAccessDistance = 5f;

            // Get all static doors
            IEnumerable<DaggerfallStaticDoors> allRdbDoors = ActiveGameObjectDatabase.GetActiveRDBStaticDoors();
            if (allRdbDoors != null && allRdbDoors.Count() > 0)
            {
                Vector3 playerPos = GameManager.Instance.PlayerObject.transform.position;
                // Find closest door to player
                float closestDoorDistance = float.MaxValue;
                foreach (DaggerfallStaticDoors doors in allRdbDoors)
                {
                    int doorIndex;
                    Vector3 doorPos;
                    if (doors.FindClosestDoorToPlayer(playerPos, -1, out doorPos, out doorIndex, DoorTypes.DungeonExit))
                    {
                        float distance = Vector3.Distance(playerPos, doorPos);
                        if (distance < closestDoorDistance)
                            closestDoorDistance = distance;
                    }
                }

                // Allow wagon access if close enough to any exit door
                if (closestDoorDistance < proximityWagonAccessDistance)
                    return true;
            }
            return false;
        }

        public static bool PlayerHasHorse()
        {
            return GameManager.Instance.PlayerEntity.Items.Contains(ItemGroups.Transportation, (int)Transportation.Horse);
        }

        public static bool PlayerHasShip()
        {
            return DaggerfallBankManager.OwnsShip || GameManager.Instance.GuildManager.FreeShipTravel();
        }
        #endregion


        #region Player Stats
        public static float GetPlayerBaseHitChancePercent(DaggerfallUnityItem weapon, int enemyID = -1)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            if (player == null)
            {
                return 0f;
            }

            int skillID = weapon != null ? weapon.GetWeaponSkillIDAsShort() : (short)DFCareer.Skills.HandToHand;
            int chanceToHit = player.Skills.GetLiveSkillValue(skillID);

            // Apply swing modifiers
            ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
            chanceToHit += swingMods.toHitMod;

            // Apply proficiency modifiers
            ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(player, weapon);
            chanceToHit += proficiencyMods.toHitMod;

            // Apply racial bonuses
            ToHitAndDamageMods racialMods = CalculateRacialModifiers(player, weapon, player);
            chanceToHit += racialMods.toHitMod;

            if (skillID != (short)DFCareer.Skills.HandToHand)
            {
                // Apply weapon material modifier.
                chanceToHit += CalculateWeaponToHit(weapon);

                // Mod support - allows x final adjustment of weapon hit
                //int weaponAnimTime = (int)(GameManager.Instance.WeaponManager.ScreenWeapon.GetAnimTime() * 1000);
                //chanceToHit = AdjustWeaponHitChanceMod(player, target, chanceToHit, weaponAnimTime, weapon);
            }

            // Apply enchantment modifier
            chanceToHit += player.ChanceToHitModifier;

            if (enemyID >= 0)
            {
                // Get enemy information
                MobileTypes enemyType = (MobileTypes)enemyID;
                MonsterCareers enemyCareer = (MonsterCareers)enemyID;
                EnemyBasics.GetEnemy(enemyType, out MobileEnemy mobileEnemy);
                DFCareer career = DaggerfallEntity.GetMonsterCareerTemplate(enemyCareer);
                int luck = career != null ? career.Luck : UIConstants.ENEMY_BASE_SKILL_LEVEL;
                int agility = career != null ? career.Luck : UIConstants.ENEMY_BASE_SKILL_LEVEL;

                // Get armor value for struck body part
                chanceToHit += (sbyte)(mobileEnemy.ArmorValue * UIConstants.ENEMY_ARMOR_MULTIPLIER);

                // Apply stat differential modifiers (default: luck and agility)
                chanceToHit += (player.Stats.LiveLuck - luck) / UIConstants.ENEMY_STAT_HIT_CHANCE_DIVISOR; // Apply luck modifier
                chanceToHit += (player.Stats.LiveAgility - agility) / UIConstants.ENEMY_STAT_HIT_CHANCE_DIVISOR; // Apply agility modifier

                // Apply skill modifiers
                // treat player as their own target
                // NOTE(Acreal): use x default enemy type in the future?
                short skillsLevel = (short)((Mathf.Max(mobileEnemy.Level, 1) * UIConstants.ENEMY_SKILL_POINTS_PER_LEVEL) + UIConstants.ENEMY_BASE_SKILL_LEVEL);
                if (skillsLevel > UIConstants.MAX_SKILL_LEVEL)
                {
                    skillsLevel = UIConstants.MAX_SKILL_LEVEL;
                }
                chanceToHit -= skillsLevel / UIConstants.ENEMY_SKILL_HIT_CHANCE_DIVISOR;
            }

            //subtract 10 (sum of adding 40 for player attacking x monster - 50 flat at the end)
            //taken from FormulaHelper.CalculateAdjustmentsToHit
            chanceToHit -= 10;

            chanceToHit = Mathf.Clamp(chanceToHit, UIConstants.MIN_HIT_CHANCE, UIConstants.MAX_HIT_CHANCE);
            return chanceToHit;
        }

        public static void GetWeaponMinMaxDamage(DaggerfallUnityItem weapon, out int minDamage, out int maxDamage, int enemyID = -1, bool assumeBackstab = false)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            if (player == null)
            {
                minDamage = 0;
                maxDamage = 0;
                return;
            }

            MobileTypes enemyType = enemyID > -1 ? (MobileTypes)enemyID : MobileTypes.None;
            MobileEnemy mobileEnemy = new MobileEnemy();
            if (enemyID > -1 && enemyType != MobileTypes.None)
            {
                EnemyBasics.GetEnemy(enemyType, out mobileEnemy);
            }

            minDamage = 0;
            maxDamage = 0;
            int damageModifiers = 0;
            short skillID = 0;


            ////////////////////////////////////////////////
            /// Below Code Was Copied From FormulaHelper.cs
            ////////////////////////////////////////////////
            if (weapon != null)
            {
                // If the attacker is using x weapon, check if the material is high enough to damage the target
                if (enemyID > -1 && enemyType != MobileTypes.None && mobileEnemy.MinMetalToHit > (WeaponMaterialTypes)weapon.NativeMaterialValue)
                {
                    DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("materialIneffective"));
                    return;
                }
                // Get weapon skill used
                skillID = weapon.GetWeaponSkillIDAsShort();
            }
            else
            {
                skillID = (short)DFCareer.Skills.HandToHand;
            }

            // Apply swing modifiers
            ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
            damageModifiers += swingMods.damageMod;

            // Apply proficiency modifiers
            ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(player, weapon);
            damageModifiers += proficiencyMods.damageMod;

            // Apply racial bonuses
            ToHitAndDamageMods racialMods = CalculateRacialModifiers(player, weapon, player);
            damageModifiers += racialMods.damageMod;

            // Apply strength modifier
            damageModifiers += DamageModifier(player.Stats.LiveStrength);

            // Apply EnemyType bonus/penalty
            if (enemyID > -1 && enemyType != MobileTypes.None)
            {
                int enemyTypeBonus = GetPlayerDamageBonusOrPenaltyByEnemyType(mobileEnemy);
                damageModifiers += enemyTypeBonus;
            }

            if (skillID == (short)DFCareer.Skills.HandToHand)
            {
                int handToHandSkill = player.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand);
                minDamage = CalculateHandToHandMinDamage(handToHandSkill) + damageModifiers;
                maxDamage = CalculateHandToHandMaxDamage(handToHandSkill) + damageModifiers;
            }
            else if (weapon != null)
            {
                // Apply material modifier.
                // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
                damageModifiers += weapon.GetWeaponMaterialModifier();

                minDamage = weapon.GetBaseDamageMin() + damageModifiers;
                maxDamage = weapon.GetBaseDamageMax() + damageModifiers;

                if (enemyID > -1 && enemyType != MobileTypes.None && enemyID == (int)MonsterCareers.SkeletalWarrior)
                {
                    // Apply edged-weapon damage modifier for Skeletal Warrior
                    if ((weapon.flags & 0x10) == 0)
                    {
                        minDamage /= 2;
                        maxDamage /= 2;
                    }

                    // Apply silver weapon damage modifier for Skeletal Warrior
                    // Arena applies x silver weapon damage bonus for undead enemies, which is probably where this comes from.
                    if (weapon.NativeMaterialValue == (int)WeaponMaterialTypes.Silver)
                    {
                        minDamage *= 2;
                        maxDamage *= 2;
                    }
                }
            }

            if (assumeBackstab)
            {
                minDamage *= UIConstants.BACKSTAB_DAMAGE_MODIFIER;
                maxDamage *= UIConstants.BACKSTAB_DAMAGE_MODIFIER;
            }

            minDamage = Mathf.Max(minDamage, 0);
            maxDamage = Mathf.Max(maxDamage, 0);
        }

        public static string GetPlayerBaseHitChanceString(DaggerfallUnityItem weapon, int enemyID = -1)
        {
            return string.Format("{0:N0}%", GetPlayerBaseHitChancePercent(weapon, enemyID));
        }

        public static string GetPlayerDamageString(DaggerfallUnityItem weapon, int enemyID = -1)
        {
            GetWeaponMinMaxDamage(weapon, out int minDmg, out int maxDmg, enemyID);
            if (minDmg == 0 && maxDmg == 0) { return string.Format("{0:N0}", minDmg); }
            else { return string.Format("{0:N0}-{1:N0}", minDmg, maxDmg); }
        }

        public static void GetPlayerAttackModifiers(DaggerfallUnityItem weapon, out int damageModifier, out int chanceToHitModifier)
        {
            // Apply swing modifiers
            ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
            damageModifier = swingMods.damageMod;
            chanceToHitModifier = swingMods.toHitMod;

            // Apply proficiency modifiers
            ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(GameManager.Instance.PlayerEntity, weapon);
            damageModifier += proficiencyMods.damageMod;
            chanceToHitModifier += proficiencyMods.toHitMod;

            // Apply racial bonuses
            ToHitAndDamageMods racialMods = CalculateRacialModifiers(GameManager.Instance.PlayerEntity, weapon, GameManager.Instance.PlayerEntity);
            damageModifier += racialMods.damageMod;
            chanceToHitModifier += racialMods.toHitMod;
        }

        public static int GetPlayerArmorAfterCalculation()
        {
            int totalArmor = 0;
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            if (playerEntity != null)
            {
                for (int i = 0; i < playerEntity.ArmorValues.Length; i++)
                {
                    int armorMod = playerEntity.DecreasedArmorValueModifier - playerEntity.IncreasedArmorValueModifier;
                    sbyte av = playerEntity.ArmorValues[i];
                    int bpAv = (100 - av) / 5 + armorMod;
                    totalArmor += bpAv;
                }
            }
            return totalArmor;
        }

        public static int GetPlayerDamageBonusOrPenaltyByEnemyType(MobileEnemy mobileEnemy)
        {
            int damage = 0;

            PlayerEntity player = GameManager.Instance.PlayerEntity;
            if (player != null)
            {
                DFCareer.EnemyGroups enemyGroup = GetEnemyGroupFromMobileEnemy(mobileEnemy);

                // Apply bonus or penalty by opponent type.
                // In classic this is broken and only works if the attack is done with x weapon that has the maximum number of enchantments.
                if (mobileEnemy.Affinity == MobileAffinity.Human)
                {
                    if (((int)player.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0) { damage += player.Level; }
                    if (((int)player.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0) { damage -= player.Level; }
                }
                else if (enemyGroup == DFCareer.EnemyGroups.Undead)
                {
                    if (((int)player.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0) { damage += player.Level; }
                    if (((int)player.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0) { damage -= player.Level; }
                }
                else if (enemyGroup == DFCareer.EnemyGroups.Daedra)
                {
                    if (((int)player.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0) { damage += player.Level; }
                    if (((int)player.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0) { damage -= player.Level; }
                }
                else if (enemyGroup == DFCareer.EnemyGroups.Animals)
                {
                    if (((int)player.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0) { damage += player.Level; }
                    if (((int)player.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0) { damage -= player.Level; }
                }
            }

            return damage;
        }

        public static DFCareer.EnemyGroups GetEnemyGroupFromMobileEnemy(MobileEnemy mobileEnemy)
        {
            MonsterCareers enemyCareer = (MonsterCareers)mobileEnemy.ID;
            switch (enemyCareer)
            {
                case MonsterCareers.Rat:
                case MonsterCareers.GiantBat:
                case MonsterCareers.GrizzlyBear:
                case MonsterCareers.SabertoothTiger:
                case MonsterCareers.Spider:
                case MonsterCareers.Slaughterfish:
                case MonsterCareers.GiantScorpion:
                case MonsterCareers.Dragonling:
                case MonsterCareers.Horse_Invalid:             // (grouped as undead in classic)
                case MonsterCareers.Dragonling_Alternate:      // (grouped as undead in classic)
                    return DFCareer.EnemyGroups.Animals;
                case MonsterCareers.Imp:
                case MonsterCareers.Spriggan:
                case MonsterCareers.Orc:
                case MonsterCareers.Centaur:
                case MonsterCareers.Werewolf:
                case MonsterCareers.Nymph:
                case MonsterCareers.OrcSergeant:
                case MonsterCareers.Harpy:
                case MonsterCareers.Wereboar:
                case MonsterCareers.Giant:
                case MonsterCareers.OrcShaman:
                case MonsterCareers.Gargoyle:
                case MonsterCareers.OrcWarlord:
                case MonsterCareers.Dreugh:                    // (grouped as undead in classic)
                case MonsterCareers.Lamia:                     // (grouped as undead in classic)
                    return DFCareer.EnemyGroups.Humanoid;
                case MonsterCareers.SkeletalWarrior:
                case MonsterCareers.Zombie:                    // (grouped as animal in classic)
                case MonsterCareers.Ghost:
                case MonsterCareers.Mummy:
                case MonsterCareers.Wraith:
                case MonsterCareers.Vampire:
                case MonsterCareers.VampireAncient:
                case MonsterCareers.Lich:
                case MonsterCareers.AncientLich:
                    return DFCareer.EnemyGroups.Undead;
                case MonsterCareers.FrostDaedra:
                case MonsterCareers.FireDaedra:
                case MonsterCareers.Daedroth:
                case MonsterCareers.DaedraSeducer:
                case MonsterCareers.DaedraLord:
                    return DFCareer.EnemyGroups.Daedra;
                case MonsterCareers.FireAtronach:
                case MonsterCareers.IronAtronach:
                case MonsterCareers.FleshAtronach:
                case MonsterCareers.IceAtronach:
                    return DFCareer.EnemyGroups.None;

                default:
                    return DFCareer.EnemyGroups.None;
            }
        }
        #endregion


        #region Paper Doll
        public static Texture2D GetPaperDollTexture()
        {
            if (GameManager.Instance != null && GameManager.Instance.PlayerEntity != null)
            {
                DaggerfallUI.Instance.PaperDollRenderer.Refresh(PaperDollRenderer.LayerFlags.All, GameManager.Instance.PlayerEntity);
                return DaggerfallUI.Instance.PaperDollRenderer.PaperDollTexture;
            }
            return null;
        }
        #endregion


        #region Items
        public static ItemType ItemToItemType(DaggerfallUnityItem item)
        {
            if (item == null) { return ItemType.Unknown; }

            if (item.IsQuestItem)
            {
                return ItemType.QuestItem;
            }
            else if (item.IsPotion)
            {
                return ItemType.Potion;
            }
            else if (item.IsClothing)
            {
                return ItemType.Clothing;
            }
            else
            {
                switch (item.ItemGroup)
                {
                    case ItemGroups.Armor:
                        return ItemType.Armor;

                    case ItemGroups.Weapons:
                        return ItemType.Weapon;

                    case ItemGroups.Currency:
                        return ItemType.Currency;

                    case ItemGroups.CreatureIngredients1:
                    case ItemGroups.CreatureIngredients2:
                    case ItemGroups.CreatureIngredients3:
                    case ItemGroups.MiscellaneousIngredients1:
                    case ItemGroups.MiscellaneousIngredients2:
                    case ItemGroups.PlantIngredients1:
                    case ItemGroups.PlantIngredients2:
                        return ItemType.Ingredient;

                    case ItemGroups.Jewellery:
                        if (ItemIsEquippable(item)) { return ItemType.Jewellery; }
                        else { return ItemType.Junk; }

                    case ItemGroups.Gems:
                    case ItemGroups.Paintings:
                    case ItemGroups.UselessItems1:
                    case ItemGroups.UselessItems2:
                    case ItemGroups.ReligiousItems:
                        return ItemType.Junk;

                    case ItemGroups.MiscItems:
                        switch ((MiscItems)item.TemplateIndex)
                        {
                            case MiscItems.Letter_of_credit:
                            case MiscItems.House_Deed:
                            case MiscItems.Ship_Deed:
                            case MiscItems.Potion_recipe:
                            case MiscItems.Spellbook:
                                return ItemType.Readable;

                            default:
                                return ItemType.Junk;
                        }

                    case ItemGroups.Books:
                    case ItemGroups.Deeds:
                    case ItemGroups.Maps:
                        return ItemType.Readable;

                    default:
                        return ItemType.Unknown;
                }
            }
        }

        public static ItemArchetype ItemToItemArchetype(DaggerfallUnityItem item)
        {
            if (item.IsLightSource)
            {
                return ItemArchetype.LightSource;
            }
            else
            {
                switch (item.ItemGroup)
                {
                    case ItemGroups.Armor:
                    case ItemGroups.Jewellery:
                    case ItemGroups.MensClothing:
                    case ItemGroups.WomensClothing:
                        {
                            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                            EquipSlots equipSlot = playerEntity != null ? playerEntity.ItemEquipTable.GetEquipSlot(item) : item.GetEquipSlot();
                            switch (equipSlot)
                            {
                                case EquipSlots.ChestClothes:
                                    return ItemArchetype.ClothingChest;

                                case EquipSlots.LegsClothes:
                                    return ItemArchetype.ClothingLegs;

                                case EquipSlots.Amulet0:
                                case EquipSlots.Amulet1:
                                    return ItemArchetype.Amulet;

                                case EquipSlots.Bracelet0:
                                case EquipSlots.Bracelet1:
                                    return ItemArchetype.Bracelet;

                                case EquipSlots.Bracer0:
                                case EquipSlots.Bracer1:
                                    return ItemArchetype.Wrists;

                                case EquipSlots.ChestArmor:
                                    return ItemArchetype.Chestpiece;

                                case EquipSlots.Cloak1:
                                case EquipSlots.Cloak2:
                                    return ItemArchetype.Cloak;

                                case EquipSlots.Feet:
                                    return ItemArchetype.Boots;

                                case EquipSlots.Gloves:
                                    return ItemArchetype.Gauntlets;

                                case EquipSlots.Head:
                                    return ItemArchetype.Helm;

                                case EquipSlots.LeftArm:
                                case EquipSlots.RightArm:
                                    return ItemArchetype.Pauldrons;

                                case EquipSlots.LegsArmor:
                                    return ItemArchetype.Greaves;

                                case EquipSlots.LeftHand:
                                case EquipSlots.RightHand:
                                    return item.IsShield ? ItemArchetype.Shield : ItemArchetype.Gauntlets;

                                case EquipSlots.Ring0:
                                case EquipSlots.Ring1:
                                    return ItemArchetype.Ring;

                                case EquipSlots.Mark0:
                                case EquipSlots.Mark1:
                                    return ItemArchetype.Mark;

                                case EquipSlots.Crystal0:
                                case EquipSlots.Crystal1:
                                default:
                                    return ItemArchetype.Unknown;
                            }
                        }

                    case ItemGroups.Weapons:
                        if (item.TemplateIndex == (int)Weapons.Arrow)
                        {
                            return ItemArchetype.Arrow;
                        }
                        else
                        {
                            WeaponTypes weaponType = DaggerfallUnity.Instance.ItemHelper.ConvertItemToAPIWeaponType(item);
                            switch (weaponType)
                            {
                                case WeaponTypes.Battleaxe:
                                case WeaponTypes.Battleaxe_Magic:
                                    return ItemArchetype.Axe;

                                case WeaponTypes.Bow:
                                    return ItemArchetype.Bow;

                                case WeaponTypes.Dagger:
                                case WeaponTypes.Dagger_Magic:
                                    return ItemArchetype.ShortBlade;

                                case WeaponTypes.Flail:
                                case WeaponTypes.Flail_Magic:
                                    return ItemArchetype.Flail;

                                case WeaponTypes.LongBlade:
                                case WeaponTypes.LongBlade_Magic:
                                    return ItemArchetype.LongBlade;

                                case WeaponTypes.Mace:
                                case WeaponTypes.Mace_Magic:
                                    return ItemArchetype.Mace;

                                case WeaponTypes.Warhammer:
                                case WeaponTypes.Warhammer_Magic:
                                    return ItemArchetype.Warhammer;

                                case WeaponTypes.Staff:
                                case WeaponTypes.Staff_Magic:
                                    return ItemArchetype.Staff;

                                default:
                                    return ItemArchetype.Unknown;
                            }
                        }

                    case ItemGroups.MiscItems:
                        switch ((MiscItems)item.TemplateIndex)
                        {
                            case MiscItems.Letter_of_credit:
                            case MiscItems.House_Deed:
                            case MiscItems.Ship_Deed:
                                return ItemArchetype.Note;

                            case MiscItems.Potion_recipe:
                                return ItemArchetype.Recipe;

                            case MiscItems.Spellbook:
                                return ItemArchetype.Spellbook;

                            default:
                                return ItemArchetype.Unknown;
                        }

                    case ItemGroups.Books:
                        return ItemArchetype.Book;

                    case ItemGroups.Maps:
                        return ItemArchetype.Map;

                    case ItemGroups.Deeds:
                        return ItemArchetype.Note;

                    case ItemGroups.CreatureIngredients1:
                    case ItemGroups.CreatureIngredients2:
                    case ItemGroups.CreatureIngredients3:
                        return ItemArchetype.CreatureIngredient;

                    case ItemGroups.MiscellaneousIngredients1:
                        if (item.TemplateIndex == (int)MiscellaneousIngredients1.Holy_relic)
                        {
                            return ItemArchetype.Religion;
                        }
                        else if (item.TemplateIndex == (int)MiscellaneousIngredients1.Small_tooth || item.TemplateIndex == (int)MiscellaneousIngredients1.Medium_tooth || item.TemplateIndex == (int)MiscellaneousIngredients1.Big_tooth)
                        {
                            return ItemArchetype.ToothIngredient;
                        }
                        else if (item.TemplateIndex == (int)MiscellaneousIngredients1.Pure_water || item.TemplateIndex == (int)MiscellaneousIngredients1.Nectar ||
                                item.TemplateIndex == (int)MiscellaneousIngredients1.Rain_water || item.TemplateIndex == (int)MiscellaneousIngredients1.Elixir_vitae)
                        {
                            return ItemArchetype.LiquidIngredient;
                        }
                        else
                        {
                            return ItemArchetype.MiscIngredient;
                        }


                    case ItemGroups.MetalIngredients:
                        return ItemArchetype.MetalIngot;

                    case ItemGroups.PlantIngredients1:
                    case ItemGroups.PlantIngredients2:
                        return ItemArchetype.PlantIngredient;

                    case ItemGroups.Gems:
                    case ItemGroups.MiscellaneousIngredients2:
                        return ItemArchetype.Gem;

                    case ItemGroups.Currency:
                        if (item.IsOfTemplate(ItemGroups.Currency, (int)Currency.Gold_pieces))
                        {
                            return ItemArchetype.Gold;
                        }
                        else
                        {
                            return ItemArchetype.Unknown;
                        }

                    case ItemGroups.Paintings:
                        return ItemArchetype.Painting;

                    case ItemGroups.ReligiousItems:
                        return ItemArchetype.Religion;

                    case ItemGroups.UselessItems1:
                    case ItemGroups.UselessItems2:
                    default:
                        return ItemArchetype.Unknown;
                }
            }
        }

        public static bool ItemIsEquippable(DaggerfallUnityItem item)
        {
            if (item == null) return false;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            return player != null ? player.ItemEquipTable.GetEquipSlot(item) != EquipSlots.None : false;
        }

        public static bool ItemIsMagic(DaggerfallUnityItem item)
        {
            return item != null && !item.IsPotion && item.HasLegacyEnchantments;
        }

        public static bool ItemTypeIsUseable(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Food:
                case ItemType.Potion:
                case ItemType.Readable:
                    return true;

                default:
                    return false;
            }
        }

        public static Sprite ItemToItemArchetypeIcon(DaggerfallUnityItem item)
        {
            ItemArchetype archetype = ItemToItemArchetype(item);
            Sprite icon = UIManager.referenceManager.GetItemArchetypeIcon(archetype);

            //fallback to itemType icon
            if (icon == null)
            {
                ItemType itemType = ItemToItemType(item);
                icon = UIManager.referenceManager.GetItemTypeIcon(itemType);
            }

            return icon;
        }

        public static string ItemToMaterialString(DaggerfallUnityItem item)
        {
            if (item == null)
            {
                return string.Empty;
            }
            else
            {
                if (item.ItemGroup == ItemGroups.Weapons)
                {
                    return ((WeaponMaterialTypes)item.nativeMaterialValue).ToString();
                }
                else if (item.ItemGroup == ItemGroups.Armor)
                {
                    return ((ArmorMaterialTypes)item.nativeMaterialValue).ToString();
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static string ItemFilterToString(ItemFilter itemFilter)
        {
            switch (itemFilter)
            {
                case ItemFilter.MiscItems:
                    return "Misc";

                case ItemFilter.MagicItems:
                    return "Magic Items";

                case ItemFilter.QuestItems:
                    return "Quest Items";

                default:
                    return itemFilter.ToString();
            }
        }

        public static ItemColumnFlags ItemFilterToColumnFlags(ItemFilter filter)
        {
            switch (filter)
            {
                case ItemFilter.Books: return ItemColumnFlags.Filter_Books;
                case ItemFilter.QuestItems: return ItemColumnFlags.Filter_QuestItems;
                case ItemFilter.MagicItems: return ItemColumnFlags.Filter_MagicItems;
                case ItemFilter.MiscItems: return ItemColumnFlags.Filter_Misc;
                case ItemFilter.Weapons: return ItemColumnFlags.Filter_Weapons;
                case ItemFilter.Armor: return ItemColumnFlags.Filter_Armor;
                default: return ItemColumnFlags.All;
            }
        }

        public static bool ItemIsGold(DaggerfallUnityItem item)
        {
            return item != null && item.IsOfTemplate(ItemGroups.Currency, (int)Currency.Gold_pieces);
        }

        public static bool ItemIsUsable(DaggerfallUnityItem item)
        {
            return item != null && (ItemIsConsumable(item) || ItemIsReadable(item) || ItemIsEquippable(item));
        }

        public static bool ItemIsConsumable(DaggerfallUnityItem item)
        {
            return item != null && (item.IsPotion);
        }

        public static bool ItemIsReadable(DaggerfallUnityItem item)
        {
            return item != null && (item.IsPotionRecipe || item.IsParchment || item.IsOfTemplate((int)MiscItems.Spellbook));
        }

        public static bool ItemIsBroken(DaggerfallUnityItem item)
        {
            return item != null && item.ConditionPercentage < 1;
        }

        public static bool ItemIsProhibitedToEquip(DaggerfallUnityItem item, UIItemQueryOptions itemQueryOptions)
        {
            if (item.ItemGroup == ItemGroups.Armor)
            {
                if (item.IsShield && ((1 << (item.TemplateIndex - (int)Armor.Buckler) & itemQueryOptions.forbiddenShieldsAsInt) != 0))
                {
                    //prohibited shield
                    return true;
                }
                else if (!item.IsShield && (1 << (item.NativeMaterialValue >> 8) & itemQueryOptions.forbiddenArmorsAsInt) != 0)
                {
                    //prohibited armor type (leather, chain or plate)
                    return true;
                }
                else if (((item.nativeMaterialValue >> 8) == 2) && (1 << (item.NativeMaterialValue & 0xFF) & itemQueryOptions.forbiddenMaterialsAsInt) != 0)
                {
                    //prohibited material
                    return true;
                }
            }
            else if (item.ItemGroup == ItemGroups.Weapons)
            {
                //prohibited weapon type
                if ((item.GetWeaponSkillUsed() & itemQueryOptions.forbiddenProficienciesAsInt) != 0)
                {
                    return true;
                }
                //prohibited material
                else if ((1 << item.NativeMaterialValue & itemQueryOptions.forbiddenMaterialsAsInt) != 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static ItemStatusFlags ItemToItemStatusFlags(DaggerfallUnityItem item, UIItemQueryOptions itemQueryOptions)
        {
            ItemStatusFlags flags = 0;
            if (item != null)
            {
                if (item.IsEquipped)
                {
                    flags |= ItemStatusFlags.Equipped;
                }

                if (item.ConditionPercentage < 1 && item.maxCondition > 0 && ItemIsEquippable(item) && !ItemIsAmmo(item))
                {
                    flags |= ItemStatusFlags.Broken;
                }

                if (item.IsEnchanted)
                {
                    flags |= ItemStatusFlags.Magic;
                }

                if (item.poisonType != Poisons.None)
                {
                    flags |= ItemStatusFlags.Poisoned;
                }

                if (ItemIsProhibitedToEquip(item, itemQueryOptions) && !ItemIsAmmo(item))
                {
                    flags |= ItemStatusFlags.Prohibited;
                }

                //if (ItemIsFavorited(item))
                //{
                //  flags |= ItemStatusFlags.Favorite;
                //}
            }
            return flags;
        }

        public static UIItemData ItemToItemData(DaggerfallUnityItem item, UIItemQueryOptions itemQueryOptions)
        {
            if (item == null)
            {
                return new UIItemData();
            }
            else
            {
                ImageData imageData = DaggerfallUnity.Instance.ItemHelper.GetInventoryImage(item);
                Sprite itemIcon = UISpriteManager.GetOrCreateSprite(imageData);

                bool ignoreCondition = item == null || item.maxCondition < 1 || (!UIUtilityFunctions.ItemIsEquippable(item) && !UIUtilityFunctions.ItemIsMagic(item));
                float conPerc = ignoreCondition ? -1f : (item.ConditionPercentage / 100f);

                int minDamage = 0;
                int maxDamage = 0;
                if (item.ItemGroup == ItemGroups.Weapons && !ItemIsAmmo(item))
                {
                    GetWeaponMinMaxDamage(item, out minDamage, out maxDamage);
                }

                int armor = 0;
                if (item.IsShield) { armor = item.GetShieldArmorValue(); }
                else
                {
                    // Get equip index with out of range check
                    PlayerEntity player = GameManager.Instance.PlayerEntity;
                    EquipSlots equipSlot = player != null ? player.ItemEquipTable.GetEquipSlot(item) : EquipSlots.None;
                    BodyParts bodyPart = DaggerfallUnityItem.GetBodyPartForEquipSlot(equipSlot);
                    if (bodyPart != BodyParts.None)
                    {
                        armor += item.GetMaterialArmorValue();
                    }
                }

                int materialModifier = item.GetWeaponMaterialModifier();

                return new UIItemData
                {
                    UID = item.UID,
                    shortName = item.shortName,
                    longName = item.LongName,
                    description = GetItemDescription(item),
                    icon = itemIcon,
                    itemType = ItemToItemType(item),
                    itemArchetype = ItemToItemArchetype(item),
                    archetypeIcon = ItemToItemArchetypeIcon(item),
                    materialString = ItemToMaterialString(item),
                    stackCount = item.stackCount,
                    itemStatusFlags = ItemToItemStatusFlags(item, itemQueryOptions),
                    minDamageValue = item.GetBaseDamageMin() + materialModifier,
                    maxDamageValue = item.GetBaseDamageMax() + materialModifier,
                    armorValue = armor,
                    conditionPercent = conPerc,
                    weightValue = item.EffectiveUnitWeightInKg(),
                    goldValue = item.value,
                };
            }
        }

        private static bool ShouldFilterOutItem(ItemType itemType, ItemFilter filterType)
        {
            switch (filterType)
            {
                case ItemFilter.Armor:
                    return itemType != ItemType.Armor && itemType != ItemType.Clothing && itemType != ItemType.Jewellery;

                case ItemFilter.Books:
                    return itemType != ItemType.Readable;

                case ItemFilter.Ingredients:
                    return itemType != ItemType.Ingredient;

                case ItemFilter.QuestItems:
                    return itemType != ItemType.QuestItem;

                case ItemFilter.MiscItems:
                    return itemType != ItemType.Currency &&
                           itemType != ItemType.Junk;

                case ItemFilter.Potions:
                    return itemType != ItemType.Potion;

                case ItemFilter.Weapons:
                    return itemType != ItemType.Weapon;

                case ItemFilter.All:
                default:
                    return false;
            }
        }

        public static bool ShouldFilterOutItem(DaggerfallUnityItem item, ItemFilter filterType)
        {
            if (item == null)
            {
                return true;
            }

            if (item.ItemGroup == ItemGroups.Transportation)
            {
                if (item.TemplateIndex == (int)Transportation.Horse || item.TemplateIndex == (int)Transportation.Small_cart)
                {
                    return true;
                }
            }

            if (filterType == ItemFilter.Favorite)
            {
                //TODO(Acreal): implement favorites
                return true;
            }
            else if (filterType == ItemFilter.MagicItems)
            {
                return !item.HasCustomEnchantments && !item.HasLegacyEnchantments;
            }
            else if (filterType == ItemFilter.Ingredients)
            {
                return !item.IsIngredient;
            }
            else
            {
                ItemType itemType = ItemToItemType(item);
                return ShouldFilterOutItem(itemType, filterType);
            }
        }

        public static List<UIItemData> ItemCollectionToItemDataList(ItemCollection itemCollection, UIItemQueryOptions itemQueryOptions, ItemFilter filter = ItemFilter.All)
        {
            if (itemCollection == null) { return null; }

            List<UIItemData> itemDataList = new List<UIItemData>();
            for (int i = 0; i < itemCollection.Count; i++)
            {
                DaggerfallUnityItem item = itemCollection.GetItem(i);
                if (!ShouldFilterOutItem(item, filter))
                {
                    UIItemData itemData = ItemToItemData(item, itemQueryOptions);
                    itemDataList.Add(itemData);
                }
            }
            return itemDataList;
        }

        public static bool ItemCollectionContainsItemsWithFilter(ItemCollection itemCollection, ItemFilter filter)
        {
            if (itemCollection != null)
            {
                for (int i = 0; i < itemCollection.Count; i++)
                {
                    if (!ShouldFilterOutItem(itemCollection.GetItem(i), filter))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool ItemIsAmmo(DaggerfallUnityItem item)
        {
            //NOTE(Acreal): as far as I know, we just need to check against Weapons.Arrow
            //this may change in the future, so keep an eye on it as necessary
            int arrowGroupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.Weapons, (int)Weapons.Arrow);
            return item.GroupIndex == arrowGroupIndex;
        }

        public static string GetItemName(UIItemData itemData)
        {
            StringBuilder nameBuilder = new StringBuilder();
            nameBuilder.Append(itemData.longName);

            if (itemData.stackCount > 1)
            {
                nameBuilder.Append(" (");
                nameBuilder.Append(itemData.stackCount.ToString("N0"));
                nameBuilder.Append(")");
            }

            return nameBuilder.ToString();
        }

        public static string GetItemDescription(DaggerfallUnityItem item)
        {
            if (item != null)
            {
                StringBuilder descBuilder = new StringBuilder();

                if (item.IsArtifact)
                {
                    TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(UIConstants.LEGACY_MAGIC_TOKEN_ID);
                    if (tokens != null && tokens.Length > 0)
                    {
                        MacroHelper.ExpandMacros(ref tokens, item);

                        ArtifactsSubTypes artifactType = ItemHelper.GetArtifactSubType(item);
                        switch (artifactType)
                        {
                            case ArtifactsSubTypes.Skull_of_Corruption:
                                {
                                    string[] splits = tokens[13].text.Split('.');
                                    descBuilder.Append(UIConstants.INDENT);
                                    descBuilder.Append(splits[1]);

                                    for (int i = 14; i < 40; i++)
                                    {
                                        TextFile.Token token = tokens[i];
                                        if (!string.IsNullOrEmpty(token.text))
                                        {
                                            descBuilder.Append(" ");
                                            descBuilder.Append(token.text);
                                        }
                                    }
                                }
                                break;

                            default:
                                descBuilder.Append("[AcrealUI] Artifact Description Not Added Yet");
                                break;
                        }
                        //for (int i = 1; i < tokens.Length-2; i++)
                        //{
                        //    TextFile.Token token = tokens[i];
                        //    if (!string.IsNullOrEmpty(token.text))
                        //    {
                        //        descBuilder.AppendLine(token.text);
                        //    }
                        //}
                        return descBuilder.ToString();
                    }
                }
            }
            return null;
        }

        public static List<string> GetItemMagicInfo(DaggerfallUnityItem item)
        {
            List<string> powersList = new List<string>();

            if (item.IsPotionRecipe)
            {
                PotionRecipe potionRecipe = GameManager.Instance.EntityEffectBroker.GetPotionRecipe(item.PotionRecipeKey);
                if (potionRecipe != null)
                {
                    foreach (PotionRecipe.Ingredient ingredient in potionRecipe.Ingredients)
                    {
                        ItemTemplate ingredientTemplate = DaggerfallUnity.Instance.ItemHelper.GetItemTemplate(ingredient.id);
                        string ingredientName = TextManager.Instance.GetLocalizedItemName(ingredientTemplate.index, ingredientTemplate.name);
                        powersList.Add(ingredientName);
                    }
                }
            }
            else if (item.IsPotion)
            {
                // Get potion recipe and main effect. (most potions only have one effect)
                EntityEffectBroker effectBroker = GameManager.Instance.EntityEffectBroker;
                PotionRecipe potionRecipe = effectBroker != null ? effectBroker.GetPotionRecipe(item.PotionRecipeKey) : null;
                IEntityEffect potionEffect = potionRecipe != null ? effectBroker.GetPotionRecipeEffect(potionRecipe) : null;

                if (potionEffect != null)
                {
                    StringBuilder powerBuilder = new StringBuilder();

                    TextFile.Token[] tokens = null;
                    {
                        if (potionEffect.SpellBookDescription != null)
                        {
                            List<TextFile.Token> tokenList = new List<TextFile.Token>();
                            for (int i = 0; i < potionEffect.SpellBookDescription.Length; i++)
                            {
                                tokenList.Add(potionEffect.SpellBookDescription[i]);
                            }
                            tokens = tokenList.ToArray();
                        }
                    }

                    if (tokens == null)
                    {
                        if (potionEffect.Properties.SupportMagnitude)
                        {
                            int casterLevel = CalculateCasterLevel(GameManager.Instance.PlayerEntity, potionEffect);
                            int multiplier = (int)Mathf.Floor(casterLevel / potionRecipe.Settings.MagnitudePerLevel);
                            int minMag = potionRecipe.Settings.MagnitudeBaseMin + potionRecipe.Settings.MagnitudePlusMin * multiplier;
                            int maxMag = Mathf.Max(potionRecipe.Settings.MagnitudeBaseMax + potionRecipe.Settings.MagnitudePlusMax * multiplier, minMag);

                            //powerBuilder.Append(potionRecipe.DisplayName);
                            //powerBuilder.Append(": ");
                            powerBuilder.Append(minMag.ToString("N0"));
                            if (minMag != maxMag)
                            {
                                powerBuilder.Append('-');
                                powerBuilder.Append(maxMag.ToString("N0"));
                            }
                        }

                        string potionFormat = null;
                        if (!_potionRecipeKeyToPowerFormatDict.TryGetValue(potionRecipe.DisplayNameKey, out potionFormat))
                        {
                            potionFormat = "{0}";
                        }

                        string powerStr = powerBuilder.ToString();
                        if (!string.IsNullOrWhiteSpace(powerStr))
                        {
                            powersList.Add(string.Format(potionFormat, powerStr));
                        }
                    }
                    else
                    {
                        int startIdx = -1;
                        int endIdx = -1;
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            tokens[i].formatting = TextFile.Formatting.Text;

                            if (!string.IsNullOrWhiteSpace(tokens[i].text))
                            {
                                if (startIdx < 0 && tokens[i].text[0] == '"')
                                {
                                    startIdx = i;
                                }
                                else if (endIdx < 0 && tokens[i].text[tokens[i].text.Length - 1] == '"')
                                {
                                    endIdx = i;
                                }
                            }
                        }
                        MacroHelper.ExpandMacros(ref tokens, potionEffect);

                        if (startIdx >= 0 && endIdx >= 0)
                        {
                            tokens[startIdx].text = tokens[startIdx].text.Remove(0, 1);
                            tokens[endIdx].text = tokens[endIdx].text.Remove(tokens[endIdx].text.Length - 1, 1);
                            for (int i = startIdx; i <= endIdx; i++)
                            {
                                string text = tokens[i].text;
                                powerBuilder.Append(tokens[i].text);
                                if (i < endIdx)
                                {
                                    string nextText = tokens[i + 1].text;
                                    if (!string.IsNullOrEmpty(nextText) && nextText[0] != ' ')
                                    {
                                        powerBuilder.Append(' ');
                                    }
                                }
                            }
                        }

                        string powerStr = powerBuilder.ToString();
                        if (!string.IsNullOrWhiteSpace(powerStr))
                        {
                            powersList.Add(powerStr);
                        }
                    }
                }
            }
            else
            {
                TextFile.Token[] tokens = null;
                if (item.legacyMagic != null)
                {
                    tokens = item.GetMacroDataSource().MagicPowers(TextFile.Formatting.Text);
                    MacroHelper.ExpandMacros(ref tokens, item);
                }

                if (tokens != null && tokens.Length > 0)
                {
                    if (item.IsArtifact)
                    {
                        StringBuilder stringBuilder = new StringBuilder();
                        ArtifactsSubTypes artifactType = ItemHelper.GetArtifactSubType(item);
                        switch (artifactType)
                        {
                            case ArtifactsSubTypes.Skull_of_Corruption:
                                for (int i = 1; i <= 12; i++)
                                {
                                    TextFile.Token token = tokens[i];
                                    if (!string.IsNullOrWhiteSpace(token.text))
                                    {
                                        stringBuilder.Append(token.text);
                                        stringBuilder.Append(" ");
                                    }
                                }

                                string[] splits = tokens[13].text.Split('.');
                                stringBuilder.Append(splits[0]);
                                stringBuilder.Append('.');
                                break;

                            default:
                                powersList.Add("[AcrealUI] Artifact Power Not Added Yet");
                                break;
                        }

                        string power = stringBuilder.ToString();
                        if (!string.IsNullOrWhiteSpace(power))
                        {
                            powersList.Add(stringBuilder.ToString());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < tokens.Length; i++)
                        {
                            TextFile.Token token = tokens[i];
                            if (!string.IsNullOrWhiteSpace(token.text))
                            {
                                powersList.Add(token.text);
                            }
                        }
                    }
                }
            }

            return powersList;
        }

        //private static string LocalizeEntityEffect(IEntityEffect entityEffect)
        //{
        //    StringBuilder stringBuilder = new StringBuilder(entityEffect.DisplayName);
        //    stringBuilder.Append(": ");
        //    for (int i = 0; i < entityEffect.SpellBookDescription.Length; i++)
        //    {
        //        if (!string.IsNullOrWhiteSpace(entityEffect.SpellBookDescription[i].text))
        //        {
        //            stringBuilder.Append(entityEffect.SpellBookDescription[i].text);
        //        }
        //    }
        //    return stringBuilder.ToString();
        //}

        //private static string LocalizeEnchantment(DaggerfallEnchantment enchantment)
        //{
        //    // Also 65535 to handle saves from when the type was read as an unsigned value
        //    if (enchantment.type == EnchantmentTypes.None || (int)enchantment.type == 65535)
        //    {
        //        return null;
        //    }

        //    string firstPart = TextManager.Instance.GetLocalizedTextList("itemPowers")[(int)enchantment.type] + " ";

        //    if (enchantment.type == EnchantmentTypes.SoulBound && enchantment.param != -1)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("enemyNames")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.ExtraSpellPts)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("extraSpellPtsTimes")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.PotentVs || enchantment.type == EnchantmentTypes.LowDamageVs)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("enemyGroupNames")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.RegensHealth)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("regensHealthTimes")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.VampiricEffect)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("vampiricEffectRanges")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.IncreasedWeightAllowance)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("increasedWeightAllowances")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.EnhancesSkill)
        //    {
        //        return firstPart + DaggerfallUnity.Instance.TextProvider.GetSkillName((DaggerfallConnect.DFCareer.Skills)enchantment.param);
        //    }
        //    else if (enchantment.type == EnchantmentTypes.ImprovesTalents)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("improvedTalents")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.GoodRepWith || enchantment.type == EnchantmentTypes.BadRepWith)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("repWithGroups")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.ItemDeteriorates)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("itemDeteriorateLocations")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.UserTakesDamage)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("userTakesDamageLocations")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.HealthLeech)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("healthLeechStopConditions")[enchantment.param];
        //    }
        //    else if (enchantment.type == EnchantmentTypes.BadReactionsFrom)
        //    {
        //        return firstPart + TextManager.Instance.GetLocalizedTextList("badReactionFromEnemyGroups")[enchantment.param];
        //    }
        //    else if (enchantment.type <= EnchantmentTypes.CastWhenStrikes)
        //    {
        //        List<DaggerfallConnect.Save.SpellRecord.SpellRecordData> spells = DaggerfallSpellReader.ReadSpellsFile();

        //        foreach (DaggerfallConnect.Save.SpellRecord.SpellRecordData spell in spells)
        //        {
        //            if (spell.index == enchantment.param)
        //            {
        //                string spellName = TextManager.Instance.GetLocalizedSpellName(spell.index);
        //                return firstPart + spellName;
        //            }
        //        }

        //        return firstPart;
        //    }
        //    else
        //    {
        //        return firstPart;
        //    }
        //}

        //private static int GetItemInfoID(DaggerfallUnityItem item)
        //{
        //    switch (item.ItemGroup)
        //    {
        //        case ItemGroups.Armor:
        //            if (ArmorShouldShowMaterial(item))
        //            {
        //                return 1000;                                                // Handle armor showing material
        //            }
        //            else
        //            {
        //                return 1014;                                                // Handle armor not showing material
        //            }

        //        case ItemGroups.Weapons:
        //            if (item.TemplateIndex == (int)Weapons.Arrow)
        //            {
        //                return 1011;                                                // Handle arrows
        //            }
        //            else if (item.IsArtifact)
        //            {
        //                return 1012;                                                // Handle artifacts
        //            }
        //            else
        //            {
        //                return 1001;                                                // Handle weapons
        //            }

        //        case ItemGroups.Books:
        //            if (item.legacyMagic != null && item.legacyMagic[0].type == EnchantmentTypes.SpecialArtifactEffect)
        //            {
        //                return 1015;                                                // Handle Oghma Infinium
        //            }
        //            else
        //            {
        //                return 1009;                                                // Handle other books
        //            }

        //        case ItemGroups.Paintings:
        //            {
        //                return 250;
        //            }

        //        case ItemGroups.MiscItems:
        //            // A few items in the MiscItems group have their own text display
        //            if (item.IsPotionRecipe)
        //            {
        //                return -1;                                                  // Special case handled outside of this function
        //            }
        //            else if (item.TemplateIndex == (int)MiscItems.House_Deed)
        //            {
        //                return 1073;                                                // Handle house deeds
        //            }
        //            else if (item.TemplateIndex == (int)MiscItems.Soul_trap)
        //            {
        //                return 1004;                                                // Handle soul traps
        //            }
        //            else if (item.TemplateIndex == (int)MiscItems.Letter_of_credit)
        //            {
        //                return 1007;                                                // Handle letters of credit
        //            }
        //            else
        //            {
        //                return 1003;                                                // Default misc items
        //            }

        //        default:
        //            // Handle potions in glass bottles
        //            // In classic, the check is whether RecordRoot.SublistHead is non-null and of PotionMix type.
        //            if (item.IsPotion)
        //            {
        //                return 1008;
        //            }
        //            // Handle Azura's Star
        //            else if (item.legacyMagic != null && item.legacyMagic[0].type == EnchantmentTypes.SpecialArtifactEffect && item.legacyMagic[0].param == 9)
        //            {
        //                return 1004;
        //            }
        //            else
        //            {
        //                // Default fallback if none of the above applied
        //                return 1003;
        //            }
        //    }
        //}

        //private static bool ArmorShouldShowMaterial(DaggerfallUnityItem item)
        //{
        //    // HelmAndShieldMaterialDisplay setting for showing material for helms and shields:
        //    // 0 : Don't show (classic behavior)
        //    // 1 : Show for all but leather and chain
        //    // 2 : Show for all but leather
        //    // 3 : Show for all
        //    if (item.IsArtifact)
        //        return false;
        //    else if (item.IsShield || item.TemplateIndex == (int)Armor.Helm)
        //    {
        //        if ((DaggerfallUnity.Settings.HelmAndShieldMaterialDisplay == 1)
        //            && ((ArmorMaterialTypes)item.nativeMaterialValue >= ArmorMaterialTypes.Iron))
        //            return true;
        //        else if ((DaggerfallUnity.Settings.HelmAndShieldMaterialDisplay == 2)
        //            && ((ArmorMaterialTypes)item.nativeMaterialValue >= ArmorMaterialTypes.Chain))
        //            return true;
        //        else if (DaggerfallUnity.Settings.HelmAndShieldMaterialDisplay == 3)
        //            return true;
        //        else
        //            return false;
        //    }
        //    else
        //        return true;
        //}
        #endregion


        #region Item Sorting
        public static void SortItemsByColumn(List<UIItemData> itemData, ItemColumnFlags column, bool sortAscending)
        {
            if (itemData == null || itemData.Count == 0) { return; }

            if (column == ItemColumnFlags.Armor)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return x.armorValue.CompareTo(y.armorValue); }); }
                else { itemData.Sort((x, y) => { return y.armorValue.CompareTo(x.armorValue); }); }
            }
            else if (column == ItemColumnFlags.Condition)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return x.conditionPercent.CompareTo(y.conditionPercent); }); }
                else { itemData.Sort((x, y) => { return y.conditionPercent.CompareTo(x.conditionPercent); }); }
            }
            else if (column == ItemColumnFlags.Damage)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return x.maxDamageValue.CompareTo(y.maxDamageValue); }); }
                else { itemData.Sort((x, y) => { return y.maxDamageValue.CompareTo(x.maxDamageValue); }); }
            }
            else if (column == ItemColumnFlags.GoldValue)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return x.goldValue.CompareTo(y.goldValue); }); }
                else { itemData.Sort((x, y) => { return y.goldValue.CompareTo(x.goldValue); }); }
            }
            else if (column == ItemColumnFlags.ItemType)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return x.itemArchetype.CompareTo(y.itemArchetype); }); }
                else { itemData.Sort((x, y) => { return y.itemArchetype.CompareTo(x.itemArchetype); }); }
            }
            else if (column == ItemColumnFlags.Name)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return GetItemName(x).CompareTo(GetItemName(y)); }); }
                else { itemData.Sort((x, y) => { return GetItemName(y).CompareTo(GetItemName(x)); }); }
            }
            else if (column == ItemColumnFlags.Weight)
            {
                if (sortAscending) { itemData.Sort((x, y) => { return x.weightValue.CompareTo(y.weightValue); }); }
                else { itemData.Sort((x, y) => { return y.weightValue.CompareTo(x.weightValue); }); }
            }
            else
            {
                Debug.LogError("[AcrealUI] Sorting by Column with Value \"" + column.ToString() + "\" is either not supported or not intended.");
            }
        }
        #endregion


        #region Enemies
        public static List<MobileTypes> GetPossibleEnemyTypes()
        {
            List<MobileTypes> enemyTypeList = new List<MobileTypes>();

            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            GameObject playerObject = GameManager.Instance.PlayerObject;
            if (playerEntity == null || playerObject == null)
            {
                return enemyTypeList;
            }

            PlayerEnterExit enterExit = playerObject.GetComponent<PlayerEnterExit>();
            if (enterExit != null)
            {
                DaggerfallDungeon dungeon = enterExit.IsPlayerInsideDungeon ? enterExit.Dungeon : null;
                if (dungeon != null)
                {
                    //NOTE(Acreal): replace this with x faster method?
                    //this may be good enough, considering how rarely it gets called
                    //and the fact that it's 100% accurate to whatever is spawned
                    SetupDemoEnemy[] allEnemySetup = dungeon.GetComponentsInChildren<SetupDemoEnemy>(true);
                    if (allEnemySetup != null)
                    {
                        for (int i = 0; i < allEnemySetup.Length; i++)
                        {
                            if (allEnemySetup[i] != null && !enemyTypeList.Contains(allEnemySetup[i].EnemyType))
                            {
                                enemyTypeList.Add(allEnemySetup[i].EnemyType);
                            }
                        }
                    }
                }
            }
            return enemyTypeList;
        }

        private static void AddTableToEnemyTypesList(RandomEncounterTable table, List<MobileTypes> enemyTypesList)
        {
            GameObject playerObject = GameManager.Instance.PlayerObject;
            PlayerEnterExit enterExit = playerObject != null ? playerObject.GetComponent<PlayerEnterExit>() : null;
            DaggerfallDungeon dungeon = enterExit != null && enterExit.IsPlayerInsideDungeon ? enterExit.Dungeon : null;
            if (dungeon != null)
            {
                PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
                int level = playerEntity != null ? playerEntity.Level : 1;
                int monsterVariance = dungeon.RandomMonsterVariance;
                float monsterPower = Mathf.Clamp01(level / 20f);

                int baseMonsterIndex = Mathf.CeilToInt(table.Enemies.Length * monsterPower);
                int minMonsterIndex = Mathf.Max(baseMonsterIndex - monsterVariance, 0);
                int maxMonsterIndex = Mathf.Min(baseMonsterIndex + monsterVariance, table.Enemies.Length - 1);
                for (int i = minMonsterIndex; i <= maxMonsterIndex; i++)
                {
                    MobileTypes enemyType = table.Enemies[i];
                    if (!enemyTypesList.Contains(enemyType))
                    {
                        enemyTypesList.Add(enemyType);
                    }
                }
            }
        }
        #endregion


        #region Sounds
        public static void PlayButtonClickSound(bool force = false)
        {
            float curTime = Time.unscaledTime;
            if (force || (curTime - _prevButtonClickSfxTime) >= 0.1f)
            {
                _prevButtonClickSfxTime = curTime;
                DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            }
        }
        #endregion


        #region Save Games
        public static UISaveGameData GetSaveGameData(int saveKey)
        {
            if (saveKey >= 0)
            {
                string path = GameManager.Instance.SaveLoadManager.GetSaveFolder(saveKey);
                if (!string.IsNullOrEmpty(path))
                {
                    SaveInfo_v1 saveInfo = GameManager.Instance.SaveLoadManager.GetSaveInfo(saveKey);
                    Texture2D saveTexture = GameManager.Instance.SaveLoadManager.GetSaveScreenshot(saveKey);

                    DaggerfallDateTime dfDateTime = new DaggerfallDateTime();
                    dfDateTime.FromSeconds(saveInfo.dateAndTime.gameTime);
                    string gameTimeStr = dfDateTime.MidDateTimeString();

                    DateTime realDateTime = DateTime.FromBinary(saveInfo.dateAndTime.realTime);
                    StringBuilder stringBuilder = new StringBuilder(realDateTime.ToLongTimeString());
                    stringBuilder.Append("  ");
                    stringBuilder.Append(realDateTime.ToLongDateString());
                    string realTime = stringBuilder.ToString();


                    return new UISaveGameData()
                    {
                        isValid = true,
                        saveKey = saveKey,
                        realTimestamp = realDateTime,
                        saveName = saveInfo.saveName,
                        saveFilePath = Path.GetFileName(path),
                        characterName = saveInfo.characterName,
                        realTimestampString = realTime,
                        gameTimestampString = gameTimeStr,
                        gameVersion = string.Format("V{0}", saveInfo.saveVersion),
                        screenshot = saveTexture,
                    };
                }
            }
            return new UISaveGameData();
        }

        public static UISaveGameData GetMostRecentSaveGameData()
        {
            int mostRecentSaveKey = GameManager.Instance.SaveLoadManager.FindMostRecentSave();
            return GetSaveGameData(mostRecentSaveKey);
        }

        public static UISaveGameData GetMostRecentSaveGameData(string characterName)
        {
            List<UISaveGameData> allSaveDatas = GetAllCharacterSaveGameData(characterName);
            return allSaveDatas != null && allSaveDatas.Count > 0 ? allSaveDatas[0] : new UISaveGameData();
        }

        public static List<UISaveGameData> GetAllCharacterSaveGameData(string characterName)
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                return null;
            }

            int[] saveKeys = GameManager.Instance.SaveLoadManager.GetCharacterSaveKeys(characterName);
            if (saveKeys != null && saveKeys.Length > 0)
            {
                List<UISaveGameData> saves = new List<UISaveGameData>();
                foreach (int key in saveKeys)
                {
                    UISaveGameData saveData = GetSaveGameData(key);
                    if (saveData.isValid) { saves.Add(saveData); }
                }
                saves.Sort((x, y) => { return y.realTimestamp.CompareTo(x.realTimestamp); });
                return saves;
            }
            else
            {
                return null;
            }
        }

        public static string[] GetAllSavedCharacterNames()
        {
            return GameManager.Instance != null ? GameManager.Instance.SaveLoadManager.GetCharacterNames() : null;
        }

        public static int FindSaveFolderByNames(string characterName, string saveName)
        {
            return GameManager.Instance.SaveLoadManager.FindSaveFolderByNames(characterName, saveName);
        }
        #endregion


        #region Text
        public static string GetLocalizedText(string key)
        {
            return TextManager.Instance != null ? TextManager.Instance.GetLocalizedText(key) : null;
        }

        public static string GetLocalizedText(string key, string localizationTableID)
        {
            return TextManager.Instance != null ? TextManager.Instance.GetText(localizationTableID, key) : null;
        }

        public static string GetLocalizedText(string key, TextCollections collection)
        {
            return TextManager.Instance != null ? TextManager.Instance.GetLocalizedText(key, collection) : null;
        }

        public static string[] GetLocalizedTextList(string key, TextCollections collection)
        {
            return TextManager.Instance != null ? TextManager.Instance.GetLocalizedTextList(key, collection) : null;
        }

        public static string SplitStringIntoWords(string text)
        {
            return SplitTextIntoWords(text, 0);
        }

        public static string SplitTextIntoWords(string text, int startIndex)
        {
            if (text == null)
            {
                return null;
            }

            startIndex = Mathf.Max(startIndex, 1);
            for (int i = startIndex; i < text.Length; i++)
            {
                int prevLetterVal = text[i - 1];
                if (prevLetterVal != ' ')
                {
                    bool shouldSplit = prevLetterVal == '.';

                    if (!shouldSplit)
                    {
                        int letterVal = text[i];
                        if (letterVal >= 'A' && letterVal <= 'Z')
                        {
                            shouldSplit = true;
                        }
                    }

                    if (shouldSplit)
                    {
                        int nextIdx = i + 1;
                        if (nextIdx < text.Length - 1)
                        {
                            return SplitTextIntoWords(text.Insert(i, " "), nextIdx);
                        }
                    }
                }
            }

            return text;
        }
        #endregion


        #region Sprites
        public static Sprite GetGroundContainerIcon()
        {
            if (DaggerfallUnity.Instance == null) { return null; }
            ImageData imgData = DaggerfallUnity.Instance.ItemHelper.GetContainerImage(InventoryContainerImages.Ground);
            return UISpriteManager.GetOrCreateSprite(imgData);
        }

        public static Sprite GetWagonContainerIcon()
        {
            if (DaggerfallUnity.Instance == null) { return null; }
            ImageData imgData = DaggerfallUnity.Instance.ItemHelper.GetContainerImage(InventoryContainerImages.Wagon);
            return UISpriteManager.GetOrCreateSprite(imgData);
        }

        public static Sprite GetLootContainerIcon(DaggerfallLoot lootTarget)
        {
            if (lootTarget != null)
            {
                int dropIconArchive = lootTarget.TextureArchive;
                int dropIconTexture = -1;

                if (lootTarget.playerOwned && dropIconArchive > 0)
                {
                    int[] iconIdxs;
                    DaggerfallLootDataTables.dropIconIdxs.TryGetValue(dropIconArchive, out iconIdxs);
                    if (iconIdxs != null)
                    {
                        for (int i = 0; i < iconIdxs.Length; i++)
                        {
                            if (iconIdxs[i] == lootTarget.TextureRecord)
                            {
                                dropIconTexture = i;
                                break;
                            }
                        }
                    }
                }

                if (dropIconTexture > -1)
                {
                    string filename = TextureFile.IndexToFileName(dropIconArchive);
                    ImageData containerImage = ImageReader.GetImageData(filename, DaggerfallLootDataTables.dropIconIdxs[dropIconArchive][dropIconTexture], 0, true);
                }
                else if (dropIconArchive > 0)
                {
                    string filename = TextureFile.IndexToFileName(lootTarget.TextureArchive);
                    ImageData containerImage = ImageReader.GetImageData(filename, lootTarget.TextureRecord, 0, true);
                    return UISpriteManager.GetOrCreateSprite(containerImage);
                }
                else
                {
                    ImageData containerImage = DaggerfallUnity.Instance.ItemHelper.GetContainerImage(lootTarget.ContainerImage);
                    return UISpriteManager.GetOrCreateSprite(containerImage);
                }
            }
            return GetRandomLootContainerIcon();
        }

        public static Sprite GetRandomLootContainerIcon()
        {
            int dropIconArchive = DaggerfallLootDataTables.randomTreasureArchive;

            int iconIndex = UnityEngine.Random.Range(0, DaggerfallLootDataTables.dropIconIdxs[dropIconArchive].Length);
            int dropIconTexture = DaggerfallLootDataTables.dropIconIdxs[dropIconArchive][iconIndex];

            string filename = TextureFile.IndexToFileName(dropIconArchive);
            ImageData containerImage = ImageReader.GetImageData(filename, dropIconTexture, 0, true);
            return UISpriteManager.GetOrCreateSprite(containerImage);
        }
        #endregion


        #region Misc Helper Functions
        /// <summary>
        /// Clamps x given RectTransform to the limits of the screen
        /// (NOTE: this function assumes that the RectTransform is the child of an overlay canvas!)
        /// </summary>
        /// <param name="rt"></param>
        public static void ClampToScreen(RectTransform rt, float edgePadding = 10f)
        {
            if (rt != null)
            {
                int screenWidth = Screen.width;
                int screenHeight = Screen.height;

                Vector2 posOffset = Vector2.zero;
                rt.GetWorldCorners(_worldCornerScratchArray);

                float leftDiff = _worldCornerScratchArray[0].x * -1f;
                float rightDiff = _worldCornerScratchArray[2].x - (screenWidth - edgePadding);
                if (rightDiff > 0f)
                {
                    posOffset.x -= rightDiff;
                }
                else if (leftDiff > 0f)
                {
                    posOffset.x += leftDiff;
                }

                float bottomDiff = _worldCornerScratchArray[0].y * -1f;
                float topDiff = _worldCornerScratchArray[2].y - (screenHeight - edgePadding);
                if (topDiff > 0f)
                {
                    posOffset.y -= topDiff;
                }
                else if (bottomDiff > 0f)
                {
                    posOffset.y += bottomDiff;
                }

                rt.anchoredPosition = rt.anchoredPosition + posOffset;
            }
        }
        #endregion


        #region Skills
        public static SkillRank SkillToSkillRank(DFCareer.Skills skill)
        {
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            if (playerEntity != null)
            {
                if (playerEntity.GetPrimarySkills().Contains(skill))
                {
                    return SkillRank.Primary;
                }
                else if (playerEntity.GetMajorSkills().Contains(skill))
                {
                    return SkillRank.Major;
                }
                else if (playerEntity.GetMinorSkills().Contains(skill))
                {
                    return SkillRank.Minor;
                }
            }
            return SkillRank.None;
        }
        #endregion


        #region Keybinding
        public static void SetDefaultKeybinds()
        {
            InputManager.Instance.ResetDefaults();
            InputManager.Instance.SaveKeyBinds();
            ControlsConfigManager.Instance.ResetUnsavedKeybinds();
        }
        #endregion
    }
}
