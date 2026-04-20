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

namespace AcrealUI
{
    public static class UITextStrings
    {
        //NOTE(Acreal): Any container below that has the first parameter defined, but
        //the second parameter as null, is NOT localized
        //the two parameters should be reversed once a localizationKey is available, such
        //that the first parameter is null, and the second is defined

        public static TextContainer Global_Label_None = new TextContainer("None", null);

        public static TextContainer InventoryWindow_Label_Armor = new TextContainer("Armor:", null);
        public static TextContainer InventoryWindow_Label_CombatSkills = new TextContainer("Combat Skills", null);
        public static TextContainer InventoryWindow_Label_Condition = new TextContainer("Condition:", null);
        public static TextContainer InventoryWindow_Label_Damage = new TextContainer("Damage:", null);
        public static TextContainer InventoryWindow_Label_LanguageSkills = new TextContainer("Language Skills", null);
        public static TextContainer InventoryWindow_Label_MagicSkills = new TextContainer("Magic Skills", null);
        public static TextContainer InventoryWindow_Label_MovementSkills = new TextContainer("Movement Skills", null);
        public static TextContainer InventoryWindow_Label_PrimaryStats = new TextContainer("Primary Stats", null);
        public static TextContainer InventoryWindow_Label_SocialSkills = new TextContainer("Social Skills", null);
        public static TextContainer InventoryWindow_Label_SplitStack = new TextContainer("Split Stack", null);
        public static TextContainer InventoryWindow_Label_StealthSkills = new TextContainer("Stealth Skills", null);
        public static TextContainer InventoryWindow_Label_Value = new TextContainer("Value:", null);
        public static TextContainer InventoryWindow_Label_Weight = new TextContainer("Weight:", null);

        public static TextContainer Abbreviation_Grams = new TextContainer("g", null);
        public static TextContainer Abbreviation_Kilograms = new TextContainer("Kg", null);
    }
}
