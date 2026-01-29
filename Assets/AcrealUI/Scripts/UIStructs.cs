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

using System;
using UnityEngine;

namespace AcrealUI
{
    #region Items
    public struct UIItemData
    {
        public ulong UID;
        public string shortName;
        public string longName;
        public string description;
        public ItemStatusFlags itemStatusFlags;
        public ItemType itemType;
        public ItemArchetype itemArchetype;
        public Sprite icon;
        public Sprite archetypeIcon;
        public string materialString;
        public int stackCount;
        public int minDamageValue;
        public int maxDamageValue;
        public int armorValue;
        public float conditionPercent;
        public float weightValue;
        public int goldValue;
    }

    public struct ItemStatData
    {
        public Sprite icon;
        public string name;
        public string description;
    }

    public struct ItemStatSliderData
    {
        public Sprite icon;
        public string name;
        public string description;
        public float sliderValue;
        public bool showSlider;
        public bool showText;
    }

    public struct ItemPowerData
    {
        public Sprite icon;
        public string name;
        public string description;
    }

    public struct UIStatData
    {
        public int statEnumAsInt;
        public string name;
        public Sprite icon;
    }

    public struct UISkillData
    {
        public int skillEnumAsInt;
        public string name;
        public Sprite icon;
        public Sprite rankIcon;
    }

    /// <summary>
    /// used to determine if an item is forbidden for the player to use
    /// </summary>
    public struct UIItemQueryOptions
    {
        public int forbiddenShieldsAsInt;
        public int forbiddenArmorsAsInt;
        public int forbiddenMaterialsAsInt;
        public int forbiddenProficienciesAsInt;
    }
    #endregion


    #region Save Games
    public struct UISaveGameData
    {
        public bool isValid;
        public int saveKey;
        public DateTime realTimestamp; //used for sorting saves by most recent
        public string saveName;
        public string saveFilePath;
        public string characterName;
        public string gameVersion;
        public string realTimestampString;
        public string gameTimestampString;
        public Texture2D screenshot;
    }
    #endregion
}
