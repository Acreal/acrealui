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

namespace AcrealUI
{
    [Flags]
    public enum ItemColumnFlags
    {
        Name = 1 << 0,
        ItemType = 1 << 1,
        Damage = 1 << 2,
        Armor = 1 << 3,
        Weight = 1 << 4,
        GoldValue = 1 << 5,
        Condition = 1 << 6,

        All = ItemType | Name | Condition | Weight | GoldValue,
        Filter_Weapons = ItemType | Name | Damage | Condition | Weight | GoldValue,
        Filter_Armor = ItemType | Name | Armor | Condition | Weight | GoldValue,
        Filter_Potions = ItemType | Name | Weight | GoldValue,
        Filter_MagicItems = ItemType | Name | Condition | Weight | GoldValue,
        Filter_Misc = ItemType | Name | Weight | GoldValue,
        Filter_Books = ItemType | Name | Weight | GoldValue,
        Filter_QuestItems = ItemType | Name | Weight | GoldValue,
    }

    public enum ItemType
    {
        Unknown = 0,
        Weapon = 1,
        Armor = 2,
        Clothing = 3,
        Potion = 4,
        Food = 5,
        Ingredient = 6,
        Readable = 7,
        QuestItem = 8,
        Junk = 9,
        Currency = 10,
        Jewellery = 11,
    }

    public enum ItemArchetype
    {
        Unknown = 0,

        Shield = 1,
        ShortBlade = 2,
        LongBlade = 3,
        Bow = 4,
        Mace = 5,
        Axe = 6,
        Warhammer = 7,
        Staff = 8,
        Flail = 9,
        Arrow = 10,

        Helm = 11,
        Chestpiece = 12,
        Gauntlets = 13,
        Greaves = 14,
        Boots = 15,
        Pauldrons = 16,
        Wrists = 17,

        MetalIngot = 18,
        PlantIngredient = 19,
        CreatureIngredient = 20,
        LiquidIngredient = 21,
        ToothIngredient = 22,
        MiscIngredient = 23,

        LightSource = 24,
        Gem = 25,
        Painting = 26,
        Religion = 27,

        Ring = 28,
        Amulet = 29,
        Bracelet = 30,
        Mark = 31,

        ClothingChest = 32,
        ClothingLegs = 33,
        Cloak = 34,

        Gold = 35,
        
        Book = 36,
        Spellbook = 37,
        Note = 38,
        Map = 39,
        Recipe = 40,

        _COUNT = 41,
    }

    public enum ItemFilter
    {
        All = 0,
        Favorite = 1,
        Weapons = 2,
        Armor = 3,
        Potions = 4,
        MagicItems = 5,
        Books = 6,
        QuestItems = 7,
        MiscItems = 8,
        Ingredients = 9,

        _COUNT = 10
    }

    [Flags]
    public enum ItemStatusFlags
    {
        Broken = 1 << 0,
        Prohibited = 1 << 1,
        Equipped = 1 << 2,
        Magic = 1 << 3,
        Poisoned = 1 << 4,
        Favorited = 1 << 5,
    }

    public enum TooltipType
    {
        Text = 0,
        IconText = 1,
        ItemDetails = 2,
    }

    public enum SkillRank
    {
        None = 0,
        Minor = 1,
        Major = 2,
        Primary = 3,
    }
}
